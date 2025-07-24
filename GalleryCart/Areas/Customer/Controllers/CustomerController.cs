using AutoMapper;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Customer.Controllers;

[Area("Customer")]
public class CustomerController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository  _userRepository;
    private readonly IMapper _mapper;
    private readonly CloudinaryUploader _cloudinaryUploader;
    private readonly IPostRepository _postRepository;
    private readonly IFavouritePostRepository _favouritePostRepository;

    public CustomerController(UserManager<User> userManager, IPostRepository postRepository, IUserRepository userRepository, IMapper mapper, CloudinaryUploader cloudinaryUploader, IFavouritePostRepository favouritePostRepository)
    {
        _userManager = userManager;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _cloudinaryUploader = cloudinaryUploader;
        _favouritePostRepository = favouritePostRepository;
    }
    
    public async Task<IActionResult> Index(Guid? userId, int pageNumber = 1, int pageSize = 21)
    {
        var currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("CurrentUser"));
        var targetUserId = userId ?? currentUser.Id;
        
        var user = await _userRepository.GetAsync(u => u.Id == targetUserId);
        if (user == null) return NotFound();
        
        var favoritePostsQuery = _favouritePostRepository.GetAllQueryable(
            fp => fp.UserId == targetUserId && fp.PostId.HasValue);

        var totalPostsCount = await favoritePostsQuery.CountAsync();
        
        var posts = await favoritePostsQuery
            .OrderByDescending(fp => fp.Post.PostDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(fp => fp.Post)
            .ToListAsync();
        
        var favoritePostVMs = _mapper.Map<List<PostVM>>(posts);
        
        // Map user and set additional properties
        var viewModel = _mapper.Map<UserProfileVM>(user);
        viewModel.IsOwnProfile = targetUserId == currentUser.Id;
        viewModel.Posts = favoritePostVMs;
        viewModel.TotalPostsCount = totalPostsCount;
        viewModel.CurrentPage = pageNumber;
        viewModel.PageSize = pageSize;
        
        return View(viewModel);
    }
    
    public async Task<IActionResult> ArtistProfile(Guid? userId, string tab = "portfolio", int pageNumber = 1, int pageSize = 21)
    {
        if (userId == null) return NotFound();
        var targetUserId = userId;
        const bool isOwnProfile = false;
        
        var user = await _userRepository.GetAsync(u => u.Id == targetUserId);
        if (user == null) return NotFound();
        
        // Validate tab parameter - non-owners can only view portfolio
        if (tab != "portfolio")
        {
            return RedirectToAction(nameof(ArtistProfile), new { userId = targetUserId, tab = "portfolio", pageNumber = 1, pageSize });
        }
        
        var postsQuery = _postRepository.GetAllQueryable(
                p => p.UserId == targetUserId && p.IsPorfolio == true);

        var totalPostsCount = await postsQuery.CountAsync();
        
        var posts = await postsQuery
            .OrderByDescending(p => p.PostDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Map to ViewModels
        var postVMs = _mapper.Map<List<PostVM>>(posts);
        
        // Map user and set additional properties
        var viewModel = _mapper.Map<UserProfileVM>(user);
        viewModel.IsOwnProfile = isOwnProfile;
        viewModel.Posts = postVMs;
        viewModel.TotalPostsCount = totalPostsCount;
        viewModel.CurrentPage = pageNumber;
        viewModel.PageSize = pageSize;
        
        // Pass the active tab to the view
        ViewBag.ActiveTab = tab;
        
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var userVM = _mapper.Map<ProfileInputModel>(user);
        
        return View(userVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileInputModel user, IFormFile? image)
    {
        try
        {
            var currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("CurrentUser"));
            if (currentUser == null) return Unauthorized();
            
            // Get the existing user from database
            var existingUser = await _userRepository.GetAsync(u => u.Id == currentUser.Id);
            if (existingUser == null) return NotFound();
            
            // Check for duplicate username only if username is being changed
            if (user.UserName != existingUser.UserName)
            {
                var existingUserName = await _userRepository.GetAsync(u => 
                   u.UserName.Equals(user.UserName));
                if (existingUserName != null)
                {
                    ModelState.AddModelError("UserName", "Username has already been taken.");
                    return View(user);
                }
            }
            
            if (string.IsNullOrEmpty(user.UserName))
            {
                ModelState.AddModelError("UserName", "Username cannot be empty.");
                return View(user);
            }
            
            // Handle image upload
            if (image != null)
            {
                var imgUrl = await _cloudinaryUploader.UploadImageAsync(image);
                if (imgUrl.Equals("Wrong file extension", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("UserAvatar", "Wrong file extension.");
                    ModelState.AddModelError("UserAvatar", "Support formats are: jpg, jpeg, png, gif, webp.");
                    return View(user);
                }

                if (imgUrl != null) 
                {
                    existingUser.UserAvatar = imgUrl;
                }
            }
            
            // Update the existing user properties instead of creating new entity because mapping is dumb dumb
            existingUser.UserName = user.UserName;
            existingUser.UserDOB = user.UserDOB;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.ProfessionSummary = user.ProfessionSummary;
            existingUser.Skills = user.Skills;
            existingUser.Software = user.Software;
            existingUser.ContactInfo = user.ContactInfo;
            existingUser.IsJobLess = user.IsJobLess;
            existingUser.CommissionStatus = user.CommissionStatus;

            if (image == null && !string.IsNullOrEmpty(user.UserAvatar))
            {
                existingUser.UserAvatar = user.UserAvatar;
            }
            
            await _userRepository.UpdateAsync(existingUser);
            
            // Update session with the updated user
            HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(existingUser));
            
            TempData["Success"] = "Profile updated successfully!";
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", "Something went wrong, please try again later.");
            return View(user);
        }
        return RedirectToAction(nameof(Index), "Home", new { area = "Customer" });
    }
}