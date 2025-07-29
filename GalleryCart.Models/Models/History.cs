using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GalleryCart.Models.Models
{
    public class History
    {
        [Key]
        public Guid HistoryId { get; set; }

        public Guid UserId { get; set; }

        public Guid PostId { get; set; }

        [Required(ErrorMessage = "You need to provide a price.")]
        [Range(0, 99999999999999999999.99, ErrorMessage = "The price must be a positive number.")]
        public decimal TotalPrice { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public float Discount { get; set; } = 0.0f;

        [ValidateNever]
        public virtual User User { get; set; }

        [ValidateNever]
        public virtual Post Post { get; set; }

        // Thêm các property cho thanh toán nếu cần
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderId { get; set; }

  

        // Tạo mới một lịch sử giao dịch
        public static History Create(Guid userId, Guid postId, decimal totalPrice, DateTime purchaseDate, float discount = 0,
            string? paymentMethod = null, string? transactionId = null, string? orderId = null)
        {
            return new History
            {
                HistoryId = Guid.NewGuid(),
                UserId = userId,
                PostId = postId,
                TotalPrice = totalPrice,
                PurchaseDate = purchaseDate,
                Discount = discount,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                OrderId = orderId
            };
        }

        // Cập nhật thông tin thanh toán
        public void UpdatePaymentInfo(string? paymentMethod, string? transactionId, string? orderId)
        {
            PaymentMethod = paymentMethod;
            TransactionId = transactionId;
            OrderId = orderId;
        }

        // Áp dụng giảm giá
        public void ApplyDiscount(float discountPercent)
        {
            Discount = discountPercent;
            TotalPrice = TotalPrice - (TotalPrice * (decimal)discountPercent / 100);
        }
    }
}
