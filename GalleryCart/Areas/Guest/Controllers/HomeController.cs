using AutoMapper;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Guest.Controllers
{
    [Area("Guest")]
    public class HomeController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;

        public HomeController(IPostRepository postRepository, IUserRepository userRepository, IMapper mapper, ITagRepository tagRepository)
        {
            _postRepository = postRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index(string? search, DateTime? startDate, DateTime? endDate, Guid? tagId)
        {
            var userJson = HttpContext.Session.GetString("CurrentUser");
            User? currentUser = null;

            if (!string.IsNullOrEmpty(userJson))
                currentUser = JsonConvert.DeserializeObject<User>(userJson);

            // Load tags for the ViewData dropdown
            var tags = await _tagRepository.GetAllQueryable().ToListAsync();
            ViewData["Tags"] = tags;
            ViewData["Search"] = search;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["TagId"] = tagId;

            // Query posts
            var postsQuery = _postRepository.GetAllQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            if (startDate.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.PostDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.PostDate <= endDate.Value);
            }

            if (tagId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.Tags.Any(t => t.TagId == tagId.Value));
            }

            var posts = postsQuery.OrderByDescending(p => p.LikeCount)
                .ThenBy(p => p.IsMature)
                .ThenBy(p => p.PostDate);

            foreach (var post in posts)
            {
                post.User = await _userRepository.GetAsync(u => u.Id == post.UserId);
            }

            var model = new IndexModel
            {
                CurrentUser = currentUser,
                Posts = posts
            };

            return View(model);
        }

    }
}
