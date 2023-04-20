using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using web_project.Data;
using web_project.Models;

namespace web_project.Services
{
    public class BidService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<AuctionHub> _auctionHub;
        private readonly ILogger<BidService> _logger;

        public BidService(ApplicationDbContext context, IHubContext<AuctionHub> auctionHub, ILogger<BidService> logger)
        {
            _context = context;
            _auctionHub = auctionHub;
            _logger = logger;
        }

        public bool IsValidBid(Bid bid, Auction auction)
        {
            // Check if the End Date has expired
            if (DateTime.UtcNow >= auction.EndDate)
            {
                return false;
            }

            // Check if the bid amount is greater than the current price
            if (bid.Amount <= auction.CurrentPrice)
            {
                return false;
            }

            // Check if the current bid is greater than the reserve price
            if (bid.Amount < auction.ReservedPrice)
            {
                return false;
            }

            // Check if the auction is already sold
            if (auction.IsSold)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> PlaceBidAsync(Bid bid, Auction auction)
        {
            // Check if the bid is valid
            if (!IsValidBid(bid, auction))
            {
                _logger.LogWarning($"Bid failed: Invalid bid. AuctionId: {auction.Id}, BidAmount: {bid.Amount}, CurrentPrice: {auction.CurrentPrice}, ReservePrice: {auction.ReservedPrice}, IsSold: {auction.IsSold}");
                return false;
            }

            // Get the current highest bid
            var highestBid = GetHighestBid(auction.Id);

            // Check if the bid amount is greater than the current highest bid
            if (highestBid != null && bid.Amount <= highestBid.Amount)
            {
                _logger.LogWarning($"Bid failed: Bid amount less than or equal to current highest bid. AuctionId: {auction.Id}, BidAmount: {bid.Amount}, HighestBid: {highestBid.Amount}");
                return false;
            }

            // If all checks pass, update the current price and add the new bid
            auction.CurrentPrice = bid.Amount;
            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            // Notify all connected clients of the new bid
            await _auctionHub.Clients.All.SendAsync("ReceiveBidUpdate", bid.AuctionId, bid.Amount, bid.UserId);

            return true;
        }

        public List<Bid> GetUserBids(string userId)
        {
            return _context.Bids.Where(b => b.UserId == userId).ToList();
        }

        public List<Bid> GetUserWonBids(string userId)
        {
            return _context.Bids
                .Where(b => b.UserId == userId && b.Auction.EndDate < DateTime.UtcNow && b.Amount == b.Auction.CurrentPrice)
                .ToList();
        }

        public List<Bid> GetUserLostBids(string userId)
        {
            return _context.Bids
                .Where(b => b.UserId == userId && b.Auction.EndDate < DateTime.UtcNow && b.Amount != b.Auction.CurrentPrice)
                .ToList();
        }

        public List<Bid> GetUserExpiredBids(string userId)
        {
            return _context.Bids
                .Where(b => b.UserId == userId && b.Auction.EndDate < DateTime.UtcNow)
                .ToList();
        }

        public Bid GetBid(int id)
        {
            return _context.Bids.FirstOrDefault(b => b.Id == id);
        }

        // Get the current highest bid for a specific auction
        public Bid GetHighestBid(int auctionId)
        {
            return _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();
        }

        public List<Bid> GetBidHistory(int auctionId)
        {
            return _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Timestamp)
                .ToList();
        }

        public bool IsBidWon(Bid bid, Auction auction)
        {
            return DateTime.UtcNow > auction.EndDate && bid.Id == GetHighestBid(auction.Id).Id;
        }

        public bool IsBidLost(Bid bid, Auction auction)
        {
            return DateTime.UtcNow > auction.EndDate && bid.Id != GetHighestBid(auction.Id).Id;
        }

        public bool IsBidExpired(Bid bid, Auction auction)
        {
            return DateTime.UtcNow > auction.EndDate;
        }

        public Bid GetWinningBid(int auctionId)
        {
            return _context.Bids
                .Where(b => b.AuctionId == auctionId)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();
        }

        public List<Bid> GetLosingBidders(int auctionId, string highestBidder)
        {
            var losingBidders = _context.Bids
                .Where(b => b.AuctionId == auctionId && b.UserId != highestBidder)
                .GroupBy(b => b.UserId)
                .Select(g => g.OrderByDescending(b => b.Amount).FirstOrDefault())
                .ToList();

            return losingBidders;
        }
    }
}
