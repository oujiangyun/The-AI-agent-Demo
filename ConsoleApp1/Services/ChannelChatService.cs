using System.Text;



namespace AIChat.Service
{

    /// <summary>
    /// 处理频道聊天的服务
    /// </summary>
    public class ChannelChatService
    {
        private readonly SessionSlot _slot;
        private readonly CancellationToken _ct;

        public ChannelChatService(SessionSlot slot, CancellationToken ct)
        {
            _slot = slot;
            _ct = ct;
        }

        public async Task RunAsync()
        {
            await foreach (var userInput in _slot.Inbox.Reader.ReadAllAsync(_ct))
            {

                try
                {
                    Console.WriteLine($"[DEBUG] 用户输入：{userInput}");
                }
                catch
                {

                    var bytes = Encoding.UTF8.GetBytes(userInput);
                    var safeString = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine($"[DEBUG] 用户输入（转换后）：{safeString}");
                }

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    var reply = await _slot.Service.SendAsync(userInput);
                    Console.WriteLine($"{_slot.Id}: {reply}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{_slot.Id} Error: {ex.Message}");
                }
            }
            Console.WriteLine($"会话 {_slot.Id} 结束");
        }
    }
}