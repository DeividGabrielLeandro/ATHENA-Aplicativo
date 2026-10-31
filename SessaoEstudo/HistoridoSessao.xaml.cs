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
    .Where(s => s.idMeta == IdMeta)
    .OrderByDescending(s => s.idSessao)
    .ToListAsync();

            if (sessao.Count > 0)
            {
                var ultimaSessao = sessao[0];
                UltimoRegistroTXT.Text = ultimaSessao.tituloSessao;
            }
            else
            {
                UltimoRegistroTXT.Text = "Nenhuma sessão registrada";
            }

            double tempoTotal = sessao.Sum(s => s.tempoEstudadoMinutos ?? 0);
            TempoTotalTXT.Text = TimeSpan.FromMinutes(tempoTotal).ToString(@"hh\:mm\:ss");

            TotalSessoesTXT.Text = sessao.Count.ToString();

            HistoricoSessao.ItemsSource = sessao;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


    }

    private async void PesquisaSessaoTXT_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string busca = PesquisaSessaoTXT.Text;

            List<Sessao_Estudo> sessao = await db.Table<Sessao_Estudo>()
                    .Where(mbox => mbox.idMeta == IdMeta && mbox.tituloSessao.Contains(busca))
                    .OrderByDescending(s => s.idSessao)
                    .ToListAsync();

            foreach (var item in sessao)
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
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void FiltroDataSessao_DateSelected(
    object sender,
    DateChangedEventArgs e)
    {
        try
        {
            DateTime? dataSelecionada = e.NewDate;

            if (!dataSelecionada.HasValue)
            {
                return;
            }

            DateTime inicioDia = new DateTime(
                dataSelecionada.Value.Year,
                dataSelecionada.Value.Month,
                dataSelecionada.Value.Day);

            DateTime fimDia = inicioDia.AddDays(1);

            List<Sessao_Estudo> todasAsSessoes =
                await db.Table<Sessao_Estudo>()
                    .Where(sessao => sessao.idMeta == IdMeta)
                    .OrderByDescending(sessao => sessao.idSessao)
                    .ToListAsync();

            List<Sessao_Estudo> sessoesFiltradas =
                todasAsSessoes
                    .Where(sessao =>
                        sessao.DataFim.HasValue &&
                        sessao.DataFim.Value >= inicioDia &&
                        sessao.DataFim.Value < fimDia)
                    .ToList();

            HistoricoSessao.ItemsSource = sessoesFiltradas;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void BtnLimparFiltros_Clicked(object sender, EventArgs e)
    {
        OnAppearing();
    }

   
    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}