using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Services;
using Microsoft.Maui.Platform;
using SQLite;
using static ATHENA.NewPage1;

namespace ATHENA.SessaoEstudo;

[QueryProperty(nameof(IdSessao), "idSessao")]
[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class InterfaceSessao : ContentPage, IQueryAttributable
{
    public int IdSessao { get; set; }
    public int IdMeta { get; set; }
    public double minutosBrutos { get; set; }
    public double minutosLiquidos { get; set; }

    public double TempoPausa => minutosBrutos - minutosLiquidos;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("IdSessao", out object? idSessao))
            IdSessao = (int)idSessao;

        if (query.TryGetValue("IdMeta", out object? idMeta))
            IdMeta = (int)idMeta;

        if (query.TryGetValue("minutosBrutos", out object? minutosBrutos))
            this.minutosBrutos = (double)minutosBrutos;

        if (query.TryGetValue("minutosLiquidos", out object? minutosLiquidos))
            this.minutosLiquidos = (double)minutosLiquidos;

    }

    bool SessaoSalva = false;
    bool IndoParaCronometro = false;

    SQLiteAsyncConnection db;

    public InterfaceSessao()
    {
        InitializeComponent();

        string dbPath = Path.Combine(

            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db"
        );

        db = new SQLiteAsyncConnection(dbPath);
    }


    protected override async void OnAppearing()
    {

        try
        {
            base.OnAppearing();

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                .Where(mbox => mbox.idSessao == IdSessao)
                .FirstOrDefaultAsync();

            if (sessao == null)
            {
                return;
            }

            int? IdMeta = sessao.idMeta;

            double? TempoEstudado = sessao.tempoEstudadoMinutos;

            double? Pausa = sessao.duracaoMinutos - sessao.tempoEstudadoMinutos;

            TimeSpan tempo = TimeSpan.FromMinutes(sessao.tempoEstudadoMinutos ?? 0);

            TempoEstudo.Text = tempo.ToFormattedString(Title);
            TxtTempoBruto.Text = TimeSpan.FromMinutes(sessao.duracaoMinutos ?? 0).ToString(@"h\:mm");
            TxtTempoPausa.Text = TimeSpan.FromMinutes(Pausa ?? 0).ToString(@"h\:mm");


            Meta? meta = await db.Table<Meta>()
                .Where(mbox => mbox.idMeta == IdMeta)
                .FirstOrDefaultAsync();

            if (meta != null)
            {
                TituloTXT.Text = sessao.tituloSessao;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private async void Contar_tempo_Clicked(object sender, EventArgs e)
    {
        IndoParaCronometro = true;
        await Task.Delay(100);

        bool configurado = CronometroService.ConfigurarContexto(
            NewPage1.OrigemCronometro.Sessao,
            idMeta: IdMeta,
            idSessao: IdSessao);

        if (!configurado)
        {
            await DisplayAlertAsync(
                "Cronômetro ativo",
                "Finalize o estudo atual antes de selecionar outra sessão.",
                "OK");

            return;
        }

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}");
    }

    private async void Salvar_Clicked_1(object sender, EventArgs e)
    {
        SessaoSalva = true;
        string titulo = TituloTXT.Text;

        if (string.IsNullOrWhiteSpace(titulo))
        {
            titulo = "Sem descrição";
        }

        string descricao = DescricaoTXT.Text;
        DateTime dataFim = DateTime.Now;

        await Models.Models_SessaoEstudo.SalvarPausaSessao(IdSessao, TempoPausa, null);
        await Models.Models_SessaoEstudo.FinalizarSessao(titulo, descricao, dataFim, minutosBrutos, minutosLiquidos, IdSessao);

        await DisplayAlertAsync("Sucesso!", "Sessão finalizada com sucesso", "Ok");

        await Shell.Current.GoToAsync("//EscolherMeta");

        await Shell.Current.GoToAsync(
            $"{nameof(InterfaceMeta)}?idMeta={IdMeta}");
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            CronometroPreferences cronometro = new CronometroPreferences();
            cronometro.Carregar();

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                    .Where(mbox => mbox.idSessao == IdSessao)
                    .FirstOrDefaultAsync();

            if (cronometro.Iniciado && cronometro.IdSessao == IdSessao)
            {
                await Shell.Current.GoToAsync("..");
            }
            else if (sessao == null || sessao.DataFim != null) 
            {
                await Shell.Current.GoToAsync("..");
            }
            else if ((sessao.duracaoMinutos ?? 0) == 0)
            {
                if (!CronometroService.Iniciado && CronometroService.IdSessao == IdSessao)
                    CronometroService.Limpar();

                await Models.Models_SessaoEstudo.DeletarSessao(IdSessao);
                await Shell.Current.GoToAsync("..");
            }
            else 
            {
                bool resposta = await DisplayAlertAsync(
                "Sair da sessão?",
                "Se você sair agora, a sessão de estudo será apagada e não poderá ser recuperada. O tempo estudado acumulado na sua meta será mantido e salvo.\n\nDeseja realmente sair?",
                "Sair",
                "Continuar estudando"
            );

                if (resposta)
                {
                    if (!CronometroService.Iniciado && CronometroService.IdSessao == IdSessao) 
                        CronometroService.Limpar();

                    await Models.Models_SessaoEstudo.DeletarSessao(IdSessao);
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "ok");
        }
    }
    protected override bool OnBackButtonPressed()
    {
        Button_Clicked(this, EventArgs.Empty);
        return true;
    }
}
