using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
using ATHENA.Services;
using ATHENA.SessaoEstudo;
using Microsoft.Win32;
using SQLite;
using System.Diagnostics;
using static ATHENA.NewPage1;

namespace ATHENA;

public partial class NewPage1 : ContentPage, IQueryAttributable
{

    public int? IdSessao { get; set; }

    public int? IdMeta { get; set; }

    private double minutosEstudadosHoje = 0;
    private double metaDiariaMinutos = 0;

    private bool CronometroRodando = false;
    private bool SessaoIniciada = false;
    private bool TemporizadorTelaAtivo = false;

    DateTime dataInicio = DateTime.Now;

    Stopwatch cronometro = new Stopwatch();  
    Stopwatch tempoBruto = new Stopwatch();
    Stopwatch tempoLiquido = new Stopwatch();
    Stopwatch tempoPausado = new Stopwatch();


    public enum OrigemCronometro
    {
        Nenhum,
        Meta,
        Sessao,
        Livre,
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
        public DateTime? DataInicio { get; set; }

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
    SQLiteAsyncConnection db;
    public NewPage1()
    {
        InitializeComponent();

        string dbPath = Path.Combine(

            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db"
        );

        db = new SQLiteAsyncConnection(dbPath);
    }

    private async Task CarregarInformacoes()
    {
        try
        {
            if (Origem == OrigemCronometro.Livre)
            {
                TxtTipoContexto.Text = "ESTUDO LIVRE";
                TxtDetalheContexto.Text = "Foco atual";
                TxtNomeContexto.Text = "Estudo sem meta selecionada";
                CategoriaContextoBorder.IsVisible = false;
            }
            else if (Origem == OrigemCronometro.Meta)
            {
                Meta? meta = await db.Table<Meta>()
                .FirstOrDefaultAsync(r => r.idMeta == IdMeta);

                Categoria? categoria = await db.Table<Categoria>()
                .FirstOrDefaultAsync(r => r.idCategoria == meta.idCategoria);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TxtTipoContexto.Text = "META";
                    TxtDetalheContexto.Text = "Foco atual";
                    TxtNomeContexto.Text = meta.tituloMeta;
                    TxtCategoriaContexto.Text = categoria?.tituloCategoria?? "Sem categoria";
                    CategoriaContextoBorder.IsVisible = true;

                });
            }
            else if (Origem == OrigemCronometro.Sessao)
            {
                Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                 .FirstOrDefaultAsync(r => r.idSessao == IdSessao);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    TxtTipoContexto.Text = "SESSÃO";
                    TxtDetalheContexto.Text = "Foco atual";
                    TxtNomeContexto.Text = sessao.tituloSessao;
                    CategoriaContextoBorder.IsVisible = true;
                    
                });
            }
            

        }
        catch (Exception ex)
        {

            System.Diagnostics.Debug.WriteLine($"[ATHENA] Erro ao consultar: {ex}");

            await MostrarErroSeguro("Erro ao consultar", ex.Message);
        }
    }

    private void AtualizarProgressoMetaDiaria(
    double minutosEstudados,
    double metaMinutos)
    {
        double tempoAtual =
            CronometroService.TempoLiquido.TotalMinutes;

        double totalMinutos =
            minutosEstudados + tempoAtual;

        double progresso = 0;

        if (metaMinutos > 0)
        {
            progresso = totalMinutos / metaMinutos;
        }

        progresso = Math.Clamp(progresso, 0, 1);

        int percentual =
            (int)Math.Round(progresso * 100);

        TxtPercentualMeta.Text = $"{percentual}%";

        BarraProgressoMeta.Progress = progresso;

        if (metaMinutos <= 0)
        {
            TxtStatusProgresso.Text =
                "Meta sem objetivo definido";
        }
        else if (percentual >= 100)
        {
            TxtStatusProgresso.Text =
                "Meta diária concluída";
        }
        else
        {
            double restantes = metaMinutos - totalMinutos;

            TxtStatusProgresso.Text =
                $"{Math.Ceiling(restantes)} min restantes";
        }

        TxtMinutosEstudadosMeta.Text = $"{minutosEstudados:F0}";
        TxtMetaMinutos.Text = metaMinutos.ToString();
    }

    private async Task MostrarErroSeguro(string titulo, string mensagem)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Shell.Current is Shell shell)
                {
                    await shell.DisplayAlertAsync(titulo, mensagem, "OK");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ATHENA] Falha ao exibir alerta: {ex}");
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            DateTime inicioDoDia = DateTime.Now.Date;

            Usuario? usuario = await db.Table<Usuario>()
                .FirstOrDefaultAsync();

            var registros = await db.Table<Registro_Tempo>()
                .Where(r => r.data >= inicioDoDia)
                .ToListAsync();

            minutosEstudadosHoje = registros.Sum(r => r.tempoEstudo);
            metaDiariaMinutos = usuario?.metaDiaria ?? 0;

            AtualizarProgressoMetaDiaria(
                minutosEstudadosHoje,
                metaDiariaMinutos
            );

            if (!CronometroService.Iniciado)
            {
                SessaoIniciada = false;
                CronometroRodando = false;

                TxtCronometro.Text = "00:00:00";
                TxtTempoBruto.Text = "00:00:00";
                TxtTempoPausas.Text = "00:00:00";

                BtnPausar.Text = "Ⅱ    PAUSAR";
            }
            else
            {

                SessaoIniciada = true;
                Contador();
                CronometroRodando = CronometroService.Rodando;

                BtnPausar.Text = CronometroService.Rodando
                    ? "Ⅱ    PAUSAR"
                    : "▶    CONTINUAR";
            }

            CronometroService.GarantirEstudoLivre();

            Origem = CronometroService.Origem;
            IdMeta = CronometroService.IdMeta;
            IdSessao = CronometroService.IdSessao;

            TxtInicioSessao.Text =
                CronometroService.Iniciado
                    ? CronometroService.DataInicio.ToString("HH:mm")
                    : DateTime.Now.ToString("HH:mm");

            Usuario? user = await db.Table<Usuario>()
                    .FirstOrDefaultAsync();

            if (user?.fotoPerfil != null &&
                File.Exists(user.fotoPerfil))
            {
                ImagemPerfil.Source = ImageSource.FromFile(
                    user.fotoPerfil);
            }
            else
            {
                ImagemPerfil.Source = null;
            }

            await CarregarInformacoes();
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao carregar o cronômetro", ex.Message);
        }
    }

    public void Contador()
    {
        if (!TemporizadorTelaAtivo)
        {
            TemporizadorTelaAtivo = true;

            Dispatcher.StartTimer(
                TimeSpan.FromMilliseconds(100),
                () =>
                {
                    if (!SessaoIniciada)
                    {
                        TemporizadorTelaAtivo = false;
                        return false;
                    }

                    TxtCronometro.Text =
                        CronometroService.TempoLiquido.ToString(@"hh\:mm\:ss");

                    TxtTempoBruto.Text =
                        CronometroService.TempoBruto.ToString(@"hh\:mm\:ss");

                    TxtTempoPausas.Text =
                        CronometroService.TempoPausado.ToString(@"hh\:mm\:ss");

                    AtualizarProgressoMetaDiaria(
                    minutosEstudadosHoje,
                    metaDiariaMinutos
                );

                    return true;
                });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

    }

    private void BtnIniciar_Click(object sender, EventArgs e)
    {
        if (!CronometroService.Iniciado)
        {
            CronometroService.GarantirEstudoLivre();
            CronometroService.Iniciar();
        }

        SessaoIniciada = CronometroService.Iniciado;
        CronometroRodando = CronometroService.Rodando;

        if (!SessaoIniciada)
            return;

        BtnPausar.Text = CronometroRodando
            ? "Ⅱ    PAUSAR"
            : "▶    CONTINUAR";

        Contador();



    }
    private void BtnPausar_Click(object sender, EventArgs e)
    {
        if (!CronometroService.Iniciado)
            return;

        if (CronometroService.Rodando)
        {
            CronometroService.Pausar();
            CronometroRodando = false;
            BtnPausar.Text = "▶    CONTINUAR";
        }
        else
        {
            CronometroService.Continuar();
            CronometroRodando = true;
            BtnPausar.Text = "Ⅱ    PAUSAR";
        }
    }


    private async void BtnParar_Clicked(object sender, EventArgs e)
    {
        if (!CronometroService.Iniciado)
            return;

        var origem = CronometroService.Origem;
        var idMeta = CronometroService.IdMeta;
        var idSessao = CronometroService.IdSessao;

        ResultadoSessao? resultadoSessao =
            CronometroService.PrepararFinalizacao();

        if (resultadoSessao == null)
            return;

        SessaoIniciada = false;
        CronometroRodando = false;

        try
        {
            try
            {

            await Models_Cronometro.SalvarEstudo(origem, idSessao, idMeta, resultadoSessao);
            }
            catch (Exception ex)
            {
                CronometroService.Erro();

                SessaoIniciada = true;

                Contador();
                CronometroRodando = false;
                BtnPausar.Text = "▶    CONTINUAR";

                await DisplayAlertAsync("Erro", ex.Message, "Ok");

                return;
            }

            CronometroService.Limpar();

            minutosEstudadosHoje += resultadoSessao.MinutosLiquidos;
            if (origem == OrigemCronometro.Livre)
            {
                await DisplayAlertAsync(
                 "Estudo salvo",
                 "Seu estudo livre foi registrado com sucesso!",
                 "OK"
                );
            }
            else if (origem == OrigemCronometro.Meta)
            {

                await Shell.Current.GoToAsync("//EscolherMeta");

                await Shell.Current.GoToAsync(
                    $"{nameof(InterfaceMeta)}?idMeta={idMeta}");


                return;
            }
            else if (origem == OrigemCronometro.Sessao)
            {
                await Shell.Current.GoToAsync("//EscolherMeta");

                await Shell.Current.GoToAsync(
                    $"{nameof(InterfaceMeta)}?idMeta={idMeta}");

                await Shell.Current.GoToAsync(
                    nameof(InterfaceSessao),
                    new Dictionary<string, object>
                    {
                        ["IdSessao"] = idSessao.Value,
                        ["IdMeta"] = idMeta.Value,
                        ["minutosBrutos"] = resultadoSessao.MinutosBrutos,
                        ["minutosLiquidos"] = resultadoSessao.MinutosLiquidos
                    });


                return;
            }
            else
            {
                throw new Exception("Origem do cronômetro inválida.");
            }


            TxtCronometro.Text = "00:00:00";
            TxtTempoBruto.Text = "00:00:00";
            TxtTempoPausas.Text = "00:00:00";

            IdMeta = null;
            IdSessao = null;
            Origem = OrigemCronometro.Nenhum;
        }
        catch (Exception ex)
        {
            SessaoIniciada = true;

            await DisplayAlertAsync("Erro", ex.Message, "Ok");
        }
    }
}