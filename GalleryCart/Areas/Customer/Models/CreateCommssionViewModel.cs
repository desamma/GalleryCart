using GalleryCart.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Areas.Customer.Models
{
    public class CreateCommssionViewModel
    {

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "You need to provide a due date for the commission.")]
        public int DueDateDays { get; set; } // Commissioner sets the due date for the commission in days from the creation date

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "You need to provide a price for the commission.")]
        [Range(0, 99999999999999999999.99, ErrorMessage = "The price must be a positive number.")]
        public decimal Price { get; set; }

        public Guid CommissionerId { get; set; }
        public Guid ArtistId { get; set; }

        public User? Artist { get; set; }
    }
}
