using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "user")]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;

        public HomeController(UserManager<User> userManager, IPostRepository postRepository, IUserRepository userRepository)
        {
            _userManager = userManager;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");
                User? currentUser = null;

                if (!string.IsNullOrEmpty(userJson))
                {
                    currentUser = JsonConvert.DeserializeObject<User>(userJson);
                }

                var posts = _postRepository.GetAllQueryable(p => p.IsImage)
                    .OrderByDescending(p => p.LikeCount)
                    .ThenBy(p => p.IsMature)
                    .ThenBy(p => p.PostDate);

                foreach (var post in posts)
                {
                    post.User = await _userRepository.GetAsync(a => a.Id.Equals(post.UserId));
                }

                var model = new IndexModel
                {
                    CurrentUser = currentUser,
                    Posts = posts
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while fetching posts: {ex.Message}");
            }
        }

        public IActionResult AllArtist()
        {
            try
            {
                var artists = _userRepository.GetAllQueryable()
                    .Where(u => u.IsArtits)
                    .OrderByDescending(u => u.NormalizedUserName);

                var model = new AllArtistModel
                {
                    Artist = artists
                };

                return View(model);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while fetching artists: {ex.Message}");
            }
        }
    }
}
