using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class Tag
    {
        public Guid TagId { get; set; }

        [Required]
        public string TagName { get; set; } = string.Empty;

        public string TagDescription { get; set; } = string.Empty;

        //public Guid PostId { get; set; }
        //[ValidateNever]
        //public virtual Post Post { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
