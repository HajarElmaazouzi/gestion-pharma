using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Categorie : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Produit>? Produits { get; set; }
    
}
}
