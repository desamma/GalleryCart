using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        
        public string Content { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CommentDate { get; set; } = DateTime.Now;

        [Required]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public float Rating { get; set; } = 0.0f;

        public Guid? UserId { get; set; }
        [ValidateNever]
        public virtual User User { get; set; }
        
        public Guid PostId { get; set; }
        [ValidateNever]
        public virtual Post Post { get; set; }
    }
}
