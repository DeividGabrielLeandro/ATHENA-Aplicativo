using ATHENA.Database;
using ATHENA.Models;
namespace ATHENA;
using SQLite;

public partial class CriarCategoria : ContentPage
{

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
        await db.CreateTableAsync<Categoria>();

        string titulo = TituloTXT.Text;
        string descricao = DescricaoTXT.Text;

        await Models_Categoria.CriarCategoria(titulo, descricao);


        var metas = await db.Table<Categoria>().ToListAsync();

        ResultadoTXT.Text = "";

        foreach (var item in metas)
        {
            ResultadoTXT.Text +=
                $"Título: {item.tituloCategoria}\n" +
                $"Descrição: {item.descricaoCategoria}\n";
        }
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {

    }
}