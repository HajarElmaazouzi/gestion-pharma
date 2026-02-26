namespace gestion_pharma.Models.Enums
{
    public enum MethodePaiment
    {
         
        // Paiements physiques
        Especes = 1,
        Cheque = 2,
        CarteBancairePhysique = 3,
        
        // Paiements en ligne
        CarteBancaireEnLigne = 10,
        PayPal = 11,
        VirementBancaire = 12,
        MobileMoney = 13,
        CarteCadeau = 14
    }
}

