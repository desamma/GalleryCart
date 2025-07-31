using GalleryCart.DataAccess;
using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Read(Guid id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get Favourite count for this post
            var favCount = await _context.FavouritePosts
                .CountAsync(f => f.PostId == id);

            // Check if current user has already favourited
            bool isFavourited = false;
            if (!string.IsNullOrEmpty(userId))
            {
                isFavourited = await _context.FavouritePosts
                    .AnyAsync(f => f.PostId == id && f.UserId.ToString() == userId);
            }

            ViewBag.FavouriteCount = favCount;
            ViewBag.IsFavourited = isFavourited;

            return View("~/Areas/Customer/Views/Post/Post.cshtml", post);
        }

        [HttpPost]
        public async Task<IActionResult> Favourite(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var fav = await _context.FavouritePosts
                .FirstOrDefaultAsync(f => f.PostId == id && f.UserId == user.Id);

            if (fav != null)
            {
                // Unfavourite
                _context.FavouritePosts.Remove(fav);
            }
            else
            {
                // Favourite
                _context.FavouritePosts.Add(new FavouritePost
                {
                    PostId = id,
                    UserId = user.Id
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Read", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Guid id, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Read", new { id });

            var comment = new Comment
            {
                PostId = id,
                UserId = user.Id,
                Content = content,
                CommentDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Read", new { id });
        }
    }
}
