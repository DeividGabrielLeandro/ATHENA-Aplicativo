using ATHENA.Database;
using ATHENA.Models;
using ATHENA.Services;
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

            SwitchConcluida.IsToggled = meta.status == "Concluida";

            if (meta.prioridadeMeta == "Baixa")
                RadioBaixa.IsChecked = true;
            else if (meta.prioridadeMeta == "Média")
                RadioMedia.IsChecked = true;
            else if (meta.prioridadeMeta == "Alta")
                RadioAlta.IsChecked = true;
            else
                RadioSemPrioridade.IsChecked = true;


        }
        catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    private async void PrazoDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        try
        {
            DateTime? dataEscolhida = PrazoDate.Date;

            await Models_Meta.AdicionarRemoverData(IdMeta, dataEscolhida, 'A');

            DataLimiteTXT.Text = dataEscolhida.Value.ToString("dd/MM/yyyy");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
    private void Button_Clicked(object sender, EventArgs e)
    {
        PrazoDate.IsVisible = true;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        try
        {
            await Models_Meta.AdicionarRemoverData(IdMeta, null, 'R');

            DataLimiteTXT.Text = "Não possui prazo";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        try
        {

            string status;
            string prioridade;
            string? titulo = string.IsNullOrWhiteSpace(TituloTXT.Text) ? null : TituloTXT.Text;
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

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        CronometroPreferences cronometro = new CronometroPreferences();
        cronometro.Carregar();

        if (cronometro.Iniciado && cronometro.IdMeta == IdMeta)
        {
            await DisplayAlertAsync("Erro", "Você não pode deletar uma meta enquanto o cronômetro está ativo", "sair");
            return;
        }

        bool confirmar = await DisplayAlertAsync(
        "Excluir meta",
        "Tem certeza de que deseja excluir esta meta?\n\nEssa ação não pode ser desfeita.",
        "Excluir",
        "Cancelar");

        if (!confirmar)
        {
            return;
        }

        try
        {
            await Models_Meta.DeletarMeta(IdMeta);

            await DisplayAlertAsync(
                "Meta excluída",
                "A meta foi excluída com sucesso.",
                "OK");

            await Shell.Current.GoToAsync("../..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro",
                $"Não foi possível excluir a meta.\n\n{ex.Message}",
                "OK");
        }
    }
    private async void Voltar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

}
