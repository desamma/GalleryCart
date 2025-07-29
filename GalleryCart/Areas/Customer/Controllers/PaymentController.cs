using GalleryCart.DataAccess;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Library;
using GalleryCart.Models.Models;
using GalleryCart.Models.Models.Vnpay;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;
        private readonly IHistoryRepository _historyRepository;

        public PaymentController(
            IConfiguration configuration,
            ApplicationDbContext db,
            IHistoryRepository historyRepository)
        {
            _configuration = configuration;
            _db = db;
            _historyRepository = historyRepository;
        }

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(HttpContext));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(Request.Query, _configuration["Vnpay:HashSecret"]);

            // Only save to history if payment is successful
            if (response.Success)
            {
                // Get current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userGuid = Guid.Parse(userId);

                    // Get user's cart and cart items
                    var cart = await _db.Carts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Post)
                        .FirstOrDefaultAsync(c => c.UserId == userGuid);

                    if (cart != null && cart.CartItems.Any())
                    {
                        foreach (var item in cart.CartItems)
                        {
                            // Use repository method to add history
                            var history = new History
                            {
                                UserId = userGuid,
                                PostId = item.PostId,
                                TotalPrice = item.Post.Price,
                                PurchaseDate = DateTime.UtcNow,
                                PaymentMethod = response.PaymentMethod,
                                TransactionId = response.TransactionId,
                                OrderId = response.OrderId,
                            };
                            await _historyRepository.AddAsync(history);
                        }
                        // Optionally: clear cart after purchase
                        _db.CartItems.RemoveRange(cart.CartItems);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Json(response);
        }
    }
}