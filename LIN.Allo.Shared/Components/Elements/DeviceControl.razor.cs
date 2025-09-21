namespace LIN.Allo.Shared.Components.Elements;


public partial class DeviceControl
{

    /// <summary>
    /// Modelo del producto.
    /// </summary>
    [Parameter]
    public DeviceOnAccountModel? Model { get; set; }


    /// <summary>
    /// Evento al hacer click.
    /// </summary>
    [Parameter]
    public Action<DeviceOnAccountModel?>? OnClick { get; set; }

    /// <summary>
    /// Enviar el evento.
    /// </summary>
    private void SendEvent()
    {
        OnClick?.Invoke(Model);
    }



    /// <summary>
    /// Obtener el icono.
    /// </summary>
    private (string device, string surface) GetImage()
    {
        if (Model is null)
            return ("", "");

        const string path = "_content/LIN.Allo.Shared/devices/{0}.png";

        string device = "";
        string surface = "";

        switch (Model.OperativeSystem)
        {
            case "android":
                device = "android";
                break;
            case "windows":
                device = "windows";
                break;
            case "ios":
                device = "ios";
                break;
            case "linux":
                device = "linux";
                break;
        }

        surface = Model.SurfaceFrom switch
        {
            "edge" => "edge",
            "chrome" => "chrome",
            "safari" => "safari",
            "native" => "",
            _ => "globe",
        };
        return (string.Format(path, device), string.IsNullOrWhiteSpace(surface) ? string.Empty : string.Format(path, surface));

    }

}
