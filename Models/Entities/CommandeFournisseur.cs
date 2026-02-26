namespace gestion_pharma.Models.Entities
{
    public class CommandeFournisseur : BaseEntity
    {
       
            public DateTime DateCommande { get; set; }

            public int ParapharmacienId { get; set; }
            public Parapharmacien Parapharmacien { get; set; }

            public ICollection<LigneCommandeFourni> LigneCommandesFourni { get; set; }
    }
}

