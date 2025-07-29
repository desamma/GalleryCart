using AutoMapper;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Guest.Controllers
{
    [Area("Guest")]
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public HomeController(IPostRepository postRepository, IUserRepository userRepository, IMapper mapper)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _mapper = mapper;
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
    }
}
