using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
using ATHENA.Services;
using Microsoft.Maui.ApplicationModel;
using SQLite;
using static ATHENA.NewPage1;

namespace ATHENA;

public partial class MainPage : ContentPage
{

    public int? IdMeta { get; set; }

    readonly SQLiteAsyncConnection db;
    bool _carregando;

    public MainPage()
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
            DateTime inicioDoDia = DateTime.Now.Date;

            int? quantidadeMetas = await db.Table<Meta>().CountAsync();
            int? quantidadeCategorias = await db.Table<Categoria>().CountAsync();
            int? quantidadeSessoes = await db.Table<Sessao_Estudo>()
                .Where(r => r.DataCriacao >= inicioDoDia)
                .CountAsync();

            Meta? meta = await db.Table<Meta>()
           .OrderByDescending(m => m.minutosEstudados)
           .FirstOrDefaultAsync();

            if (meta == null)
                return;

            IdMeta = meta.idMeta;

            var registros = await db.Table<Registro_Tempo>()
                .Where(r => r.data >= inicioDoDia)
                .ToListAsync();

            double tempoEstudado = registros.Sum(r => r.tempoEstudo);

            double tempoTotal = registros.Sum(r => r.tempoEstudo);
            int total = (int)tempoTotal;
            int horas = total / 60;
            int minutos = total % 60;

            if (meta is not null)
            {
                lblTituloMaiorSessao.Text = meta.tituloMeta;
                lblDescricaoMaiorSessao.Text = meta.descricaoMeta;
                lblMinutosSessao.Text =
                    $"{meta.minutosEstudados ?? 0} / {meta.metaMinutos} min";

                AreaMaiorMetaTitulo.IsVisible = true;
                CardMaiorMeta.IsVisible = true;
                BtnEntrarMaiorMeta.IsVisible = true;
            }


            MainThread.BeginInvokeOnMainThread(() =>
            {
                lblTituloMaiorSessao.Text = meta.tituloMeta;
                lblDescricaoMaiorSessao.Text = meta.descricaoMeta;
                lblMinutosSessao.Text = $"{meta.minutosEstudados.ToString()} minutos estudados";
                lblQuantidadeMetas.Text = quantidadeMetas.ToString();
                lblQuantidadeCategorias.Text = quantidadeCategorias.ToString();
                lblQuantidadeSessoes.Text = quantidadeSessoes.ToString();
                lblTempoEstudado.Text = $"{horas}h {minutos:D2}min";
            });
        }
        catch (Exception ex)
        {
   
            System.Diagnostics.Debug.WriteLine($"[ATHENA] Erro ao consultar: {ex}");

            await MostrarErroSeguro("Erro ao consultar", ex.Message);
        }
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
    private async Task InicializarBanco()
    {
        await db.CreateTableAsync<Meta>();
        await db.CreateTableAsync<Categoria>();
        await db.CreateTableAsync<Sessao_Estudo>();
        await db.CreateTableAsync<Registro_Tempo>();
        await db.CreateTableAsync<MensagemChat>();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        AreaMaiorMetaTitulo.IsVisible = false;
        CardMaiorMeta.IsVisible = false;
        BtnEntrarMaiorMeta.IsVisible = false;

        if (_carregando)
            return;

        _carregando = true;

        try
        {

            DateTime agora = DateTime.Now;
            int hora = agora.Hour;

            Usuario? usuario = await db.Table<Usuario>()
                        .FirstOrDefaultAsync();

            if (usuario == null)
            {
                await Models_Usuario.LoginUsuario();

                usuario = await db.Table<Usuario>()
                    .FirstOrDefaultAsync();
            }

            string saudacao;
            if (hora >= 5 && hora < 12)
                saudacao = "Bom dia";
            else if (hora >= 12 && hora < 18)
                saudacao = "Boa tarde";
            else
                saudacao = "Boa noite";

            string mensagem = $"{saudacao}, " + usuario.nome;

            lblBoasVindas.Text = mensagem;


            Usuario? user = await db.Table<Usuario>()
                .FirstOrDefaultAsync();

            if (user?.fotoPerfil != null &&
                File.Exists(usuario.fotoPerfil))
            {
                ImagemPerfil.Source = ImageSource.FromFile(
                    user.fotoPerfil);
            }
            else
            {
                ImagemPerfil.Source = null;
            }

            await InicializarBanco();

            await CarregarInformacoes();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ATHENA] Erro ao carregar a tela: {ex}");
            await MostrarErroSeguro("Erro ao carregar a tela", ex.Message);
        }
        finally
        {
            _carregando = false;
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        bool configurado = CronometroService.ConfigurarContexto(
    NewPage1.OrigemCronometro.Livre);

        if (!configurado)
        {
            await DisplayAlertAsync(
                "Cronômetro ativo",
                "Finalize o estudo atual antes de selecionar outro tipo de estudo.",
                "OK");

            return;
        }

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}");
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//EscolherMeta");
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//EscolherMeta");
        await Shell.Current.GoToAsync($"{nameof(InterfaceMeta)}?idMeta={IdMeta}");
    }

    private async void BtnMeuPerfil_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PerfilPage");
    }
}