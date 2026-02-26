using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class PaiementEnLigne : Paiement
    {
        // Optional transaction number; will be generated server-side if not provided
        public int? NumTransaction { get; set; }

        [Required]
        public bool StatutTransaction { get; set; } = false; 
    }
}

