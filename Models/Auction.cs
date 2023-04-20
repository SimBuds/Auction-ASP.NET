using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project.Models
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingPrice { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public Category Category { get; set; }

        [Required]
        public Condition Condition { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ReservedPrice { get; set; }

        [Required]
        public bool IsSold { get; set; }

        [ForeignKey("UserId")]
        public string UserId { get; set; }

        public ICollection<Bid> Bids { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public Auction() { }

        public bool IsExpired()
        {
            return DateTime.Now > EndDate;
        }
    }
}
