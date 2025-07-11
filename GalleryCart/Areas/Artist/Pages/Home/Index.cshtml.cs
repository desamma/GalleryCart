using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GalleryCart.Areas.Artist.Pages.Home;

public class IndexModel : PageModel
{
    private readonly IPostRepository  _postRepository;

    public IndexModel(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public List<Post> Posts { get; set; }
    public async Task OnGetAsync()
    {
        //Posts = await _postRepository.GetAllQueryable().ToListAsync();
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