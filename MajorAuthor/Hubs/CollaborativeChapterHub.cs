using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MajorAuthor.Hubs
{
    public class CollaborativeChapterHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserConnectionInfo> _connectedUsers = new();
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _roomConnections = new();

        public CollaborativeChapterHub() { }

        public async Task JoinChapterGroup(string chapterId, string penName, string color)
        {
            try
            {
                Console.WriteLine($"JoinChapterGroup: User {penName} ({Context.ConnectionId}) joining chapter {chapterId}");

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

                Console.WriteLine($"Room {chapterId} has {room.Count} users. Other users: {otherConnectionIds.Count}");

                if (otherConnectionIds.Count > 0)
                {
                    Console.WriteLine($"Requesting state from {otherConnectionIds[0]} for {Context.ConnectionId}");
                    await Clients.Client(otherConnectionIds[0]).SendAsync("RequestState", Context.ConnectionId);
                }
                else
                {
                    Console.WriteLine($"No other users in room, initializing from DB for {Context.ConnectionId}");
                    await Clients.Caller.SendAsync("InitializeFromContent");
                }

                if (otherConnectionIds.Count > 0)
                {
                    Console.WriteLine($"Notifying others about {penName} joining");
                    await Clients.GroupExcept(chapterId, Context.ConnectionId)
                        .SendAsync("UserJoined", new { penName, color });
                }

                Console.WriteLine($"User {penName} successfully joined chapter {chapterId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinChapterGroup for {Context.ConnectionId}: {ex.Message}");
                // Не бросаем исключение, чтобы не разрывать соединение
            }
        }

        public async Task SyncStateToUser(string targetConnectionId, string stateBase64)
        {
            try
            {
                Console.WriteLine($"SyncStateToUser: Sending state to {targetConnectionId}");
                await Clients.Client(targetConnectionId).SendAsync("ReceiveSyncState", stateBase64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SyncStateToUser: {ex.Message}");
            }
        }

        public async Task SendUpdate(string chapterId, string updateBase64)
        {
            try
            {
                Console.WriteLine($"SendUpdate: User {Context.ConnectionId} sending update to chapter {chapterId}");
                await Clients.GroupExcept(chapterId, Context.ConnectionId).SendAsync("ReceiveUpdate", updateBase64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendUpdate: {ex.Message}");
            }
        }

        public async Task SendAwarenessUpdate(string chapterId, string updateBase64)
        {
            try
            {
                Console.WriteLine($"SendAwarenessUpdate: User {Context.ConnectionId} sending awareness to chapter {chapterId}");
                await Clients.GroupExcept(chapterId, Context.ConnectionId).SendAsync("ReceiveAwarenessUpdate", updateBase64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendAwarenessUpdate: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                Console.WriteLine($"OnDisconnectedAsync: {Context.ConnectionId}, exception: {exception?.Message}");

                if (_connectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
                {
                    Console.WriteLine($"Removing user {userInfo.PenName} from room {userInfo.ChapterId}");

                    if (_roomConnections.TryGetValue(userInfo.ChapterId, out var room))
                    {
                        room.TryRemove(Context.ConnectionId, out _);

                        if (room.Count > 0)
                        {
                            Console.WriteLine($"Notifying {room.Count} users about {userInfo.PenName} leaving");
                            await Clients.Group(userInfo.ChapterId).SendAsync("UserLeft", new { penName = userInfo.PenName });
                        }

                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userInfo.ChapterId);

                        Console.WriteLine($"User {userInfo.PenName} left chapter {userInfo.ChapterId}. Remaining in room: {room.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }
        // Изменить метод SendImageUpdate:
        public async Task SendImageUpdate(string chapterId, string updateType, string dataJson)
        {
            try
            {
                Console.WriteLine($"SendImageUpdate: User {Context.ConnectionId} sending {updateType} to {chapterId}");

                // Исправление: используем Clients.Group(chapterId), но исключаем отправителя
                // Это у тебя уже сделано правильно: GroupExcept
                await Clients.GroupExcept(chapterId, Context.ConnectionId)
                    .SendAsync("ReceiveImageUpdate", updateType, dataJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendImageUpdate: {ex.Message}");
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