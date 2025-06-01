using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalleryCart.Models.Models
{
    public class FavouritePost
    {
        // Only need to include in User, only care about which posts a user favorited not which users favorited a post
        public Guid UserId { get; set; }
        [ValidateNever]
        public virtual User User { get; set; }

        public Guid? PostId { get; set; }
        [ValidateNever]
        public virtual Post Post { get; set; }
    }
}
