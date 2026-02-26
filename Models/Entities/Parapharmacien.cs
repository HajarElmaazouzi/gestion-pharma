namespace gestion_pharma.Models.Entities
{
    public class Parapharmacien : User
    {
       
        public virtual ICollection<CommandeFournisseur> CommandesFournisseur { get; set; }
    }
}

