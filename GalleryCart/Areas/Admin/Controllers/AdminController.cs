using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.ViewModels;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GalleryCart.Areas.Admin.Controllers;

    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IPostRepository _postRepo;
        private readonly ICommissionRepository _commissionRepo;
        private readonly IHistoryRepository _historyRepo;
        private const int PageSize = 20;

        public AdminController(
            IUserRepository userRepo,
            IPostRepository postRepo,
            ICommissionRepository commissionRepo,
            IHistoryRepository historyRepo)
        {
            _userRepo = userRepo;
            _postRepo = postRepo;
            _commissionRepo = commissionRepo;
            _historyRepo = historyRepo;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new DashboardViewModel
            {
                TotalUsers = await _userRepo.GetAllQueryable().CountAsync(),
                TotalArtists = await _userRepo.GetAllQueryable(u => u.IsArtits).CountAsync(),
                TotalRevenue = await _historyRepo.GetAllQueryable().SumAsync(h => (decimal?)h.TotalPrice) ?? 0,
                Growth = 30.56m,
                MonthlyRevenues = await _historyRepo.GetAllQueryable()
                    .GroupBy(h => new { h.PurchaseDate.Year, h.PurchaseDate.Month })
                    .Select(g => new MonthlyRevenueDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Total = g.Sum(h => h.TotalPrice)
                    }).ToListAsync(),
                RecentActivities = (await Task.WhenAll(
                    _userRepo.GetAllQueryable().OrderByDescending(u => u.CreatedDate).Take(5).Select(u => new RecentActivityDto { Type = "User", Name = u.UserName, Date = u.CreatedDate }).ToListAsync(),
                    _postRepo.GetAllQueryable().OrderByDescending(p => p.PostDate).Take(5).Select(p => new RecentActivityDto { Type = "Post", Name = p.Title, Date = p.PostDate }).ToListAsync(),
                    _commissionRepo.GetAllQueryable().OrderByDescending(c => c.CreatedDate).Take(5).Select(c => new RecentActivityDto { Type = "Commission", Name = c.Description, Date = c.CreatedDate }).ToListAsync(),
                    _historyRepo.GetAllQueryable().OrderByDescending(h => h.PurchaseDate).Take(5).Include(h => h.Post).Select(h => new RecentActivityDto { Type = "Purchase", Name = h.Post.Title, Date = h.PurchaseDate }).ToListAsync()
                )).SelectMany(x => x).OrderByDescending(x => x.Date).Take(20).ToList()
            };

            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> ManageUser(int page = 1)
        {
            var query = _userRepo.GetAllQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var users = await query
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    CreatedDate = u.CreatedDate,
                    UserDOB = u.UserDOB,
                    IsBanned = u.IsBanned,
                    IsArtist = u.IsArtits,
                    IsEmailConfirmed = u.EmailConfirmed,
                })
                .ToListAsync();
            var viewModel = new UserPaginateVM
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBan(Guid id)
        {
            var user = await _userRepo.GetAsync(u => u.Id == id);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            user.IsBanned = !user.IsBanned;
            if (await _userRepo.UpdateAsync(user))
            {
                return Json(new { success = true, isBanned = user.IsBanned });
            }

            return Json(new { success = false, message = "Failed to update user" });
        }
        
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userRepo.GetAsync(u => u.Id == id);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            if (await _userRepo.DeleteAsync(id))
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Failed to delete user" });
        }
        
        [HttpGet]
        public async Task<IActionResult> ManagePost(int page = 1)
        {
            var query = _postRepo.GetAllQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var posts = await query
                .OrderByDescending(p => p.PostDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new PostVM
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    PostAuthor = p.User.UserName,
                    PostDate = p.PostDate,
                    LikeCount = p.LikeCount,
                    DislikeCount = p.DislikeCount,
                    SaleCount = p.SaleCount,
                    IsMature = p.IsMature,
                    Price = p.Price,
                    Path = p.Path,
                    IsImage = p.IsImage
                })
                .ToListAsync();

            var viewModel = new PostPaginateVM
            {
                Posts = posts,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        
        [HttpGet]
        public async Task<IActionResult> ViewPost(Guid id)
        {
            var post = await _postRepo.GetAsync(p => p.PostId == id);
            if (post == null) return NotFound();

            return PartialView("_PostDetailPartial", post);
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var post = await _postRepo.GetAsync(p => p.PostId == id);
            if (post == null)
                return Json(new { success = false, message = "Post not found" });

            if (await _postRepo.DeleteAsync(id))
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to delete post" });
        }

        
        [HttpGet]
        public async Task<IActionResult> ManageCommission(int page = 1)
        {
            var query = _commissionRepo.GetAllQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var commissions = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new CommissionListItemVM
                {
                    CommissionId = c.CommissionId,
                    ArtistName = c.Artist.UserName,
                    CommissionerName = c.Commissioner.UserName,
                    Description = c.Description,
                    CreatedDate = c.CreatedDate,
                    DueDate = c.DueDate,
                    Price = c.Price,
                    Status = c.Status
                })
                .ToListAsync();

            var viewModel = new CommissionPaginateVM
            {
                Commissions = commissions,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> GetSellingHistory(DateTime? dateFrom, DateTime? dateTo)
        {
            var histories = await _historyRepo.GetAllQueryable(h =>
                (!dateFrom.HasValue || h.PurchaseDate >= dateFrom) &&
                (!dateTo.HasValue || h.PurchaseDate <= dateTo))
                .Select(h => new SellingHistoryDto
                {
                    Title = h.Post.Title,
                    Price = h.TotalPrice,
                    Quantity = 1,
                    PurchaseDate = h.PurchaseDate
                }).ToListAsync();

            return PartialView("_SellingHistoryPartial", histories);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? dateFrom, DateTime? dateTo)
        {
            var histories = await _historyRepo.GetAllQueryable(h =>
                (!dateFrom.HasValue || h.PurchaseDate >= dateFrom) &&
                (!dateTo.HasValue || h.PurchaseDate <= dateTo))
                .Include(h => h.Post)
                .Select(h => new SellingHistoryDto
                {
                    Title = h.Post.Title,
                    Price = h.TotalPrice,
                    Quantity = 1,
                    PurchaseDate = h.PurchaseDate
                }).ToListAsync();

            var bytes = ExcelExportHelper.ExportSellingHistoryToExcel(histories);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SellingHistory_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }