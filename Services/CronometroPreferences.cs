
using Microsoft.Maui.Storage;

namespace ATHENA.Services
{
    /// <summary>Cópia do estado do CronometroService guardada no Preferences do aparelho.</summary>
    internal class CronometroPreferences
    {
        public string Origem { get; set; } = string.Empty;
        public int? IdMeta { get; set; } = -1;
        public int? IdSessao { get; set; } = -1;

        public bool Iniciado { get; set; }
        public bool Rodando { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime InicioTrecho { get; set; }

        public long TempoLiquidoAcumulado { get; set; }

        public void Salvar()
        {
            Preferences.Set("Cronometro_Origem", Origem);
            Preferences.Set("Cronometro_IdMeta", IdMeta ?? -1);
            Preferences.Set("Cronometro_IdSessao", IdSessao ?? -1);

            Preferences.Set("Cronometro_Iniciado", Iniciado);
            Preferences.Set("Cronometro_Rodando", Rodando);

            Preferences.Set("Cronometro_DataInicio", DataInicio);
            Preferences.Set("Cronometro_InicioTrecho", InicioTrecho);

            Preferences.Set("Cronometro_TempoLiquidoAcumulado",TempoLiquidoAcumulado);
        }

        public void Carregar()
        {
            Origem = Preferences.Get("Cronometro_Origem", string.Empty);
            int idMeta = Preferences.Get("Cronometro_IdMeta", -1);
            IdMeta = idMeta == -1 ? null : idMeta;

            int idSessao = Preferences.Get("Cronometro_IdSessao", -1);
            IdSessao = idSessao == -1 ? null : idSessao;

            Iniciado = Preferences.Get("Cronometro_Iniciado", false);
            Rodando = Preferences.Get("Cronometro_Rodando", false);

            DataInicio = Preferences.Get(
                "Cronometro_DataInicio",
                DateTime.MinValue);

            InicioTrecho = Preferences.Get(
                "Cronometro_InicioTrecho",
                DateTime.MinValue);

            TempoLiquidoAcumulado = Preferences.Get(
                "Cronometro_TempoLiquidoAcumulado",
                0L);
        }
    }
}
