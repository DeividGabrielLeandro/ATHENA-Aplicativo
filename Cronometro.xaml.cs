using ATHENA.Database;
using ATHENA.Models;
using System.Diagnostics;

namespace ATHENA;

[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class NewPage1 : ContentPage
{
    public int IdMeta { get; set; }

    DateTime dataInicio = DateTime.Now;

    Stopwatch cronometro = new Stopwatch();  
    Stopwatch tempoBruto = new Stopwatch();
    Stopwatch tempoLiquido = new Stopwatch();
     
    public class ResultadoSessao
    {
        /// <summary>Data e hora de início da sessão de foco.</summary>
        public DateTime DataInicio { get; set; }

        /// <summary>Data e hora de término da sessão de foco.</summary>
        public DateTime DataFim { get; set; }

        /// <summary>Tempo total em que o usuário permaneceu focado.</summary>
        public TimeSpan TempoLiquido { get; set; }

        /// <summary>Tempo total absoluto decorrido (incluindo as pausas).</summary>
        public TimeSpan TempoBruto { get; set; }

        /// <summary>Tempo total consumido em pausas.</summary>
        public TimeSpan TempoPausa => TempoBruto - TempoLiquido;

        /// <summary>Total de minutos líquidos acumulados de foco.</summary>
        public double MinutosLiquidos => TempoLiquido.TotalMinutes;

        /// <summary>Total de minutos brutos decorridos na sessão.</summary>
        public double MinutosBrutos => TempoBruto.TotalMinutes;

        /// <summary>Total de minutos acumulados em pausa.</summary>
        public double MinutosPausa => TempoPausa.TotalMinutes;
    }
    public static ResultadoSessao FinalizarSessao(Stopwatch tempoLiquido, Stopwatch tempoBruto, DateTime dataInicio, DateTime dataFim)
    {
        return new ResultadoSessao
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            TempoLiquido = tempoLiquido.Elapsed,
            TempoBruto = tempoBruto.Elapsed,

        };      
    }
    public NewPage1()
    {
        InitializeComponent();
    }

    [Obsolete]
    private void BtnIniciar_Click(object sender, EventArgs e)
    {
        cronometro.Start();
        tempoBruto.Start();
        tempoLiquido.Start();

        Device.StartTimer(
            TimeSpan.FromMilliseconds(100),
            () =>
            {
                TxtCronometro.Text = cronometro.Elapsed.ToString(@"hh\:mm\:ss");

                return cronometro.IsRunning;
            });
    }

    private void BtnPausar_Click(object sender, EventArgs e)
    {
        tempoLiquido.Stop();
        cronometro.Stop();
    }


    private async void BtnParar_Clicked(object sender, EventArgs e)
    {
        tempoLiquido.Stop();
        tempoBruto.Stop();
        cronometro.Reset();
        cronometro.Stop();
        DateTime dataFim = DateTime.Now;

        ResultadoSessao resultadoSessao = FinalizarSessao(
            tempoLiquido,
            tempoBruto,
            dataInicio,
            dataFim);

if (resultadoSessao != null)
        await Models_Cronometro.AdicionaTempoMeta(resultadoSessao, IdMeta);
        
    }
}