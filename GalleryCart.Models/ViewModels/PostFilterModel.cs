namespace GalleryCart.Models.ViewModels;

public class PostFilterModel
{
    public string? Search { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TagId { get; set; }
}
