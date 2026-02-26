using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace gestion_pharma.Models.Entities
{
    public class Commentaire : BaseEntity
    {
        [Required]
        public string Content { get; set; }

        public int ProduitId { get; set; }
        public virtual Produit Produit { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }

        public virtual ICollection<ReactionCommentaire> Reactions { get; set; }
    }
}
