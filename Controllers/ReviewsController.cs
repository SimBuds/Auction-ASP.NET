using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using web_project.Data;
using web_project.Models;

namespace web_project.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<BidsController> _logger;

        public ReviewsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<BidsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int auctionId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);

            if (auction == null)
            {
                return NotFound();
            }

            var reviewer = await _userManager.GetUserAsync(User);
            var reviewee = await _context.Users.FindAsync(auction.UserId);

            var review = new Review()
            {
                AuctionId = auctionId,
                ReviewerId = reviewer.Id,
                RevieweeId = reviewee.Id,
            };

            return View(review);

        }

        [HttpPost]
        public IActionResult Create([Bind("AuctionId,ReviewerId,RevieweeId,Rating,Comment")] Review review)
        {
            ModelState.Clear(); // clear model state before adding errors

            var auction = _context.Auctions.Find(review.AuctionId);
            if (auction == null)
            {
                ModelState.AddModelError("AuctionId", "Auction not found");
            }

            var reviewer = _userManager.FindByIdAsync(review.ReviewerId).Result;
            if (reviewer == null)
            {
                ModelState.AddModelError("ReviewerId", "Reviewer not found");
            }

            var reviewee = _userManager.FindByIdAsync(review.RevieweeId).Result;
            if (reviewee == null)
            {
                ModelState.AddModelError("RevieweeId", "Reviewee not found");
            }

            if (review.Rating < 1 || review.Rating > 5)
            {
                ModelState.AddModelError("Rating", "Rating must be between 1 and 5");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                review.Auction = auction;
                review.Reviewer = reviewer;
                review.Reviewee = reviewee;

                _context.Reviews.Add(review);
                _context.SaveChanges();
                return RedirectToAction("Details", "Auctions", new { id = review.AuctionId });
            }
            else
            {
                return View(review);
            }
        }
    }
}