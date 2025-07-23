using GalleryCart.Areas.Customer.Models;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [AutoValidateAntiforgeryToken]
    public class CommissionController : Controller
    {
        private readonly ICommissionRepository commissionRepository;
        private readonly IUserRepository userRepository;
        public CommissionController(ICommissionRepository commissionRepository, IUserRepository userRepository)
        {
            this.commissionRepository = commissionRepository;
            this.userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Create(string artistId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await userRepository.GetAsync(a => a.Id.ToString().Equals(artistId));
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
            if (!ModelState.IsValid)
            {
                // Reload artist data if validation fails
                if (model.ArtistId != Guid.Empty)
                {
                    model.Artist = await userRepository.GetAsync(a => a.Id.Equals(model.ArtistId));
                }
                return View(model);
            }

            // Set CreatedDate if not already set
            if (model.CreatedDate == default)
            {
                model.CreatedDate = DateTime.Now;
            }

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
            return RedirectToAction("CommissionManagement");
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
            try
            {
                // Fetch all commissions for the current user
                var commissions = commissionRepository.GetAllQueryable(c => c.CommissionerId.ToString() == userId, true).ToList();

                var artistIds = commissions.Select(c => c.ArtistId).ToList();
                var artists = await userRepository.GetAllQueryable(u => artistIds.Contains(u.Id))
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
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while retrieving commissions: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(string commissionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return NotFound("User not found.");
            }
            try
            {
                var entity = await commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                if (userId != entity.CommissionerId.ToString())
                {
                    return Unauthorized("You do not have permission to edit this commission.");
                }
                if (entity.Artist == null && entity.ArtistId != Guid.Empty)
                {
                    entity.Artist = await userRepository.GetAsync(a => a.Id.Equals(entity.ArtistId));
                }
                return View(entity);
            }
            catch (Exception ex)
            {
                return NotFound($"An error occurred while retrieving the commission: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(Commission model)
        {
            if (!ModelState.IsValid)
            {
                // Reload artist if validation fails
                if (model.ArtistId != Guid.Empty)
                {
                    model.Artist = await userRepository.GetAsync(a => a.Id.Equals(model.ArtistId));
                }
                return View(model);
            }

            var existingCommission = await commissionRepository.GetAsync(c => c.CommissionId == model.CommissionId);
            if (existingCommission == null)
            {
                return NotFound("Commission not found.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != existingCommission.CommissionerId.ToString())
            {
                return Unauthorized("You do not have permission to edit this commission.");
            }

            // Update only editable fields
            existingCommission.Description = model.Description;
            existingCommission.Price = model.Price;
            existingCommission.DueDate = model.DueDate;

            await commissionRepository.UpdateAsync(existingCommission);
            return RedirectToAction("CommissionManagement");
        }
    }
}
