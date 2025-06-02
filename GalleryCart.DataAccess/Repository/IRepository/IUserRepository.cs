using GalleryCart.Models.Models;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository.IRepository
{
    public interface IUserRepository
    {
        // AsNoTracking when readonly to improve performance
        // Just ToListAsync() in the controller
        IQueryable<User> GetAllQueryable(Expression<Func<User, bool>>? predicate = null, bool asNoTracking = true);
        Task<User?> GetAsync(Expression<Func<User, bool>> predicate);
        // Using bool to indicate success or failure of the operation
        Task<bool> AddAsync(User entity);
        Task<bool> UpdateAsync(User entity);
        Task<bool> DeleteAsync(Guid userId);
        // Check if an entity exists
        Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
    }
}
