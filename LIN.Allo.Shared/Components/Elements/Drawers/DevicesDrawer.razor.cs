namespace LIN.Allo.Shared.Components.Elements.Drawers;

public partial class DevicesDrawer
{

    /// <summary>
    /// ID del elemento Html.
    /// </summary>
    public string _id = $"element-{Guid.NewGuid()}";


    /// <summary>
    /// Lista de dispositivos.
    /// </summary>
    [Parameter]
    public static ReadAllResponse<DeviceOnAccountModel> DevicesList { get; set; } = null!;


    /// <summary>
    /// Evento onclick.
    /// </summary>
    [Parameter]
    public Action<DeviceOnAccountModel> OnInvoke { get; set; } = (d) => { };


    /// <summary>
    /// Es la primer abierta?
    /// </summary>
    public bool FirstShow { get; set; } = true;


    /// <summary>
    /// Abrir el elemento.
    /// </summary>
    public async void Show()
    {

        // Abrir el elemento.
        await JsRuntime.InvokeVoidAsync("showBottomDrawer", _id, DotNetObjectReference.Create(this), $"btn-close-{_id}", "close-all-all");

        // Si es el primer open.
        if (FirstShow)
        {
            _ = GetDevices();
            FirstShow = false;
        }
        StateHasChanged();
    }


    /// <summary>
    /// Obtener los dispositivos.
    /// </summary>
    private async Task<bool> GetDevices()
    {

        // Items
        var items = await Access.Communication.Controllers.Profiles.Devices(Access.Communication.Session.Instance.Token);

        // Rellena los items
        DevicesList = items;

        // Eliminar dispositivo local.
        items.Models = [.. items.Models.Where(t => t.ConnectionId != Device.ConnectionId)];
        StateHasChanged();
        return true;

    }

}