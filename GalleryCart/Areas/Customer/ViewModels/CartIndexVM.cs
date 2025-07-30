using GalleryCart.Models.Models;
using GalleryCart.Models.ViewModels;

namespace GalleryCart.Areas.Customer.ViewModels;

public class CartIndexVM
{
    public Cart CartUser { get; set; }
    public List<PostVM> Posts { get; set; } = new();
    public string? PaymentMessage { get; set; }
}
