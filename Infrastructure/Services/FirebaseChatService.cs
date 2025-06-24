using Core.Interfaces;
using Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FirebaseChatService : IChatService
    {
        // URL cơ sở của Firebase Realtime Database
        private const string FirebaseDbBaseUrl = "https://chatmessage-bfa21-default-rtdb.asia-southeast1.firebasedatabase.app/";
        private readonly IHttpClientFactory _clientFactory;

        public FirebaseChatService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Triển khai phương thức GetConversationHistoryAsync
        public async Task<IEnumerable<ChatMessage>> GetConversationHistoryAsync(string conversationId)
        {
            var client = _clientFactory.CreateClient();
            // Tạo URL cụ thể cho cuộc trò chuyện
            var url = $"{FirebaseDbBaseUrl}conversations/{conversationId}.json";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                // Xử lý lỗi nếu cần, ở đây trả về danh sách rỗng
                return new List<ChatMessage>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            // Nếu không có lịch sử, Firebase trả về "null"
            if (string.IsNullOrEmpty(jsonResponse) || jsonResponse.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return new List<ChatMessage>();
            }

            // Firebase trả về một Dictionary, với key là ID tự sinh và value là object tin nhắn
            var messagesDict = JsonConvert.DeserializeObject<Dictionary<string, ChatMessage>>(jsonResponse);

            // Chuyển đổi từ Dictionary sang List và sắp xếp theo thời gian
            return messagesDict.Values.OrderBy(m => m.Timestamp).ToList();
        }

        // Triển khai phương thức SaveMessageAsync với conversationId
        public async Task SaveMessageAsync(string conversationId, ChatMessage message)
        {
            var client = _clientFactory.CreateClient();
            // Tạo URL cụ thể để POST tin nhắn vào cuộc trò chuyện
            var url = $"{FirebaseDbBaseUrl}conversations/{conversationId}.json";

            var jsonMessage = JsonConvert.SerializeObject(message);
            var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            // Sử dụng POST để Firebase tự động tạo một key duy nhất cho mỗi tin nhắn
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode(); // Ném ra exception nếu request thất bại
        }
    }
}
