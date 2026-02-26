using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gestion_pharma.Controllers
{
    // Allow clients to view fournisseurs list and details, management reserved to Parapharmacien
    [Authorize(Roles = "Parapharmacien,Client")]

    public class FournisseursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FournisseursController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Fournisseurs
        public async Task<IActionResult> Index()
        {
            return View(await _context.Fournisseurs.ToListAsync());
        }

        // GET: Fournisseurs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fournisseur = await _context.Fournisseurs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fournisseur == null)
            {
                return NotFound();
            }

            return View(fournisseur);
        }

        // GET: Fournisseurs/Create
        [Authorize(Roles = "Parapharmacien")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fournisseurs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Parapharmacien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NomF,PrenomF,Adresse,Telephone,Id,CreatedAt,UpdatedAt,IsActive")] Fournisseur fournisseur)
        {
            if (ModelState.IsValid)
            {
                _context.Add(fournisseur);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(fournisseur);
        }

        // GET: Fournisseurs/Edit/5
        [Authorize(Roles = "Parapharmacien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fournisseur = await _context.Fournisseurs.FindAsync(id);
            if (fournisseur == null)
            {
                return NotFound();
            }
            return View(fournisseur);
        }

        // POST: Fournisseurs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Parapharmacien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NomF,PrenomF,Adresse,Telephone,Id,CreatedAt,UpdatedAt,IsActive")] Fournisseur fournisseur)
        {
            if (id != fournisseur.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fournisseur);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FournisseurExists(fournisseur.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fournisseur);
        }

        // GET: Fournisseurs/Delete/5
        [Authorize(Roles = "Parapharmacien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fournisseur = await _context.Fournisseurs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fournisseur == null)
            {
                return NotFound();
            }

            return View(fournisseur);
        }

        // POST: Fournisseurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Parapharmacien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Check if fournisseur has associated products
                var productsCount = await _context.Produits.CountAsync(p => p.FournisseurId == id);
                if (productsCount > 0)
                {
                    TempData["ErrorMessage"] = $"Impossible de supprimer ce fournisseur car il y a {productsCount} produit(s) associé(s). Veuillez d'abord réaffecter ou supprimer ces produits.";
                    return RedirectToAction(nameof(Index));
                }

                var fournisseur = await _context.Fournisseurs.FindAsync(id);
                if (fournisseur != null)
                {
                    _context.Fournisseurs.Remove(fournisseur);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Fournisseur supprimé avec succès.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression du fournisseur.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FournisseurExists(int id)
        {
            return _context.Fournisseurs.Any(e => e.Id == id);
        }
    }
}
