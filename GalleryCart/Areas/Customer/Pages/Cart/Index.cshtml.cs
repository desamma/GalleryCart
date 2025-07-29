using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ICartRepository _cartRepository;

        public IndexModel(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public GalleryCart.Models.Models.Cart? CartUser { get; set; }

        [TempData]
        public string? PaymentMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }


            if (!Guid.TryParse(userId, out var guidUserId))
            {
                return BadRequest("Invalid user ID format.");
            }
            CartUser = await _cartRepository
                .GetAllQueryable(c => c.UserId == guidUserId)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Post)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync();

            return Page();
        }
    }
}
