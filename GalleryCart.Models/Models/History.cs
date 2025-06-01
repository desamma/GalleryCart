using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class History
    {
        public Guid HistoryId { get; set; }
        
        public float Discount { get; set; } = 0.0f;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "You need to provide a price.")]
        [Range(0, 99999999999999999999.99, ErrorMessage = "The price must be a positive number.")]
        public decimal TotalPrice { get; set; }

        public Guid UserId { get; set; }
        [ValidateNever]
        public virtual User User { get; set; }

        public Guid PostId { get; set; }
        [ValidateNever]
        public virtual Post Post { get; set; }
        //public virtual ICollection<Post> PurchasedPosts { get; set; } = new List<Post>();
    }
}
