using gestion_pharma.Data;
using gestion_pharma.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace gestion_pharma.Services
{
    public class FidelityService
    {
        private readonly ApplicationDbContext _context;
        private const int DOLLARS_PER_POINT = 20; // 1 point per $20 spent
        private const int POINTS_PER_DOLLAR_DISCOUNT = 10; // 10 points = $1 discount

        public FidelityService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds points to customer loyalty card based on order amount.
        /// Formula: Points = Floor(OrderAmount / 20)
        /// </summary>
        public async Task AddPoints(int clientId, double orderAmount)
        {
            // Calculate points: 1 point per $20 spent
            int pointsToAdd = (int)Math.Floor(orderAmount / DOLLARS_PER_POINT);
            
            if (pointsToAdd <= 0) return;

            // Get or create loyalty card
            var carteFidelite = await _context.CartesFidelite.FirstOrDefaultAsync(c => c.ClientId == clientId);
            
            if (carteFidelite == null)
            {
                carteFidelite = new CarteFidelite
                {
                    ClientId = clientId,
                    Points = 0,
                    DateCreation = DateTime.UtcNow
                };
                _context.CartesFidelite.Add(carteFidelite);
            }

            // Add points
            carteFidelite.Points += pointsToAdd;
            _context.CartesFidelite.Update(carteFidelite);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the loyalty card for a customer.
        /// </summary>
        public async Task<CarteFidelite?> GetLoyaltyCard(int clientId)
        {
            return await _context.CartesFidelite.FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        /// <summary>
        /// Applies a specified amount of points to an order as discount.
        /// Formula: Each 10 points = $1 discount
        /// Returns the actual discount amount applied.
        /// </summary>
        public async Task<double> ApplyPointsToOrder(int clientId, int pointsToUse, int? commandeId = null)
        {
            var carte = await _context.CartesFidelite.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (carte == null || carte.Points < pointsToUse) return 0;

            // Calculate discount: POINTS_PER_DOLLAR_DISCOUNT points = $1
            double discountAmount = (double)pointsToUse / POINTS_PER_DOLLAR_DISCOUNT;

            // Deduct points
            carte.Points -= pointsToUse;
            if (carte.Points < 0) carte.Points = 0;
            _context.CartesFidelite.Update(carte);

            // Optionally apply discount directly to commande
            if (commandeId.HasValue)
            {
                var commande = await _context.Commandes.FindAsync(commandeId.Value);
                if (commande != null && commande.IsActive)
                {
                    commande.Montant = Math.Max(0, commande.Montant - discountAmount);
                    _context.Commandes.Update(commande);
                }
            }

            await _context.SaveChangesAsync();
            return discountAmount;
        }

        /// <summary>
        /// Redeem all available points (in multiples of 10) for a given client and optionally apply the discount to an order.
        /// Returns the dollar amount applied as discount.
        /// </summary>
        public async Task<double> RedeemAllPointsToOrder(int clientId, int? commandeId = null)
        {
            var carte = await _context.CartesFidelite.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (carte == null || carte.Points < POINTS_PER_DOLLAR_DISCOUNT) return 0;

            // Calculate redeemable points (in multiples of 10)
            int pointsToUse = (carte.Points / POINTS_PER_DOLLAR_DISCOUNT) * POINTS_PER_DOLLAR_DISCOUNT;
            double discountAmount = (double)pointsToUse / POINTS_PER_DOLLAR_DISCOUNT;

            // Deduct points
            carte.Points -= pointsToUse;
            if (carte.Points < 0) carte.Points = 0;
            _context.CartesFidelite.Update(carte);

            // Optionally apply discount directly to commande
            if (commandeId.HasValue)
            {
                var commande = await _context.Commandes.FindAsync(commandeId.Value);
                if (commande != null && commande.IsActive)
                {
                    commande.Montant = Math.Max(0, commande.Montant - discountAmount);
                    _context.Commandes.Update(commande);
                }
            }

            await _context.SaveChangesAsync();
            return discountAmount;
        }
    }
}
