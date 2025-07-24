using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GalleryCart.Models.ViewModels;

public class ProfileInputModel
{
    [MaxLength(50)]
    [Required]
    public string UserName { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? UserDOB { get; set; }

    [Display(Name = "Phone Number")]
    [DataType(DataType.PhoneNumber)]
    [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
    [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
    public string? PhoneNumber { get; set; }

    [ValidateNever] 
    public string? UserAvatar { get; set; }

    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
    public DateTime? CreatedDate { get; set; }

    public bool IsBanned { get; set; }
    public bool IsArtits { get; set; }
    public string ProfessionSummary { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;
    public string Software { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public bool IsJobLess { get; set; }
    public bool CommissionStatus { get; set; } // true: open
}