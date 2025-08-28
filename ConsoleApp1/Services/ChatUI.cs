using AIChat.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
namespace AIChat.Service
{
    public class ChatUI
    {
        private readonly KimiClient _kimiClient;
        private readonly ConcurrentDictionary<int, SessionInfo> _sessions;
        private int _currentSessionIndex = 1;
        private int _nextSessionNumber = 1;

        public ChatUI(KimiClient kimiClient)
        {
            _kimiClient = kimiClient;
            _sessions = new ConcurrentDictionary<int, SessionInfo>();
        }

        public async Task StartAsync()
        {
            // 创建默认会话
            await CreateNewSessionAsync();

            Console.WriteLine("=== Kimi 聊天助手 ===");

            while (true)
            {
                DisplaySessions();
                Console.WriteLine();
                Console.WriteLine("输入消息开始聊天，或输入数字切换会话，输入 \"new\" 创建新会话，输入 \"?\" 进入无上下文模式，输入 \"exit\" 退出");
                Console.Write("> ");

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (input.Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    await CreateNewSessionAsync();
                    continue;
                }

                if (input.Equals("?", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleContextFreeMode();
                    continue;
                }

                // 检查是否是数字（切换会话）
                if (int.TryParse(input, out var sessionNumber) && _sessions.ContainsKey(sessionNumber))
                {
                    _currentSessionIndex = sessionNumber;
                    Console.WriteLine($"已切换到会话 {sessionNumber}");
                    continue;
                }

                // 处理普通消息
                await HandleMessageAsync(input);
            }
        }

        /// <summary>
        /// 显示所有会话
        /// </summary>
        private void DisplaySessions()
        {
            Console.WriteLine("\\n可用会话：");
            foreach (var session in _sessions.OrderBy(s => s.Key))
            {
                var indicator = session.Key == _currentSessionIndex ? " (当前活跃) ✓" : "";
                Console.WriteLine($"[{session.Key}] {session.Value.Title}{indicator}");
            }
        }

        private async Task CreateNewSessionAsync()
        {
            var sessionId = Guid.NewGuid().ToString()[..8];
            var sessionInfo = new SessionInfo
            {
                Id = sessionId,
                Title = $"会话 {_nextSessionNumber}",
                ChatService = new ChatService(sessionId, _kimiClient)
            };

            _sessions[_nextSessionNumber] = sessionInfo;
            _currentSessionIndex = _nextSessionNumber;
            _nextSessionNumber++;

            // 在数据库中创建会话记录
            try
            {
                using var db = new ChatDbContext();
                await db.Database.EnsureCreatedAsync();

                var existingSession = await db.Sessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (existingSession == null)
                {
                    db.Sessions.Add(new AIChat.Models.Session
                    {
                        SessionId = sessionId,
                        Title = sessionInfo.Title,
                        Created = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建会话记录时出错: {ex.Message}");
            }

            Console.WriteLine($"已创建新会话: {sessionInfo.Title}");
        }



        private async Task HandleMessageAsync(string message)
        {
            if (!_sessions.TryGetValue(_currentSessionIndex, out var session))
            {
                Console.WriteLine("错误：没有活跃的会话");
                return;
            }

            try
            {
                Console.WriteLine($"[会话{_currentSessionIndex}] 用户: {message}");
                var reply = await session.ChatService.SendAsync(message);
                Console.WriteLine($"[会话{_currentSessionIndex}] Kimi: {reply}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[会话{_currentSessionIndex}] 错误: {ex.Message}");
            }
        }


        /// <summary>
        /// 无上下文模式
        /// </summary>
        /// <returns></returns>

        private async Task HandleContextFreeMode()
        {
            Console.WriteLine("\\n=== 无上下文模式 ===");
            Console.WriteLine("在此模式下，每个问题都是独立的，不会保存对话历史");
            Console.WriteLine("输入 \"back\" 返回正常模式");

            while (true)
            {
                Console.Write("[无上下文] > ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input.Equals("back", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("已返回正常模式");
                    break;
                }

                try
                {
                    // 验证输入不为空
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("[无上下文] 请输入有效的消息");
                        continue;
                    }

                    Console.WriteLine($"[无上下文] 用户: {input}");

                    // 创建临时的 ChatService 进行无上下文查询
                    var tempService = new ChatService("temp", _kimiClient);
                    var reply = await tempService.AskWithoutContextAsync(input);

                    Console.WriteLine($"[无上下文] Kimi: {reply}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[无上下文] 错误: {ex.Message}");
                }
            }
        }
    }

}