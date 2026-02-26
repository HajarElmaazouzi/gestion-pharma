using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace gestion_pharma.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly gestion_pharma.Data.ApplicationDbContext _context;

        public AdminController(gestion_pharma.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Total Products
            ViewBag.TotalProducts = await _context.Produits.CountAsync(p => p.IsActive);

            // 2. Low Stock Count (< 10 units but > 0)
            var lowStockProducts = await _context.Produits
                .Where(p => p.Stock < 10 && p.Stock > 0)
                .ToListAsync();
            ViewBag.LowStockCount = lowStockProducts.Count;
            ViewBag.LowStockProducts = lowStockProducts;

            // 3. Out of Stock Products (= 0)
            var outOfStockProducts = await _context.Produits
                .Where(p => p.Stock <= 0)
                .ToListAsync();
            ViewBag.OutOfStockProducts = outOfStockProducts;
            ViewBag.OutOfStockCount = outOfStockProducts.Count;

            // 4. Total Categories
            ViewBag.TotalCategories = await _context.Categories.CountAsync(c => c.IsActive);

            // 5. Sales by Category
            var salesByCategory = await _context.LigneCommandes
                .Include(l => l.Produit)
                .ThenInclude(p => p.Categorie)
                .Where(l => l.Produit.Categorie != null)
                .GroupBy(l => l.Produit.Categorie.Nom)
                .Select(g => new { Category = g.Key, TotalSales = g.Sum(l => l.Quantite * l.PrixUnitaire) })
                .ToListAsync();

            ViewBag.Categories = salesByCategory.Select(s => s.Category).ToList();
            ViewBag.CategorySales = salesByCategory.Select(s => s.TotalSales).ToList();

            // 6. Top 10 Best Selling Products
            var topProducts = await _context.LigneCommandes
                .Include(l => l.Produit)
                .GroupBy(l => new { l.ProduitId, l.Produit.Nom })
                .Select(g => new { ProductId = g.Key.ProduitId, ProductName = g.Key.Nom, TotalQuantity = g.Sum(l => l.Quantite) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToListAsync();
            
            var topProductsWithData = await _context.Produits
                .Where(p => topProducts.Select(t => t.ProductId).Contains(p.Id))
                .ToListAsync();

            ViewBag.TopProducts = topProductsWithData.OrderByDescending(p => 
                topProducts.FirstOrDefault(t => t.ProductId == p.Id)?.TotalQuantity ?? 0
            ).ToList();
            ViewBag.TopQuantities = topProducts
                .OrderByDescending(x => x.TotalQuantity)
                .Select(x => x.TotalQuantity)
                .ToList();

            // 7. Product sales timeseries for top products (last 30 days)
            var topN = topProducts.Take(3).ToList(); // keep chart readable
            var topIds = topN.Select(t => t.ProductId).ToList();
            var startDate = DateTime.UtcNow.Date.AddDays(-29);
            var dateRange = Enumerable.Range(0, 30).Select(i => startDate.AddDays(i)).ToList();

            var recentLines = await _context.LigneCommandes
                .Where(l => topIds.Contains(l.ProduitId) && l.CreatedAt >= startDate)
                .ToListAsync();

            var productLabels = dateRange.Select(d => d.ToString("dd/MM")).ToList();
            var productDatasets = new List<object>();
            var colorPalette = new[] { "#0d6efd", "#198754", "#ffc107", "#dc3545", "#6f42c1" };
            for (int i = 0; i < topN.Count; i++)
            {
                var t = topN[i];
                var data = dateRange.Select(d => recentLines.Where(l => l.ProduitId == t.ProductId && l.CreatedAt.Date == d.Date).Sum(l => l.Quantite)).ToList();
                productDatasets.Add(new { label = t.ProductName ?? ("Produit " + t.ProductId), data, borderColor = colorPalette[i % colorPalette.Length], backgroundColor = colorPalette[i % colorPalette.Length] + "33" });
            }

            ViewBag.ProductSalesLabels = productLabels;
            ViewBag.ProductSalesDatasets = productDatasets;

            // 8. Payment method breakdown
            var paymentBreakdown = await _context.Paiements
                .GroupBy(p => p.Methode)
                .Select(g => new { Methode = g.Key, Count = g.Count(), Total = g.Sum(p => p.Montant) })
                .ToListAsync();

            ViewBag.PaymentLabels = paymentBreakdown.Select(p => p.Methode.ToString()).ToList();
            ViewBag.PaymentData = paymentBreakdown.Select(p => p.Count).ToList();

            return View();
        }
    }
}
