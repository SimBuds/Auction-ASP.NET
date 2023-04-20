using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BuyerId")]
        public string BuyerId { get; set; }
        public User Buyer { get; set; }

        [Required]
        [ForeignKey("SellerId")]
        public string SellerId { get; set; }
        public User Seller { get; set; }

        [Required]
        [ForeignKey("AuctionId")]
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TransactionAmount { get; set; }

        [Required]
        public bool IsPaymentSuccessful { get; set; }

        public bool IsValidTransaction()
        {
            return this.TransactionAmount > 0;
        }

        public bool IsFinalTransaction()
        {
            return this.Auction.IsSold && this.TransactionAmount == this.Auction.CurrentPrice;
        }

    }
}
