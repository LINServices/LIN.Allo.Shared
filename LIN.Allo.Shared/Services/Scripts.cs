using SILF.Script;
using SILF.Script.DotnetRun.Interop;
using SILF.Script.Elements;
using SILF.Script.Elements.Functions;
using SILF.Script.Interfaces;
using SILF.Script.Runtime;

namespace LIN.Allo.Shared.Services;


public class Scripts
{


    public class SILFFunction : IFunction
    {

        public Tipo? Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Parameter> Parameters { get; set; } = new();

        Context IFunction.Context { get; set; }
        private Action<List<SILF.Script.Elements.ParameterValue>> Action;

        public SILFFunction(Action<List<SILF.Script.Elements.ParameterValue>> action)
        {
            Action = action;
        }



        public FuncContext Run(Instance instance, List<SILF.Script.Elements.ParameterValue> values, ObjectContext @object)
        {
            Action.Invoke(values);
            return new();
        }

    }



    /// <summary>
    /// Funciones.
    /// </summary>
    public static List<IFunction> Actions { get; set; } = [];
    public static List<Delegate> Delegates { get; set; } = [];





    /// <summary>
    /// Construye las funciones.
    /// </summary>
    public static void Build()
    {


        // Acción.
        SILFFunction actionSelect =
        new(async (param) =>
        {

            // Propiedades.
            var id = param.Where(T => T.Name == "id").FirstOrDefault();

            // Obtener la conversación.
            _ = int.TryParse(id?.Objeto.Value.ToString(), out int idInt);

            ChatPage.ChatViewer.Select(idInt);

        })
        {
            Name = "select",
            Parameters =
            [
                new Parameter("id", new("number"))
            ]
        };

        Delegates.Add(Mensaje);


        //// Agregar.
        //Actions.AddRange([actionMessage, actionSelect]);

        //Actions.Add(Mensaje);

    }





    [SILFFunctionName("mensaje")]
    public static async void Mensaje(decimal id, string content)
    {

        var conversation = ConversationsObserver.Get((int)id);

        // No existe.
        if (conversation == null)
            return;

        // Generar guid.
        string guid = Guid.NewGuid().ToString();

        ConversationsObserver.PushMessage(conversation.Conversation.Id, new()
        {
            Contenido = content,
            Time = DateTime.Now,
            Guid = guid,
            IsLocal = true,
            Conversacion = conversation.Conversation,
            Remitente = Access.Communication.Session.Instance.Profile
        });

        // Enviar el mensaje al servicio.
        // await RealTime.Hub!.SendMessage(conversation.Conversation.Id, content ?? string.Empty, guid, Access.Communication.Session.Instance.Token);

    }









}