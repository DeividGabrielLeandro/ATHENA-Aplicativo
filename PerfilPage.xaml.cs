using ATHENA.Database;
using ATHENA.Models;
using SQLite;

namespace ATHENA;

public partial class PerfilPage : ContentPage
{
    SQLiteAsyncConnection db;

    public PerfilPage()
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

        Usuario? usuario = await db.Table<Usuario>()
                .FirstOrDefaultAsync();

        NomeUsuario.Text = usuario.nome;

        ImagemPerfil.Source = ImageSource.FromFile(usuario.fotoPerfil);
    }

    private string? caminhoFotoPerfil;

    private async void EscolherFoto_Clicked(object sender, EventArgs e)
    {
		var foto = await MediaPicker.Default.PickPhotoAsync();

		if(foto == null)
		{
			return;
		}

		string nomeArquivo = "foto_perfil.png";
        caminhoFotoPerfil = Path.Combine(FileSystem.Current.AppDataDirectory, nomeArquivo);

		using Stream origem = await foto.OpenReadAsync();
		using FileStream destino = File.Create(caminhoFotoPerfil);

		await origem.CopyToAsync(destino);

        ImagemPerfil.Source = ImageSource.FromFile(caminhoFotoPerfil);

    }

    private async void Salvar_Clicked(object sender, EventArgs e)
    {
		string nome = NomeUsuario.Text;

        await DisplayAlertAsync("Perfil salvo!!", "Perfil salvo com sucesso", "ok");
		await Models_Usuario.AtualizarUsuario(nome, caminhoFotoPerfil);
    }
}