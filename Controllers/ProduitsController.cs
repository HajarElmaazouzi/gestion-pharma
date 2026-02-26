using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;                                         
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using gestion_pharma.Models.Enums;

namespace gestion_pharma.Controllers
{
    // Removed class-level Authorize so anonymous users can view the catalogue
    public class ProduitsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Userpers> _userManager;
        private readonly SignInManager<Userpers> _signInManager;
        private readonly ILogger<ProduitsController> _logger;

        public ProduitsController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<Userpers> userManager, SignInManager<Userpers> signInManager, ILogger<ProduitsController> logger)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: Produits (management/catalogue)
        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            var produitsQuery = _context.Produits.Include(p => p.Categorie).Include(p => p.Fournisseur).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                produitsQuery = produitsQuery.Where(p => p.Nom.Contains(searchString) || p.Description.Contains(searchString));
            }

            // Parapharmacien or Admin sees the scaffolded management UI; others see client catalogue
            if (User.IsInRole("Parapharmacien") || User.IsInRole("Admin"))
            {
                return View(await produitsQuery.ToListAsync());
            }

            ViewData["CurrentFilter"] = searchString;
            return View("IndexClient", await produitsQuery.ToListAsync());
        }

        // GET: Produits/IndexClient (client-facing catalogue)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> IndexClient(string searchString)
        {
            var produitsQuery = _context.Produits.Include(p => p.Categorie).Include(p => p.Fournisseur).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                produitsQuery = produitsQuery.Where(p => p.Nom.Contains(searchString) || p.Description.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View("IndexClient", await produitsQuery.ToListAsync());
        }

        // Dedicated route: /produit should always show the client-facing catalogue (no edit/delete links)
        [HttpGet("/produit")]
        [AllowAnonymous]
        public async Task<IActionResult> Catalogue(string searchString)
        {
            var produitsQuery = _context.Produits.Include(p => p.Categorie).Include(p => p.Fournisseur).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                produitsQuery = produitsQuery.Where(p => p.Nom.Contains(searchString) || p.Description.Contains(searchString));
            }

            ViewData["CurrentFilter"] = searchString;
            return View("IndexClient", await produitsQuery.ToListAsync());
        }



        // GET: Produits/Create
        [Authorize(Roles = "Parapharmacien,Admin")]
        public IActionResult Create()
        {
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom");
            ViewData["FournisseurId"] = new SelectList(_context.Fournisseurs, "Id", "Adresse");
            return View();
        }

        // POST: Produits/Create        
        [HttpPost]
        [Authorize(Roles = "Parapharmacien,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prix,Stock,Description,CategorieId,FournisseurId")] Produit produit, IFormFile? ImageFile)
        {
            // ImagePath is required in model but populated after upload, so remove from validation
            ModelState.Remove("ImagePath");

            if (ImageFile == null || ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImagePath", "L'image du produit est requise.");
            }

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    produit.ImagePath = await SaveProductImage(ImageFile);
                }

                produit.CreatedAt = DateTime.UtcNow;
                produit.IsActive = true;
                _context.Add(produit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", produit.CategorieId);
            ViewData["FournisseurId"] = new SelectList(_context.Fournisseurs, "Id", "Adresse", produit.FournisseurId);
            return View(produit);
        }

        // GET: Produits/Edit/5
        [Authorize(Roles = "Parapharmacien,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produit = await _context.Produits.FindAsync(id);
            if (produit == null) return NotFound();

            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", produit.CategorieId);
            ViewData["FournisseurId"] = new SelectList(_context.Fournisseurs, "Id", "Adresse", produit.FournisseurId);
            return View(produit);
        }

        // POST: Produits/Edit/5
        [HttpPost]
        [Authorize(Roles = "Parapharmacien,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prix,Stock,Description,CategorieId,FournisseurId,IsActive,ImagePath")] Produit produit, IFormFile? ImageFile)
        {
            if (id != produit.Id) return NotFound();

            // ImagePath might be unchanged, or updated via file, handled manually
            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing product to preserve ImagePath if no new image is uploaded
                    var existing = await _context.Produits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (existing == null) return NotFound();

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(existing.ImagePath))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, existing.ImagePath.TrimStart('/','\\').Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                        produit.ImagePath = await SaveProductImage(ImageFile);
                    }
                    else
                    {
                        // Preserve existing ImagePath if no new image is uploaded
                        produit.ImagePath = existing.ImagePath;
                    }

                    produit.UpdatedAt = DateTime.UtcNow;
                    _context.Update(produit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduitExists(produit.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieId"] = new SelectList(_context.Categories, "Id", "Nom", produit.CategorieId);
            ViewData["FournisseurId"] = new SelectList(_context.Fournisseurs, "Id", "Adresse", produit.FournisseurId);
            return View(produit);
        }

        // GET: Produits/Delete/5
        [Authorize(Roles = "Parapharmacien,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Fournisseur)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (produit == null) return NotFound();

            return View(produit);
        }

        // POST: Produits/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Parapharmacien,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var produit = await _context.Produits.FindAsync(id);
                if (produit != null)
                {
                    _context.Produits.Remove(produit);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Produit supprimé avec succès.";
                }
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Impossible de supprimer ce produit car il est utilisé dans des commandes. Veuillez d'abord supprimer les commandes associées.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du produit.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Produits/StartBuy?productId=5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StartBuy(int productId)
        {
            try
            {
                var produit = await _context.Produits.FindAsync(productId);
                if (produit == null) return NotFound();

                // get current identity user reliably
                var identityUser = await _userManager.GetUserAsync(User);
                if (identityUser == null)
                {
                    _logger.LogWarning("StartBuy: identity user null");
                    return Challenge();
                }

                var userEmail = identityUser.Email;

                // Ensure there is a Client record for this identity user
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == userEmail);

                if (client == null)
                {
                    client = new Client
                    {
                        Email = identityUser.Email ?? string.Empty,
                        Nom = identityUser.Nom ?? string.Empty,
                        Prenom = string.Empty,
                        Password = string.Empty,
                        Telephone = string.Empty,
                        DateInscription = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                }

                // Ensure identity user has Client role so authorization checks work later
                if (!await _userManager.IsInRoleAsync(identityUser, "Client"))
                {
                    var added = await _userManager.AddToRoleAsync(identityUser, "Client");
                    if (added.Succeeded)
                    {
                        // refresh the sign-in cookie so the new role is effective immediately
                        await _signInManager.RefreshSignInAsync(identityUser);
                    }
                }

                var commande = new Commande
                {
                    ClientId = client.Id,
                    Montant = (double)produit.Prix,
                    Statut = StatutCommande.EnAttente,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                var ligne = new LigneCommande
                {
                    CommandeId = commande.Id,
                    ProduitId = produit.Id,
                    Quantite = 1,
                    PrixUnitaire = (double)produit.Prix,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.LigneCommandes.Add(ligne);

                commande.Montant = ligne.PrixUnitaire * ligne.Quantite;
                _context.Commandes.Update(commande);

                await _context.SaveChangesAsync();

                // Redirect to payment action using RedirectToAction to ensure proper routing
                TempData["Success"] = "Commande créée. Vous allez être redirigé vers la page de paiement.";
                return RedirectToAction("ProcessEnLigne", "Paiements", new { commandeId = commande.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans StartBuy");
                TempData["Error"] = "Erreur serveur lors du démarrage de l'achat.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Produits/Buy
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int productId)
        {
            // kept for compatibility, but prefer StartBuy GET to avoid POST -> login redirect issue
            return await StartBuy(productId);
        }

        // GET: Produits/DetailsClient/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var produit = await _context.Produits
                .Include(p => p.Categorie)
                .Include(p => p.Fournisseur)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produit == null) return NotFound();

            ViewBag.Comments = await _context.Commentaires
                .Include(c => c.Client)
                .Include(c => c.Reactions)
                .Where(c => c.ProduitId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Likes = await _context.Reactions.CountAsync(r => r.ProduitId == id && r.Type == ReactionType.Like);
            ViewBag.Dislikes = await _context.Reactions.CountAsync(r => r.ProduitId == id && r.Type == ReactionType.Dislike);

            // Parapharmacien or Admin sees management details; others see the client-facing details
            if (User.IsInRole("Parapharmacien") || User.IsInRole("Admin"))
                return View(produit); // Use standard view for admin

            return View("DetailsClient", produit);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> AddComment(int produitId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return Forbid();

            var comment = new Commentaire
            {
                ProduitId = produitId,
                ClientId = client.Id,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Commentaires.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = produitId });
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> React(int produitId, ReactionType type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return Forbid();

            var existing = await _context.Reactions.FirstOrDefaultAsync(r => r.ProduitId == produitId && r.ClientId == client.Id);
            if (existing != null)
            {
                if (existing.Type == type)
                {
                    _context.Reactions.Remove(existing); // Toggle off
                }
                else
                {
                    existing.Type = type; // Change vote
                }
            }
            else
            {
                var reaction = new Reaction
                {
                    ProduitId = produitId,
                    ClientId = client.Id,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Reactions.Add(reaction);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = produitId });
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ReactToComment(int commentId, ReactionType type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return Forbid();

            var existing = await _context.ReactionCommentaires.FirstOrDefaultAsync(r => r.CommentaireId == commentId && r.ClientId == client.Id);
            if (existing != null)
            {
                if (existing.Type == type)
                {
                    _context.ReactionCommentaires.Remove(existing);
                }
                else
                {
                    existing.Type = type;
                }
            }
            else
            {
                var reaction = new ReactionCommentaire
                {
                    CommentaireId = commentId,
                    ClientId = client.Id,
                    Type = type,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.ReactionCommentaires.Add(reaction);
            }
            await _context.SaveChangesAsync();

            // Return to details page. Need product ID.
            var comment = await _context.Commentaires.FindAsync(commentId);
            return RedirectToAction("Details", new { id = comment?.ProduitId });
        }

        private bool ProduitExists(int id)
        {
            return _context.Produits.Any(e => e.Id == id);
        }

        // helper : sauvegarde image dans wwwroot/images/products et retourne chemin relatif
        private async Task<string> SaveProductImage(IFormFile file)
        {
            var imagesFolder = Path.Combine(_env.WebRootPath, "images", "products");
            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(imagesFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // chemin relatif utilisé dans les vues
            return $"/images/products/{fileName}";
        }
    }
}
