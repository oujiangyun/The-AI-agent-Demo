using OpenAI;
using OpenAI.Chat;
namespace AIChat.Service
{
    public class KimiClient
    {
        private readonly string _model;
        private readonly int _maxTokens;
        private readonly ChatClient _chatClient;
        private readonly string? _systemPrompt;


        /// <summary>
        /// 初始化 Kimi 客户端
        /// </summary>
        /// <param name="apiKey">模型key</param>
        /// <param name="baseUrl">模型URL地址</param>
        /// <param name="model">模型</param>
        /// <param name="maxTokens"></param>
        /// <param name="systemPrompt">系统提示词</param>
        public KimiClient(string apiKey, string? baseUrl = null, string? model = "moonshot-v1-8k", int? maxTokens = null, string? systemPrompt = null)
        {
            _model = model;
            _maxTokens = maxTokens ?? 512;
            _systemPrompt = systemPrompt;

            var clientOptions = new OpenAIClientOptions();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                clientOptions.Endpoint = new Uri(baseUrl);
            }

            try
            {
                var openAi = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), clientOptions);
                _chatClient = openAi.GetChatClient(_model);
                Console.WriteLine($"[INFO] Kimi 客户端初始化成功 - 模型: {_model}, 端点: {baseUrl ?? "默认"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Kimi 客户端初始化失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 无上下文的一问一答
        /// </summary>
        public async Task<string> AskAsync(string prompt)
        {
            var messages = new List<ChatMessage>();

            // 添加系统提示词（如果有）
            if (!string.IsNullOrWhiteSpace(_systemPrompt))
            {
                messages.Add(new SystemChatMessage(_systemPrompt));
            }

            messages.Add(new UserChatMessage(prompt));

            return await AskAsync(messages);
        }

        /// <summary>
        /// 带上下文的多轮对话
        /// </summary>
        public async Task<string> AskAsync(IEnumerable<ChatMessage> messages)
        {
            try
            {
                var messageList = new List<ChatMessage>();

                // 添加系统提示词（如果有且消息列表中没有系统消息）
                if (!string.IsNullOrWhiteSpace(_systemPrompt) && !messages.Any(m => m is SystemChatMessage))
                {
                    messageList.Add(new SystemChatMessage(_systemPrompt));
                }

                messageList.AddRange(messages);

                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = _maxTokens
                };

                var response = await _chatClient.CompleteChatAsync(messageList, options);
                return response.Value.Content[0].Text;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Kimi API call 错误: {ex.Message}", ex);
            }
        }






    }
}