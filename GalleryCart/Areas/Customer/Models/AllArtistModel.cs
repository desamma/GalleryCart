using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Customer.Models
{
    public class AllArtistModel
    {
        public required IQueryable<User> Artist { get; set; }
    }
}
