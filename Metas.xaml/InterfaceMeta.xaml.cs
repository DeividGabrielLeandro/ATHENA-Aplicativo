
using ATHENA.Database;
using SQLite;

namespace ATHENA.Metas.xaml;

[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class InterfaceMeta : ContentPage
{
    public int IdMeta { get; set; }

    SQLiteAsyncConnection db;

    public InterfaceMeta()
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

            List<Meta> metas = await db.Table<Meta>()
                .Where(mbox  => mbox.idMeta == IdMeta)
                .ToListAsync();

            listaMetas.ItemsSource = metas;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
       
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        int idMeta = meta.idMeta;

        await Shell.Current.GoToAsync($"{nameof(NewPage1)}?idMeta={idMeta}");
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        int idMeta = meta.idMeta;

        await Shell.Current.GoToAsync($"{nameof(EditarMeta)}?idMeta={idMeta}");
    }
}