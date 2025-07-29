using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class PostRepository : IPostRepository
    {
        public void AttachTag(Tag tag)
        {
            _db.Attach(tag); // Let EF know: this already exists
        }

        private readonly ApplicationDbContext _db;

        public PostRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Post> GetAllQueryable(Expression<Func<Post, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Post> query = _db.Posts.Include(p => p.Tags);
            if (asNoTracking)
            {
                query = query.AsNoTracking(); // Use AsNoTracking for read-only queries
            }
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public async Task<Post?> GetAsync(Expression<Func<Post, bool>> predicate)
        {
            return await _db.Posts.Include(p => p.Tags)
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching post or null if none found
        }

        public async Task<bool> AddAsync(Post entity)
        {
            _db.Posts.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Post entity)
        {
            _db.Posts.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid postId)
        {
            var post = await _db.Posts.FindAsync(postId);
            if (post == null)
            {
                return false; // Post not found
            }

            _db.Posts.Remove(post);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Post, bool>> predicate)
        {
            return await _db.Posts.AnyAsync(predicate);
        }
    }
}