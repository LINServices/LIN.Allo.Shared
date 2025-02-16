namespace LIN.Allo.Shared.Components.Elements;


public partial class Profile
{

    /// <summary>
    /// Modelo de la sesión.
    /// </summary>
    [Parameter]
    public SessionModel<ProfileModel>? SessionModel { get; set; } = null;


    /// <summary>
    /// Estado.
    /// </summary>
    [Parameter]
    public int State { get; set; } = 1;


    /// <summary>
    /// Acción al seleccionar.
    /// </summary>
    [Parameter]
    public Action<SessionModel<ProfileModel>> OnSelect { get; set; } = (e) => { };


    /// <summary>
    /// Es Online.
    /// </summary>
    public IsOnlineResult? isOnline = null;

    /// <summary>
    /// Obtener el estado.
    /// </summary>
    /// <param name="id">Id del perfil.</param>
    private async void Confirm(int id)
    {
        // Solicitud.
        var response = await Access.Communication.Controllers.Members.IsOnline(id, LIN.Access.Communication.Session.Instance.Token);

        isOnline = response.Model;
        StateHasChanged();
    }


}