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
        private readonly ICartRepository _cartRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly ICommissionPaymentRepository _commissionPaymentRepository;
        private readonly ICommissionRepository _commissionRepository;

        public PaymentController(
            IConfiguration configuration,
            ICartRepository cartRepository,
            ApplicationDbContext db,
            IHistoryRepository historyRepository,
            ICommissionPaymentRepository commissionPaymentRepository,
            ICommissionRepository commissionRepository)
        {
            _configuration = configuration;
            _db = db;
            _cartRepository = cartRepository;
            _cartRepository = cartRepository;
            _historyRepository = historyRepository;
            _commissionPaymentRepository = commissionPaymentRepository;
            _commissionRepository = commissionRepository;
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

            // Store in session with the transaction reference as key
            HttpContext.Session.SetString($"OrderType_{tick}", model.OrderType);
            HttpContext.Session.SetString($"CommissionId_{tick}", model.OrderDescription);

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
                var txnRef = Request.Query["vnp_TxnRef"].ToString();
                var orderType = HttpContext.Session.GetString($"OrderType_{txnRef}");
                var commissionId = HttpContext.Session.GetString($"CommissionId_{txnRef}");

                // Clean up session
                HttpContext.Session.Remove($"OrderType_{txnRef}");
                HttpContext.Session.Remove($"CommissionId_{txnRef}");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var userGuid = Guid.Parse(userId);
                    if (orderType == "CommissionPayment")
                    {
                        var amount = decimal.Parse(Request.Query["vnp_Amount"].ToString());
                        var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString() == commissionId);
                        if (commissionId != null)
                        {
                            if ((amount/100) == commission.Price)
                            {
                                var commissionPayment = new CommissionPayment
                                {
                                    Amount = commission.Price,
                                    CommissionId = Guid.Parse(commissionId)
                                };
                                await _commissionPaymentRepository.AddAsync(commissionPayment);
                            }
                        }
                        return RedirectToAction("CommissionManagement", "Commission");
                    }
                    else
                    {
                        var cart = await _db.Carts
                            .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Post)
                            .FirstOrDefaultAsync(c => c.UserId == userGuid);

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
                            _db.CartItems.RemoveRange(cart.CartItems);
                            await _db.SaveChangesAsync();
                        }
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