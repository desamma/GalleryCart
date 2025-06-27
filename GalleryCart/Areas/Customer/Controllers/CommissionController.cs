using GalleryCart.Areas.Customer.Models;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize(Roles = "Admin,user")]
    //[Route("Customer/[controller]/[action]")]
    [AutoValidateAntiforgeryToken]
    public class CommissionController : Controller
    {
        private readonly ICommissionRepository commissionRepository;
        private readonly UserManager<User> userManager;

        public CommissionController(ICommissionRepository commissionRepository, UserManager<User> userManager)
        {
            this.commissionRepository = commissionRepository;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(string artistId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await userManager.FindByIdAsync(artistId);
            if (artist == null)
            {
                return NotFound("Artist not found.");
            }
            if (Guid.TryParse(userId, out Guid userGuid) && Guid.TryParse(artistId, out Guid artistGuid))
            {
                var model = new CreateCommssionViewModel
                {
                    CommissionerId = userGuid,
                    CreatedDate = DateTime.Now,
                    ArtistId = artistGuid,
                    Artist = artist
                };
                return model == null ? NotFound() : View(model);
            }
            return BadRequest("Invalid authentication.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCommssionViewModel model)
        {
            var commission = new Commission
            {
                Description = model.Description,
                ArtistId = model.ArtistId,
                CommissionerId = model.CommissionerId,
                CreatedDate = model.CreatedDate,
                Price = model.Price,
                Status = "Pending",
                DueDate = model.CreatedDate.AddDays(model.DueDateDays)
            };

            await commissionRepository.AddAsync(commission);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DetailAsync(string commissionId)
        {
            var entity = await commissionRepository.GetAsync(c => c.CommissionerId.Equals(commissionId));
            return View(entity);
        }

        [HttpGet]
        public async Task<IActionResult> CommissionManagement()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            // Initialize the view model
            var model = new CommissionManagementViewModel
            {
                CommissionKvp = new Dictionary<User, List<Commission>>()
            };

            // Fetch all commissions for the current user
            var commissions = commissionRepository.GetAllQueryable(c => c.CommissionerId.ToString() == userId, true).ToList();

            var artistIds = commissions.Select(c => c.ArtistId).ToList();
            var artists = await userManager.Users
                .Where(u => artistIds.Contains(u.Id))
                .ToListAsync();

            // Map each commission to its corresponding artist
            foreach (var commission in commissions)
            {
                var artist = artists.FirstOrDefault(a => a.Id == commission.ArtistId);
                if (artist != null)
                {
                    if (!model.CommissionKvp.TryGetValue(artist, out List<Commission>? value))
                    {
                        value = new List<Commission>();
                        model.CommissionKvp[artist] = value;
                    }

                    value.Add(commission);
                }
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditAsync(string commissionId)
        {
            var entity = await commissionRepository.GetAsync(c => c.CommissionerId.Equals(commissionId));
            return View(entity);
        }
        /*
                [HttpPost("Edit")]
                public IActionResult Edit(Commission model)
                {

                }*/
    }
}
