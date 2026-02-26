using gestion_pharma.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace gestion_pharma.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        // DbSets - une table par Entity
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Parapharmacien> Parapharmaciens { get; set; } = null!;
        public DbSet<Produit> Produits { get; set; } = null!;
        public DbSet<Categorie> Categories { get; set; } = null!;
        public DbSet<Commande> Commandes { get; set; } = null!;
        public DbSet<LigneCommande> LigneCommandes { get; set; } = null!;
        public DbSet<Fournisseur> Fournisseurs { get; set; } = null!;
        public DbSet<LigneCommandeFourni> LigneCommandesFourni { get; set; } = null!;
        public DbSet<Paiement> Paiements { get; set; } = null!;
        public DbSet<PaiementEnLigne> PaiementsEnLigne { get; set; } = null!;
        public DbSet<CarteFidelite> CartesFidelite { get; set; } = null!;
        
        public DbSet<Facture> Factures { get; set; } = null!;
        public DbSet<CommandeFournisseur> CommandesFournisseurs { get; set; } = null!;

        // Cart
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Commentaire> Commentaires { get; set; } = null!;
        public DbSet<Reaction> Reactions { get; set; } = null!;
        public DbSet<ReactionCommentaire> ReactionCommentaires { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de l'héritage TPH pour User
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Client>("Client")
                .HasValue<Admin>("Admin")
                .HasValue<Parapharmacien>("Parapharmacien");

            // Configuration de l'héritage TPH pour Paiment 
            modelBuilder.Entity<Paiement>()
              .HasDiscriminator<string>("PaiementType")
              .HasValue<PaiementEnLigne>("PaiementEnLigne");

            // relation one to one 
            modelBuilder.Entity<Commande>()
            .HasOne(c => c.Paiement)
            .WithOne(p => p.Commande)
            .HasForeignKey<Paiement>(p => p.CommandeId)
            .IsRequired(false) // Paiement peut être null
            .OnDelete(DeleteBehavior.Cascade);

            //relation one to one 
            modelBuilder.Entity<Paiement>()
            .HasOne(p => p.Facture)
            .WithOne(f => f.Paiement)
            .HasForeignKey<Facture>(f => f.PaiementId)
            .IsRequired(false) // Facture peut être null
            .OnDelete(DeleteBehavior.Cascade);

            // commande => ligne de commande
            modelBuilder.Entity<LigneCommande>()
            .HasOne(lc => lc.Commande)
            .WithMany(c => c.LigneCommandes)
            .HasForeignKey(lc => lc.CommandeId)
            .OnDelete(DeleteBehavior.Cascade);
            // produit => ligne de commandes 
            modelBuilder.Entity<LigneCommande>()
            .HasOne(lc => lc.Produit)
            .WithMany(p => p.LigneCommandes)
            .HasForeignKey(lc => lc.ProduitId)
            .OnDelete(DeleteBehavior.Restrict);// Empêche suppression si produit dans commande            //produit => categorie 
            // produt => categorie
            modelBuilder.Entity<Produit>()
            .HasOne(p => p.Categorie)
            .WithMany(c => c.Produits)
            .HasForeignKey(p => p.CategorieId)
            .OnDelete(DeleteBehavior.Restrict);
            // precision de nombre de decimal 
            modelBuilder.Entity<Produit>()
             .Property(p => p.Prix)
             .HasPrecision(10, 2);
            // commane => client 
            modelBuilder.Entity<Commande>()
            .HasOne(c => c.Client)
            .WithMany(cl => cl.Commandes)
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
            // carete de fidelité => client 
            modelBuilder.Entity<Client>()
            .HasOne(c => c.CarteFidelite)
            .WithOne(cf => cf.Client)
            .HasForeignKey<CarteFidelite>(cf => cf.ClientId)
            .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            

            // fournisseur => lignecommandefourniss
            modelBuilder.Entity<Fournisseur>()
        .HasMany(f => f.LigneCommandesFourni)
        .WithOne(lcf => lcf.Fournisseur)
        .HasForeignKey(lcf => lcf.FournisseurId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LigneCommandeFourni>()
       .HasOne(lcf => lcf.Produit)
       .WithMany(p => p.LigneCommandesFourni)
       .HasForeignKey(lcf => lcf.ProduitId);
            // pharmacien => commande fournisseur one to many 
            modelBuilder.Entity<CommandeFournisseur>()
    .HasOne(cf => cf.Parapharmacien)
    .WithMany(p => p.CommandesFournisseur)
    .HasForeignKey(cf => cf.ParapharmacienId)
    .OnDelete(DeleteBehavior.Restrict);
            // CommandeFournisseur → LigneCommandeFourni (One-To-Many)
            modelBuilder.Entity<LigneCommandeFourni>()
    .HasOne(lcf => lcf.CommandeFournisseur)
    .WithMany(cf => cf.LigneCommandesFourni)
    .HasForeignKey(lcf => lcf.CommandeFournisseurId)
    .OnDelete(DeleteBehavior.Cascade);

            // Cart relations
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Client)
                .WithMany() // client may have multiple carts in model but we'll assume one active; keep simple
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Produit)
                .WithMany()
                .HasForeignKey(ci => ci.ProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReactionCommentaire configuration to avoid cycle
            modelBuilder.Entity<ReactionCommentaire>()
                .HasOne(rc => rc.Client)
                .WithMany()
                .HasForeignKey(rc => rc.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Avoid multiple cascade paths

            modelBuilder.Entity<ReactionCommentaire>()
                .HasOne(rc => rc.Commentaire)
                .WithMany(c => c.Reactions)
                .HasForeignKey(rc => rc.CommentaireId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}

