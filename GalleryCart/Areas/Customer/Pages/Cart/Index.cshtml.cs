using GalleryCart.Areas.Customer.ViewModels;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using CartModel = GalleryCart.Models.Models.Cart;


namespace GalleryCart.Areas.Customer.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ICartRepository _cartRepository;

        public IndexModel(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public CartIndexVM ViewModel { get; set; } = new();

        [TempData]
        public string? PaymentMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = await _cartRepository.GetAsync(c => c.UserId.ToString() == userId);
            if (cart == null)
            {
                ViewModel.CartUser = new CartModel();

                ViewModel.Posts = new List<PostVM>();
                return Page();
            }

            ViewModel.CartUser = cart;
            ViewModel.Posts = cart.CartItems
                .Where(ci => ci.Post != null)
                .Select(ci => new PostVM
                {
                    PostId = ci.Post.PostId,
                    Title = ci.Post.Title,
                    Description = ci.Post.Description,
                    Path = ci.Post.Path,
                    PostDate = ci.Post.PostDate,
                    LikeCount = ci.Post.LikeCount,
                    DislikeCount = ci.Post.DislikeCount,
                    SaleCount = ci.Post.SaleCount,
                    IsPorfolio = ci.Post.IsPorfolio,
                    IsMature = ci.Post.IsMature,
                    IsImage = ci.Post.IsImage,
                    Price = ci.Post.Price,
                    PostAuthor = ci.Post.User?.UserName ?? "Unknown",
                    PostTags = ci.Post.Tags
                }).ToList();

            return Page();
        }
    }
}
