using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class LigneCommandeFourni : BaseEntity
    {
        [Required]
        [Range(0, double.MaxValue)]
        public double Prix { get; set; } 

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantite { get; set; }

        // Foreign Keys
        public int FournisseurId { get; set; }
        public int ProduitId { get; set; }
        public int CommandeFournisseurId { get; set; }

        // Navigation properties
        public virtual Fournisseur? Fournisseur { get; set; }
        public virtual Produit? Produit { get; set; }

        public virtual CommandeFournisseur CommandeFournisseur { get; set; }

    }
}

