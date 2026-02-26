using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = null!;

        public int ProduitId { get; set; }
        public virtual Produit Produit { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantite { get; set; } = 1;

        [Required]
        [Range(0, double.MaxValue)]
        public double PrixUnitaire { get; set; }

        public double Total => PrixUnitaire * Quantite;
    }
}
