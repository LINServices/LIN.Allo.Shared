using LIN.Access.Communication;
using LIN.Types.Communication.Enumerations;

namespace LIN.Allo.Shared.Components.Elements.Drawers;


public partial class Members
{



    bool IsEdit = false;

    string Pattern;
    string NewName = "";


    /// <summary>
    /// Name del grupo.
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Tipo de conversación.
    /// </summary>
    public ConversationsTypes Type { get; set; } = ConversationsTypes.None;



    /// <summary>
    /// Id único del elemento.
    /// </summary>
    private string UniqueId { get; init; } = Guid.NewGuid().ToString();



    /// <summary>
    /// Lista de modelos de miembros.
    /// </summary>
    private List<SessionModel<MemberChatModel>> MemberModels { get; set; } = [];



    /// <summary>
    /// Cache de miembros.
    /// </summary>
    private List<(int, List<SessionModel<MemberChatModel>>)> Cache { get; set; } = [];



    /// <summary>
    /// Lista de controles de miembros.
    /// </summary>
    private List<Member> MemberControls { get; set; } = [];



    /// <summary>
    /// Control de un integrante.
    /// </summary>
    private Member MemberControl { set => MemberControls.Add(value); }




    /// <summary>
    /// Abrir el elemento.
    /// </summary>
    public async void Show()
    {
        await jsRuntime.InvokeVoidAsync("showDrawer", $"drawer-{UniqueId}", DotNetObjectReference.Create(this), $"close-drawer-{UniqueId}");
        StateHasChanged();
    }



    ConversationModel? ConversationContext { get; set; }

    /// <summary>
    /// Cargar los miembros.
    /// </summary>
    /// <param name="id">Id de la conversación.</param>
    public async Task LoadData(int id, bool force = false)
    {
        IsShowAdd = false;
        StateHasChanged();

        // Busca en el cache.
        var cache = Cache.Where(t => t.Item1 == id).FirstOrDefault();

        ConversationContext = new()
        {
            ID = id
        };

        // Si no existe en el cache.
        if (cache.Item2 == null || force)
        {
            // Respuesta de la API.
            var result = await Access.Communication.Controllers.Members.MembersInfo(id, Session.Instance.Token, Session.Instance.AccountToken);

            // Modelos a la UI.
            MemberModels = [.. result.Models.OrderByDescending(t => t.Profile.Rol)];

            Cache.RemoveAll(t => t.Item1 == id);
            Cache.Add(new(id, MemberModels));
        }
        else
        {
            MemberModels = cache.Item2;
        }

        StateHasChanged();

    }


    private List<SessionModel<ProfileModel>> SearchResult { get; set; } = new();



    public void SetDefault(string name, ConversationsTypes type)
    {
        Name = name;
        NewName = name;
        Type = type;
        IsEdit = false;
        IsShowAdd = false;
        StateHasChanged();
    }


    /// <summary>
    /// Buscar elementos.
    /// </summary>
    private async void Search()
    {
        string pattern = Pattern;

        if (pattern.Trim() == "")
        {
            //AreSearch = false;
            //IsSearching = false;
            StateHasChanged();
            return;
        }

        //AreSearch = true;
        //IsSearching = true;
        StateHasChanged();
        var result = await Access.Communication.Controllers.Members.SearchProfiles(Pattern, Session.Instance.AccountToken);

        //IsSearching = false;
        SearchResult = result.Models;
        StateHasChanged();
    }



    /// <summary>
    /// Lista de controles de integrantes.
    /// </summary>
    private List<Profile> SearchControls { get; set; } = new();



    /// <summary>
    /// Control de integrante actual.
    /// </summary>
    private Profile SearchResultControl
    {
        set => SearchControls.Add(value);
    }



    bool IsShowAdd = false;
    void ShowAdd()
    {
        IsShowAdd = !IsShowAdd;
        StateHasChanged();
    }



    void ShowEdit()
    {

        MessageLittle = ("", "");

        if (Type == ConversationsTypes.Personal)
            return;

        IsEdit = !IsEdit;

        if (IsEdit)
            NewName = Name;

        StateHasChanged();
    }


    (string, string) MessageLittle = ("text-red-500", "");

    async void UpdateName()
    {

        if (string.IsNullOrWhiteSpace(NewName))
        {
            NewName = Name;
            MessageLittle = ("text-red-500", "El nombre no puede esta vacío");
            StateHasChanged();
            return;
        }

        MessageLittle = ("text-blue-500", "Espera...");
        StateHasChanged();

        var response = await LIN.Access.Communication.Controllers.Conversations.UpdateName(ConversationContext.ID, NewName, LIN.Access.Communication.Session.Instance.Token);

        if (response.Response != Responses.Success)
        {
            MessageLittle = ("text-red-500", "No se puede actualizar el nombre");
        }
        else
        {
            MessageLittle = ("text-green-500", "");
            ConversationsObserver.IsUpdate(ConversationContext.ID, NewName);

        }



        StateHasChanged();


    }



    /// <summary>
    /// Items seleccionados.
    /// </summary>
    private List<SessionModel<ProfileModel>> NewMembers { get; set; } = new();



    /// <summary>
    /// Al seleccionar un elemento.
    /// </summary>
    /// <param name="model">Modelo seleccionado.</param>
    private void OnSelect(SessionModel<ProfileModel> model)
    {
        // Si existe.
        var have = NewMembers.Where(T => T.Account.Id == model.Account.Id).Any();

        if (have)
        {
            NewMembers.RemoveAll(T => T.Account.Id == model.Account.Id);
            StateHasChanged();
            return;
        }

        NewMembers.Add(model);
        StateHasChanged();
    }


    async void Insert()
    {
        if (ConversationContext == null)
        {
            return;
        }

        List<Task> tasks = [];

        var token = Session.Instance.Token;
        foreach (var x in NewMembers)
        {
            tasks.Add(Access.Communication.Controllers.Members.Insert(ConversationContext.ID, x.Profile.ID, token));
        }

        await Task.WhenAll(tasks);

        NewMembers = [];
        IsShowAdd = false;
        await LoadData(ConversationContext.ID, true);

        StateHasChanged();

    }



    async void Remove(int profile)
    {

        var remove = await Access.Communication.Controllers.Members.Remove(ConversationContext.ID, profile, Session.Instance.Token);

        try
        {
            Cache.Where(t => t.Item1 == ConversationContext.ID).FirstOrDefault().Item2.RemoveAll(t => t.Profile.Profile.ID == profile);
        }
        catch
        {
        }

        if (ChatPage.ChatViewer.Id == ConversationContext.ID && profile == Session.Instance.Profile.ID)
        {
            await jsRuntime.InvokeVoidAsync("ForceClick", $"close-drawer-{UniqueId}");
            ConversationsObserver.Remove(ConversationContext.ID);
            ChatPage.ChatViewer.Go(0);
        }

        await LoadData(ConversationContext.ID);
        StateHasChanged();
    }

}