using ATHENA.Database;
using ATHENA.Models;
namespace ATHENA;

using ATHENA.Categorias;
using SQLite;

public partial class CriarCategoria : ContentPage
{
    private List<int> idsMetasSelecionadas = new();

    SQLiteAsyncConnection db;

    public CriarCategoria()
    {
        InitializeComponent();

        string dbPath = Path.Combine(

            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db"
        );

        db = new SQLiteAsyncConnection(dbPath);
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            string titulo = TituloTXT.Text;
            string descricao = DescricaoTXT.Text;

            if (string.IsNullOrWhiteSpace(titulo))
            {
                await DisplayAlertAsync("Atenção", "O título da categoria é obrigatório", "OK");
                return;
            }

            int idCategoria = await Models_Categoria.CriarCategoria(titulo, descricao);

            foreach (int idMeta in idsMetasSelecionadas)
            {
                await Models_Categoria.AdicionarMetaCategoria(idMeta, idCategoria);
            }

            await DisplayAlertAsync(
                "Categoria criada com sucesso!",
                "Sua categoria foi registrada e está pronta para ajudar você a organizar suas metas de estudo.",
                "Continuar");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void BtnSelecionarMetas_Clicked(object sender, EventArgs e)
    {
        try
        {
            List<Meta> metas = await db.Table<Meta>().ToListAsync();

            ListaMetas.ItemsSource = metas;
            ListaMetasContainer.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void ListaMetas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        idsMetasSelecionadas.Clear();

        foreach(var item in e.CurrentSelection)
        {
            if (item is Meta meta)
            {
                idsMetasSelecionadas.Add(meta.idMeta);
            }
        }
    }

  
}