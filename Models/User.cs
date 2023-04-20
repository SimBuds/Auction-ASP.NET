using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace web_project.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public ICollection<Auction> Auctions { get; set; }

        public ICollection<Bid> Bids { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public User() { }
    }
}