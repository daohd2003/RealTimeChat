using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IChatService
    {
        // Sửa lại phương thức này để nhận conversationId
        Task SaveMessageAsync(string conversationId, ChatMessage message);

        // THÊM MỚI: Phương thức để lấy lịch sử cuộc trò chuyện
        Task<IEnumerable<ChatMessage>> GetConversationHistoryAsync(string conversationId);
    }
}
