using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Guest.Models
{
    public class IndexModel
    {
        public User? CurrentUser { get; set; }
        public required IQueryable<Post> Posts { get; set; }
    }
}
