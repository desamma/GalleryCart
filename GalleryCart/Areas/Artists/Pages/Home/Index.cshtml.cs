using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Artists.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        public IndexModel(UserManager<User> userManager, IPostRepository postRepository, IUserRepository userRepository)
        {
            _userManager = userManager;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        public required User CurrentUser { get; set; }
        public required IQueryable<Post> Posts { get; set; }
        public async Task OnGetAsync()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("CurrentUser");

                if (!string.IsNullOrEmpty(userJson))
                {
                    CurrentUser = JsonConvert.DeserializeObject<User>(userJson);
                }

                Posts = _postRepository.GetAllQueryable(p => p.IsImage)
                    .OrderByDescending(p => p.LikeCount)
                    .ThenBy(p => p.IsMature)
                    .ThenBy(p => p.PostDate);

                foreach (var post in Posts)
                {
                    post.User = await _userRepository.GetAsync(a => a.Id.Equals(post.UserId));
                }
            }

            catch (Exception ex)
            {
                BadRequest($"An error occurred while fetching posts: {ex.Message}");
            }
        }
    }
}
