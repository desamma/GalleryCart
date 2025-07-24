using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryCart.Models.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        [Key]
        public Guid CartItemId { get; set; }

        [Required]
        public Guid CartId { get; set; }
        [ForeignKey(nameof(CartId))]
        public virtual Cart Cart { get; set; }

        [Required]
        public Guid CommissionId { get; set; }
        [ForeignKey(nameof(CommissionId))]
        public virtual Commission Commission { get; set; }
    }
}