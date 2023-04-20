namespace web_project.Models
{
    public class PaymentModel
    {
        public int TransactionId { get; set; }
        public int CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public int SecurityCode { get; set; }
        public string Name { get; set; }
    }
}
