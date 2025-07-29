using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Artist.Models
{
    public class CommissionManagementModel
    {
        public User? CurrentUser { get; set; }
        public required List<Commission> Posts { get; set; }
        public required Dictionary<string, int> StatusOrder { get; set; }
    }
}
