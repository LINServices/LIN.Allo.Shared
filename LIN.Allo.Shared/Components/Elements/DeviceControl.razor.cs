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

        switch (Model.Platform)
        {
            case Types.Enumerations.Platforms.Android:
                device = "android";
                break;
            case Types.Enumerations.Platforms.Windows:
                device = "windows";
                break;
            case Types.Enumerations.Platforms.IOs:
                device = "ios";
                break;
            case Types.Enumerations.Platforms.Linux:
                device = "linux";
                break;
            case Types.Enumerations.Platforms.MacOs:
                device = "macos";
                break;
        }

        if (Model.SurfaceFrom == Types.Enumerations.SurfaceFrom.WebApp)
        {
            surface = Model.Browser switch
            {
                Types.Enumerations.Browsers.Edge => "edge",
                Types.Enumerations.Browsers.Chrome => "chrome",
                Types.Enumerations.Browsers.Safari => "safari",
                _ => "globe",
            };
        }


        return (string.Format(path, device), string.IsNullOrWhiteSpace(surface) ? string.Empty : string.Format(path, surface));

    }

}
