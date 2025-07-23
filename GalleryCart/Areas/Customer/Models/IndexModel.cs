using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Customer.Models
{
    public class IndexModel
    {
        public User? CurrentUser { get; set; }
        public required IQueryable<Post> Posts { get; set; }
    }
}
