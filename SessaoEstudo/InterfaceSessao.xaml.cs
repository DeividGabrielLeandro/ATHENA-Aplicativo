using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
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

            if(sessao == null)
            {
                return;
            }

            int? IdMeta = sessao.idMeta;

            double? TempoEstudado = minutosLiquidos;

            if (TempoEstudado != null)
            {
                TimeSpan tempo = TimeSpan.FromHours((Double)TempoEstudado);
                TempoEstudo.Text = tempo.ToFormattedString(Title);
            }

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

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}", new Dictionary<string, object>
        {
            ["Origem"] = OrigemCronometro.Sessao,
            ["IdMeta"] = IdMeta,
            ["IdSessao"] = IdSessao,
        });

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
        await Models.Models_SessaoEstudo.FinalizarSessao(titulo,descricao,dataFim, minutosBrutos, minutosLiquidos,IdSessao);

        await DisplayAlertAsync("Sucesso!","Sessão finalizada com sucesso", "Ok");

        await Shell.Current.GoToAsync("//EscolherMeta");

        await Shell.Current.GoToAsync(
            $"{nameof(InterfaceMeta)}?idMeta={IdMeta}");
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (!SessaoSalva && !IndoParaCronometro)
        {
            await Models.Models_SessaoEstudo.DeletarSessao(IdSessao);
        }
    }

}