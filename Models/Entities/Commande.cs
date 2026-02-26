using gestion_pharma.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace gestion_pharma.Models.Entities
{
    public class Commande : BaseEntity
    {
        [Required]
        [Range(0, double.MaxValue)]
        public double Montant { get; set; } = 0;

        [Required]
        public StatutCommande Statut { get; set; } = StatutCommande.EnAttente;

        [Required]
        public int ClientId { get; set; }

        [Range(0, int.MaxValue)]
        public int PointsEarned { get; set; } = 0; // Points earned from this order

        // Navigation properties
        public virtual Client Client { get; set; } = null!; // ✅ Enlever ?, ajouter = null!
        public virtual ICollection<LigneCommande> LigneCommandes { get; set; } = new List<LigneCommande>();
        public virtual Paiement? Paiement { get; set; } // ✅ Garder ? (optionnel avant paiement)
        
    }
}

