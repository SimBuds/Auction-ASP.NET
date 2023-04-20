using Microsoft.AspNetCore.SignalR;

namespace web_project.Services
{
    public class AuctionHub : Hub
    {

        public AuctionHub() { }


        public async Task NotifyBidUpdate(int itemId, decimal newBidAmount, string bidder)
        {
            await Clients.All.SendAsync("ReceiveBidUpdate", itemId, newBidAmount, bidder);
        }

        public async Task NotifyBidExpired(int itemId, string bidder)
        {
            await Clients.User(bidder).SendAsync("ReceiveBidExpired", itemId);
        }

        public async Task NotifyBidWon(int itemId, string winner)
        {
            await Clients.User(winner).SendAsync("ReceiveBidWon", itemId);
        }

        public async Task NotifyBidLost(int itemId, string loser)
        {
            await Clients.User(loser).SendAsync("ReceiveBidLost", itemId);
        }
    }
}
