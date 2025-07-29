using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly ApplicationDbContext _db;

        public HistoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<History> GetAllQueryable(Expression<Func<History, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<History> query = _db.Histories;
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

        public async Task<History?> GetAsync(Expression<Func<History, bool>> predicate)
        {
            return await _db.Histories
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching history or null if none found
        }

        public async Task<bool> AddAsync(History entity)
        {
            _db.Histories.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(History entity)
        {
            _db.Histories.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid historyId)
        {
            var history = await _db.Histories.FindAsync(historyId);
            if (history == null)
            {
                return false; 
            }

            _db.Histories.Remove(history);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<History, bool>> predicate)
        {
            return await _db.Histories.AnyAsync(predicate);
        }
    }
}
