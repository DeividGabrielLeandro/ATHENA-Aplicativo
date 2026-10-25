using ATHENA.Database;
using ATHENA.Metas.xaml;
using SQLite;
using static ATHENA.Metas.xaml.EscolherMeta;

namespace ATHENA.Categorias;

public partial class ListarCategoria : ContentPage
{

    public class Informacoes
    {
        public int idCategoria { get; set; }
        public string? tituloCategoria { get; set; }

        public string? descricaoCategoria { get; set; }

        public int metasEmCategoria { get; set; }

        public int metasConcluidas { get; set; }

        public string TextoMetas { get; set; } = string.Empty;

        public string TextoConcluidas { get; set; } = string.Empty;

        public string TextoProgresso { get; set; } = "0%";

        public double ProgressoConclusao { get; set; }

        public bool ProgressoConcluido { get; set; }
    }


    SQLiteAsyncConnection db;

    public ListarCategoria()
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

            List<Categoria> categorias = await db.Table<Categoria>().ToListAsync();
            List<Meta> metas = await db.Table<Meta>().ToListAsync();

            int metasEmCategoria = await db.Table<Meta>()
                    .Where(r => r.idCategoria != null)
                    .CountAsync();
            int metasSemCategoria = await db.Table<Meta>()
                    .Where(r => r.idCategoria == null)
                    .CountAsync();



            MainThread.BeginInvokeOnMainThread(() =>
            {

                lblTotalCategorias.Text = categorias.Count().ToString();
                lblMetasComCategoria.Text = metasEmCategoria.ToString();
                lblMetasSemCategoria.Text = metasSemCategoria.ToString();
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
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            List<Meta> metas = await db.Table<Meta>().ToListAsync();
            List<Categoria> categorias = await db.Table<Categoria>().ToListAsync();
            var listaCategoria = new List<Informacoes>();


            foreach (var categoria in categorias)
            {
                int metaCategoria = metas.Count(
                    m => m.idCategoria == categoria.idCategoria);

                int metaConcluidaCategoria = metas.Count(
                    m =>
                        m.idCategoria == categoria.idCategoria &&
                        m.status == "Concluida");

                double progresso = metaCategoria > 0
                    ? (double)metaConcluidaCategoria / metaCategoria
                    : 0;

                bool progressoConcluido =
                    metaCategoria > 0 &&
                    metaConcluidaCategoria == metaCategoria;

                string textoMetas =
                    metaCategoria == 1
                        ? "1 meta"
                        : $"{metaCategoria} metas";

                string textoConcluidas =
                    metaConcluidaCategoria == 1
                        ? "1 concluída"
                        : $"{metaConcluidaCategoria} concluídas";

                listaCategoria.Add(new Informacoes
                {
                    idCategoria = categoria.idCategoria,

                    tituloCategoria = categoria.tituloCategoria,
                    descricaoCategoria = categoria.descricaoCategoria,

                    metasEmCategoria = metaCategoria,
                    metasConcluidas = metaConcluidaCategoria,

                    TextoMetas = textoMetas,
                    TextoConcluidas = textoConcluidas,

                    ProgressoConclusao = progresso,

                    TextoProgresso = $"{progresso * 100:0}%",

                    ProgressoConcluido = progressoConcluido
                });
            }

            lista.ItemsSource = listaCategoria;

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
            Console.WriteLine(ex.Message);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(CriarCategoria)}");
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Informacoes categoria = (Informacoes)botao.BindingContext;

        int idCategoria = categoria.idCategoria;


        await Shell.Current.GoToAsync($"{nameof(InterfaceCategoria)}?idCategoria={idCategoria}");
    }

    private async void BuscaCategoriaTXT_Completed(object sender, EventArgs e)
    {
        string busca = BuscaCategoriaTXT.Text;

        List<Categoria> categorias = await db.Table<Categoria>()
            .Where(r => r.tituloCategoria.Contains(busca))
            .ToListAsync();

        List<Meta> metas = await db.Table<Meta>().ToListAsync();
        var listaCategoria = new List<Informacoes>();


        foreach (var categoria in categorias)
        {
            int metaCategoria = metas.Count(
                m => m.idCategoria == categoria.idCategoria);

            int metaConcluidaCategoria = metas.Count(
                m =>
                    m.idCategoria == categoria.idCategoria &&
                    m.status == "Concluida");

            double progresso = metaCategoria > 0
                ? (double)metaConcluidaCategoria / metaCategoria
                : 0;

            bool progressoConcluido =
                metaCategoria > 0 &&
                metaConcluidaCategoria == metaCategoria;

            string textoMetas =
                metaCategoria == 1
                    ? "1 meta"
                    : $"{metaCategoria} metas";

            string textoConcluidas =
                metaConcluidaCategoria == 1
                    ? "1 concluída"
                    : $"{metaConcluidaCategoria} concluídas";

            listaCategoria.Add(new Informacoes
            {
                idCategoria = categoria.idCategoria,

                tituloCategoria = categoria.tituloCategoria,
                descricaoCategoria = categoria.descricaoCategoria,

                metasEmCategoria = metaCategoria,
                metasConcluidas = metaConcluidaCategoria,

                TextoMetas = textoMetas,
                TextoConcluidas = textoConcluidas,

                ProgressoConclusao = progresso,

                TextoProgresso = $"{progresso * 100:0}%",

                ProgressoConcluido = progressoConcluido
            });
            lista.ItemsSource = listaCategoria;
        }
    }
}