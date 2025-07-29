namespace GalleryCart.Models.Models
{
    public class CommissionPayment
    {
        public Guid CommissionPaymentId { get; set; }
        public Guid CommissionId { get; set; }
        public decimal Amount { get; set; }
    }
}
