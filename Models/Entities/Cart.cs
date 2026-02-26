using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class Cart : BaseEntity
    {
        // Foreign key to Client
        public int ClientId { get; set; }
        public virtual Client Client { get; set; } = null!;

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
