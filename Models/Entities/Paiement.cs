
using gestion_pharma.Models.Enums;

using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Paiement : BaseEntity
    {
        [Required]
        [Range(0, double.MaxValue)]
        public double Montant { get; set; } 

        [Required]
        public bool StatutPaiement { get; set; } = false;

        [Required]
        public MethodePaiment Methode { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        public int CommandeId { get; set; }
        // Navigation
        public virtual Commande? Commande { get; set; }
        public virtual Facture Facture { get; set; }
    }
}

