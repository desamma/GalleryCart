using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [AutoValidateAntiforgeryToken]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICommissionRepository _commissionRepository;

        public CartController(ICartRepository cartRepository, ICommissionRepository commissionRepository)
        {
            _cartRepository = cartRepository;
            _commissionRepository = commissionRepository;
        }

        // Thêm commission vào cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid commissionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Lấy cart hiện tại của user, nếu chưa có thì tạo mới
            var cart = _cartRepository.GetAllQueryable(c => c.UserId.ToString() == userId, false).FirstOrDefault();
            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = Guid.NewGuid(),
                    UserId = Guid.Parse(userId)
                };
                await _cartRepository.AddAsync(cart);
            }

            // Kiểm tra commission đã có trong cart chưa
            if (cart.CartItems.Any(ci => ci.CommissionId == commissionId))
            {
                return BadRequest("Commission already in cart.");
            }

            // Thêm commission vào cart
            cart.CartItems.Add(new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cart.CartId,
                CommissionId = commissionId
            });

            await _cartRepository.UpdateAsync(cart);
            return RedirectToAction("Index", "Cart");
        }

        // Xóa commission khỏi cart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid commissionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = _cartRepository.GetAllQueryable(c => c.UserId.ToString() == userId, false).FirstOrDefault();
            if (cart == null) return NotFound();

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CommissionId == commissionId);
            if (cartItem == null) return NotFound();

            cart.CartItems.Remove(cartItem);
            await _cartRepository.UpdateAsync(cart);

            return RedirectToAction("Index", "Cart");
        }

        // Hiển thị cart
        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = _cartRepository.GetAllQueryable(c => c.UserId.ToString() == userId, true)
                .FirstOrDefault();

            return View(cart);
        }
    }
}