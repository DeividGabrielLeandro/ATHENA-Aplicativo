using ATHENA.Database;
using ATHENA.Models;
using Microsoft.Maui.Platform;
using SQLite;

namespace ATHENA.Metas.xaml;

[QueryProperty(nameof(IdMeta), "idMeta")]
public partial class EditarMeta : ContentPage
{
        public int IdMeta { get; set; }
        


        SQLiteAsyncConnection db;

        public EditarMeta()
        {
            InitializeComponent();

        BindingContext = this;

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
       .Where(mbox => mbox.idMeta == IdMeta)
       .FirstOrDefaultAsync();


            if (meta == null)
                return;
            
            listaMetas.ItemsSource = new List<Meta> { meta };

            TituloTXT.Placeholder = meta.tituloMeta;
            DescricaoTXT.Placeholder = meta.descricaoMeta;
            MinutosTXT.Placeholder = meta.metaMinutos.ToString();
            DataLimiteTXT.Text = meta.TextoDataLimite;


        }
        catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    private async void PrazoDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        DateTime? dataEscolhida = PrazoDate.Date;

        await Models_Meta.AdicionarRemoverData(IdMeta, dataEscolhida, 'A');

        DataLimiteTXT.Text = dataEscolhida.Value.ToString("dd/MM/yyyy");

    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        PrazoDate.IsVisible = true;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Models_Meta.AdicionarRemoverData(IdMeta, null, 'R');

        DataLimiteTXT.Text = "Não possui prazo";

    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        try
        {
            await db.CreateTableAsync<Meta>();

            string status;
            string prioridade;
            string titulo = TituloTXT.Text;
            string descricao = DescricaoTXT.Text;
            int? minutos = null;

            if (int.TryParse(MinutosTXT.Text, out int valor))
            {
                minutos = valor;
            }

            if (RadioSemPrioridade.IsChecked)  
        prioridade = "Sem prioridade";
        
        else if (RadioBaixa.IsChecked)
        prioridade = "Baixa";
        
        else if (RadioMedia.IsChecked)
        prioridade = "Média";
        
        else
        prioridade = "Alta";
              

            if (SwitchConcluida.IsToggled)
            {
                status = "Concluida";
            }
            else
            {
                status = "Em andamento";
            }


            await Models_Meta.EditarMeta(IdMeta, titulo, descricao, minutos, prioridade, status);

            await DisplayAlertAsync("Sucesso!", "Meta editada com sucesso", "OK");

            PrazoDate.IsVisible = false;

            Meta? meta = await db.Table<Meta>()
                .Where(m => m.idMeta == IdMeta)
                .FirstOrDefaultAsync();

            if (meta != null)
            {
                listaMetas.ItemsSource = new List<Meta> { meta };

                TituloTXT.Placeholder = meta.tituloMeta;
                DescricaoTXT.Placeholder = meta.descricaoMeta;
                MinutosTXT.Placeholder = meta.metaMinutos.ToString();
            }

        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Models_Meta.DeletarMeta(IdMeta);
        await DisplayAlertAsync("Sucesso!", "Meta deletada", "OK");
        await Shell.Current.GoToAsync("../..");
    }

   
}
