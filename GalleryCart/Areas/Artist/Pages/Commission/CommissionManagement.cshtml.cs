using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Artist.Pages.Commission
{
    public class CommissionManagementModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommissionRepository _commissionRepository;
        private readonly IChatRepository _chatRepository;
        public CommissionManagementModel(IUserRepository userRepository, ICommissionRepository commissionRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _commissionRepository = commissionRepository;
            _chatRepository = chatRepository;
        }

        public Dictionary<string, int> StatusOrder = new()
        {
            ["Pending"] = 1,
            ["In Progress"] = 2,
            ["Completed"] = 3,
            ["Rejected"] = 4
        };

        public required User CurrentUser { get; set; }

        //load into memory first to fix bug and shit
        public required List<Models.Models.Commission> Commissions { get; set; } = new();
        public async Task OnGetAsync()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");

                if (!string.IsNullOrEmpty(userJson))
                {
                    CurrentUser = JsonConvert.DeserializeObject<User>(userJson);
                }

                var commissions = await _commissionRepository
                    .GetAllQueryable(c => c.ArtistId.Equals(CurrentUser.Id), asNoTracking: true)
                    .ToListAsync();

                foreach (var commission in commissions)
                {
                    if (commission.CommissionerId != Guid.Empty && commission.Commissioner == null)
                    {
                        commission.Commissioner = await _userRepository.GetAsync(u => u.Id.Equals(commission.CommissionerId));
                    }
                }

                // sorting type shit 
                Commissions = commissions
               .OrderBy(c => StatusOrder.ContainsKey(c.Status) ? StatusOrder[c.Status] : 5)
               .ThenByDescending(c => c.CreatedDate)
               .ToList();
            }
            catch (Exception ex)
            {
                BadRequest($"An error occurred while fetching posts: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAcceptAsync(string commissionId)
        {
            try
            {
                if (commissionId != string.Empty)
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "In Progress";
                        await _commissionRepository.UpdateAsync(commission);
                    }
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Log the error
                return BadRequest($"Error accepting commission: {ex.Message}");
            }
        }
        public async Task<IActionResult> OnPostCompleteAsync(string commissionId)
        {
            try
            {
                if (commissionId != string.Empty)
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "Completed";
                        await _commissionRepository.UpdateAsync(commission);
                    }
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Log the error
                return BadRequest($"Error accepting commission: {ex.Message}");
            }
        }
        public async Task<IActionResult> OnPostRejectAsync(string commissionId, string? rejectionReason)
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");

                if (!string.IsNullOrEmpty(userJson))
                {
                    CurrentUser = JsonConvert.DeserializeObject<User>(userJson);
                }
                if (commissionId != string.Empty)
                {
                    var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                    if (commission != null)
                    {
                        commission.Status = "Rejected";
                        var rejectMessage = $"This commission (id: {commission.CommissionId} has been rejected.\nReason: {rejectionReason}";
                        var chat = new Models.Models.Chat
                        {
                            ChatId = Guid.NewGuid(),
                            SenderId = CurrentUser.Id,
                            ReceiverId = commission.CommissionerId,
                            Message = rejectionReason ?? "No reason provided.",
                            Timestamp = DateTime.Now
                        };

                        await _commissionRepository.UpdateAsync(commission);
                        await _chatRepository.AddAsync(chat);
                    }
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // Log the error
                return BadRequest($"Error accepting commission: {ex.Message}");
            }
        }
    }
}
