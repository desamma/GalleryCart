using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Comment> GetAllQueryable(Expression<Func<Comment, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Comment> query = _db.Comments;
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

        public async Task<Comment?> GetAsync(Expression<Func<Comment, bool>> predicate)
        {
            return await _db.Comments
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching comment or null if none found
        }

        public async Task<bool> AddAsync(Comment entity)
        {
            _db.Comments.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Comment entity)
        {
            _db.Comments.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid commentId)
        {
            var comment = await _db.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return false; // Comment not found
            }

            _db.Comments.Remove(comment);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Comment, bool>> predicate)
        {
            return await _db.Comments.AnyAsync(predicate);
        }
    }
}
