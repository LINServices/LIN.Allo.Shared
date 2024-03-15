namespace LIN.Allo.Shared.Components.Elements;


public partial class Control : IConversationViewer, IDisposable
{

    /// <summary>
    /// Evento al darle click.
    /// </summary>
    [Parameter]
    public Action<ConversationLocal> OnClick { get; set; } = (e) => { };



    /// <summary>
    /// Modelo.
    /// </summary>
    [Parameter]
    public ConversationLocal Member { get; set; }



    /// <summary>
    /// El control esta seleccionado.
    /// </summary>
    [Parameter]
    public bool IsSelect { get; set; } = false;



    /// <summary>
    /// Tiene nuevos mensajes.
    /// </summary>
    public bool IsNew = false;




    /// <summary>
    /// Renderizar.
    /// </summary>
    public void Render()
    {
        InvokeAsync(StateHasChanged);
    }



    /// <summary>
    /// Seleccionar.
    /// </summary>
    public void Select()
    {
        IsSelect = true;
        Render();
    }



    /// <summary>
    /// Deseleccionar.
    /// </summary>
    public void Unselect()
    {
        IsSelect = false;
        Render();
    }




    public string GetPicture(AccountModel? profile)
    {

        string final;

        if (profile != null && profile.Profile.Length > 0)
        {
            final = $"data:image/png;base64,{Convert.ToBase64String(profile.Profile)}";
        }
        else
        {
            final = "./img/people.png";
        }

        return final;






    }

    public void Change(string newName)
    {
        Member.Conversation.Name = newName;
        StateHasChanged();
    }


    protected override void OnParametersSet()
    {
        ConversationsObserver.UnSuscribe(this);
        ConversationsObserver.Suscribe(Member.Conversation.ID, this);
    }


    public void Dispose()
    {
        ConversationsObserver.UnSuscribe(this);
    }
}