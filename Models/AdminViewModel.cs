namespace web_project.Models
{
    public class AdminViewModel
    {
        public Auction SelectedAuction { get; set; }
        public Bid SelectedBid { get; set; }
        public User SelectedUser { get; set; }
        public Review SelectedReview { get; set; }
        public Transaction SelectedTransaction { get; set; }

        public List<Auction> Auctions { get; set; }
        public List<Bid> Bids { get; set; }
        public List<User> Users { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}

