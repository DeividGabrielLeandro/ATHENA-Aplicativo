namespace ATHENA;
using ATHENA.Database;
using ATHENA.Models;
using SQLite;

[QueryProperty(nameof(IdCategoriaParametro), "idCategoria")]
public partial class NewPage2 : ContentPage
{

    public int? IdCategoria { get; set; }

    SQLiteAsyncConnection db;

    public string IdCategoriaParametro
    {
        set
        {
            if (int.TryParse(value, out int id))
                IdCategoria = id;
            else
                IdCategoria = null;
        }
    }

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

        await Models_Meta.CriarMeta(titulo, descricao, minutos, prioridade, status, IdCategoria);


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

        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) {
            await DisplayAlert("Erro", ex.Message, "Sair");

        }
            
    }
}