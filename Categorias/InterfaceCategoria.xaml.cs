using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
using SQLite;

namespace ATHENA.Categorias;


[QueryProperty(nameof(IdCategoria), "idCategoria")]
public partial class InterfaceCategoria : ContentPage
{
    public int IdCategoria { get; set; }


    SQLiteAsyncConnection db;

    public InterfaceCategoria()
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

        try
        {
            base.OnAppearing();

            List<Meta> metaCategoria = await db.Table<Meta>()
                .Where(mbox => mbox.idCategoria == IdCategoria)
                .ToListAsync();

            listaMeta.ItemsSource = metaCategoria.Take(3);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            base.OnAppearing();

            var metas = await db.Table<Meta>()
                .Where(m => m.idCategoria != IdCategoria)
                .ToListAsync();

            listaMetaEscolha.ItemsSource = metas;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void ButtonCriarMeta_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(NewPage2)}?idCategoria={IdCategoria}");
        }
        catch(Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "ok");
        }
    }



    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Models_Categoria.DeletarCategoria(IdCategoria);
        await Shell.Current.GoToAsync("..");
    }

    [Obsolete]
    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        try
        {
            Button botao = (Button)sender;

            Meta meta = (Meta)botao.BindingContext;

            int idMeta = meta.idMeta;

            await Models_Categoria.AdicionarMetaCategoria(idMeta, IdCategoria);

            var metas = await db.Table<Meta>()
           .Where(m => m.idCategoria == IdCategoria)
           .ToListAsync();

            listaMeta.ItemsSource = metas.Take(3);

            var metasDisponiveis = await db.Table<Meta>()
                .Where(m => m.idCategoria != IdCategoria)
                .ToListAsync();

            listaMetaEscolha.ItemsSource = metasDisponiveis;

            await DisplayAlert("Sucesso!", "Meta adicionada", "ok");
        }
        catch(Exception ex)
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

    private async void VerMetas_Clicked(object sender, EventArgs e)
    {
        MetasCategoria.CategoriaSelecionada = IdCategoria;

        await Shell.Current.GoToAsync(nameof(MetasCategoria));
    }
}