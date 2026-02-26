using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Client : User 
    {
        [MaxLength(200)]
        public string? Adresse { get; set; }

        public DateTime DateInscription { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual CarteFidelite? CarteFidelite { get; set; }
        public virtual ICollection<Commande>? Commandes { get; set; }
    }
}

