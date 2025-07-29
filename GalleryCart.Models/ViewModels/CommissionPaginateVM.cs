namespace GalleryCart.Models.ViewModels;

public class CommissionPaginateVM
{
    public List<CommissionListItemVM> Commissions { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}