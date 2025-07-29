namespace GalleryCart.Models.ViewModels;

public class CommissionListItemVM
{
    public Guid CommissionId { get; set; }
    public string ArtistName { get; set; }
    public string CommissionerName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
}