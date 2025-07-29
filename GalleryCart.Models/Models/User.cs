using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public bool IsBanned { get; set; }
        public bool IsArtits { get; set; }
        public string ProfessionSummary { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Software { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public bool IsJobLess { get; set; }
        public bool CommissionStatus { get; set; } // true: open

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<FavouritePost> FavouritePosts { get; set; } = new List<FavouritePost>();

        [InverseProperty(nameof(Chat.Sender))]
        public virtual ICollection<Chat> MessageSent { get; set; } = new List<Chat>();

        [InverseProperty(nameof(Chat.Receiver))]
        public virtual ICollection<Chat> MessageReceived { get; set; } = new List<Chat>();

        // where the user is the commissioner (client)
        [InverseProperty(nameof(Commission.Commissioner))]
        public virtual ICollection<Commission> CommissionsRequested { get; set; } = new List<Commission>();

        // where the user is the artist (provider)
        [InverseProperty(nameof(Commission.Artist))]
        public virtual ICollection<Commission> CommissionsReceived { get; set; } = new List<Commission>();

        public virtual ICollection<History> Histories { get; set; } = new List<History>();
        public virtual ICollection<Cart> Carts { get; set; }
    }
}
