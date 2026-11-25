using MajorAuthor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MajorAuthor.Hubs
{
    [Authorize]
    public class CollaborativeChapterHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserConnectionInfo> _connectedUsers = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _roomConnections = new();

        private readonly IDbContextFactory<MajorAuthorDbContext> _dbContextFactory;
        private readonly ILogger<CollaborativeChapterHub> _logger;

        public CollaborativeChapterHub(
            IDbContextFactory<MajorAuthorDbContext> dbContextFactory,
            ILogger<CollaborativeChapterHub> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        private async Task EnsureChapterAccessAsync(string chapterId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("Пользователь не авторизован.");
            }

            if (!int.TryParse(chapterId, out var parsedChapterId))
            {
                throw new HubException("Некорректный идентификатор главы.");
            }

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            var hasAccess = await db.Chapters
                .AsNoTracking()
                .Where(c => c.Id == parsedChapterId)
                .AnyAsync(c => c.Book.BookAuthors.Any(ba => ba.Author.ApplicationUserId == userId));

            if (!hasAccess)
            {
                _logger.LogWarning("Access denied for user {UserId} to chapter {ChapterId}", userId, chapterId);
                throw new HubException("Доступ к редактированию этой главы запрещён.");
            }
        }

        public async Task JoinChapterGroup(string chapterId, string penName, string color)
        {
            await EnsureChapterAccessAsync(chapterId);

            try
            {
                _logger.LogInformation("JoinChapterGroup: User {PenName} ({ConnectionId}) joining chapter {ChapterId}", penName, Context.ConnectionId, chapterId);

                await Groups.AddToGroupAsync(Context.ConnectionId, chapterId);

                var userInfo = new UserConnectionInfo
                {
                    ConnectionId = Context.ConnectionId,
                    PenName = penName,
                    Color = color,
                    ChapterId = chapterId,
                    JoinTime = DateTime.UtcNow
                };
                _connectedUsers[Context.ConnectionId] = userInfo;

                var room = _roomConnections.GetOrAdd(chapterId, _ => new ConcurrentDictionary<string, bool>());
                room.TryAdd(Context.ConnectionId, true);

                var otherConnectionIds = room.Keys.Where(k => k != Context.ConnectionId).ToList();

                _logger.LogDebug("Room {ChapterId} has {RoomCount} users. Other users: {OtherCount}", chapterId, room.Count, otherConnectionIds.Count);

                if (otherConnectionIds.Count > 0)
                {
                    _logger.LogDebug("Requesting state from {SourceConnectionId} for {TargetConnectionId}", otherConnectionIds[0], Context.ConnectionId);
                    await Clients.Client(otherConnectionIds[0]).SendAsync("RequestState", Context.ConnectionId);
                }
                else
                {
                    _logger.LogDebug("No other users in room, initializing from DB for {ConnectionId}", Context.ConnectionId);
                    await Clients.Caller.SendAsync("InitializeFromContent");
                }

                if (otherConnectionIds.Count > 0)
                {
                    _logger.LogDebug("Notifying others about {PenName} joining", penName);
                    await Clients.GroupExcept(chapterId, Context.ConnectionId)
                        .SendAsync("UserJoined", new { penName, color });
                }

                _logger.LogInformation("User {PenName} successfully joined chapter {ChapterId}", penName, chapterId);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinChapterGroup for {ConnectionId}", Context.ConnectionId);
            }
        }

        public async Task SyncStateToUser(string targetConnectionId, string stateBase64)
        {
            if (!_connectedUsers.TryGetValue(Context.ConnectionId, out var senderInfo))
            {
                throw new HubException("Соединение не зарегистрировано в комнате главы.");
            }

            await EnsureChapterAccessAsync(senderInfo.ChapterId);

            if (!_connectedUsers.TryGetValue(targetConnectionId, out var targetInfo) ||
                targetInfo.ChapterId != senderInfo.ChapterId)
            {
                throw new HubException("Целевой пользователь не находится в этой комнате главы.");
            }

            try
            {
                _logger.LogDebug("SyncStateToUser: Sending state to {TargetConnectionId}", targetConnectionId);
                await Clients.Client(targetConnectionId).SendAsync("ReceiveSyncState", stateBase64);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SyncStateToUser");
            }
        }

        public async Task SendUpdate(string chapterId, string updateBase64)
        {
            await EnsureChapterAccessAsync(chapterId);

            try
            {
                _logger.LogDebug("SendUpdate: User {ConnectionId} sending update to chapter {ChapterId}", Context.ConnectionId, chapterId);
                await Clients.GroupExcept(chapterId, Context.ConnectionId).SendAsync("ReceiveUpdate", updateBase64);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendUpdate");
            }
        }

        public async Task SendAwarenessUpdate(string chapterId, string updateBase64)
        {
            await EnsureChapterAccessAsync(chapterId);

            try
            {
                _logger.LogDebug("SendAwarenessUpdate: User {ConnectionId} sending awareness to chapter {ChapterId}", Context.ConnectionId, chapterId);
                await Clients.GroupExcept(chapterId, Context.ConnectionId).SendAsync("ReceiveAwarenessUpdate", updateBase64);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendAwarenessUpdate");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                _logger.LogInformation(exception, "OnDisconnectedAsync: {ConnectionId}", Context.ConnectionId);

                if (_connectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
                {
                    _logger.LogInformation("Removing user {PenName} from room {ChapterId}", userInfo.PenName, userInfo.ChapterId);

                    if (_roomConnections.TryGetValue(userInfo.ChapterId, out var room))
                    {
                        room.TryRemove(Context.ConnectionId, out _);

                        if (room.IsEmpty)
                        {
                            _roomConnections.TryRemove(KeyValuePair.Create(userInfo.ChapterId, room));
                        }
                        else
                        {
                            _logger.LogInformation("Notifying {RoomCount} users about {PenName} leaving", room.Count, userInfo.PenName);
                            await Clients.Group(userInfo.ChapterId).SendAsync("UserLeft", new { penName = userInfo.PenName });
                        }

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userInfo.ChapterId);

                        _logger.LogInformation("User {PenName} left chapter {ChapterId}. Remaining in room: {RoomCount}", userInfo.PenName, userInfo.ChapterId, room.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendImageUpdate(string chapterId, string updateType, string dataJson)
        {
            await EnsureChapterAccessAsync(chapterId);

            try
            {
                _logger.LogDebug("SendImageUpdate: User {ConnectionId} sending {UpdateType} to {ChapterId}", Context.ConnectionId, updateType, chapterId);

                await Clients.GroupExcept(chapterId, Context.ConnectionId)
                    .SendAsync("ReceiveImageUpdate", updateType, dataJson);
            }
            catch (HubException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendImageUpdate");
            }
        }
    }

    public class UserConnectionInfo
    {
        public string ConnectionId { get; set; }
        public string PenName { get; set; }
        public string Color { get; set; }
        public string ChapterId { get; set; }
        public DateTime JoinTime { get; set; }
    }
}