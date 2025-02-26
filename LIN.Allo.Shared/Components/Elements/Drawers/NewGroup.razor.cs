﻿namespace LIN.Allo.Shared.Components.Elements.Drawers;

public partial class NewGroup
{

    /// <summary>
    /// Correcto.
    /// </summary>
    [Parameter]
    public Action? OnSuccess { get; set; }


    /// <summary>
    /// Id único del elemento.
    /// </summary>
    private string UniqueId { get; init; } = Guid.NewGuid().ToString();


    /// <summary>
    /// Estado actual.
    /// </summary>
    private State ActualState = State.Modeling;


    /// <summary>
    /// Lista de estados.
    /// </summary>
    private enum State
    {
        Modeling,
        Creating,
        Success,
        Failure
    }


    /// <summary>
    /// Estado (Buscando).
    /// </summary>
    private bool IsSearching { get; set; } = false;


    /// <summary>
    /// Estado (Buscado).
    /// </summary>
    private bool AreSearch { get; set; } = false;


    /// <summary>
    /// Items seleccionados.
    /// </summary>
    private List<SessionModel<ProfileModel>> SelectedItems { get; set; } = new();


    /// <summary>
    /// Lista de modelos de integrantes.
    /// </summary>
    private List<SessionModel<ProfileModel>> MemberModels { get; set; } = new();


    /// <summary>
    /// Lista de controles de integrantes.
    /// </summary>
    private List<Profile> MemberControls { get; set; } = new();


    /// <summary>
    /// Control de integrante actual.
    /// </summary>
    private Profile MemberControl
    {
        set => MemberControls.Add(value);
    }


    /// <summary>
    /// Patron de búsqueda.
    /// </summary>
    private string pattern = "";


    /// <summary>
    /// Patron de búsqueda.
    /// </summary>
    private string Pattern
    {
        get => pattern;
        set
        {
            pattern = value;
            AreSearch = false;
            StateHasChanged();
        }
    }


    /// <summary>
    /// Name de la conversación.
    /// </summary>
    private string Name { get; set; } = string.Empty;


    /// <summary>
    /// Abrir el cajon.
    /// </summary>
    public async void Show()
    {
        await js.InvokeAsync<object>("showDrawer", $"drawerIG-{UniqueId}", DotNetObjectReference.Create(this), $"close-drawerIG-{UniqueId}", $"close-2-drawerIG-{UniqueId}");
        StateHasChanged();
    }


    /// <summary>
    /// Cerrar el cajon.
    /// </summary>
    public async void Hide()
    {
        await js.InvokeAsync<object>("forceClick", $"close-2-drawerIG-{UniqueId}");
        StateHasChanged();
    }


    /// <summary>
    /// Al seleccionar un elemento.
    /// </summary>
    /// <param name="model">Modelo seleccionado.</param>
    private void OnSelect(SessionModel<ProfileModel> model)
    {
        // Si existe.
        var have = SelectedItems.Where(T => T.Account.Id == model.Account.Id).Any();

        if (have)
            return;

        SelectedItems.Add(model);
        StateHasChanged();
    }


    /// <summary>
    /// Al eliminar.
    /// </summary>
    /// <param name="model">Modelo.</param>
    private void OnRemove(SessionModel<ProfileModel> model)
    {
        SelectedItems = SelectedItems.Where(T => T.Account.Id != model.Account.Id).ToList();
        StateHasChanged();
    }


    /// <summary>
    /// Evento click
    /// </summary>
    private void Click()
    {
        AreSearch = false;
        IsSearching = false;
        MemberModels?.Clear();
        SelectedItems?.Clear();
        Name = "";
        Pattern = "";
        ActualState = State.Modeling;
        StateHasChanged();
    }


    /// <summary>
    /// Buscar elementos.
    /// </summary>
    private async void Search()
    {

        if (pattern.Trim() == "")
        {
            AreSearch = false;
            IsSearching = false;
            StateHasChanged();
            return;
        }

        AreSearch = true;
        IsSearching = true;
        StateHasChanged();
        var result = await Access.Communication.Controllers.Members.SearchProfiles(Pattern, Access.Communication.Session.Instance.AccountToken);

        IsSearching = false;
        MemberModels = result.Models;
        StateHasChanged();
    }


    /// <summary>
    /// Acción al crear.
    /// </summary>
    private async void Crear()
    {
        ActualState = State.Creating;
        StateHasChanged();

        var modelo = new ConversationModel()
        {
            Id = 0,
            Members = SelectedItems.Select(T => new MemberChatModel()
            {
                Rol = Types.Communication.Enumerations.MemberRoles.None,
                Profile = new()
                {
                    Id = T.Profile.Id
                }
            }).ToList(),
            Name = Name,
            Type = Types.Communication.Enumerations.ConversationsTypes.Group
        };

        modelo.Members.Add(new()
        {
            Profile = new()
            {
                Id = Access.Communication.Session.Instance.Profile.Id
            },
            Rol = Types.Communication.Enumerations.MemberRoles.Admin
        });

        var res = await Access.Communication.Controllers.Conversations.Create(modelo, Access.Communication.Session.Instance.Token);
        if (res.Response == Responses.Success)
        {
            ActualState = State.Success;
            //StateHasChanged();
            OnSuccess();
            return;
        }

        ActualState = State.Failure;
        StateHasChanged();
    }

}