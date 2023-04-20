using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_project.Data;
using web_project.Services;
using web_project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Hangfire;

namespace web_project.Controllers
{
    public class AuctionsController : Controller
    {
        private readonly AuctionService _auctionService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuctionsController(AuctionService auctionService, ApplicationDbContext context, UserManager<User> userManager, IBackgroundJobClient backgroundJobClient)
        {
            _auctionService = auctionService;
            _context = context;
            _userManager = userManager;
            _backgroundJobClient = backgroundJobClient;
        }

        // GET: Auctions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Auctions.ToListAsync());
        }

        // GET: Auctions/Details
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (auction == null)
            {
                return NotFound();
            }

            // Check if the current user is a buyer
            bool isBuyer = User.IsInRole("Buyer");

            // Pass IsBuyer flag to the view
            ViewBag.IsBuyer = isBuyer;

            return View(auction);
        }

        // GET: Auctions/Create
        [Authorize(Roles = "Seller,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Auctions/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ImageUrl,StartingPrice,EndDate,Category,Condition,ReservedPrice,UserId")] Auction auction)
        {
            auction.StartDate = DateTime.Now;
            auction.UserId = _userManager.GetUserId(User);
            auction.CurrentPrice = auction.StartingPrice;

            ModelState.Remove("Bids");
            ModelState.Remove("Reviews");
            ModelState.Remove("Transactions");

            if (ModelState.IsValid)
            {
                // Schedule the CompleteAuction method to be called when the auction ends
                _auctionService.ScheduleAuctionCompletion(auction.Id, auction.EndDate);

                _context.Add(auction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(auction);
        }

        // GET: Auctions/Edit
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null)
            {
                return NotFound();
            }

            // Check if the current user is the auction owner
            var currentUserId = _userManager.GetUserId(User);
            if (auction.UserId != currentUserId)
            {
                return Unauthorized();
            }

            return View(auction);
        }

        // POST: Auctions/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageUrl,StartingPrice,StartDate,EndDate,Category,Condition,UserId")] Auction auction)
        {
            if (id != auction.Id)
            {
                return NotFound();
            }

            // Check if the current user is the auction owner
            var currentUserId = _userManager.GetUserId(User);
            if (auction.UserId != currentUserId)
            {
                return Unauthorized();
            }

            auction.UserId = currentUserId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuctionExists(auction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(auction);
        }

        // GET: Auctions/Delete
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auction = await _context.Auctions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auction == null)
            {
                return NotFound();
            }

            return View(auction);
        }

        // POST: Auctions/Delete
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Built-in Search
        private bool AuctionExists(int id)
        {
            return _context.Auctions.Any(e => e.Id == id);
        }

        // POST: Auctions/SearchBar
        [HttpPost]
        public async Task<IActionResult> SearchBar(string SearchString)
        {
            var auctions = from m in _context.Auctions
                           select m;

            if (!String.IsNullOrEmpty(SearchString))
            {
                auctions = auctions.Where(s => s.Name.Contains(SearchString));
            }

            return View("Index", await auctions.ToListAsync());
        }

        // POST: Auctions/Category
        // Display Specific Categories
        [HttpPost]
        public async Task<IActionResult> CategorySort(int CategorySelection)
        {
            var auctions = from m in _context.Auctions
                           select m;

            if (CategorySelection == 0)
            {
                auctions = auctions.Where(s => s.Category == Category.Electronics);
            }
            else if (CategorySelection == 1)
            {
                auctions = auctions.Where(s => s.Category == Category.Fashion);
            }
            else if (CategorySelection == 2)
            {
                auctions = auctions.Where(s => s.Category == Category.Home);
            }
            else if (CategorySelection == 3)
            {
                auctions = auctions.Where(s => s.Category == Category.Sports);
            }
            else if (CategorySelection == 4)
            {
                auctions = auctions.Where(s => s.Category == Category.Books);
            }
            else if (CategorySelection == 5)
            {
                auctions = auctions.Where(s => s.Category == Category.Other);
            }

            return View("Index", await auctions.ToListAsync());
        }

        // POST: Auctions/Condition
        // Display Specific Conditions
        [HttpPost]
        public async Task<IActionResult> ConditionSort(int ConditionSelection)
        {
            var auctions = from m in _context.Auctions
                           select m;

            if (ConditionSelection == 0)
            {
                auctions = auctions.Where(s => s.Condition == Condition.New);

            }
            else
            {
                auctions = auctions.Where(s => s.Condition == Condition.Used);
            }

            return View("Index", await auctions.ToListAsync());
        }

        // POST: Auctions/Price
        // Display Specific Price Ranges high to low
        [HttpPost]
        public async Task<IActionResult> PriceSort(int PriceSelection)
        {
            var auctions = from m in _context.Auctions
                           select m;

            if (PriceSelection == 1)
            {
                auctions = auctions.OrderByDescending(s => s.StartingPrice);
            }
            else
            {
                auctions = auctions.OrderBy(s => s.StartingPrice);
            }

            return View("Index", await auctions.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AdvancedSearch(string NameSearch, int? ConditionSearch, int? CategorySearch)
        {
            var auctions = from m in _context.Auctions
                           select m;

            if (!String.IsNullOrEmpty(NameSearch))
            {
                auctions = auctions.Where(s => s.Name.Contains(NameSearch));
            }

            if (ConditionSearch.HasValue)
            {
                auctions = auctions.Where(s => s.Condition == (Condition)ConditionSearch.Value);
            }

            if (CategorySearch.HasValue)
            {
                auctions = auctions.Where(s => s.Category == (Category)CategorySearch.Value);
            }

            return View("Index", await auctions.ToListAsync());
        }
    }
}
