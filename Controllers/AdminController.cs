using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using web_project.Data;
using web_project.Models;

namespace web_project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Auctions = await _context.Auctions.ToListAsync();
            ViewBag.Bids = await _context.Bids.ToListAsync();
            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.Transactions = await _context.Transactions.ToListAsync();
            ViewBag.UserManager = _userManager;

            return View();
        }

        // GET: Admin - Add Auction
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST : Admin - Add Auction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ImageUrl,StartDate,StartingPrice,EndDate,Category,Condition,CurrentPrice,ReservedPrice,IsSold,UserId")] Auction auction)
        {
            ModelState.Remove("Bids");
            ModelState.Remove("Reviews");

            if (ModelState.IsValid)
            {
                // Get the current user's ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Set the UserId on the auction object
                auction.UserId = userId;

                _context.Add(auction);
                await _context.SaveChangesAsync();
                // redirect to the index action
                return RedirectToAction("Index");
            }
            else
            {
                // display errors to console
                foreach (var error in ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(auction);
        }

        // Check Auction
        private bool AuctionExists(int id)
        {
            return _context.Auctions.Any(e => e.Id == id);
        }

        // GET: Admin - Edit Auction
        [HttpGet]
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

            ViewData["ExistingImageUrl"] = auction.ImageUrl;

            return View(auction);
        }

        // POST: Admin - Edit Auction
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageUrl,StartDate,StartingPrice,EndDate,Category,Condition,CurrentPrice,ReservedPrice,IsSold,UserId")] Auction auction, string ExistingImageUrl, bool KeepExistingImage)
        {
            if (id != auction.Id)
            {
                return NotFound();
            }

            if (KeepExistingImage)
            {
                auction.ImageUrl = ExistingImageUrl;
            }

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

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            _context.Auctions.Remove(auction);
            await _context.SaveChangesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LockUser(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Lock user account indefinitely
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        // Redirect to a confirmation page or the user list page
                        return RedirectToAction("UserList");
                    }
                }
            }
            // Redirect to an error page or the user list page
            return RedirectToAction("UserList");
        }

    }
}
