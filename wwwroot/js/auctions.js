// Create a connection to AuctionHub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/Services/AuctionHub")
    .build();

// Start the connection
// Client-side method to handle receiving bid updates
function onBidUpdateReceived(itemId, newBidAmount, bidder) {
    console.log(`New bid for item ${itemId}: ${newBidAmount} by ${bidder}`);
    alert(`New bid for item ${itemId}: ${newBidAmount} by ${bidder}`);
}

// Register the onBidUpdateReceived function with the SignalR connection
connection.on("ReceiveBidUpdate", onBidUpdateReceived);

// Start the connection
connection.start()
    .then(() => console.log("Connected to AuctionHub"))
    .catch(err => console.error("Error connecting to AuctionHub:", err));