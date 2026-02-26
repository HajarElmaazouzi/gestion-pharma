using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class LigneCommande : BaseEntity
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantite { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double PrixUnitaire { get; set; } // Double selon diagramme

        // Foreign Keys
        public int CommandeId { get; set; }
        public int ProduitId { get; set; }

        // Navigation properties pour definir les relations
        public virtual Commande? Commande { get; set; }
        public virtual Produit? Produit { get; set; }
    
}
}
