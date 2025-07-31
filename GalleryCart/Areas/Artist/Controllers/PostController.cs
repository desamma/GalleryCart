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
        private readonly IFavouritePostRepository _favouriteRepo;
        private readonly CloudinaryUploader _cloudinaryUploader;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PostController(
            IPostRepository postRepository,
            ITagRepository tagRepository,
            IFavouritePostRepository favouriteRepo,
            CloudinaryUploader cloudinaryUploader,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _favouriteRepo = favouriteRepo;
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var post = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.PostId == id && p.UserId.ToString() == userId);

            if (post == null) return NotFound();

            ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
            ViewBag.SelectedTags = post.Tags.Select(t => t.TagId).ToList();

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Post model, List<IFormFile> files, List<Guid> SelectedTags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var existingPost = await _context.Posts
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.PostId == id && p.UserId.ToString() == userId);
            if (existingPost == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Upload lại ảnh nếu có
                if (files != null && files.Count > 0)
                {
                    existingPost.Path = await _cloudinaryUploader.UploadMultiImagesAsync(files);
                }

                // Cập nhật các field
                existingPost.Title = model.Title;
                existingPost.Description = model.Description;
                existingPost.Price = model.Price;
                existingPost.IsImage = model.IsImage;
                existingPost.IsMature = model.IsMature;
                existingPost.IsPorfolio = model.IsPorfolio;
                existingPost.PostDate = DateTime.Now;

                // Xóa và cập nhật lại tags
                // Xóa tags cũ
                existingPost.Tags.Clear();

                // Gán lại tag từ database
                foreach (var tagId in SelectedTags)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        existingPost.Tags.Add(tag);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home", new { area = "Artist" });
            }

            ViewBag.Tags = await _tagRepository.GetAllQueryable().ToListAsync();
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isFavourited = false;
            int favouriteCount = await _favouriteRepo.GetAllQueryable(fp => fp.PostId == id).CountAsync();

            if (!string.IsNullOrEmpty(userId))
            {
                isFavourited = await _favouriteRepo.ExistsAsync(fp => fp.UserId.ToString() == userId && fp.PostId == id);
            }

            ViewBag.FavouriteCount = favouriteCount;
            ViewBag.IsFavourited = isFavourited;

            return View("~/Areas/Artist/Views/Post/Post.cshtml", post);
        }

        [HttpPost]
        public async Task<IActionResult> Favourite(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _favouriteRepo.GetAsync(fp => fp.UserId.ToString() == userId && fp.PostId == id);
            if (existing != null)
            {
                _context.FavouritePosts.Remove(existing);
                await _context.SaveChangesAsync();
            }
            else
            {
                var fav = new FavouritePost
                {
                    UserId = Guid.Parse(userId),
                    PostId = id
                };
                await _favouriteRepo.AddAsync(fav);
            }

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
