using AIChat.Models;
using OpenAI.Chat;
namespace AIChat.Service
{
    public class ChatService
    {
        private readonly string _sessionId;
        private readonly KimiClient _kimi;
        public ChatService(string sessionId, KimiClient kimi)
        {
            _sessionId = sessionId;
            _kimi = kimi;
        }

        /// <summary>
        /// 带上下文的一问一答（保存历史，读取历史）
        /// </summary>
        /// <param name="userInput">用户的输入消息</param>
        /// <returns></returns>
        public async Task<string> SendAsync(string userInput)
        {
            using var db = new ChatDbContext();

            //从数据库读取历史消息
            var historyMessages = db.Messages
                            .Where(m => m.SessionId == _sessionId)
                            .OrderBy(m => m.Created)
                            .Select(m =>
                                m.Role == "user"
                                    ? (ChatMessage)new UserChatMessage(m.Content)
                                    : new AssistantChatMessage(m.Content))
                            .ToList();


            historyMessages.Add(new UserChatMessage(userInput));

            Console.WriteLine($"[DEBUG] 会话 {_sessionId} 发送 {historyMessages.Count} 条消息到 Kimi");


            //把历史消息代入 调用 Kimi API 获取回复
            var reply = await _kimi.AskAsync(historyMessages);


            db.Messages.Add(new Message
            {
                SessionId = _sessionId,
                Role = "user",
                Content = userInput
            });

            db.Messages.Add(new Message
            {
                SessionId = _sessionId,
                Role = "assistant",
                Content = reply
            });

            await db.SaveChangesAsync();
            return reply;
        }

        /// <summary>
        /// 无上下文的一问一答（不保存历史，不读取历史）
        /// </summary>
        public async Task<string> AskWithoutContextAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                throw new ArgumentException("用户输入不能为空", nameof(userInput));
            }

            Console.WriteLine($"[DEBUG] 无上下文模式 - 直接调用 Kimi API");

            // 直接调用 API，不读取历史消息
            var reply = await _kimi.AskAsync(userInput);

            Console.WriteLine($"[DEBUG] 无上下文模式 - 收到回复，不保存到数据库");

            return reply;
        }
    }
}