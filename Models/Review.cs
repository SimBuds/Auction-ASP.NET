using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web_project.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AuctionId")]
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }

        [Required]
        [ForeignKey("ReviewerId")]
        public string ReviewerId { get; set; }
        public User Reviewer { get; set; }

        [Required]
        [ForeignKey("RevieweeId")]
        public string RevieweeId { get; set; }
        public User Reviewee { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Comment { get; set; }

        public Review() { }
    }
}
