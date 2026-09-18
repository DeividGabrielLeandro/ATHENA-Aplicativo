namespace ATHENA;
using SQLite;
using ATHENA.Database;
using ATHENA.Models;

public partial class NewPage2 : ContentPage
{
    SQLiteAsyncConnection db;

    public NewPage2()
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
        await db.CreateTableAsync<Meta>();

        string prioridade = "Sem prioridade";
        string status = "Em andamento";
        string titulo = TituloTXT.Text;
        string descricao = DescricaoTXT.Text;
        int minutos = int.Parse(MetaMinutosTXT.Text);

        await Models_Meta.CriarMeta(titulo, descricao, minutos, prioridade, status);


        var metas = await db.Table<Meta>().ToListAsync();

        ResultadoTXT.Text = "";

        foreach (var item in metas)
        {
            ResultadoTXT.Text +=
                $"Título: {item.tituloMeta}\n" +
                $"Descrição: {item.descricaoMeta}\n" +
                $"Minutos: {item.metaMinutos}\n" +
                $"Prioridade: {item.prioridadeMeta}\n" +
                $"Status: {item.status}\n\n";
        }


    }
}