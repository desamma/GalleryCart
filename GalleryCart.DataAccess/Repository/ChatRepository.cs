using GalleryCart.DataAccess.Repository.IRepository;
using GalleryCart.Models.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GalleryCart.DataAccess.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _db;

        public ChatRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IQueryable<Chat> GetAllQueryable(Expression<Func<Chat, bool>>? predicate = null, bool asNoTracking = true)
        {
            IQueryable<Chat> query = _db.Chats;
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

        public async Task<Chat?> GetAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _db.Chats
                .AsNoTracking() // Use AsNoTracking for read-only queries
                .FirstOrDefaultAsync(predicate); // Return the first matching chat or null if none found
        }

        public async Task<bool> AddAsync(Chat entity)
        {
            _db.Chats.Add(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> UpdateAsync(Chat entity)
        {
            _db.Chats.Update(entity);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> DeleteAsync(Guid chatId)
        {
            var chat = await _db.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return false; // Chat not found
            }

            _db.Chats.Remove(chat);
            return await _db.SaveChangesAsync() > 0; // Return true if any changes were made
        }

        public async Task<bool> ExistsAsync(Expression<Func<Chat, bool>> predicate)
        {
            return await _db.Chats.AnyAsync(predicate);
        }
    }
}
