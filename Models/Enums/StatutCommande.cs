namespace gestion_pharma.Models.Enums
{
    public enum StatutCommande
    {
        EnAttente = 1,      // Créée mais pas encore traitée
        Validee = 2,        // Validée, paiement effectué
        EnPreparation = 3,  // En cours de préparation
        Expediee = 4,       // Expédiée
        Livree = 5,         // Livrée au client
        Annulee = 6         // Annulée avant paiement
    }
}

