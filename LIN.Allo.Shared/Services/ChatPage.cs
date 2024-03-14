using LIN.Allo.Shared.Components.Elements;

namespace LIN.Allo.Shared.Services;

public static class ChatPage
{

    public static IChatViewer ChatViewer { get; set; }

    public static List<Message> MessageTasker { get; set; } = new();


}
