using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class Commission
    {
        public Guid CommissionId { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "You need to provide a due date for the commission.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime DueDate { get; set; } // Commisioner sets the due date for the commission

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "You need to provide a price for the commission.")]
        [Range(0, 99999999999999999999.99, ErrorMessage = "The price must be a positive number.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "You need to provide a status for the commission.")]
        //[MaxLength(50, ErrorMessage = "The status must be at most 50 characters.")]
        public string Status { get; set; } = "Pending"; // Default status is "Pending", "Rejected", "In Progress", "Completed"

        public Guid CommissionerId { get; set; }
        [ValidateNever]
        public virtual User Commissioner { get; set; }

        public Guid ArtistId { get; set; }
        [ValidateNever]
        public virtual User Artist { get; set; }
    }
}
