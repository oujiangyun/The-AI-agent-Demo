
using AIChat.Service;
using System.Threading.Channels;

namespace AIChat

{

    public record SessionSlot(string Id, Channel<string> Inbox, ChatService Service);
}
