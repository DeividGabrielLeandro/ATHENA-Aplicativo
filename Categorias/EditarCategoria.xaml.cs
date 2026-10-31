using ATHENA.Database;
using ATHENA.Models;
using SQLite;

namespace ATHENA.Categorias;

[QueryProperty(nameof(IdCategoria), "idCategoria")]
public partial class EditarCategoria : ContentPage
{

    public int IdCategoria { get; set; }

    SQLiteAsyncConnection db;
    public EditarCategoria()
	{
		InitializeComponent();

        BindingContext = this;

        string dbPath = Path.Combine(

                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ATHENA.db"
            );

        db = new SQLiteAsyncConnection(dbPath);
    }

    private async void BtnVoltarCategoria_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void BtnSalvarCategoria_Clicked(object sender, EventArgs e)
    {
        try
        {
            string? titulo = string.IsNullOrWhiteSpace(TituloTXT.Text) ? null : TituloTXT.Text;
            string descricao = DescricaoTXT.Text;
            await Models_Categoria.EditarCategoria(IdCategoria, titulo, descricao);

            await DisplayAlertAsync("Sucesso", "Categoria editada com sucesso", "ok");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void BtnExcluirCategoria_Clicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync(
            "Excluir categoria",
            "Tem certeza de que deseja excluir esta categoria?\n\nAs metas dela não serão apagadas, apenas ficarão sem categoria.",
            "Excluir",
            "Cancelar");

        if (!confirmar)
            return;

        try
        {
            await Models_Categoria.DeletarCategoria(IdCategoria);

            await DisplayAlertAsync(
                    "Categoria excluída",
                    "A categoria foi excluída com sucesso.",
                    "OK");

            await Shell.Current.GoToAsync("../..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
}