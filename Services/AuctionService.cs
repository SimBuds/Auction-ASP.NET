using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using web_project.Data;


namespace web_project.Services
{
    public class AuctionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly BidService _bidService;
        private readonly TransactionService _transactionService;
        private readonly IHubContext<AuctionHub> _auctionHub;
        private readonly IEmailSender _emailSender;

        public AuctionService(ApplicationDbContext context, IBackgroundJobClient backgroundJobClient, TransactionService transactionService, BidService bidService, IHubContext<AuctionHub> auctionHub, IEmailSender emailSender)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _transactionService = transactionService;
            _bidService = bidService;
            _auctionHub = auctionHub;
            _emailSender = emailSender;
        }

        public void ScheduleAuctionCompletion(int auctionId, DateTime endDate)
        {
            _backgroundJobClient.Schedule(() => CompleteAuction(auctionId), endDate);
        }

        public async Task<(bool isSuccess, string message)> CompleteAuction(int auctionId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);

            if (auction == null || auction.IsSold || !auction.IsExpired())
            {
                return (false, "Auction not found or already completed.");
            }

            // Get the winning bid
            var highestBid = _bidService.GetHighestBid(auctionId);

            if (highestBid != null)
            {
                // Update the auction status
                auction.IsSold = true;
                _context.Auctions.Update(auction);
                _context.Bids.Update(highestBid);

                // Notify the winner
                await _auctionHub.Clients.User(highestBid.UserId.ToString()).SendAsync("ReceiveBidWon", auction.Id);
                await _emailSender.SendEmailAsync(highestBid.UserId, "You won an auction!", "Congratulations!");

                // Notify losers
                var losingBidders = _bidService.GetLosingBidders(auctionId, highestBid.UserId);
                foreach (var loser in losingBidders)
                {
                    await _auctionHub.Clients.User(loser.UserId.ToString()).SendAsync("ReceiveBidLost", auction.Id);
                    await _emailSender.SendEmailAsync(loser.UserId, "You lost an auction!", "Better luck next time!");
                }

                return (true, "Auction completed successfully.");
            }
            else
            {
                return (false, "No bids were placed on this auction.");
            }
        }

        public bool IsFinalTransaction(int auctionId)
        {
            var auction = _context.Auctions.Find(auctionId);
            if (auction == null || !auction.IsSold || auction.Transactions.Any())
            {
                return false;
            }

            var highestBid = _bidService.GetHighestBid(auctionId);
            if (highestBid == null)
            {
                return false;
            }

            var transaction = _transactionService.CreateTransaction(highestBid.UserId, auction.UserId, auction.Id, highestBid.Amount).Result;
            auction.Transactions.Add(transaction);
            _context.Auctions.Update(auction);
            _context.SaveChanges();

            return true;
        }
    }
}