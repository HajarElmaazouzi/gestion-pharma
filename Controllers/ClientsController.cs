using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Userpers> _userManager;

        public ClientsController(ApplicationDbContext context, UserManager<Userpers> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            // Fetch all Clents.
            // Assuming "Client" is the discrimination or we can filter by Role if strictly used.
            // Based on Class Diagram and Model structure, Client inherits User/Userpers logic.
            // Let's rely on the DbSet<Client> if it exists, or filter User table.
            // Looking at context usage in ParapharmaciensController, it uses _context.Set<Parapharmacien>().
            // So we should use _context.Set<Client>().
            
            // Exclure explicitement l'admin système si une entité Client a été créée par erreur
            var adminEmail = "admin@local";
            var clients = await _context.Set<Client>()
                .Where(c => c.Email != adminEmail)
                .ToListAsync();
            return View(clients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Set<Client>()
                .Include(c => c.Commandes) // Include orders if relevant
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Set<Client>()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Set<Client>().FindAsync(id);
            if (client != null)
            {
                // Also delete the associated Identity User if email matches?
                // In ParapharmaciensController, we delete the Identity User.
                // It is safer to do so if we want to remove login access.
                var identityUser = await _userManager.FindByEmailAsync(client.Email);
                if (identityUser != null)
                {
                    await _userManager.DeleteAsync(identityUser);
                }

                _context.Set<Client>().Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
