using GalleryCart.Areas.Artist.Models;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ChatModel = GalleryCart.Models.Models.Chat;

namespace GalleryCart.Areas.Artist.Controllers
{
    [Area("Artist")]
    [Authorize(Roles = "artist")]
    public class CommissionController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommissionRepository _commissionRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

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
            IChatRepository chatRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _userRepository = userRepository;
            _commissionRepository = commissionRepository;
            _chatRepository = chatRepository;
            _webHostEnvironment = webHostEnvironment;
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
                return RedirectToAction("CommissionManagement");
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
                return RedirectToAction("CommissionManagement");
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

                return RedirectToAction("CommissionManagement");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error rejecting commission: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(string commissionId, List<IFormFile> files, List<string> links, string? notes)
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");
                if (string.IsNullOrEmpty(userJson))
                    return Unauthorized();

                var currentUser = JsonConvert.DeserializeObject<User>(userJson);

                if (string.IsNullOrEmpty(commissionId) || currentUser == null)
                    return BadRequest("Some data is missing.");

                var commission = await _commissionRepository.GetAsync(c => c.CommissionId.ToString().Equals(commissionId));
                if (commission == null)
                    return NotFound("Commission not found.");

                var uploadResults = new List<string>();

                // Initialize the list if it's null
                if (commission.CommissionResultLink == null)
                    commission.CommissionResultLink = new List<string>();

                // Handle file uploads
                if (files != null && files.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "commissions", commissionId);
                    Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            // Validate file type
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".zip", ".rar", ".psd", ".ai" };
                            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                return BadRequest($"File type {fileExtension} is not allowed.");
                            }

                            // Check file size (max 50MB per file)
                            if (file.Length > 50 * 1024 * 1024)
                            {
                                return BadRequest($"File {file.FileName} is too large. Maximum size is 50MB.");
                            }

                            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            // Add to the list with FILE prefix to distinguish from links
                            var relativePath = $"{commissionId}/{uniqueFileName}";
                            commission.CommissionResultLink.Add($"FILE:{relativePath}|{file.FileName}");

                            uploadResults.Add($"File uploaded: {file.FileName}");
                        }
                    }
                }

                // Handle links
                if (links != null && links.Count > 0)
                {
                    foreach (var link in links)
                    {
                        if (!string.IsNullOrWhiteSpace(link))
                        {
                            // Basic URL validation
                            if (Uri.TryCreate(link, UriKind.Absolute, out var validatedUri) &&
                                (validatedUri.Scheme == Uri.UriSchemeHttp || validatedUri.Scheme == Uri.UriSchemeHttps))
                            {
                                // Add to the list with LINK prefix
                                commission.CommissionResultLink.Add($"LINK:{link}");
                                uploadResults.Add($"Link added: {link}");
                            }
                            else
                            {
                                return BadRequest($"Invalid URL: {link}");
                            }
                        }
                    }
                }

                // Add notes if provided
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    commission.CommissionResultLink.Add($"NOTES:{notes}");
                }

                // Add timestamp
                commission.CommissionResultLink.Add($"UPLOADED_AT:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // Save to database
                await _commissionRepository.UpdateAsync(commission);

                // Send notification to commissioner
                var notificationMessage = $"Your commission has been completed! ";
                if (uploadResults.Count > 0)
                {
                    notificationMessage += $"Files and links have been uploaded for your review. ";
                }
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    notificationMessage += $"Artist notes: {notes}";
                }

                var chat = new ChatModel
                {
                    ChatId = Guid.NewGuid(),
                    SenderId = currentUser.Id,
                    ReceiverId = commission.CommissionerId,
                    Message = notificationMessage,
                    Timestamp = DateTime.Now
                };

                await _chatRepository.AddAsync(chat);

                TempData["SuccessMessage"] = $"Successfully uploaded {uploadResults.Count} items for commission.";
                return RedirectToAction("CommissionManagement");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error uploading files: {ex.Message}");
            }
        }
    }
}