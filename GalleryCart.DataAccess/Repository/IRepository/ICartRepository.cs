using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public interface ICartRepository
{
    Task<Cart?> GetAsync(
        Expression<Func<Cart, bool>> predicate,
        Func<IQueryable<Cart>, IIncludableQueryable<Cart, object>>? include = null
    );

    Task<bool> AddAsync(Cart entity);
    Task<bool> UpdateAsync(Cart entity);
    Task<bool> DeleteAsync(Guid cartId);
    Task<bool> ExistsAsync(Expression<Func<Cart, bool>> predicate);
    IQueryable<Cart> GetAllQueryable(Expression<Func<Cart, bool>>? predicate = null);
}