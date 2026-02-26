using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ParapharmaciensController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Userpers> _userManager;
        private readonly ILogger<ParapharmaciensController> _logger;

        public ParapharmaciensController(ApplicationDbContext context, UserManager<Userpers> userManager, ILogger<ParapharmaciensController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Parapharmaciens
        public async Task<IActionResult> Index()
        {
            var list = await _context.Set<Parapharmacien>().ToListAsync();
            return View(list);
        }

        // GET: Parapharmaciens/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var para = await _context.Set<Parapharmacien>().FirstOrDefaultAsync(p => p.Id == id);
            if (para == null) return NotFound();
            return View(para);
        }

        // GET: Parapharmaciens/Create
        public IActionResult Create()
        {
            return View(new gestion_pharma.Models.ViewModels.ParapharmacienCreateViewModel());
        }

        // POST: Parapharmaciens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(gestion_pharma.Models.ViewModels.ParapharmacienCreateViewModel input)
        {
            if (!ModelState.IsValid) return View(input);

            // create Identity user
            var identityUser = new Userpers
            {
                UserName = input.Email,
                Email = input.Email,
                Nom = input.Nom
            };

            var res = await _userManager.CreateAsync(identityUser, input.Password);
            if (!res.Succeeded)
            {
                foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(input);
            }

            await _userManager.AddToRoleAsync(identityUser, "Parapharmacien");

            // create domain Parapharmacien record
            var para = new Parapharmacien
            {
                Nom = input.Nom,
                Prenom = input.Prenom,
                Email = input.Email,
                Telephone = input.Telephone,
                Password = string.Empty, // Password managed by Identity
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Set<Parapharmacien>().Add(para);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Parapharmacien created: {Email}", input.Email);
            return RedirectToAction(nameof(Index));
        }

        // GET: Parapharmaciens/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var para = await _context.Set<Parapharmacien>().FindAsync(id);
            if (para == null) return NotFound();
            return View(para);
        }

        // POST: Parapharmaciens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email,Telephone,IsActive")] Parapharmacien input)
        {
            if (id != input.Id) return NotFound();
            if (!ModelState.IsValid) return View(input);

            var para = await _context.Set<Parapharmacien>().FindAsync(id);
            if (para == null) return NotFound();

            // update domain fields
            para.Nom = input.Nom;
            para.Prenom = input.Prenom;
            para.Telephone = input.Telephone;
            para.IsActive = input.IsActive;
            para.UpdatedAt = DateTime.UtcNow;

            // if email changed, update identity user
            if (!string.Equals(para.Email, input.Email, StringComparison.OrdinalIgnoreCase))
            {
                var identityUser = await _userManager.FindByEmailAsync(para.Email);
                if (identityUser != null)
                {
                    identityUser.Email = input.Email;
                    identityUser.UserName = input.Email;
                    var updateRes = await _userManager.UpdateAsync(identityUser);
                    if (!updateRes.Succeeded)
                    {
                        foreach (var e in updateRes.Errors) ModelState.AddModelError(string.Empty, e.Description);
                        return View(input);
                    }
                }
                para.Email = input.Email;
            }

            try
            {
                _context.Update(para);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Set<Parapharmacien>().Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Parapharmaciens/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var para = await _context.Set<Parapharmacien>().FirstOrDefaultAsync(p => p.Id == id);
            if (para == null) return NotFound();
            return View(para);
        }

        // POST: Parapharmaciens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var para = await _context.Set<Parapharmacien>().FindAsync(id);
            if (para != null)
            {
                // delete identity user
                var identityUser = await _userManager.FindByEmailAsync(para.Email);
                if (identityUser != null)
                {
                    await _userManager.DeleteAsync(identityUser);
                }

                _context.Set<Parapharmacien>().Remove(para);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
