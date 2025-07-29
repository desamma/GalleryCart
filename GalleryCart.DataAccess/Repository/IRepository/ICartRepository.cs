using System.Linq.Expressions;
using GalleryCart.Models.Models;

public interface ICartRepository
{
    Task<Cart?> GetAsync(Expression<Func<Cart, bool>> predicate);
    Task<bool> AddAsync(Cart entity);
    Task<bool> UpdateAsync(Cart entity);
    Task<bool> DeleteAsync(Guid cartId);
    Task<bool> ExistsAsync(Expression<Func<Cart, bool>> predicate);
    IQueryable<Cart> GetAllQueryable(Expression<Func<Cart, bool>>? predicate = null);
}