using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MajorAuthorDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorService _authorService;

        public UsersController(MajorAuthorDbContext context, UserManager<ApplicationUser> userManager, IAuthorService authorService)
        {
            _context = context;
            _userManager = userManager;
            _authorService = authorService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string email)
        {
            var query = email?.Trim();
            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                return Ok(Array.Empty<object>());
            }

            // Получаем ID текущего пользователя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var normalizedEmail = _userManager.NormalizeEmail(query);

            var usersQuery = _context.Users
                .AsNoTracking()
                .Where(u =>
                    (u.UserName != null && u.UserName.Contains(query)) ||
                    (u.AuthorProfile != null && u.AuthorProfile.PenName != null && u.AuthorProfile.PenName.Contains(query)) ||
                    (u.NormalizedEmail != null && u.NormalizedEmail == normalizedEmail));

            // Исключаем текущего пользователя из результатов
            if (!string.IsNullOrEmpty(currentUserId))
            {
                usersQuery = usersQuery.Where(u => u.Id != currentUserId);
            }

            var users = await usersQuery
                .Select(u => new
                {
                    id = u.Id,
                    userName = u.UserName,
                    hasAuthorProfile = u.AuthorProfile != null,
                    penName = u.AuthorProfile != null ? u.AuthorProfile.PenName : null,
                    photoUrl = u.ProfilePictureUrl
                })
                .Take(10)
                .ToListAsync();

            return Ok(users);
        }
    }
}