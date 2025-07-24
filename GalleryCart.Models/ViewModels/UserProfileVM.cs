using System.ComponentModel.DataAnnotations;
using GalleryCart.Models.Models;

namespace GalleryCart.Models.ViewModels;

public class UserProfileVM
{
    // User Information
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? UserDOB { get; set; }
    public string UserAvatar { get; set; }
    public string ProfessionSummary { get; set; }
    public string Skills { get; set; }
    public string Software { get; set; }
    public string ContactInfo { get; set; }
    public bool IsJobLess { get; set; }
    public bool CommissionStatus { get; set; }
    public bool IsOwnProfile { get; set; } // To show/hide edit buttons
    
    // Posts Information
    public IEnumerable<PostVM> Posts { get; set; } = new List<PostVM>();
    public int TotalPostsCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 21;
    public bool HasMorePosts => (CurrentPage * PageSize) < TotalPostsCount;
}