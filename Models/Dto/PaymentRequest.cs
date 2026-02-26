namespace gestion_pharma.Models.Dto
{
    public class PaymentRequest
    {
        public int CommandeId { get; set; }
        public double Montant { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHolder { get; set; }
        public string? Expiry { get; set; }
        public string? Cvv { get; set; }
    }
}