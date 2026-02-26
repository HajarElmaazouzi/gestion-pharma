using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using gestion_pharma.Models.Enums;
using gestion_pharma.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    [Authorize] // allow any authenticated user; actions verify ownership/roles as needed
    public class PaiementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly FidelityService _fidelityService;

        public PaiementsController(ApplicationDbContext context, IWebHostEnvironment env, FidelityService fidelityService)
        {
            _context = context;
            _env = env;
            _fidelityService = fidelityService;
        }

        // GET: Paiements
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Paiements.Include(p => p.Commande);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Paiements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var paiement = await _context.Paiements
                .Include(p => p.Commande)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paiement == null) return NotFound();

            return View(paiement);
        }

        // Manual create/edit/delete are forbidden; payments are created only by checkout flows
        // GET: Paiements/Create
        public IActionResult Create()
        {
            return Forbid();
        }

        // POST: Paiements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Montant,StatutPaiement,Methode,Price,CommandeId")] Paiement paiement)
        {
            return Forbid();
        }

        // GET: Paiements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return Forbid();
        }

        // POST: Paiements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Montant,StatutPaiement,Methode,Price,CommandeId,CreatedAt,IsActive")] Paiement paiement)
        {
            return Forbid();
        }

        // GET: Paiements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            return Forbid();
        }

        // POST: Paiements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            return Forbid();
        }

        // GET: Paiements/SelectMethod
        [HttpGet]
        public async Task<IActionResult> SelectMethod(int? commandeId)
        {
            if (commandeId == null) return BadRequest();
            var commande = await _context.Commandes.FindAsync(commandeId);
            if (commande == null) return NotFound();
            
            // Ensure owner
            var userEmail = User.Identity?.Name;
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == commande.ClientId);
            if (client == null)
            {
                return Forbid();
            }
            if (!_env.IsDevelopment() && !string.Equals(client.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return View(commande);
        }

        // GET: Paiements/ProcessEnLigne/5
        // Montre le formulaire de paiement en ligne pour une commande donnée
        public async Task<IActionResult> ProcessEnLigne(int? commandeId)
        {
            if (commandeId == null) return BadRequest();

            var commande = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.LigneCommandes)
                .ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync(c => c.Id == commandeId);

            if (commande == null) return NotFound();

            // ensure current user owns the commande
            var userEmail = User.Identity?.Name;
            if (!_env.IsDevelopment() && !string.Equals(commande.Client?.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Préparer un PaiementEnLigne prérempli
            var paiement = new PaiementEnLigne
            {
                CommandeId = commande.Id,
                Montant = commande.Montant,
                Price = commande.Montant,
                Methode = MethodePaiment.CarteBancaireEnLigne,
                StatutPaiement = false
            };

            ViewData["Commande"] = commande;
            ViewData["Methode"] = new SelectList(Enum.GetValues(typeof(MethodePaiment)), paiement.Methode);
            return View(paiement);
        }

        // POST: Paiements/ProcessEnLigne
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessEnLigne([Bind("Montant,Price,Methode,CommandeId,NumTransaction,StatutTransaction")] PaiementEnLigne paiementEnLigne)
        {
            if (!ModelState.IsValid)
            {
                // Ensure the related commande is loaded for the view to avoid NullReference
                var commande = await _context.Commandes
                    .Include(c => c.Client)
                    .Include(c => c.LigneCommandes)
                    .ThenInclude(l => l.Produit)
                    .FirstOrDefaultAsync(c => c.Id == paiementEnLigne.CommandeId);

                if (commande == null)
                {
                    return NotFound();
                }

                ViewData["Commande"] = commande;
                ViewData["Methode"] = new SelectList(Enum.GetValues(typeof(MethodePaiment)), paiementEnLigne.Methode);
                return View(paiementEnLigne);
            }

            // ensure current user owns the commande
            var commandeCheck = await _context.Commandes.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == paiementEnLigne.CommandeId);
            var userEmail = User.Identity?.Name;
            if (commandeCheck == null) return NotFound();
            if (!_env.IsDevelopment() && !string.Equals(commandeCheck.Client?.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Simuler/valider la transaction auprès du provider ici.
            // Pour l'instant : simuler succès si StatutTransaction == true, sinon échouer.
            paiementEnLigne.CreatedAt = DateTime.UtcNow;
            paiementEnLigne.IsActive = true;

            // Générer un numéro de transaction si nécessaire
            if (paiementEnLigne.NumTransaction == 0)
                paiementEnLigne.NumTransaction = new Random().Next(100000, 999999);

            // Si la transaction est réussie, marquer StatutTransaction et StatutPaiement true
            paiementEnLigne.StatutTransaction = true;
            paiementEnLigne.StatutPaiement = true;

            // Check for existing payment to avoid duplicate key error
            var existingPaiement = await _context.Paiements.FirstOrDefaultAsync(p => p.CommandeId == paiementEnLigne.CommandeId);
            if (existingPaiement != null)
            {
                _context.Paiements.Remove(existingPaiement); // Remove old attempt or update it. Removing safest to avoid type discriminator issues if switching types (Paiement vs PaiementEnLigne)
                await _context.SaveChangesAsync();
            }

            _context.Paiements.Add(paiementEnLigne);
            await _context.SaveChangesAsync();

            // Traiter ce paiement (création de facture + points fidélité + validation commande)
            await HandleSuccessfulPayment(paiementEnLigne);

            TempData["Success"] = "Paiement effectué avec succès ! Merci de votre commande.";
            return RedirectToAction(nameof(Success), new { id = paiementEnLigne.Id });
        }

        // POST API: /api/payments/process
        [HttpPost]
        [AllowAnonymous]
        [Route("api/payments/process")]
        [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        public async Task<IActionResult> ApiProcess([FromBody] gestion_pharma.Models.Dto.PaymentRequest req)
        {
            if (req == null) return BadRequest(new { success = false, message = "Invalid request" });

            var commande = await _context.Commandes
                .Include(c => c.Client)
                .Include(c => c.LigneCommandes)
                .ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync(c => c.Id == req.CommandeId);
            if (commande == null) return NotFound(new { success = false, message = "Commande non trouvée" });

            var userEmail = User.Identity?.Name;
            if (!_env.IsDevelopment() && !string.Equals(commande.Client?.Email, userEmail, System.StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Simuler la validation du paiement par le provider
            var paiementEnLigne = new PaiementEnLigne
            {
                CommandeId = commande.Id,
                Montant = req.Montant,
                Price = req.Montant,
                Methode = Models.Enums.MethodePaiment.CarteBancaireEnLigne,
                StatutTransaction = true,
                StatutPaiement = true,
                CreatedAt = System.DateTime.UtcNow,
                IsActive = true,
                NumTransaction = new System.Random().Next(100000, 999999)
            };

            // Remove existing payment if any
            var existing = await _context.Paiements.FirstOrDefaultAsync(p => p.CommandeId == commande.Id);
            if (existing != null)
            {
                _context.Paiements.Remove(existing);
                await _context.SaveChangesAsync();
            }

            _context.Paiements.Add(paiementEnLigne);
            await _context.SaveChangesAsync();

            await HandleSuccessfulPayment(paiementEnLigne);

            return Ok(new { success = true, id = paiementEnLigne.Id });
        }

        // DEBUG: create a test commande (Development only)
        [HttpPost]
        [AllowAnonymous]
        [Route("api/debug/create-test-commande")]
        [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateTestCommande()
        {
            if (!_env.IsDevelopment()) return Forbid();

            // find or create a test client
            var testEmail = "test@local";
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == testEmail);
            if (client == null)
            {
                client = new gestion_pharma.Models.Entities.Client
                {
                    Email = testEmail,
                    Nom = "Client Test",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            // pick an existing product
            var produit = await _context.Produits.FirstOrDefaultAsync();
            if (produit == null) return BadRequest(new { success = false, message = "No product available to create test commande" });

            var commande = new gestion_pharma.Models.Entities.Commande
            {
                ClientId = client.Id,
                Montant = (double)produit.Prix,
                Statut = Models.Enums.StatutCommande.EnAttente,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Commandes.Add(commande);
            await _context.SaveChangesAsync();

            var ligne = new gestion_pharma.Models.Entities.LigneCommande
            {
                CommandeId = commande.Id,
                ProduitId = produit.Id,
                Quantite = 1,
                PrixUnitaire = (double)produit.Prix,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.LigneCommandes.Add(ligne);
            await _context.SaveChangesAsync();

            // update montant
            commande.Montant = await _context.LigneCommandes.Where(l => l.CommandeId == commande.Id).SumAsync(l => l.Quantite * l.PrixUnitaire);
            _context.Commandes.Update(commande);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, commandeId = commande.Id });
        }

        // GET: Paiements/Success/5
        public async Task<IActionResult> Success(int? id)
        {
               if (id == null) return NotFound();
               var paiement = await _context.Paiements
                  .Include(p => p.Commande)
                  .ThenInclude(c => c.Client)
                  .FirstOrDefaultAsync(p => p.Id == id);
               if (paiement == null) return NotFound();

               // Add loyalty points after successful online payment
                   // Points and promo generation are handled in the payment processing pipeline (HandleSuccessfulPayment)

               return View(paiement);
        }

        // POST: Paiements/PaiementSurPlace
        // Crée un paiement en attente (paiement à effectuer sur place / à la livraison)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> PaiementSurPlace(int commandeId, MethodePaiment methode = MethodePaiment.Especes)
        {
            var commande = await _context.Commandes.FindAsync(commandeId);
            if (commande == null) return NotFound();

            // ensure current user owns the commande
            var userEmail = User.Identity?.Name;
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == commande.ClientId);
            if (client == null || !string.Equals(client.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Check if payment already exists
            var existingPaiement = await _context.Paiements.FirstOrDefaultAsync(p => p.CommandeId == commande.Id);
            
            Paiement paiement;
            if (existingPaiement != null)
            {
                paiement = existingPaiement;
                paiement.Methode = methode;
                // If previously online/failed, reset? No, keep logic simple: update method.
                _context.Paiements.Update(paiement);
            }
            else
            {
                paiement = new Paiement
                {
                    CommandeId = commande.Id,
                    Montant = commande.Montant,
                    Price = commande.Montant,
                    Methode = methode,
                    StatutPaiement = false, // paiement non encore réalisé
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Paiements.Add(paiement);
            }

            // garder la commande en état EnAttente (paiement à effectuer sur place)
            commande.Statut = StatutCommande.EnAttente;
            _context.Commandes.Update(commande);

            await _context.SaveChangesAsync();

            // Add loyalty points after payment creation (in-store payment = payment complete at order time)
            await _fidelityService.AddPoints(commande.ClientId, commande.Montant);

            return RedirectToAction(nameof(Details), new { id = paiement.Id });
        }

        private bool PaiementExists(int id)
        {
            return _context.Paiements.Any(e => e.Id == id);
        }

        // GET: Paiements/Invoice/5
        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null) return NotFound();

            var paiement = await _context.Paiements
                .Include(p => p.Commande)
                .ThenInclude(c => c.Client)
                .Include(p => p.Commande)
                .ThenInclude(c => c.LigneCommandes)
                .ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (paiement == null) return NotFound();

            // Ensure owner
            var userEmail = User.Identity?.Name;
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == paiement.Commande.ClientId);
             if (client == null || !string.Equals(client.Email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return View(paiement);
        }

        private async Task HandleSuccessfulPayment(Paiement paiement)
        {
            if (paiement == null) return;

            // Mettre à jour la commande liée à ce paiement : marquer comme validée
            var commande = await _context.Commandes
                .Include(c => c.LigneCommandes)
                .FirstOrDefaultAsync(c => c.Id == paiement.CommandeId);

            if (commande != null)
            {
                commande.Statut = StatutCommande.Validee;
                _context.Commandes.Update(commande);

                // Decrement stock
                if (commande.LigneCommandes != null)
                {
                    foreach (var ligne in commande.LigneCommandes)
                    {
                        var produit = await _context.Produits.FindAsync(ligne.ProduitId);
                        if (produit != null)
                        {
                            produit.Stock = Math.Max(0, produit.Stock - ligne.Quantite);
                            _context.Produits.Update(produit);
                        }
                    }
                }
            }

            // Créer la facture si elle n'existe pas encore pour ce paiement
            var existingFacture = await _context.Factures.FirstOrDefaultAsync(f => f.PaiementId == paiement.Id);
            if (existingFacture == null)
            {
                var facture = new Facture
                {
                    PaiementId = paiement.Id,
                    Date = DateTime.UtcNow,
                    MontantTotal = paiement.Montant,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Factures.Add(facture);
            }

            // Mettre à jour la carte de fidélité via le service dédié
            if (commande == null) return;

            // Add loyalty points based on payment amount
            await _fidelityService.AddPoints(commande.ClientId, paiement.Montant);

            await _context.SaveChangesAsync();
        }
    }
}
