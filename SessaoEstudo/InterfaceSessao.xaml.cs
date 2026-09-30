using ATHENA.Database;
using Microsoft.Maui.Platform;
using SQLite;
using static ATHENA.NewPage1;

namespace ATHENA.SessaoEstudo;

[QueryProperty(nameof(IdSessao), "idSessao")]
[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class InterfaceSessao : ContentPage
{
    public int IdSessao { get; set; }
    public int IdMeta { get; set; }

    SQLiteAsyncConnection db;

    public InterfaceSessao()
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

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                .Where(mbox => mbox.idSessao == IdSessao)
                .FirstOrDefaultAsync();

            if(sessao == null)
            {
                return;
            }

            int? IdMeta = sessao.idMeta;

            int? TempoEstudado = sessao.tempoEstudadoMinutos;

            if (TempoEstudado != null)
            {
                TimeSpan tempo = TimeSpan.FromHours((Double)TempoEstudado);
                TempoEstudo.Text = tempo.ToFormattedString(Title);
            }

            Meta? meta = await db.Table<Meta>()
                .Where(mbox => mbox.idMeta == IdMeta)
                .FirstOrDefaultAsync();

            if (meta != null)
            {
                TituloTXT.Text = sessao.tituloSessao;
            }

            //if (NewPage1.ResultadoAtual != null)
            //{
            //    TimeSpan tempo = NewPage1.ResultadoAtual.TempoLiquido;

            //    TempoEstudo.Text = tempo.ToString(@"hh\:mm\:ss");

            //    NewPage1.ResultadoAtual = null;
            //}

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private async void Contar_tempo_Clicked(object sender, EventArgs e)
    {
        await Task.Delay(100);

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}", new Dictionary<string, object>
        {
            ["Origem"] = OrigemCronometro.Sessao,
            ["IdMeta"] = IdMeta,
            ["IdSessao"] = IdSessao,
        });

    }

    private void Salvar_Clicked(object sender, EventArgs e)
    {
        //if(Salvar_Clicked(sender, e))
        //{
        //    Models.Models_SessaoEstudo.ApagarSessao();
        //}
    }

}