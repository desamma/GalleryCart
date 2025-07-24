using GalleryCart.Areas.Artist.Models;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ChatModel = GalleryCart.Models.Models.Chat;

namespace GalleryCart.Areas.Artist.Controllers
{
    [Area("Artist")]
    public class CommissionController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommissionRepository _commissionRepository;
        private readonly IChatRepository _chatRepository;

        private readonly Dictionary<string, int> StatusOrder = new()
        {
            ["Pending"] = 1,
            ["In Progress"] = 2,
            ["Completed"] = 3,
            ["Rejected"] = 4
        };

        public CommissionController(
            IUserRepository userRepository,
            ICommissionRepository commissionRepository,
            IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _commissionRepository = commissionRepository;
            _chatRepository = chatRepository;
        }

        public async Task<IActionResult> CommissionManagement()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");
                if (string.IsNullOrEmpty(userJson))
                    return Unauthorized();

                var currentUser = JsonConvert.DeserializeObject<User>(userJson);

                var commissions = await _commissionRepository
                    .GetAllQueryable(c => c.ArtistId.Equals(currentUser.Id), asNoTracking: true)
                    .ToListAsync();

                foreach (var commission in commissions)
                {
                    if (commission.CommissionerId != Guid.Empty && commission.Commissioner == null)
                    {
                        commission.Commissioner = await _userRepository.GetAsync(u => u.Id.Equals(commission.CommissionerId));
                    }
                }

                var orderedCommissions = commissions
                    .OrderBy(c => StatusOrder.TryGetValue(c.Status, out int value) ? value : 5)
                    .ThenByDescending(c => c.CreatedDate)
                    .ToList();

                var model = new CommissionManagementModel
                {
                    CurrentUser = currentUser,
                    Posts = orderedCommissions,
                    StatusOrder = StatusOrder
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while fetching commissions: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string commissionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(commissionId))
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "In Progress";
                        await _commissionRepository.UpdateAsync(commission);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error accepting commission: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Complete(string commissionId)
        {
            try
            {
                if (!string.IsNullOrEmpty(commissionId))
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "Completed";
                        await _commissionRepository.UpdateAsync(commission);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error completing commission: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string commissionId, string? rejectionReason)
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");
                if (string.IsNullOrEmpty(userJson))
                    return Unauthorized();

                var currentUser = JsonConvert.DeserializeObject<User>(userJson);

                if (!string.IsNullOrEmpty(commissionId))
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "Rejected";

                        var chat = new ChatModel
                        {
                            ChatId = Guid.NewGuid(),
                            SenderId = currentUser.Id,
                            ReceiverId = commission.CommissionerId,
                            Message = $"Commission rejected with reason: {rejectionReason}" ?? "No reason provided.",
                            Timestamp = DateTime.Now
                        };

                        await _commissionRepository.UpdateAsync(commission);
                        await _chatRepository.AddAsync(chat);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error rejecting commission: {ex.Message}");
            }
        }
    }
}
