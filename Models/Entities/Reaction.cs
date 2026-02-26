using System;
using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public enum ReactionType
    {
        Like,
        Dislike
    }

    public class Reaction : BaseEntity
    {
        public ReactionType Type { get; set; }

        public int ProduitId { get; set; }
        public virtual Produit Produit { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
    }
}
