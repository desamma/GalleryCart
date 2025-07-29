using GalleryCart.Models.Models;

namespace GalleryCart.Models.ViewModels
{
    public class AllArtistModel
    {
        public required IQueryable<User> Artist { get; set; }
    }
}
