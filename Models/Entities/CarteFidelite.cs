using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class CarteFidelite : BaseEntity
    {
        [Required]
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(0, int.MaxValue)]
        public int Points { get; set; } = 0;

        [MaxLength(50)]
        public String? CodePromo { get; set; } 

        // Foreign Key
        public int ClientId { get; set; }

        // Navigation
        public virtual Client Client { get; set; }
    }
}

