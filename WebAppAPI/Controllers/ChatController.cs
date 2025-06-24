using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs; // Cần using đến Hubs để truy cập UserConnections
using System;
using System.Threading.Tasks;

namespace WebAppAPI.Controllers
{
    // Lớp DTO (Data Transfer Object) để nhận dữ liệu từ client
    // Vì ChatMessage không có người nhận, chúng ta cần một model mới cho request
    public class SendMessageRequest
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Message { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService _chatService;

        public ChatController(IHubContext<ChatHub> hubContext, IChatService chatService)
        {
            _hubContext = hubContext;
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                // 1. Tạo conversationId và đối tượng ChatMessage để lưu trữ
                var conversationId = GetConversationId(request.Sender, request.Recipient);
                var chatMessage = new ChatMessage
                {
                    Sender = request.Sender,
                    Message = request.Message,
                    Timestamp = DateTime.UtcNow
                };

                // 2. Dùng service để lưu tin nhắn vào Firebase
                await _chatService.SaveMessageAsync(conversationId, chatMessage);

                // 3. Gửi tin nhắn đến các client đang kết nối
                // LƯU Ý: Để làm được việc này từ Controller, chúng ta phải có cách
                // lấy ConnectionId của người nhận. Ở đây, chúng ta sẽ truy cập vào
                // danh sách UserConnections của ChatHub.

                // Gửi cho người nhận
                if (ChatHub.UserConnections.TryGetValue(request.Recipient, out var recipientConnectionId))
                {
                    await _hubContext.Clients.Client(recipientConnectionId).SendAsync("ReceivePrivateMessage", request.Sender, request.Message);
                }

                // Gửi cho người gửi
                if (ChatHub.UserConnections.TryGetValue(request.Sender, out var senderConnectionId))
                {
                    await _hubContext.Clients.Client(senderConnectionId).SendAsync("ReceivePrivateMessage", request.Sender, request.Message);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để dễ dàng debug
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Có lỗi xảy ra khi gửi tin nhắn.");
            }
        }

        // Phương thức trợ giúp để tạo ID cuộc trò chuyện duy nhất
        private string GetConversationId(string user1, string user2)
        {
            return string.Compare(user1, user2) < 0 ? $"{user1}-{user2}" : $"{user2}-{user1}";
        }
    }
}
