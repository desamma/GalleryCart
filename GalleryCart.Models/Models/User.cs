using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalleryCart.Models.Models
{
    public class User : IdentityUser<Guid>
    {
        [MaxLength(50)]
        [Required]
        public override string UserName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateOnly? UserDOB { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(11, ErrorMessage = "The phone number must be at most 11 digits.")]
        [RegularExpression(@"^\d{1,11}$", ErrorMessage = "The phone number must only contain digits")]
        public override string? PhoneNumber { get; set; }

        [ValidateNever] 
        public string? UserAvatar { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public bool isBanned { get; set; }
        public bool isArtits { get; set; }
        public string ProfessionSummary { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Software { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public bool JobLess { get; set; }
        public bool CommissionStatus { get; set; } // true: open
    }
}
