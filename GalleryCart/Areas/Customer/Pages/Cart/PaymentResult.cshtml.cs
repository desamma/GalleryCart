using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GalleryCart.Models.Models.Vnpay;
using System.Text.Json;

namespace GalleryCart.Areas.Customer.Pages.Cart
{
    public class PaymentResultModel : PageModel
    {
        public PaymentInformationModel? PaymentInfo { get; set; }
        public string? PaymentMessage { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderId { get; set; }
        public string? PaymentMethod { get; set; }

        public void OnGet()
        {
            if (TempData.ContainsKey("PaymentInfo"))
            {
                PaymentInfo = JsonSerializer.Deserialize<PaymentInformationModel>((string)TempData.Peek("PaymentInfo")!);
            }

            PaymentMessage = TempData.Peek("PaymentMessage") as string;
            TransactionId = TempData.Peek("TransactionId") as string;
            OrderId = TempData.Peek("OrderId") as string;
            PaymentMethod = TempData.Peek("PaymentMethod") as string;
        }
    }
}
