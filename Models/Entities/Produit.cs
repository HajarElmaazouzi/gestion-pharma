using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Produit : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Prix { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        [Display(Name = "Nom de l'image")]
        public string ImagePath { get; set; }


        // Foreign Keys
        public int? CategorieId { get; set; }
        public int? FournisseurId { get; set; }

        // Navigation properties
        public virtual Categorie? Categorie { get; set; }
        public virtual Fournisseur? Fournisseur { get; set; }
        public virtual ICollection<LigneCommande>? LigneCommandes { get; set; }
        public virtual ICollection<LigneCommandeFourni>? LigneCommandesFourni { get; set; }
    }
}

