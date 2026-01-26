using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using MajorAuthor.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MajorAuthor.Controllers.Api
{
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
            if (string.IsNullOrEmpty(email) || email.Length < 2)
            {
                return Ok(new object[0]);
            }

            // Получаем ID текущего пользователя
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var usersQuery = _context.Users
                .Where(u => (u.Email.Contains(email) || u.UserName.Contains(email)));

            // Исключаем текущего пользователя из результатов
            if (!string.IsNullOrEmpty(currentUserId))
            {
                usersQuery = usersQuery.Where(u => u.Id != currentUserId);
            }

            var users = await usersQuery
                .Select(u => new
                {
                    id = u.Id,
                    email = u.Email,
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