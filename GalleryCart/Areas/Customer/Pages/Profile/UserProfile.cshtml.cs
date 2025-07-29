/*using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GalleryCart.Areas.Customer.Pages.Profile;

[Authorize(Roles = "user")]
public class UserProfileModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _env;

    public UserProfileModel(UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
    }

    [BindProperty]
    public ProfileInputModel Input { get; set; }
    
    [BindProperty]
    public string? CurrentAvatarUrl { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        Input = new ProfileInputModel
        {
            UserName = user.UserName,
            UserDOB = user.UserDOB.HasValue,
            PhoneNumber = user.PhoneNumber
        };
        CurrentAvatarUrl = user.UserAvatar;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        // Update fields
        user.UserName = Input.UserName;
        user.UserDOB = Input.UserDOB.HasValue ? DateOnly.FromDateTime(Input.UserDOB.Value) : null;
        user.PhoneNumber = Input.PhoneNumber;

        // Upload avatar
        if (Input.UserAvatar != null)
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "uploads/avatars");
            Directory.CreateDirectory(uploadPath);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Input.UserAvatar.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = System.IO.File.Create(filePath);
            await Input.UserAvatar.CopyToAsync(stream);
            user.UserAvatar = $"/uploads/avatars/{fileName}";
        }

        // Password change
        if (!string.IsNullOrEmpty(Input.OldPassword) && !string.IsNullOrEmpty(Input.NewPassword))
        {
            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return Page();
            }

            var result = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToPage("/Profile/UserProfile");
    }
}*/