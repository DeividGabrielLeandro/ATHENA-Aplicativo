using ATHENA.Database;
using Microsoft.Maui.Platform;
using SQLite;

namespace ATHENA.SessaoEstudo;

[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class HistoridoSessao : ContentPage
{

    public int IdMeta { get; set; }


    SQLiteAsyncConnection db;

    public HistoridoSessao()
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

            List<Sessao_Estudo> sessao = await db.Table<Sessao_Estudo>()
                .Where(mbox => mbox.idMeta == IdMeta)
                .ToListAsync();

            foreach(var item in sessao)
            {
                double tempoBruto = item.duracaoMinutos ?? 0;
                double tempoLiquido = item.tempoEstudadoMinutos ?? 0;
                double tempoPausa = tempoBruto - tempoLiquido;

            }

            HistoricoSessao.ItemsSource = sessao;

            
            if (sessao == null)
            {
                return;
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


    }
}