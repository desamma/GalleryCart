using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace GalleryCart.Models.Models
{
    public class Chat
    {
        public Guid ChatId { get; set; }

        public string Message { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{HH:mm dd/MM/yyyy}")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Don't know if we need this :v
        //public bool IsRead { get; set; } = false;

        public Guid? SenderId { get; set; }
        [ValidateNever]
        public virtual User Sender { get; set; }

        public Guid? ReceiverId { get; set; }
        [ValidateNever]
        public virtual User Receiver { get; set; }
    }
}
