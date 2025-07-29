using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    [AutoValidateAntiforgeryToken]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPostRepository _postRepository; // Changed from ICommissionRepository

        public CartController(ICartRepository cartRepository, IPostRepository postRepository)
        {
            _cartRepository = cartRepository;
            _postRepository = postRepository;
        }

        // Add post to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Lấy cart hiện tại của user, nếu chưa có thì tạo mới  
            var cart = await _cartRepository.GetAsync(c => c.UserId.ToString() == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = Guid.Parse(userId)
                };
                await _cartRepository.AddAsync(cart);
            }


            // Kiểm tra post đã có trong cart chưa
            if (cart.CartItems.Any(ci => ci.PostId == postId))
            {
                return BadRequest("Post already in cart.");
            }

            // Thêm post vào cart
            cart.CartItems.Add(new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                PostId = postId
            });

            await _cartRepository.UpdateAsync(cart);
            return RedirectToAction("Index", "Cart");
        }

        // Remove post from cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var cart = await _cartRepository.GetAsync(c => c.UserId.ToString() == userId);
            if (cart == null) return NotFound();

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.PostId == postId);
            if (cartItem == null) return NotFound();

            cart.CartItems.Remove(cartItem);
            await _cartRepository.UpdateAsync(cart);

            return RedirectToAction("Index", "Cart");
        }

        // Display cart
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = await _cartRepository.GetAsync(c => c.UserId.ToString() == userId);
            return View(cart);
        }
    }
}