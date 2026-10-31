namespace ATHENA.Services
{
    internal class FormatadorTempo
    {
        /// <summary>
        /// Formata uma duração em minutos do mesmo jeito em todas as telas:
        /// "40s" abaixo de 1 minuto, "25min" abaixo de 1 hora e "1h 05min" a partir de 1 hora.
        /// </summary>
        public static string Duracao(double minutos)
        {
            TimeSpan tempo = TimeSpan.FromMinutes(Math.Max(0, minutos));

            if (tempo.TotalMinutes < 1)
                return $"{tempo.Seconds}s";

            if (tempo.TotalHours < 1)
                return $"{tempo.Minutes}min";

            return $"{(int)tempo.TotalHours}h {tempo.Minutes:D2}min";
        }
    }
}
