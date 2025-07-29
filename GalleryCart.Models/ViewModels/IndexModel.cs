using GalleryCart.Models.Models;

namespace GalleryCart.Models.ViewModels
{
    public class IndexModel
    {
        public User? CurrentUser { get; set; }
        public IEnumerable<Post> Posts { get; set; } = Enumerable.Empty<Post>();
        public IEnumerable<Tag> AllTags { get; set; } = Enumerable.Empty<Tag>();
        public PostFilterModel Filter { get; set; } = new PostFilterModel();
    }

}
