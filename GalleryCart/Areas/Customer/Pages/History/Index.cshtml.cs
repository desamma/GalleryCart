using GalleryCart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using HistoryModel = GalleryCart.Models.Models.History;

namespace GalleryCart.Areas.Customer.Pages.History
{
    public class IndexModel : PageModel
    {
        private readonly IHistoryRepository _historyRepository;

        public IndexModel(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public List<HistoryModel> Histories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
                return RedirectToPage("/Account/Login", new { area = "Identity" });

            Histories = await _historyRepository
                .GetAllQueryable(h => h.UserId == guid)
                .Include(h => h.Post)
                .OrderByDescending(h => h.PurchaseDate)
                .ToListAsync();

            return Page();
        }
    }
}
