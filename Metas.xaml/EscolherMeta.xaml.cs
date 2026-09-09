
using ATHENA.Database;
using ATHENA.Models;
using SQLite;
using System.Linq.Expressions;

namespace ATHENA.Metas.xaml;

public partial class EscolherMeta : ContentPage
{
    private int idMeta;

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
    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            List<Meta> metas = await db.Table<Meta>().ToListAsync();

            listaMetas.ItemsSource = metas;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        int idMeta = meta.idMeta;

        await Shell.Current.GoToAsync($"{nameof(NewPage1)}?idMeta={idMeta}");
    }
}