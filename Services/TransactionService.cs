using web_project.Data;
using web_project.Models;

namespace web_project.Services
{
    public class TransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> CreateTransaction(string buyerId, string sellerId, int auctionId, decimal transactionAmount)
        {
            var transaction = new Transaction
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                AuctionId = auctionId,
                TransactionDate = DateTime.UtcNow,
                TransactionAmount = transactionAmount,
                IsPaymentSuccessful = true // Add your payment processing logic here
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }
    }
}
