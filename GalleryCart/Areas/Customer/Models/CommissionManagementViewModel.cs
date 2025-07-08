using GalleryCart.Models.Models;

namespace GalleryCart.Areas.Customer.Models
{
    public class CommissionManagementViewModel
    {
        public Dictionary<User, List<Commission>> CommissionKvp{ get; set;} = new Dictionary<User, List<Commission>>();
    }
}
