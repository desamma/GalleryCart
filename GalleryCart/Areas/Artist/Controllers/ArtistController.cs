using AutoMapper;
using CloudinaryDotNet;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Artist.Controllers;

[Area("Artist")]
public class ArtistController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository  _userRepository;
    private readonly IMapper _mapper;
    private readonly CloudinaryUploader _cloudinaryUploader;
    private readonly IFavouritePostRepository _favouritePostRepository;
    private readonly IPostRepository _postRepository;

    public ArtistController(UserManager<User> userManager,
        IMapper mapper, 
        CloudinaryUploader cloudinaryUploader, IUserRepository userRepository, IFavouritePostRepository favouritePostRepository, IPostRepository postRepository)
    {
        _userManager = userManager;
        _mapper = mapper;
        _cloudinaryUploader = cloudinaryUploader;
        _userRepository = userRepository;
        _favouritePostRepository = favouritePostRepository;
        _postRepository = postRepository;
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
        var currentUser = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("CurrentUser"));
        var targetUserId = userId ?? currentUser.Id;
        var isOwnProfile = targetUserId == currentUser.Id;
        
        var user = await _userRepository.GetAsync(u => u.Id == targetUserId);
        if (user == null) return NotFound();
        
        // Validate tab parameter - non-owners can only view portfolio
        if (!isOwnProfile && tab != "portfolio")
        {
            return RedirectToAction(nameof(ArtistProfile), new { userId = targetUserId, tab = "portfolio", pageNumber = 1, pageSize });
        }
        
        IQueryable<Post> postsQuery;
        
        if (tab == "portfolio")
        {
            postsQuery = _postRepository.GetAllQueryable(
                p => p.UserId == targetUserId && p.IsPorfolio == true);
        }
        else // tab == "all"
        {
            postsQuery = _postRepository.GetAllQueryable(
                p => p.UserId == targetUserId);
        }

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
            
            // no mapping - broken
            existingUser.UserName = user.UserName;
            existingUser.UserDOB = user.UserDOB;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.ProfessionSummary = user.ProfessionSummary;
            existingUser.Skills = user.Skills;
            existingUser.Software = user.Software;
            existingUser.ContactInfo = user.ContactInfo;
            existingUser.IsJobLess = user.IsJobLess;
            existingUser.CommissionStatus = user.CommissionStatus;
            // Don't update UserAvatar here if no new image - it's handled above
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
        return RedirectToAction(nameof(Index), "Home", new { area = "Artist" });
    }
}