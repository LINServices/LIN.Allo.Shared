namespace LIN.Allo.Shared.Components.Elements;


public partial class NewFriend
{


    /// <summary>
    /// Información del usuario y el perfil.
    /// </summary>
    [Parameter]
    public SessionModel<ProfileModel>? UserInformation { get; set; } = null;



    /// <summary>
    /// Acción a realizar cuando se haga click.
    /// </summary>
    [Parameter]
    public Action<SessionModel<ProfileModel>> OnSelect { get; set; } = (e) => { };


    /// <summary>
    /// Si se cargando información.
    /// </summary>
    private Sections Section { get; set; } = Sections.Button;




    /// <summary>
    /// Encuentra una conversación.
    /// </summary>
    private async void Find()
    {

        // Sesión.
        var session = Access.Communication.Session.Instance;

        // Validar los parámetros disponibles.
        if (UserInformation == null || ChatPage.ChatViewer == null)
            return;

        // Cambia los estados.
        Section = Sections.Loading;
        StateHasChanged();

        // Obtiene la información de la conversación.
        var conversation = await Access.Communication.Controllers.Members.Find(UserInformation.Profile.Id, Access.Communication.Session.Instance.Token);

        // Error.
        if (conversation.Response != Responses.Success)
        {
            Section = Sections.Error;
            StateHasChanged();
            return;
        }

        //Encuentra la conversación local.
        var localConversation = ConversationsObserver.Get(conversation.LastId);

        // Si existe local.
        if (localConversation != null)
        {
            // Seleccionar la conversación.
            ChatPage.ChatViewer.IsSearching = false;
            ChatPage.ChatViewer.Go(localConversation.Conversation.Id);
            return;
        }


        // Crear o encontrar la conversación en la API.
        var apiConversation = await Access.Communication.Controllers.Conversations.Read(conversation.LastId, session.Token, session.AccountToken);


        // Error.
        if (apiConversation.Response != Responses.Success)
        {
            Section = Sections.Error;
            StateHasChanged();
            return;
        }


        // Modelo de conversación.
        ChatPage.ChatViewer.Suscribe(apiConversation.Model.Conversation);

        ChatPage.ChatViewer.IsSearching = false;
        ChatPage.ChatViewer.Go(apiConversation.Model.Conversation.Id);

    }



    /// <summary>
    /// Secciones.
    /// </summary>
    private enum Sections
    {
        Button,
        Loading,
        Error
    }



}