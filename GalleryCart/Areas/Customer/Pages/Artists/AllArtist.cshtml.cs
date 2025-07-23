using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GalleryCart.Areas.Customer.Pages.Artists
{
    [Authorize(Roles = "user")]
    public class AllArtistModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        public AllArtistModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public required IQueryable<User> Artist { get; set; }
        public void OnGet()
        {
            Artist = _userRepository.GetAllQueryable()
                .Where(u => u.IsArtits)
                .OrderByDescending(u => u.NormalizedUserName);
        }
    }
}
