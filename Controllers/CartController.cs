using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using gestion_pharma.Models.Enums;
using gestion_pharma.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    [Authorize(Roles = "Client")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Userpers> _userManager;
        private readonly FidelityService _fidelityService;

        public CartController(ApplicationDbContext context, UserManager<Userpers> userManager, FidelityService fidelityService)
        {
            _context = context;
            _userManager = userManager;
            _fidelityService = fidelityService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return Challenge();
            }
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return RedirectToAction("Index", "Home");
            var cart = await _context.Commandes.Include(c => c.LigneCommandes).ThenInclude(l => l.Produit).FirstOrDefaultAsync(c => c.ClientId == client.Id && c.Statut == StatutCommande.EnAttente);
            
            // Load loyalty card
            var loyaltyCard = await _fidelityService.GetLoyaltyCard(client.Id);
            ViewBag.LoyaltyCard = loyaltyCard;

            return View(cart);
        }

        // GET: Intermédiaire pour les utilisateurs non authentifiés
        // Quand un utilisateur non authentifié clique sur "Ajouter au panier", il est redirigé vers la page de login
        // Après authentification, il revient ici avec les paramètres (productId, quantity)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Utilisateur non authentifié - le rediriger vers le login avec ReturnUrl
                return Challenge();
            }
            
            // Utilisateur authentifié - effectuer l'ajout au panier (appeler la logique POST)
            return await AddToCartInternal(productId, quantity);
        }

        [HttpPost]
        [Route("Cart/AddToCart")]
        public async Task<IActionResult> AddToCartPost(int productId, int quantity = 1)
        {
            return await AddToCartInternal(productId, quantity);
        }

        // Logique interne pour ajouter au panier
        private async Task<IActionResult> AddToCartInternal(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                return Challenge();
            }
            
            // Vérifier le stock avant d'ajouter
            var product = await _context.Produits.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Produit non trouvé.";
                return RedirectToAction("Index", "Produits");
            }
            
            if (product.Stock <= 0)
            {
                TempData["ErrorMessage"] = $"Le produit '{product.Nom}' est en rupture de stock.";
                return RedirectToAction("Details", "Produits", new { id = productId });
            }
            
            if (quantity > product.Stock)
            {
                TempData["ErrorMessage"] = $"Quantité demandée ({quantity}) dépasse le stock disponible ({product.Stock}).";
                return RedirectToAction("Details", "Produits", new { id = productId });
            }
            
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) 
            { 
                client = new Client { Email = user.Email, Nom = user.Nom ?? "Client", CreatedAt = DateTime.UtcNow, IsActive = true }; 
                _context.Clients.Add(client); 
                await _context.SaveChangesAsync(); 
            }
            
            var cart = await _context.Commandes.Include(c => c.LigneCommandes).FirstOrDefaultAsync(c => c.ClientId == client.Id && c.Statut == StatutCommande.EnAttente);
            if (cart == null) 
            { 
                cart = new Commande { ClientId = client.Id, Statut = StatutCommande.EnAttente, Montant = 0, CreatedAt = DateTime.UtcNow, IsActive = true }; 
                _context.Commandes.Add(cart); 
                await _context.SaveChangesAsync(); 
            }
            
            var existingLine = cart.LigneCommandes.FirstOrDefault(l => l.ProduitId == productId);
            if (existingLine != null) 
            { 
                existingLine.Quantite += quantity; 
                _context.LigneCommandes.Update(existingLine); 
            } 
            else 
            { 
                var newLine = new LigneCommande { CommandeId = cart.Id, ProduitId = productId, Quantite = quantity, PrixUnitaire = (double)product.Prix, CreatedAt = DateTime.UtcNow, IsActive = true }; 
                _context.LigneCommandes.Add(newLine); 
            }
            
            await _context.SaveChangesAsync(); 
            cart.Montant = await _context.LigneCommandes.Where(l => l.CommandeId == cart.Id).SumAsync(l => l.Quantite * l.PrixUnitaire); 
            _context.Commandes.Update(cart); 
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"'{product.Nom}' ajouté au panier.";
            return RedirectToAction("Index"); 
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int ligneId)
        {
            var ligne = await _context.LigneCommandes.FindAsync(ligneId);
            if (ligne != null) 
            { 
                _context.LigneCommandes.Remove(ligne); 
                await _context.SaveChangesAsync(); 
                var cart = await _context.Commandes.FindAsync(ligne.CommandeId); 
                if(cart != null) 
                { 
                    cart.Montant = await _context.LigneCommandes.Where(l => l.CommandeId == cart.Id).SumAsync(l => l.Quantite * l.PrixUnitaire); 
                    _context.Commandes.Update(cart); 
                    await _context.SaveChangesAsync(); 
                } 
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int ligneId)
        {
            var ligne = await _context.LigneCommandes.FindAsync(ligneId);
            if (ligne != null) { _context.LigneCommandes.Remove(ligne); await _context.SaveChangesAsync(); var cart = await _context.Commandes.FindAsync(ligne.CommandeId); if(cart != null) { cart.Montant = await _context.LigneCommandes.Where(l => l.CommandeId == cart.Id).SumAsync(l => l.Quantite * l.PrixUnitaire); _context.Commandes.Update(cart); await _context.SaveChangesAsync(); } }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPoints(int commandeId, int points)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return RedirectToAction("Index", "Home");

            var commande = await _context.Commandes.FindAsync(commandeId);
            if (commande == null || commande.ClientId != client.Id) return Forbid();

            // Validate that points is a multiple of 10
            if (points <= 0 || points % 10 != 0)
            {
                TempData["ErrorMessage"] = "Le nombre de points doit être un multiple de 10 et positif.";
                return RedirectToAction("Index");
            }

            var discount = await _fidelityService.ApplyPointsToOrder(client.Id, points, commandeId);
            if (discount > 0)
            {
                TempData["SuccessMessage"] = $"{points} points utilisés : réduction appliquée -{discount:C}";
            }
            else
            {
                TempData["ErrorMessage"] = "Vous n'avez pas assez de points pour cette réduction.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseAllPoints(int commandeId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == user.Email);
            if (client == null) return RedirectToAction("Index", "Home");

            var commande = await _context.Commandes.FindAsync(commandeId);
            if (commande == null || commande.ClientId != client.Id) return Forbid();

            var discount = await _fidelityService.RedeemAllPointsToOrder(client.Id, commandeId);
            if (discount > 0)
            {
                TempData["SuccessMessage"] = $"Tous les points utilisés : réduction appliquée -{discount:C}";
            }
            else
            {
                TempData["ErrorMessage"] = "Pas assez de points pour générer une réduction.";
            }

            return RedirectToAction("Index");
        }
    }
}