namespace LIN.Allo.Shared.Services;


/// <summary>
/// Observador.
/// </summary>
public static class ConversationsObserver
{


    /// <summary>
    /// Data de las conversaciones.
    /// </summary>
    public static List<(int, ConversationLocal)> Data = [];


    /// <summary>
    /// Elementos a observar.
    /// </summary>
    private readonly static Dictionary<int, List<IMessageChanger>> Trackers = [];




    private readonly static Dictionary<int, List<IConversationViewer>> TrackersConversations = [];




    /// <summary>
    /// Agregar elemento al observador.
    /// </summary>
    /// <param name="conversation">Id de la conversación.</param>
    /// <param name="messageChanger">Objecto a observar.</param>
    public static void Suscribe(int conversation, IMessageChanger messageChanger)
    {

        // Obtener los observables.
        Trackers.TryGetValue(conversation, out var trackers);

        // Si no existe.
        if (trackers == null)
        {
            // Crear la lista de observables.
            Trackers.Add(conversation,
            [
                messageChanger
            ]);
            return;
        }

        // Agregar el objeto a la lista.
        trackers.Add(messageChanger);

    }



    /// <summary>
    /// Agregar elemento al observador.
    /// </summary>
    /// <param name="conversation">Id de la conversación.</param>
    /// <param name="messageChanger">Objecto a observar.</param>
    public static void Suscribe(int conversation, IConversationViewer messageChanger)
    {

        // Obtener los observables.
        TrackersConversations.TryGetValue(conversation, out var trackers);

        // Si no existe.
        if (trackers == null)
        {
            // Crear la lista de observables.
            TrackersConversations.Add(conversation,
            [
                messageChanger
            ]);
            return;
        }

        // Agregar el objeto a la lista.
        trackers.Add(messageChanger);

    }







    /// <summary>
    /// Eliminar objeto de la lista de observables.
    /// </summary>
    /// <param name="messageChanger">Objeto,</param>
    public static void UnSuscribe(IMessageChanger messageChanger)
    {
        // Eliminar objetos.
        foreach (var e in Trackers.Values)
            e.RemoveAll(t => t == messageChanger);

    }



    /// <summary>
    /// Eliminar objeto de la lista de observables.
    /// </summary>
    /// <param name="messageChanger">Objeto,</param>
    public static void UnSuscribe(IConversationViewer messageChanger)
    {
        // Eliminar objetos.
        foreach (var e in TrackersConversations.Values)
            e.RemoveAll(t => t == messageChanger);

    }



    /// <summary>
    /// Notificar cambios a los observables.
    /// </summary>
    /// <param name="conversation">Id de la conversación.</param>
    public static void Notification(int conversation)
    {

        // Obtener.
        Trackers.TryGetValue(conversation, out var trackers);

        // No hay.
        if (trackers == null)
            return;

        // Notificar cambios.
        foreach (var item in trackers)
            item.Change();

    }



    /// <summary>
    /// Notificar cambios a los observables.
    /// </summary>
    /// <param name="conversation">Id de la conversación.</param>
    public static void IsUpdate(int conversation, string newName)
    {

        // Obtener.
        TrackersConversations.TryGetValue(conversation, out var trackers);

        // No hay.
        if (trackers == null)
            return;

        // Notificar cambios.
        foreach (var item in trackers)
            item.Change(newName);

    }





    /// <summary>
    /// Crear conversación.
    /// </summary>
    /// <param name="conversation">Modelo.</param>
    public static void Create(ConversationModel conversation)
    {

        if (conversation == null)
            return;


        var local = Data.Where(t => t.Item1 == conversation.ID).FirstOrDefault();


        if (local.Item2 == null)
        {
            conversation.Mensajes = null;

            Data.Add((conversation.ID, new()
            {
                Conversation = conversation,
                Messages = null!
            }));

            return;
        }


    }



    /// <summary>
    /// Agregar mensaje.
    /// </summary>
    /// <param name="conversation">Id de la conversación.</param>
    /// <param name="message">Modelo del mensaje.</param>
    public static void PushMessage(int conversation, MessageModel message)
    {

        var local = Data.Where(t => t.Item1 == conversation).FirstOrDefault();


        Trackers.TryGetValue(conversation, out var trackers);

        if (local.Item2 == null || trackers == null)
            return;



        local.Item2.Messages ??= [];

        var exist = local.Item2.Messages.Where(t => t.Guid == message.Guid);

        if (exist.Any())
        {
            foreach (var item in exist)
                item.IsLocal = false;

            foreach (var tracker in trackers)
                tracker.Change();



            Remove(conversation);

            Data.Insert(0, (conversation, local.Item2));

            ChatPage.ChatViewer.RefreshUI();

            return;
        }


        local.Item2.Messages.Add(message);

        foreach (var tracker in trackers)
            tracker.Change();



        Remove(conversation);

        Data.Insert(0, (conversation, local.Item2));

        ChatPage.ChatViewer.RefreshUI();

    }



    /// <summary>
    /// Obtener la conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public static ConversationLocal? Get(int id)
    {

        var local = Data.Where(t => t.Item1 == id).FirstOrDefault();
        return local.Item2;
    }


    /// <summary>
    /// ELiminar la conversación.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public static void Remove(int id)
    {
        Data.RemoveAll(t => t.Item1 == id);
    }


}


public class ConversationLocal
{

    public ConversationModel Conversation { get; set; } = null!;

    public List<MessageModel> Messages { get; set; } = [];

}


public interface IMessageChanger
{

    void Change();

}


public interface IConversationViewer
{

    void Change(string newName);

}