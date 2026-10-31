using ATHENA.AthenaAI;
using ATHENA.Database;
using ATHENA.Models;
using ATHENA.Services;
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
    private async Task CarregarConfigIAAsync()
    {
        IASettings settings = new IASettings();
        settings.Carregar();

        if (!string.IsNullOrEmpty(settings.Model))
            PickerModeloIA.SelectedItem = settings.Model;

        EditorInstrucoesIA.Text = settings.SystemPrompt;

        string? chaveSalva = await SecureStorage.Default.GetAsync("GeminiApiKey");
        EntryApiKey.Placeholder = string.IsNullOrEmpty(chaveSalva)
            ? "Cole aqui a sua chave de API"
            : "Chave já salva — deixe em branco para manter";
    }

    public async void Informacoe()
    {
        try
        {
            List<Sessao_Estudo> sessao = await db.Table<Sessao_Estudo>().ToListAsync();
            List<Registro_Tempo> registros = await db.Table<Registro_Tempo>().ToListAsync();


            double tempoTotal = registros.Sum(r => r.tempoEstudo);

            DateTime inicioDoDia = DateTime.Now.Date;

            Usuario? usuario = await db.Table<Usuario>()
                            .FirstOrDefaultAsync();

            var registro_ = await db.Table<Registro_Tempo>()
                    .Where(r => r.data >= inicioDoDia)
                    .ToListAsync();

            double tempoEstudado = registro_.Sum(r => r.tempoEstudo);
            double totalMinutos = tempoEstudado;

            double progresso = usuario != null && usuario.metaDiaria > 0
        ? Math.Clamp(totalMinutos / usuario.metaDiaria.Value, 0, 1)
        : 0;

            int percentual = (int)(progresso * 100);

            List<Registro_Tempo> tempo = await db
        .Table<Registro_Tempo>()
        .ToListAsync();

            List<DateTime> diasEstudados = tempo
                .Where(r => r.tempoEstudo > 0)
                .Select(r => r.data.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            DateTime hoje = DateTime.Today;

            if (diasEstudados.Count > 0 &&
                (diasEstudados[0] == hoje ||
                 diasEstudados[0] == hoje.AddDays(-1)))
            {
                DateTime diaEsperado = diasEstudados[0];

                foreach (DateTime dia in diasEstudados)
                {
                    if (dia == diaEsperado)
                    {
                        streak++;
                        diaEsperado = diaEsperado.AddDays(-1);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TxtQuantidadeSessoes.Text = sessao.Count.ToString();
                TxtTempoEstudadoHoje.Text = $"{totalMinutos:F0} / {usuario?.metaDiaria} min";
                TxtPercentualMetaDiaria.Text = $"{percentual}%";
                TxtTempoTotal.Text = FormatadorTempo.Duracao(tempoTotal);
                TxtStreak.Text = $"{streak.ToString()} dias";
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            Usuario? usuario = await db.Table<Usuario>()
                .FirstOrDefaultAsync();

            if (usuario?.fotoPerfil != null &&
                File.Exists(usuario.fotoPerfil))
            {
                ImagemPerfil.Source = ImageSource.FromFile(
                    usuario.fotoPerfil);
            }
            else
            {
                ImagemPerfil.Source = null;
            }

            await CarregarConfigIAAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }

        Informacoe();
    }

    private string? caminhoFotoPerfil;

    private async void EscolherFoto_Clicked(object sender, EventArgs e)
    {
        try
        {
            var foto = await MediaPicker.Default.PickPhotoAsync();

            if (foto == null)
                return;

            string extensao = Path.GetExtension(foto.FileName);

            if (string.IsNullOrWhiteSpace(extensao))
                extensao = ".jpg";

            string caminhoFoto = Path.Combine(
                FileSystem.Current.AppDataDirectory,
                $"foto_perfil_{Guid.NewGuid():N}{extensao}");

            using (Stream origem = await foto.OpenReadAsync())

            using (FileStream destino = File.Create(caminhoFoto))
            {
                await origem.CopyToAsync(destino);
            }

            ImagemPerfil.Source = null;
            ImagemPerfil.Source = ImageSource.FromFile(caminhoFoto);

            // Salva o novo caminho no usuário para as outras telas.
            await Models_Usuario.AtualizarUsuario(null, null, caminhoFoto);

            Usuario? usuarioAtualizado = await db.Table<Usuario>()
        .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Erro ao alterar foto",
                ex.Message,
                "OK");
        }
    }



    private async void MetaDiariaTXT_Completed(object sender, EventArgs e)
    {
        try
        {
            string metaMinutos = MetaDiariaTXT.Text;

            if (double.TryParse(metaMinutos, out double minutos))
            {
                await Models_Usuario.AtualizarUsuario(null, minutos);
            }

            DateTime inicioDoDia = DateTime.Now.Date;

            Usuario? usuario = await db.Table<Usuario>()
                            .FirstOrDefaultAsync();

            if (usuario == null)
                return;

            var registros = await db.Table<Registro_Tempo>()
                    .Where(r => r.data >= inicioDoDia)
                    .ToListAsync();

            double tempoEstudado = registros.Sum(r => r.tempoEstudo);
            double totalMinutos = tempoEstudado;

            double progresso = usuario.metaDiaria > 0
        ? Math.Min(totalMinutos / usuario.metaDiaria.Value, 1.0)
        : 0;


            TxtTempoEstudadoHoje.Text = $"{totalMinutos:F0} / {usuario.metaDiaria} min";
            TxtPercentualMetaDiaria.Text = $"{progresso:P0}";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async void NomeUsuario_Completed(object sender, EventArgs e)
    {
        try
        {
            string nome = NomeUsuario.Text;
            await Models_Usuario.AtualizarUsuario(nome, null);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }
    private async void BtnConfigurarIA_Clicked(object sender, EventArgs e)
    {
        PainelConfigIA.IsVisible = true;
    }
    private async void BtnAbrirAIStudio_Clicked(object sender, EventArgs e)
    {
        try
        {
            string url = "https://aistudio.google.com/app/apikey";
            await Launcher.Default.OpenAsync(url);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }



    private async void BtnSalvarConfigIA_Clicked(object sender, EventArgs e)
    {
        try
        {
            string novaChave = EntryApiKey.Text?.Trim() ?? "";
            string? chaveSalva = await SecureStorage.Default.GetAsync("GeminiApiKey");
            string? modelo = PickerModeloIA.SelectedItem?.ToString();
            string instrucoes = EditorInstrucoesIA.Text?.Trim() ?? "";

            bool temChave = !string.IsNullOrEmpty(novaChave) || !string.IsNullOrEmpty(chaveSalva);

            if (!temChave || string.IsNullOrEmpty(modelo))
            {
                await DisplayAlertAsync("Atenção", "É preciso ter uma chave de API e um modelo escolhido.", "OK");
                return;
            }

            IASettings settings = new IASettings();
            settings.Model = modelo;
            settings.SystemPrompt = instrucoes;
            settings.Salvar();

            if (!string.IsNullOrEmpty(novaChave))
                await SecureStorage.Default.SetAsync("GeminiApiKey", novaChave);

            EntryApiKey.Text = string.Empty;
            await CarregarConfigIAAsync();
            TxtStatusConfigIA.Text = "Configuração salva.";
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
