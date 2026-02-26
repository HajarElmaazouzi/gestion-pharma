using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Fournisseur : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string NomF { get; set; } = string.Empty; // NomF selon diagramme

        [Required]
        [MaxLength(100)]
        public string PrenomF { get; set; } = string.Empty; // PrenomF selon diagramme

        [Required]
        [MaxLength(200)]
        public string Adresse { get; set; } = string.Empty;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string Telephone { get; set; } = string.Empty; // Telephone sans accent

        // Navigation
        public virtual ICollection<Produit>? Produits { get; set; }
        public virtual ICollection<LigneCommandeFourni>? LigneCommandesFourni { get; set; }
    }
}

