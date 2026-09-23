using ATHENA.Database;
using ATHENA.Metas.xaml;
using SQLite;

namespace ATHENA.Categorias;

public partial class MetasCategoria : ContentPage
{
    public static int CategoriaSelecionada { get; set; }

    public int IdCategoria { get; set; }

    SQLiteAsyncConnection db;

    public MetasCategoria()
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

            List<Categoria> categoria = await db.Table<Categoria>()
                .Where(mbox => mbox.idCategoria == IdCategoria)
                .ToListAsync();

            listaCategoria.ItemsSource = categoria;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        IdCategoria = CategoriaSelecionada;

        try
        {
            var metas = await db.Table<Meta>()
                .Where(m => m.idCategoria == IdCategoria)
                .ToListAsync();

            listaMeta.ItemsSource = metas;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }

    }

    private async void EscolherMeta_Clicked(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        int idMeta = meta.idMeta;

        await Shell.Current.GoToAsync($"{nameof(InterfaceMeta)}?idMeta={idMeta}");
    }
}