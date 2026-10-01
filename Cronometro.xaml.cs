using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
using ATHENA.SessaoEstudo;
using System.Diagnostics;

namespace ATHENA;

public partial class NewPage1 : ContentPage, IQueryAttributable
{

    public int? IdSessao { get; set; }

    public int? IdMeta { get; set; }

    private bool CronometroRodando = false;

    DateTime dataInicio = DateTime.Now;

    Stopwatch cronometro = new Stopwatch();  
    Stopwatch tempoBruto = new Stopwatch();
    Stopwatch tempoLiquido = new Stopwatch();


    public enum OrigemCronometro
    {
        Meta,
        Sessao,
    }

    public OrigemCronometro Origem { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdMeta", out object? idMeta))
            IdMeta = (int)idMeta;

        if (query.TryGetValue("IdSessao", out object? idSessao))
            IdSessao = (int)idSessao;

        if (query.TryGetValue("Origem", out object? origem))
            Origem = (OrigemCronometro)origem;

    }

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
    protected override void OnAppearing()
    {
        base.OnAppearing();
        BtnPausar.Text = "Pausar";
    }

    [Obsolete]
    private void BtnIniciar_Click(object sender, EventArgs e)
    {
        cronometro.Start();
        tempoBruto.Start();
        tempoLiquido.Start();

        BtnPausar.Text = "Pausar";

        

        Device.StartTimer(
            TimeSpan.FromMilliseconds(100),
            () =>
            {
                TxtCronometro.Text = cronometro.Elapsed.ToString(@"hh\:mm\:ss");

                return cronometro.IsRunning;
            });

    }
    [Obsolete]
    private void BtnPausar_Click(object sender, EventArgs e)
    {
        if (CronometroRodando) 
        {
            BtnPausar.Text = "Continuar";
            tempoLiquido.Stop();
            cronometro.Stop();

            CronometroRodando = false;
        }
        else
        {

            BtnPausar.Text = "Pausar";
            tempoLiquido.Start();
            cronometro.Start();

            CronometroRodando = true;

            Device.StartTimer(
            TimeSpan.FromMilliseconds(100),
            () =>
            {
                TxtCronometro.Text = cronometro.Elapsed.ToString(@"hh\:mm\:ss");

                return cronometro.IsRunning;
            });
        }

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


        await DisplayAlertAsync(
    "DEBUG",
    $"IdMeta: {IdMeta}\n" +
    $"IdSessao: {IdSessao}\n" +
    $"Minutos: {resultadoSessao.MinutosLiquidos}",
    "OK");

        if (resultadoSessao != null)

            if (Origem == OrigemCronometro.Meta)
            {
                try 
                { 
                    await Models_Cronometro.AdicionaTempoMeta(resultadoSessao, IdMeta.Value);
                    await Shell.Current.GoToAsync("//EscolherMeta");

                    await Shell.Current.GoToAsync(
                        $"{nameof(InterfaceMeta)}?idMeta={IdMeta}");
                }
                catch(Exception ex)
                {
                    await DisplayAlertAsync("Erro", ex.Message, "Ok");
                }
                IdSessao = null;
                IdMeta = null;

            }
            else if(Origem == OrigemCronometro.Sessao)
            {
                try
                {
                    await Models_Cronometro.AdicionaTempoMeta(resultadoSessao, IdMeta.Value);

                    await Shell.Current.GoToAsync("//EscolherMeta");

                    await Shell.Current.GoToAsync(
            nameof(InterfaceSessao),
            new Dictionary<string, object>
            {
                ["IdSessao"] = IdSessao.Value,
                ["IdMeta"] = IdMeta.Value,
                ["minutosBrutos"] = resultadoSessao.MinutosBrutos,
                ["minutosLiquidos"] = resultadoSessao.MinutosLiquidos
            });
                }
                catch (Exception ex) 
                { 
                    await DisplayAlertAsync("Erro", ex.Message, "Ok");
                }

                IdSessao = null;
                IdMeta = null;
                    
            }
            else
            {
                try 
                { 
                    await Models_Cronometro.AdicionaTempoEstudoLivre(resultadoSessao); 
                }
                catch (Exception ex)
                {
                    await DisplayAlertAsync("Erro", ex.Message, "Ok");
                }
            }
    }
}