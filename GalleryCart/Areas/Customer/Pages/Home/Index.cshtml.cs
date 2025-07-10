using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace GalleryCart.Areas.Customer.Pages.Home
{
    //[Authorize(Roles = "Admin,user")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public IndexModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public required User CurrentUser { get; set; }
        public User Artist { get; set; }
        public async Task OnGetAsync()
        {
            var userJson = HttpContext.Session.GetString("CurrentUser");

            if (!string.IsNullOrEmpty(userJson))
            {
                CurrentUser = JsonConvert.DeserializeObject<User>(userJson);
            }
            Artist = await _userManager.FindByEmailAsync("artist@gmail.com");
        }
    }
}
