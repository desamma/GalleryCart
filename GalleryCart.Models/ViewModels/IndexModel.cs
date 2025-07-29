using GalleryCart.Models.Models;

namespace GalleryCart.Models.ViewModels
{
    public class IndexModel
    {
        public User? CurrentUser { get; set; }
        public required IQueryable<Post> Posts { get; set; }
    }
}
