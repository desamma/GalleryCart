using GalleryCart.DataAccess;
using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using GalleryCart.Utilities.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GalleryCart.Areas.Artist.Controllers
{
    [Area("Artist")]
    [Authorize(Roles = "artist")]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PostController(
            IPostRepository postRepository,
            ITagRepository tagRepository,
            CloudinaryUploader cloudinaryUploader,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _cloudinaryUploader = cloudinaryUploader;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post model, List<IFormFile> files, List<Guid> SelectedTags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (ModelState.IsValid)
            {
                var postId = Guid.NewGuid();
                var imageUrls = await _cloudinaryUploader.UploadMultiImagesAsync(files);
                if (string.IsNullOrWhiteSpace(imageUrls))
                {
                    ModelState.AddModelError("", "Image upload failed.");
                    ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
                    return View(model);
                }

                var newPost = new Post
                {
                    PostId = postId,
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    IsImage = model.IsImage,
                    IsMature = model.IsMature,
                    IsPorfolio = model.IsPorfolio,
                    Path = imageUrls,
                    PostDate = DateTime.Now,
                    UserId = Guid.Parse(userId),
                    Tags = new List<Tag>()
                };

                foreach (var tagId in SelectedTags)
                {
                    var tag = new Tag { TagId = tagId };
                    _postRepository.AttachTag(tag);
                    newPost.Tags.Add(tag);
                }

                await _postRepository.AddAsync(newPost);
                return RedirectToAction("Index", "Home", new { area = "Artist" });
            }

            ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Post model, List<IFormFile> files, List<Guid> SelectedTags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            if (id != model.PostId) return BadRequest();

            if (ModelState.IsValid)
            {
                string imageUrls = model.Path;
                if (files != null && files.Count > 0)
                {
                    imageUrls = await _cloudinaryUploader.UploadMultiImagesAsync(files);
                }

                var newPost = new Post
                {
                    PostId = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    IsImage = model.IsImage,
                    IsMature = model.IsMature,
                    IsPorfolio = model.IsPorfolio,
                    Path = imageUrls,
                    PostDate = DateTime.Now,
                    UserId = Guid.Parse(userId),
                    Tags = new List<Tag>()
                };

                foreach (var tagId in SelectedTags)
                {
                    var tag = new Tag { TagId = tagId };
                    _postRepository.AttachTag(tag);
                    newPost.Tags.Add(tag);
                }

                await _postRepository.AddAsync(newPost);
                await _postRepository.DeleteAsync(id);

                return RedirectToAction("Index", "Home", new { area = "Artist" });
            }

            ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
            ViewBag.SelectedTags = SelectedTags;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var post = await _postRepository.GetAsync(p => p.PostId == id && p.UserId.ToString() == userId);
            if (post == null) return NotFound();

            await _postRepository.DeleteAsync(id);
            return RedirectToAction("Index", "Home", new { area = "Artist" });
        }

        [AllowAnonymous]
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

            return View("~/Areas/Artist/Views/Post/Post.cshtml", post);
        }

        [AllowAnonymous]
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
        [ValidateAntiForgeryToken]
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
