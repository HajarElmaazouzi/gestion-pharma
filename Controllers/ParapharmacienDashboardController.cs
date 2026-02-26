using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    [Authorize(Roles = "Parapharmacien")]
    public class ParapharmacienDashboardController : Controller
    {
        private readonly gestion_pharma.Data.ApplicationDbContext _context;

        public ParapharmacienDashboardController(gestion_pharma.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch low stock count for alert
            var lowStockCount = await _context.Set<gestion_pharma.Models.Entities.Produit>()
                                              .CountAsync(p => p.Stock < 10);
            
            ViewBag.LowStockCount = lowStockCount;

            return View();
        }
    }
}
