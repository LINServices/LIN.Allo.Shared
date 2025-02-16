namespace LIN.Allo.Shared.Components.Elements;


public partial class Member
{

    // Cache
    private static List<(int, IsOnlineResult, DateTime)> Cache { get; set; } = [];


    private int Id => (e?.Profile.Profile.Id) ?? 0;




    [Parameter]
    public SessionModel<MemberChatModel>? e { get; set; } = null;



    [Parameter]
    public Action<int> OnDelete { get; set; }




    public IsOnlineResult? isOnline = null;




    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
    }


    public void Con()
    {
        Confirm();
    }


    public async void Confirm()
    {

        Console.WriteLine("Confirm " + Id);
        var inCache = Cache.Where(t => t.Item1 == Id).FirstOrDefault();


        if (inCache.Item2 == null || inCache.Item3 < DateTime.Now.AddMinutes(-1))
        {

            Cache.RemoveAll(t => t.Item1 == Id);

            Console.WriteLine("Usando INTERNET");
            var x = await Access.Communication.Controllers.Members.IsOnline(Id, LIN.Access.Communication.Session.Instance.Token);

            Cache.Add(new(Id, x.Model, DateTime.Now));
            isOnline = x.Model;
        }
        else
        {
            isOnline = inCache.Item2;
        }

        StateHasChanged();
    }



    protected override void OnParametersSet()
    {
        Confirm();
        base.OnParametersSet();
    }

}
