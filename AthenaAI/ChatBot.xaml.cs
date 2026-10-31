namespace ATHENA.AthenaAI;

using ATHENA.Database;
using SQLite;
using System.Collections.ObjectModel;
public partial class ChatBot : ContentPage
{
    private readonly ObservableCollection<MensagemChat> mensagens = new();
    public class MensagemChat
    {
        public string Texto { get; set; } = "";
        public bool EhUsuario { get; set; }
        public bool EhIA => !EhUsuario;
        public string Hora { get; set; } = DateTime.Now.ToString("HH:mm");

        public FormattedString TextoFormatado => FormatadorTexto.Converter(Texto);
    }
    SQLiteAsyncConnection db;

    public ChatBot()
	{
		InitializeComponent();

        ListaMensagens.ItemsSource = mensagens;

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
            await IAService.ApagarMensagem();

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
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void BtnSugestao_Clicked(object sender, EventArgs e) 
    {
        if (sender is Button botao && botao.CommandParameter is string texto)
        {
            EditorMensagem.Text = texto;                 
            EditorMensagem.Focus();                      
            EditorMensagem.CursorPosition = texto.Length; 
        }
    }
    private async void BtnEnviar_Clicked(object sender, EventArgs e) 
    {
        DateTime hora = DateTime.Now;


        string pergunta = EditorMensagem.Text;

        if(string.IsNullOrWhiteSpace(pergunta))
            return;

        BtnEnviar.IsEnabled = false;
        EditorMensagem.Text = string.Empty;

        MensagemChat mensagemChat_User = new MensagemChat
        {
            Texto = pergunta,
            EhUsuario = true,
            Hora = hora.ToString("g"),
        };

        mensagens.Add(mensagemChat_User);          
        ListaMensagens.ScrollTo(mensagemChat_User); 

        AreaDigitando.IsVisible = true;
        try
        {
            string resposta = await IAService.Mensagens(pergunta);

            MensagemChat mensagemChat_IA = new MensagemChat
            {
                Texto = resposta,
                EhUsuario = false,
                Hora = hora.ToString("g"),
            };
            mensagens.Add(mensagemChat_IA);
            ListaMensagens.ScrollTo(mensagemChat_IA);

        }
        catch (Exception ex) 
        {
            await DisplayAlertAsync("Erro", ex.Message, "sair");
        }
        finally
        {
            AreaDigitando.IsVisible = false;
            BtnEnviar.IsEnabled = true;
        }
    }
    private void EditorMensagem_TextChanged(object sender, TextChangedEventArgs e)
    {
        string antigo = e.OldTextValue ?? "";
        string novo = e.NewTextValue ?? "";

       
        bool apertouEnter = novo.Length > antigo.Length
            && novo.Length - antigo.Length <= 2
            && novo.Replace("\r", "").Replace("\n", "") == antigo.Replace("\r", "").Replace("\n", "");

        if (apertouEnter)
        {
           
            EditorMensagem.Text = antigo;

         
            if (BtnEnviar.IsEnabled)
                BtnEnviar_Clicked(BtnEnviar, EventArgs.Empty);

            return;
        }

        TxtContadorCaracteres.Text = $"{novo.Length} / {EditorMensagem.MaxLength}";
    }
}