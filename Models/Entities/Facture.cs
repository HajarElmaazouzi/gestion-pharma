using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Facture : BaseEntity
    {
        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, double.MaxValue)]
        public double MontantTotal { get; set; } // Double selon diagramme
     
        public int PaiementId { get; set; }
        
        // Navigation
        public virtual Paiement? Paiement { get; set; }
    }
}

