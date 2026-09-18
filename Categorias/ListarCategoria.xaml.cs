using ATHENA.Database;
using ATHENA.Metas.xaml;
using SQLite;

namespace ATHENA.Categorias;

public partial class ListarCategoria : ContentPage
{

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
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            List<Categoria> categoria = await db.Table<Categoria>().ToListAsync();

            lista.ItemsSource = categoria.Take(6);
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
}