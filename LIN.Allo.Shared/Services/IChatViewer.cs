namespace LIN.Allo.Shared.Services;

public interface IChatViewer
{
    int Id { get; set; }
    void Go(ConversationLocal chat);
    void Go(int chat);
    bool IsSearching { get; set; }
    void Select(int chat, bool force = false);
    void Suscribe(ConversationModel conversation);
    void RefreshUI();

}