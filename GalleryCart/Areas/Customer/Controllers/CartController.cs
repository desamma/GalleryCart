using GalleryCart.Areas.Customer.ViewModels;
using GalleryCart.DataAccess;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IPostRepository _postRepository;
        private readonly ApplicationDbContext _db;

        public CartController(ICartRepository cartRepository, IPostRepository postRepository, ApplicationDbContext db)
        {
            _cartRepository = cartRepository;
            _postRepository = postRepository;
            _db = db;
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

            // Cheking Post exists in the cart
            if (cart.CartItems.Any(ci => ci.PostId == postId))
            {
                return BadRequest("Post already in cart.");
            }
            _db.CartItems.Add(new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                PostId = postId
            });
            await _db.SaveChangesAsync();

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

            _db.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Cart");
        }

        // Display cart
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = await _cartRepository.GetAsync(c => c.UserId.ToString() == userId);
            if (cart == null)
            {
                return View(new CartIndexVM
                {
                    CartUser = new Cart(),
                    Posts = new List<PostVM>()
                });
            }

            var postVMs = cart.CartItems
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

            var vm = new CartIndexVM
            {
                CartUser = cart,
                Posts = postVMs
            };

            return View(vm);
        }
    }
}
