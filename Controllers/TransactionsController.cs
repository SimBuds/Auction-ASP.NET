using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_project.Data;
using web_project.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace web_project.Controllers
{
    public class TransactionsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TransactionsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var userTransactions = await _context.Transactions
                .Where(t => t.BuyerId == userId)
                .ToListAsync();

            return View(userTransactions);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(int auctionId, string buyerId)
        {
            var auction = await _context.Auctions.FindAsync(auctionId);
            var buyer = await _context.Users.FindAsync(buyerId);

            if (auction == null || buyer == null)
            {
                return BadRequest();
            }

            if (auction.CurrentPrice < auction.ReservedPrice)
            {
                return BadRequest("The auction did not meet the reserved price.");
            }

            if (auction.IsSold)
            {
                return BadRequest("The auction has already been sold.");
            }

            var seller = await _context.Users.FindAsync(auction.UserId);
            var transaction = new Transaction
            {
                AuctionId = auctionId,
                BuyerId = buyerId,
                SellerId = seller.Id,
                TransactionDate = DateTime.Now,
                TransactionAmount = auction.CurrentPrice,
                IsPaymentSuccessful = true

            };
            auction.IsSold = true;
            _context.Update(auction);

            _context.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Payment(int id)
        {
            var paymentModel = new PaymentModel
            {
                TransactionId = id
            };

            return View(paymentModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Payment(PaymentModel paymentModel)
        {
            var transaction = await _context.Transactions.FindAsync(paymentModel.TransactionId);
            if (transaction == null)
            {
                return NotFound();
            }

            // Perform credit card validation here

            transaction.IsPaymentSuccessful = true;
            _context.Update(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}