using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Customer.Pages.Home
{
    //[Authorize(Roles = "Admin,user")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IPostRepository  _postRepository;

        public IndexModel(UserManager<User> userManager, IPostRepository postRepository)
        {
            _userManager = userManager;
            _postRepository = postRepository;
        }

        public required User CurrentUser { get; set; }
        public User Artist { get; set; }
        public List<Post> Posts { get; set; }
        public async Task OnGetAsync()
        {
            var userJson = HttpContext.Session.GetString("CurrentUser");

            if (!string.IsNullOrEmpty(userJson))
            {
                CurrentUser = JsonConvert.DeserializeObject<User>(userJson);
            }
            Artist = await _userManager.FindByEmailAsync("lennaqb4@gmail.com");
            // Posts = await _postRepository.GetAllQueryable().ToListAsync();
            Posts = new List<Post>
            {
                new Post { PostId = Guid.NewGuid(), Path = "/images/image1.png", Title = "Character Design" },
                new Post { PostId = Guid.NewGuid(), Path = "/images/image2.png", Title = "Village Map" },
                new Post { PostId = Guid.NewGuid(), Path = "/images/image3.png", Title = "Forest Scene" },
                new Post { PostId = Guid.NewGuid(), Path = "/images/image3.png", Title = "Forest Scene 2" },
                new Post { PostId = Guid.NewGuid(), Path = "/images/image3.png", Title = "Forest Scene 3" },
                // Add more posts
            };
        }
    }
}
