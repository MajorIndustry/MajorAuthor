using MajorAuthor.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static MajorAuthor.Services.AuthorService;

namespace MajorAuthor.Services
{
    public interface IAuthorService
    {
        Task<Author> GetAuthorByUserIdAsync(string userId);
        Task<Author> GetAuthorByIdAsync(int authorId);
        Task<(bool success, string message)> RegisterAuthorAsync(string userId, string penName);
        Task<(bool available, string message)> CheckPenNameAvailabilityAsync(string penName, string currentUserId = null);
        Task<(bool success, string message)> UpdatePenNameAsync(string userId, string newPenName);
        Task<List<Author>> SearchAuthorsAsync(string query);
        Task<(bool success, string message)> FollowAuthorAsync(string followerUserId, int authorId);
        Task<(bool success, string message)> UnfollowAuthorAsync(string followerUserId, int authorId);
        Task<bool> IsFollowingAsync(string followerUserId, int authorId);
        Task<int> GetFollowerCountAsync(int authorId);
        Task<(string userId, string userName)> GetFollowerInfoAsync(string followerUserId);
        Task<List<FollowerInfo>> GetAuthorFollowersAsync(int authorId);
    }
}