using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace web_project.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AuctionId")]
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public Bid() { }
    }
}