using LIN.Allo.Shared.Components.Elements;
using Microsoft.AspNetCore.Components;

namespace LIN.Allo.Shared.Services;


public interface IChatViewer
{


    public int Id { get; set; }


    public void Go(ConversationLocal chat);


    public void Go(int chat);


    public bool IsSearching { get; set; }


   



    public void Select(int chat, bool force = false);



    public void Suscribe(ConversationModel conversation);



    void RefreshUI();


}