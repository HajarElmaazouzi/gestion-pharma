using System;
using System.ComponentModel.DataAnnotations;

namespace gestion_pharma.Models.Entities
{
    public class ReactionCommentaire : BaseEntity
    {
        public ReactionType Type { get; set; }

        public int CommentaireId { get; set; }
        public virtual Commentaire Commentaire { get; set; }

        public int ClientId { get; set; }
        public virtual Client Client { get; set; }
    }
}
