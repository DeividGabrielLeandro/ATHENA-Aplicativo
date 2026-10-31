
using ATHENA.Database;
using ATHENA.Models;
using SQLite;
using System.Linq.Expressions;

namespace ATHENA.Metas.xaml;

public partial class EscolherMeta : ContentPage
{
    public class Informacoes 
    {
            public string? tituloMeta { get; set; }
            public string? descricaoMeta { get; set; }
            public string? prioridadeMeta { get; set; }
            public string? status { get; set; }
            public int? minutosEstudados { get; set; }
            public int metaMinutos { get; set; }
            public double Progresso { get; set; }
            public string? CategoriaTexto { get; set; }
            public int idMeta { get; set; }
            public string? dataLimite { get; set; }
    }

    SQLiteAsyncConnection db;

    public EscolherMeta()
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

            int quantidadeMetas = await db.Table<Meta>().CountAsync();
            int quantidadeMetasPendentes = await db.Table<Meta>()
                .Where(r => r.status == "Em andamento")
                .CountAsync();
            int quantidadeMetasConcluidas = await db.Table<Meta>()
                .Where(r => r.status == "Concluida")
                .CountAsync();

         

            MainThread.BeginInvokeOnMainThread(() =>
            {
             
                lblTotalMetas.Text = quantidadeMetas.ToString();
                lblMetasPendentes.Text = quantidadeMetasPendentes.ToString();
                lblMetasConcluidas.Text = quantidadeMetasConcluidas.ToString();
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
    private Color ObterCor(string nome)
    {
        return (Color)Application.Current.Resources[nome];
    }

    private void AplicarEstadoNormal(Button botao)
    {
        botao.BackgroundColor = ObterCor("AthenaCard");
        botao.BorderColor = ObterCor("AthenaBorderSoft");
        botao.TextColor = ObterCor("AthenaTextSecondary");
    }

    private void SelecionarFiltro(Button botaoSelecionado)
    {
        AplicarEstadoNormal(BtnFiltroTodas);
        AplicarEstadoNormal(BtnFiltroPendentes);
        AplicarEstadoNormal(BtnFiltroConcluidas);

        if (botaoSelecionado == BtnFiltroConcluidas)
        {
            botaoSelecionado.BackgroundColor =
                ObterCor("AthenaSuccessBackground");

            botaoSelecionado.BorderColor =
                ObterCor("AthenaSuccess");

            botaoSelecionado.TextColor =
                ObterCor("AthenaSuccess");
        }
        else
        {
            botaoSelecionado.BackgroundColor =
                ObterCor("AthenaBlueBackground");

            botaoSelecionado.BorderColor =
                ObterCor("AthenaBlue");

            botaoSelecionado.TextColor =
                ObterCor("AthenaTextPrimary");
        }
    }


    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
    
            AplicarEstadoNormal(BtnFiltroTodas);
            AplicarEstadoNormal(BtnFiltroPendentes);
            AplicarEstadoNormal(BtnFiltroConcluidas);

            List<Meta> metas = await db.Table<Meta>().ToListAsync();
            var categorias = await db.Table<Categoria>().ToListAsync();
            var listaMetas = new List<Informacoes>();

            foreach(var meta in metas)
            {
                var categoria = categorias.FirstOrDefault(
        c => c.idCategoria == meta.idCategoria);

                listaMetas.Add(new Informacoes
                {
                    idMeta = meta.idMeta,
                    tituloMeta = meta.tituloMeta,
                    descricaoMeta = meta.descricaoMeta,
                    prioridadeMeta = meta.prioridadeMeta,
                    status = meta.status,
                    minutosEstudados = meta.minutosEstudados,
                    metaMinutos = meta.metaMinutos,
                    Progresso = meta.Progresso,
                    CategoriaTexto = categoria?.tituloCategoria,
                    dataLimite = meta.dataLimite?.ToString("dd MMM"),
                });
            }
            lista.ItemsSource = listaMetas;


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

            SelecionarFiltro(BtnFiltroTodas);

            await CarregarInformacoes();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("NewPage2");
    }


    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        try
        { Button botao = (Button)sender;

            Informacoes meta = (Informacoes)botao.BindingContext;

            int idMeta = meta.idMeta;

            await Shell.Current.GoToAsync($"{nameof(InterfaceMeta)}?idMeta={idMeta}");
        } catch (Exception ex) 
        {
            await DisplayAlertAsync("Erro", ex.Message, "sair");
        }
    }

    private async void BtnFiltroPendentes_Clicked(object sender, EventArgs e)
    {
        try
        {
            SelecionarFiltro(BtnFiltroPendentes);

            List<Meta> metas = await db.Table<Meta>()
              .Where(r => r.status == "Em andamento")
              .ToListAsync();

            var categorias = await db.Table<Categoria>().ToListAsync();
            var listaMetas = new List<Informacoes>();

            foreach (var meta in metas)
            {
                var categoria = categorias.FirstOrDefault(
        c => c.idCategoria == meta.idCategoria);



                listaMetas.Add(new Informacoes
                {
                    idMeta = meta.idMeta,
                    tituloMeta = meta.tituloMeta,
                    descricaoMeta = meta.descricaoMeta,
                    prioridadeMeta = meta.prioridadeMeta,
                    status = meta.status,
                    minutosEstudados = meta.minutosEstudados,
                    metaMinutos = meta.metaMinutos,
                    Progresso = meta.Progresso,
                    CategoriaTexto = categoria?.tituloCategoria,
                    dataLimite = meta.dataLimite?.ToString("dd MMM"),
                });
            }
            lista.ItemsSource = listaMetas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
    private async void BtnFiltroTodas_Clicked(object sender, EventArgs e)
    {
        SelecionarFiltro(BtnFiltroTodas);
        OnAppearing();
    }

    private async void BtnFiltroConcluidas_Clicked(object sender, EventArgs e)
    {
        try
        {
            SelecionarFiltro(BtnFiltroConcluidas);

            List<Meta> metas = await db.Table<Meta>()
                .Where(r => r.status == "Concluida")
                .ToListAsync();

            var categorias = await db.Table<Categoria>().ToListAsync();
            var listaMetas = new List<Informacoes>();

            foreach (var meta in metas)
            {
                var categoria = categorias.FirstOrDefault(
        c => c.idCategoria == meta.idCategoria);



                listaMetas.Add(new Informacoes
                {
                    idMeta = meta.idMeta,
                    tituloMeta = meta.tituloMeta,
                    descricaoMeta = meta.descricaoMeta,
                    prioridadeMeta = meta.prioridadeMeta,
                    status = meta.status,
                    minutosEstudados = meta.minutosEstudados,
                    metaMinutos = meta.metaMinutos,
                    Progresso = meta.Progresso,
                    CategoriaTexto = categoria?.tituloCategoria,
                    dataLimite = meta.dataLimite?.ToString("dd MMM"),
                });
            }
            lista.ItemsSource = listaMetas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void BuscaMetaTXT_Completed(object sender, EventArgs e)
    {
        try
        {
            string busca = BuscaMetaTXT.Text;

            List<Meta> metas = await db.Table<Meta>()
                .Where(r =>  r.tituloMeta.Contains(busca))
                .ToListAsync();

            var categorias = await db.Table<Categoria>().ToListAsync();
            var listaMetas = new List<Informacoes>();

            foreach (var meta in metas)
            {
                var categoria = categorias.FirstOrDefault(
        c => c.idCategoria == meta.idCategoria);



                listaMetas.Add(new Informacoes
                {
                    idMeta = meta.idMeta,
                    tituloMeta = meta.tituloMeta,
                    descricaoMeta = meta.descricaoMeta,
                    prioridadeMeta = meta.prioridadeMeta,
                    status = meta.status,
                    minutosEstudados = meta.minutosEstudados,
                    metaMinutos = meta.metaMinutos,
                    Progresso = meta.Progresso,
                    CategoriaTexto = categoria?.tituloCategoria,
                    dataLimite = meta.dataLimite?.ToString("dd MMM"),
                });
            }
            lista.ItemsSource = listaMetas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
}