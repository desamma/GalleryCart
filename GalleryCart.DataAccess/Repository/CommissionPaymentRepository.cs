using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class CommissionPaymentRepository : ICommissionPaymentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommissionPaymentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<CommissionPayment> GetAllQueryable(Expression<Func<CommissionPayment, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<CommissionPayment> query = _db.CommissionPayments;
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

        public async Task<CommissionPayment?> GetAsync(Expression<Func<CommissionPayment, bool>> predicate)
        {
            return await _db.CommissionPayments
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching CommissionPayment or null if none found
        }

        public async Task<bool> AddAsync(CommissionPayment entity)
        {
            _db.CommissionPayments.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(CommissionPayment entity)
        {
            _db.CommissionPayments.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid CommissionPaymentId)
        {
            var CommissionPayment = await _db.CommissionPayments.FindAsync(CommissionPaymentId);
            if (CommissionPayment == null)
            {
                return false; // CommissionPayment not found
            }

            _db.CommissionPayments.Remove(CommissionPayment);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<CommissionPayment, bool>> predicate)
        {
            return await _db.CommissionPayments.AnyAsync(predicate);
        }
    }
}
