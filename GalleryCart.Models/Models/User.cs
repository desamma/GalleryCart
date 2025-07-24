using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryCart.Models.Models
{
    public class User : IdentityUser<Guid>
    {
        public override string UserName { get; set; }
        public DateOnly? UserDOB { get; set; }
        public override string? PhoneNumber { get; set; }
        public string? UserAvatar { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsBanned { get; set; }
        public bool IsArtits { get; set; }
        public string ProfessionSummary { get; set; }
        public string Skills { get; set; }
        public string Software { get; set; }
        public string ContactInfo { get; set; }
        public bool IsJobLess { get; set; }
        public bool CommissionStatus { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<FavouritePost> FavouritePosts { get; set; }
        public virtual ICollection<Chat> MessageSent { get; set; }
        public virtual ICollection<Chat> MessageReceived { get; set; }
        public virtual ICollection<Commission> CommissionsRequested { get; set; }
        public virtual ICollection<Commission> CommissionsReceived { get; set; }
        public virtual ICollection<History> Histories { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
    }
}
