using ATHENA.Database;
using SQLite;

namespace ATHENA.SessaoEstudo;

[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class CriarSessao : ContentPage
{
    public static int IdMeta { get; set; }

    SQLiteAsyncConnection db;

    public CriarSessao()
    {
        InitializeComponent();

        string dbPath = Path.Combine(

            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db"
        );

        db = new SQLiteAsyncConnection(dbPath);
    }

    private async void BTNCriarSessao_Clicked(object sender, EventArgs e)
    {

        DateTime dataInicio = DateTime.Now;
        int IdSessao = await Models.Models_SessaoEstudo.CriarSessao(IdMeta, dataInicio);

        await Shell.Current.GoToAsync(
        $"{nameof(InterfaceSessao)}?idSessao={IdSessao}&idMeta={IdMeta}");
    }
}