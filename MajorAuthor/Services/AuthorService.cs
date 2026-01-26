using MajorAuthor.Data;
using MajorAuthor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MajorAuthor.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly MajorAuthorDbContext _context;

        public AuthorService(MajorAuthorDbContext context)
        {
            _context = context;
        }

        public async Task<Author> GetAuthorByUserIdAsync(string userId)
        {
            return await _context.Authors
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);
        }

        public async Task<Author> GetAuthorByIdAsync(int authorId)
        {
            return await _context.Authors
                .Include(a => a.Followers)
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(a => a.Id == authorId);
        }

        /// <summary>
        /// Регистрация автора с проверкой уникальности псевдонима
        /// </summary>
        public async Task<(bool success, string message)> RegisterAuthorAsync(string userId, string penName)
        {
            try
            {
                // Проверяем, не зарегистрирован ли пользователь уже как автор
                var existingAuthor = await GetAuthorByUserIdAsync(userId);
                if (existingAuthor != null)
                {
                    return (false, "Вы уже зарегистрированы как автор.");
                }

                // Проверяем уникальность псевдонима (без учета регистра)
                var normalizedPenName = penName.Trim().ToLower();
                var penNameExists = await _context.Authors
                    .AnyAsync(a => a.PenName.ToLower() == normalizedPenName);

                if (penNameExists)
                {
                    return (false, "Этот псевдоним уже занят. Пожалуйста, выберите другой.");
                }

                // Создаем нового автора
                var newAuthor = new Author
                {
                    ApplicationUserId = userId,
                    PenName = penName.Trim(),
                    AuthorProfileCreationDate = DateTime.UtcNow,
                    Description = string.Empty
                };

                _context.Authors.Add(newAuthor);
                await _context.SaveChangesAsync();

                return (true, "Поздравляем, вы стали автором! Теперь вы можете создавать произведения.");
            }
            catch (DbUpdateException dbEx)
            {
                // Обработка ошибок уникальности на уровне БД
                if (dbEx.InnerException?.Message?.Contains("IX_Authors_PenName") == true ||
                    dbEx.InnerException?.Message?.Contains("unique") == true)
                {
                    return (false, "Этот псевдоним уже занят. Пожалуйста, выберите другой.");
                }

                return (false, $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Произошла ошибка при регистрации: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверка доступности псевдонима
        /// </summary>
        public async Task<(bool available, string message)> CheckPenNameAvailabilityAsync(string penName, string currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(penName))
            {
                return (false, "Псевдоним не может быть пустым.");
            }

            if (penName.Length < 2)
            {
                return (false, "Псевдоним должен содержать минимум 2 символа.");
            }

            if (penName.Length > 50)
            {
                return (false, "Псевдоним не может превышать 50 символов.");
            }

            var normalizedPenName = penName.Trim().ToLower();

            // Если указан currentUserId, проверяем возможность обновления для существующего автора
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var currentAuthor = await GetAuthorByUserIdAsync(currentUserId);
                if (currentAuthor != null && currentAuthor.PenName.ToLower() == normalizedPenName)
                {
                    return (true, "Это ваш текущий псевдоним.");
                }
            }

            var penNameExists = await _context.Authors
                .AnyAsync(a => a.PenName.ToLower() == normalizedPenName);

            if (penNameExists)
            {
                return (false, "Этот псевдоним уже занят.");
            }

            return (true, "Псевдоним доступен.");
        }

        /// <summary>
        /// Обновление псевдонима автора
        /// </summary>
        public async Task<(bool success, string message)> UpdatePenNameAsync(string userId, string newPenName)
        {
            try
            {
                var author = await GetAuthorByUserIdAsync(userId);
                if (author == null)
                {
                    return (false, "Автор не найден.");
                }

                // Проверяем доступность нового псевдонима
                var availabilityCheck = await CheckPenNameAvailabilityAsync(newPenName, userId);
                if (!availabilityCheck.available)
                {
                    return (false, availabilityCheck.message);
                }

                author.PenName = newPenName.Trim();
                await _context.SaveChangesAsync();

                return (true, "Псевдоним успешно обновлен.");
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.Message?.Contains("IX_Authors_PenName") == true)
                {
                    return (false, "Этот псевдоним уже занят. Пожалуйста, выберите другой.");
                }

                return (false, $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Произошла ошибка при обновлении: {ex.Message}");
            }
        }

        // Остальные методы остаются без изменений...
        public async Task<List<Author>> SearchAuthorsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Author>();
            }

            var lowerQuery = query.ToLower();

            return await _context.Authors
                .Include(a => a.Followers)
                .Include(a => a.ApplicationUser)
                .Where(a => a.PenName.ToLower().Contains(lowerQuery) ||
                            (a.ApplicationUser.FirstName != null && a.ApplicationUser.FirstName.ToLower().Contains(lowerQuery)) ||
                            (a.ApplicationUser.LastName != null && a.ApplicationUser.LastName.ToLower().Contains(lowerQuery)) ||
                            (a.ApplicationUser.UserName != null && a.ApplicationUser.UserName.ToLower().Contains(lowerQuery)))
                .ToListAsync();
        }

        public async Task<(bool success, string message)> FollowAuthorAsync(string followerUserId, int authorId)
        {
            try
            {
                var author = await _context.Authors
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == authorId);

                if (author == null)
                    return (false, "Автор не найден");

                var authorUser = await _context.Authors
                    .Where(a => a.Id == authorId)
                    .Select(a => a.ApplicationUserId)
                    .FirstOrDefaultAsync();

                if (authorUser == followerUserId)
                    return (false, "Нельзя подписаться на самого себя");

                var existingFollow = await _context.Followers
                    .FirstOrDefaultAsync(f => f.FollowerApplicationUserId == followerUserId && f.AuthorId == authorId);

                if (existingFollow != null)
                    return (true, "Уже подписан");

                var follow = new Follower
                {
                    FollowerApplicationUserId = followerUserId,
                    AuthorId = authorId,
                    FollowDate = DateTime.UtcNow
                };

                _context.Followers.Add(follow);
                await _context.SaveChangesAsync();

                return (true, "Успешная подписка");
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.Message?.Contains("UNIQUE") == true ||
                    dbEx.InnerException?.Message?.Contains("unique") == true ||
                    dbEx.InnerException?.Message?.Contains("duplicate") == true)
                {
                    return (true, "Уже подписан");
                }
                return (false, $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при подписке: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UnfollowAuthorAsync(string followerUserId, int authorId)
        {
            try
            {
                var follow = await _context.Followers
                    .FirstOrDefaultAsync(f => f.FollowerApplicationUserId == followerUserId && f.AuthorId == authorId);

                if (follow != null)
                {
                    _context.Followers.Remove(follow);
                    await _context.SaveChangesAsync();
                    return (true, "Успешная отписка");
                }
                return (true, "Подписка не найдена");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при отписке: {ex.Message}");
            }
        }

        public async Task<bool> IsFollowingAsync(string followerUserId, int authorId)
        {
            try
            {
                return await _context.Followers
                    .AnyAsync(f => f.FollowerApplicationUserId == followerUserId && f.AuthorId == authorId);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetFollowerCountAsync(int authorId)
        {
            try
            {
                return await _context.Followers
                    .AsNoTracking()
                    .Where(f => f.AuthorId == authorId)
                    .CountAsync();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<(string userId, string userName)> GetFollowerInfoAsync(string followerUserId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == followerUserId)
                    .Select(u => new { u.Id, u.UserName })
                    .FirstOrDefaultAsync();

                return user != null ? (user.Id, user.UserName) : (followerUserId, "Пользователь");
            }
            catch (Exception)
            {
                return (followerUserId, "Пользователь");
            }
        }
        /// <summary>
        /// Получение списка подписчиков автора
        /// </summary>
        public async Task<List<FollowerInfo>> GetAuthorFollowersAsync(int authorId)
        {
            try
            {
                return await _context.Followers
                    .Where(f => f.AuthorId == authorId)
                    .Include(f => f.FollowerApplicationUser)
                    .Select(f => new FollowerInfo
                    {
                        UserId = f.FollowerApplicationUserId,
                        UserName = f.FollowerApplicationUser.UserName
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<FollowerInfo>();
            }
        }

        public class FollowerInfo
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
        }
    }
}