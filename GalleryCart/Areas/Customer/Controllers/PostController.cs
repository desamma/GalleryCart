using GalleryCart.DataAccess;

using GalleryCart.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return View("~/Areas/Customer/Views/Post/Post.cshtml", post);
        }

        [HttpPost]
        public async Task<IActionResult> Like(Guid id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
                return NotFound();

            post.LikeCount += 1;
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
