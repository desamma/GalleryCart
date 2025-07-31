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
        private readonly ICartRepository _cartRepository;
        private readonly IHistoryRepository _historyRepository;

        public PaymentController(
            IConfiguration configuration,
            ICartRepository cartRepository,
            IHistoryRepository historyRepository)
        {
            _configuration = configuration;
            _cartRepository = cartRepository;
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

            if (response.Success)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userGuid = Guid.Parse(userId);
                    var cart = await _cartRepository.GetAsync(
                        c => c.UserId == userGuid,
                        include: q => q.Include(c => c.CartItems).ThenInclude(ci => ci.Post)
                    );

                    if (cart != null && cart.CartItems.Any())
                    {
                        foreach (var item in cart.CartItems)
                        {
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

                      
                        cart.CartItems.Clear();
                        await _cartRepository.UpdateAsync(cart);

                    }
                }

                TempData["PaymentMessage"] = "Payment successful! Thank you for your purchase.";
                TempData["TransactionId"] = response.TransactionId;
                TempData["OrderId"] = response.OrderId;
                TempData["PaymentMethod"] = response.PaymentMethod;

                return RedirectToPage("/Cart/PaymentResult", new { area = "Customer" });
            }
            else
            {
                TempData["PaymentMessage"] = "Payment failed or invalid signature. Please try again.";
                return RedirectToPage("/Cart/PaymentResult", new { area = "Customer" });
            }
            //return Json(response);
        }
    }
}