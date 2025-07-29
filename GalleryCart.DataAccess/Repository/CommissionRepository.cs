using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly ApplicationDbContext _db;

        public CommissionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Commission> GetAllQueryable(Expression<Func<Commission, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Commission> query = _db.Commissions.Include(c => c.Commissioner).Include(c => c.Artist);
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

        public async Task<Commission?> GetAsync(Expression<Func<Commission, bool>> predicate)
        {
            return await _db.Commissions.Include(c => c.Commissioner).Include(c => c.Artist)
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching commission or null if none found
        }

        public async Task<bool> AddAsync(Commission entity)
        {
            _db.Commissions.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Commission entity)
        {
            _db.Commissions.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid commissionId)
        {
            var commission = await _db.Commissions.FindAsync(commissionId);
            if (commission == null)
            {
                return false; // Commission not found
            }

            _db.Commissions.Remove(commission);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Commission, bool>> predicate)
        {
            return await _db.Commissions.AnyAsync(predicate);
        }
    }
}
