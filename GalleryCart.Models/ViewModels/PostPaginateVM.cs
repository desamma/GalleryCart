namespace GalleryCart.Models.ViewModels;

public class PostPaginateVM
{
    public List<PostVM> Posts { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}