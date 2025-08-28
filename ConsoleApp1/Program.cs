using AIChat.Service;
using Microsoft.Extensions.Configuration;

// 设置控制台编码为 UTF-8
//try
//{
//    //Console.OutputEncoding = Encoding.UTF8;
//    //Console.InputEncoding = Encoding.UTF8;


//}
//catch (Exception ex)
//{
//    Console.WriteLine($"设置编码时出错: {ex.Message}");
//}

// 读取配置




var configuration = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddEnvironmentVariables()
.Build();

var apiKey = configuration["Kimi:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("请在 appsettings.json 中配置 Kimi:ApiKey");
    return;
}

var kimiConfig = configuration.GetSection("Kimi");


var kimi = new KimiClient(
    apiKey,
    kimiConfig["BaseUrl"],
    kimiConfig["Model"],
    int.TryParse(kimiConfig["MaxTokens"], out var maxTokens) ? maxTokens : null,
    kimiConfig["SystemPrompt"]);

//测试
Console.WriteLine("正在测试 Kimi API 连接...");
try
{

    var testReply = await kimi.AskAsync("你好，请简单介绍一下你自己");
    Console.WriteLine($"Kimi API 测试成功！回复: {testReply}");
}
catch (Exception ex)
{
    Console.WriteLine($"Kimi API 测试失败: {ex.Message}");
}
// 启动新的聊天界面
var chatUI = new ChatUI(kimi);
await chatUI.StartAsync();
