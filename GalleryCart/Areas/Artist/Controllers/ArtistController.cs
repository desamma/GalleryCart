using AutoMapper;
using CloudinaryDotNet;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Artist.Controllers;

[Area("Artist")]
public class ArtistController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository  _userRepository;
    private readonly IMapper _mapper;
    private readonly CloudinaryUploader _cloudinaryUploader;

    public ArtistController(UserManager<User> userManager,
        IMapper mapper, 
        CloudinaryUploader cloudinaryUploader, IUserRepository userRepository)
    {
        _userManager = userManager;
        _mapper = mapper;
        _cloudinaryUploader = cloudinaryUploader;
        _userRepository = userRepository;
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
            if  (currentUser == null) return Unauthorized();
            
            var exsitingUsername = await _userManager.FindByNameAsync(user.UserName);
            if (exsitingUsername != null)
            {
                ModelState.AddModelError("UserName", "Username has already been taken.");
                return View(user);
            }
            if (user.UserName == null)
            {
                ModelState.AddModelError("UserName", "UsCloudinaryUploaderername cannot be empty.");
                return View(user);
            }
            
            if (image != null)
            {
                var imgUrl = await _cloudinaryUploader.UploadImageAsync(image);
                if (imgUrl.Equals("Wrong file extension", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("UserAvatar", "Wrong file extension.");
                    ModelState.AddModelError("UserAvatar", "Support formats are: jpg, jpeg, png, gif, webp.");
                    return View(user);
                }

                if (imgUrl != null) user.UserAvatar = imgUrl;
            }
            
            await _userRepository.UpdateAsync(_mapper.Map<User>(user));
            
            HttpContext.Session.SetString("CurrentUser", JsonConvert.SerializeObject(user));
        }
        catch (Exception e)
        {
            ModelState.AddModelError("", "Something went wrong, please try again later.");
            return View(user);
        }
        return View(user);
    }
}