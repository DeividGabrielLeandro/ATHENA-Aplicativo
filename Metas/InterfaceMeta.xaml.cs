
using ATHENA.Database;
using ATHENA.Models;
using ATHENA.SessaoEstudo;
using Microsoft.Maui.Platform;
using SQLite;
using static ATHENA.NewPage1;

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

            Meta? meta = await db.Table<Meta>()
     .FirstOrDefaultAsync(m => m.idMeta == IdMeta);

            if (meta is null)
            {
                return;
            }

            ConteudoMeta.IsVisible = true;

            ConteudoMeta.BindingContext = meta;

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
            .Where(s => s.idMeta == meta.idMeta)
            .OrderByDescending(s => s.idSessao)
            .FirstOrDefaultAsync();

            if (sessao == null)
                return;
            UltimaSessaoCard.IsVisible = true;
            TimeSpan tempo = TimeSpan.FromMinutes((double)sessao.tempoEstudadoMinutos);

            UltimaSessaoTempoTXT.Text = tempo.ToFormattedString(Title);
            UltimaSessaoTituloTXT.Text = sessao.tituloSessao;
            UltimaSessaoDataTXT.Text = sessao.DataFim.ToString();
            UltimaSessaoDescricaoTXT.Text = sessao.descricaoSessao;



        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            DateTime dataInicio = DateTime.Now;
            int IdSessao = await Models.Models_SessaoEstudo.CriarSessao(IdMeta, dataInicio);

            await Shell.Current.GoToAsync(
            $"{nameof(InterfaceSessao)}?idSessao={IdSessao}&idMeta={IdMeta}");
        }

        catch (Exception ex)
        {
            await DisplayAlertAsync("erro", ex.Message, "Sair");
        }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        bool configurado = CronometroService.ConfigurarContexto(
        NewPage1.OrigemCronometro.Meta,
        idMeta: meta.idMeta);

        if (!configurado)
        {
            await DisplayAlertAsync(
            "Cronômetro ativo",
            "Finalize o estudo atual antes de selecionar outra meta.",
            "OK");

            return;

        }

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}");
    }
    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        Button botao = (Button)sender;

        Meta meta = (Meta)botao.BindingContext;

        int idMeta = meta.idMeta;

        await Shell.Current.GoToAsync($"{nameof(EditarMeta)}?idMeta={idMeta}");
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(HistoridoSessao)}?idMeta={IdMeta}");
    }

    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

}