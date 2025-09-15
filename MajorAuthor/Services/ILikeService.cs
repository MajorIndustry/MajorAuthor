using MajorAuthor.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;

// Принцип разделения интерфейса (ISP).
// Этот интерфейс предназначен только для работы с лайками,
// отделяя эту функциональность от других сервисов.
namespace MajorAuthor.Services
{
    public interface ILikeService
    {
        Task<int> GetLikesCountForWorkAsync(int workId, string workType);
        Task<bool> AddLikeAsync(int workId, string userId, string workType);
        Task<bool> HasUserLikedWorkAsync(int workId, string userId, string workType);
    }
}
