using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _db;

        public TagRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Tag> GetAllQueryable(Expression<Func<Tag, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Tag> query = _db.Tags;
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

        public async Task<Tag?> GetAsync(Expression<Func<Tag, bool>> predicate)
        {
            return await _db.Tags
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching tag or null if none found
        }

        public async Task<bool> AddAsync(Tag entity)
        {
            _db.Tags.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Tag entity)
        {
            _db.Tags.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid tagId)
        {
            var tag = await _db.Tags.FindAsync(tagId);
            if (tag == null)
            {
                return false; // Tag not found
            }

            _db.Tags.Remove(tag);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Tag, bool>> predicate)
        {
            return await _db.Tags.AnyAsync(predicate);
        }
    }
}
