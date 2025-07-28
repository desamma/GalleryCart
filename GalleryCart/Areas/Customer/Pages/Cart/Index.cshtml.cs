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

        public class Cart
        {
            public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        }

        public class CartItem
        {
            public int CommissionId { get; set; }
            public Commission Commission { get; set; } = new Commission();
        }

        public class Commission
        {
            public string Description { get; set; } = string.Empty;
            public Artist? Artist { get; set; }
            public decimal Price { get; set; }
            public DateTime DueDate { get; set; }
        }

        public class Artist
        {
            public string UserName { get; set; } = string.Empty;
        }
    }
}