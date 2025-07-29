namespace GalleryCart.Models.ViewModels;

public class UserPaginateVM
{
    public List<UserListItemViewModel> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}