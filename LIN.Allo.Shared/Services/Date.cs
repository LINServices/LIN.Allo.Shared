namespace LIN.Allo.Shared.Services;


internal class Date
{
    public static string CalcTime(DateTime? fecha)
    {

        if (fecha == null)
            return "";


        DateTime ahora = DateTime.Now;
        TimeSpan diferencia = ahora - fecha.Value;

        if (diferencia.Days == 0)
        {
            return "Hoy";
        }
        else if (diferencia.Days == 1)
        {
            return "Ayer";
        }
        else if (diferencia.Days < 7)
        {
            return "Esta semana";
        }
        else if (diferencia.Days < 14)
        {
            return "La semana pasada";
        }
        else if (ahora.Month == fecha.Value.Month)
        {
            return "Este mes";
        }
        else if (ahora.Month - fecha.Value.Month == 1)
        {
            return "El mes pasado";
        }
        else
        {
            return "Hace más de un mes";
        }
    }
}
