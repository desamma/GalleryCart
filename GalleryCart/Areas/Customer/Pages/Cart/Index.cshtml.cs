using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Pages.Cart
{
    public class IndexModel : PageModel
    {
        public Cart? CartUser { get; set; }

        [TempData]
        public string? PaymentMessage { get; set; }

        public class Cart
        {
            public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        }

        public class CartItem
        {
            public Guid PostId { get; set; } // Changed from CommissionId
            public Post Post { get; set; } = new Post(); // Changed from Commission
        }

        public class Post
        {
            public string Title { get; set; } = string.Empty;
            public Artist? Artist { get; set; }
            public decimal Price { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Artist
        {
            public string UserName { get; set; } = string.Empty;
        }

        public IActionResult OnPostPayNow()
        {
            PaymentMessage = "Payment test successful! (This is a test message for Pay Now button.)";
            return RedirectToPage();
        }
    }
}