using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class Post
    {
        public Guid PostId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        // The path to the image or video file (use cloudinary)
        public string Path { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime? PostDate { get; set; } = DateTime.Now;

        public int LikeCount { get; set; } = 0;

        public int DislikeCount { get; set; } = 0;

        public int SaleCount { get; set; } = 0;

        public bool IsPorfolio { get; set; } // true: show in portfolio

        public bool IsMature { get; set; } // true: mature content

        public bool IsImage { get; set; } // true: image, false: video

        [Required(ErrorMessage = "You need to provide a price for the commission.")]
        [Range(0, 99999999999999999999.99, ErrorMessage = "The price must be a positive number.")]
        public decimal Price { get; set; }

        public Guid? UserId { get; set; }
        [ValidateNever]
        public virtual User User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<FavouritePost> FavouritePosts { get; set; } = new List<FavouritePost>();

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

        public virtual ICollection<History> Histories { get; set; } = new List<History>();
    }
}
