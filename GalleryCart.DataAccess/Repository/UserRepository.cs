using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<User> GetAllQueryable(Expression<Func<User, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<User> query = _db.Users;
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

        public async Task<User?> GetAsync(Expression<Func<User, bool>> predicate)
        {
            return await _db.Users
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching user or null if none found
        }

        public async Task<bool> AddAsync(User entity)
        {
            _db.Users.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(User entity)
        {
            _db.Users.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return false; // User not found
            }

            _db.Users.Remove(user);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
        {
            return await _db.Users.AnyAsync(predicate);
        }
    }
}
