namespace GalleryCart.Models.ViewModels;

public class UserListItemViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateOnly? UserDOB { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsBanned { get; set; }
    public bool IsArtist { get; set; }
    public bool IsEmailConfirmed { get; set; }
}