using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using web_project.Data;
using web_project.Models;
using web_project.Services;
using Microsoft.AspNetCore.Authorization;
using web_project.Data.Migrations;
using System.Security.Claims;

namespace web_project.Controllers
{
    public class BidsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BidService _bidService;
        private readonly IHubContext<AuctionHub> _auctionHub;
        private readonly AuctionService _auctionService;
        private readonly ILogger<BidsController> _logger;

        public BidsController(ApplicationDbContext context, BidService bidService, IHubContext<AuctionHub> auctionHub, AuctionService auctionService, ILogger<BidsController> logger)
        {
            _context = context;
            _bidService = bidService;
            _auctionHub = auctionHub;
            _auctionService = auctionService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> PlaceBid(int auctionId, decimal amount)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized user tried to place a bid.");
                return Unauthorized();
            }

            var auction = _context.Auctions.FirstOrDefault(a => a.Id == auctionId);
            if (auction == null)
            {
                _logger.LogWarning($"Auction not found with id: {auctionId}");
                return NotFound();
            }

            if (auction.IsExpired())
            {
                _logger.LogWarning($"Bid attempt on expired auction with id: {auctionId}");
                return BadRequest("This auction has already expired.");
            }

            var bid = new Bid
            {
                AuctionId = auctionId,
                Amount = amount,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            var result = await _bidService.PlaceBidAsync(bid, auction);
            if (result)
            {
                await _auctionHub.Clients.All.SendAsync("ReceiveBidUpdate", auctionId, amount, userId);
                return RedirectToAction("Details", "Auctions", new { id = auctionId });
            }
            else
            {
                _logger.LogWarning($"Error placing bid for auction id: {auctionId}, amount: {amount}, user id: {userId}");
                ModelState.AddModelError("BidError", "Error placing bid. Please ensure your bid is higher than the current price and reserve price, and the auction has not expired or sold.");
                return View("~/Views/Auctions/Details.cshtml", auction);
            }
        }

        // GET: Bids/HighestBid
        [HttpGet]
        public IActionResult HighestBid(int auctionId)
        {
            var highestBid = _bidService.GetHighestBid(auctionId);
            if (highestBid == null) return NotFound();

            return View(highestBid);
        }

        // GET: Bids/TrackBid/{bidId}
        [HttpGet]
        public IActionResult TrackBid(int bidId)
        {
            var bid = _bidService.GetBid(bidId);
            if (bid == null) return NotFound();

            var auction = _context.Auctions.FirstOrDefault(a => a.Id == bid.AuctionId);
            if (auction == null) return NotFound();

            ViewBag.IsBidExpired = _bidService.IsBidExpired(bid, auction);
            ViewBag.IsBidWon = _bidService.IsBidWon(bid, auction);
            ViewBag.IsBidLost = _bidService.IsBidLost(bid, auction);

            return View(bid);
        }

        // GET: Bids/Notification/{userId}
        [HttpGet]
        public IActionResult Notifications(string userId)
        {
            var bids = _bidService.GetUserBids(userId);
            return View(bids);
        }

        public async Task<IActionResult> CompleteAuction(int auctionId)
        {
            var result = await _auctionService.CompleteAuction(auctionId);

            if (result.isSuccess)
            {
                return Ok(result.message);
            }
            else
            {
                return BadRequest(result.message);
            }
        }
    }
}