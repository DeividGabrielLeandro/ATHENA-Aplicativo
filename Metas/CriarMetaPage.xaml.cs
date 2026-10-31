namespace ATHENA;
using ATHENA.Database;
using ATHENA.Models;
using SQLite;

[QueryProperty(nameof(IdCategoriaParametro), "idCategoria")]
public partial class NewPage2 : ContentPage
{
    public DateTime? dataLimite { get; set; }

    public int? IdCategoria { get; set; }
    public int? IdMeta { get; set; }


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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            List<Categoria> categorias = await db.Table<Categoria>().ToListAsync();

            ListaCategorias.ItemsSource = categorias;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }



    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            string? prioridade = null;
            if (RadioSemPrioridade.IsChecked)
                prioridade = "Sem prioridade";

            else if (RadioBaixa.IsChecked)
                prioridade = "Baixa";

            else if (RadioMedia.IsChecked)
                prioridade = "Média";

            else
                prioridade = "Alta";

        string status = "Em andamento";

        string titulo = TituloTXT.Text;

        if (string.IsNullOrWhiteSpace(titulo))
        {
            await DisplayAlertAsync("Atenção", "O título da meta é obrigatório", "OK");
            return;
        }

        string? descricao = string.IsNullOrWhiteSpace(DescricaoTXT.Text) ? null : DescricaoTXT.Text;
            
        int? minutos = int.TryParse(MetaMinutosTXT.Text, out int valor) ? valor : null;

        await Models_Meta.CriarMeta(titulo, descricao, minutos, prioridade, status, IdCategoria, dataLimite);


     
        await DisplayAlertAsync("Meta criada com sucesso!",
            "Sua nova meta de estudo foi registrada. Agora é hora de manter o foco e avançar rumo aos seus objetivos.",
            "Continuar");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex) {
            await DisplayAlertAsync("Erro", ex.Message, "Ok");

        }
            
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        BtnRemoverData.IsVisible = true;
        PrazoDate.IsVisible = true;
    }

    private void BtnRemoverData_Clicked(object sender, EventArgs e)
    {
        BtnRemoverData.IsVisible = false;
        PrazoDate.IsVisible = false;

        TxtDataLimite.Text = "Sem data limite";
        TxtResumoPrazo.Text = "Sem data limite";
    }
    private void PrazoDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        dataLimite = PrazoDate.Date;

        TxtDataLimite.Text = dataLimite.Value.ToString("dd/MM/yyyy");
        TxtResumoPrazo.Text = dataLimite.Value.ToString("dd/MM/yyyy");
    }
    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        ListaCategoriasContainer.IsVisible = true;
    }

    private async void ListaCategorias_SelectionChanged(
     object sender,
     SelectionChangedEventArgs e)
    {
        try
        {
            Categoria? categoriaSelecionada =
                e.CurrentSelection.FirstOrDefault() as Categoria;

            if (categoriaSelecionada is null)
            {
                return;
            }

            IdCategoria =
                categoriaSelecionada.idCategoria;

            Categoria? categoria =
                await db.Table<Categoria>()
                    .Where(r => r.idCategoria == IdCategoria.Value)
                    .FirstOrDefaultAsync();

            if (categoria is null)
            {
                return;
            }

            TxtCategoriaSelecionada.Text =
                categoria.tituloCategoria;

            TxtStatusCategoria.Text =
                "Categoria vinculada";

            TxtResumoCategoria.Text =
                categoria.tituloCategoria;

            ListaCategoriasContainer.IsVisible =
                false;

            ListaCategorias.SelectedItem =
                null;
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

}