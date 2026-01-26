using MajorAuthor.Data;
using MajorAuthor.Services;
using Microsoft.EntityFrameworkCore;

namespace MajorAuthor.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly MajorAuthorDbContext _context;

        public AuthorizationService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsAuthorAsync(string userId)
        {
            return await _context.Authors
                .AnyAsync(a => a.ApplicationUserId == userId);
        }

        public async Task<bool> IsUserAuthorizedAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return false;
            return await IsAuthorAsync(userId);
        }
    }
}