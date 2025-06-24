using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Hubs
{
    public class ChatHub : Hub
    {
        // Dùng Dictionary để theo dõi người dùng đang online
        public static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();

        // MỚI: Inject service để xử lý việc lưu trữ dữ liệu
        private readonly IChatService _chatService;

        // XÓA: Không còn lưu lịch sử trong bộ nhớ nữa
        // private static readonly ConcurrentDictionary<string, List<ChatMessage>> ConversationHistory = ...

        // MỚI: Constructor để nhận service thông qua Dependency Injection
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // Phương thức này sẽ được gọi bởi client ngay sau khi kết nối thành công
        public async Task RegisterUser(string username)
        {
            var connectionId = Context.ConnectionId;
            UserConnections[username] = connectionId;
            await BroadcastUserList();
        }

        // Gửi tin nhắn đến một người cụ thể và LƯU lại tin nhắn qua service
        public async Task SendPrivateMessage(string recipientUsername, string message)
        {
            var senderUsername = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (senderUsername == null) return;

            // MỚI: Dùng service để lưu tin nhắn vào Firebase
            var conversationId = GetConversationId(senderUsername, recipientUsername);
            var chatMessage = new ChatMessage { Sender = senderUsername, Message = message, Timestamp = DateTime.UtcNow };
            await _chatService.SaveMessageAsync(conversationId, chatMessage);

            // Gửi tin nhắn đến các client đang kết nối
            if (UserConnections.TryGetValue(recipientUsername, out var recipientConnectionId))
            {
                await Clients.Client(recipientConnectionId).SendAsync("ReceivePrivateMessage", senderUsername, message);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceivePrivateMessage", senderUsername, message);
            }
        }

        // MỚI: Phương thức để client yêu cầu lịch sử cuộc trò chuyện từ service
        public async Task GetConversationHistory(string otherUsername)
        {
            var currentUser = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (currentUser == null) return;

            var conversationId = GetConversationId(currentUser, otherUsername);

            // MỚI: Lấy lịch sử từ Firebase thông qua service
            var history = await _chatService.GetConversationHistoryAsync(conversationId);

            if (history != null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("LoadHistory", history);
            }
        }

        // Khi một người dùng ngắt kết nối
        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var user = UserConnections.FirstOrDefault(x => x.Value == connectionId);

            if (!user.Equals(default(KeyValuePair<string, string>)))
            {
                UserConnections.TryRemove(user.Key, out _);
                await BroadcastUserList();
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Phương thức trợ giúp để phát danh sách người dùng
        private async Task BroadcastUserList()
        {
            var userList = UserConnections.Keys.ToList();
            await Clients.All.SendAsync("UpdateUserList", userList);
        }

        // Phương thức trợ giúp để tạo ID cuộc trò chuyện duy nhất
        private string GetConversationId(string user1, string user2)
        {
            return string.Compare(user1, user2) < 0 ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }
    }
}
