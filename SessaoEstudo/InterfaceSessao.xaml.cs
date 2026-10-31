using ATHENA.Database;
using ATHENA.Services;
using SQLite;

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

            if (sessao == null)
            {
                return;
            }

            int? idMetaSessao = sessao.idMeta;

            double tempoEstudado = sessao.tempoEstudadoMinutos ?? 0;
            double tempoBruto = sessao.duracaoMinutos ?? 0;

            TempoEstudo.Text = FormatadorTempo.Duracao(tempoEstudado);
            TxtTempoBruto.Text = FormatadorTempo.Duracao(tempoBruto);
            TxtTempoPausa.Text = FormatadorTempo.Duracao(tempoBruto - tempoEstudado);


            Meta? meta = await db.Table<Meta>()
                .Where(mbox => mbox.idMeta == idMetaSessao)
                .FirstOrDefaultAsync();

            if (meta != null)
            {
                TituloTXT.Text = sessao.tituloSessao;
                TxtMetaRelacionada.Text = meta.tituloMeta;

                int? idCategoria = meta.idCategoria;

                Categoria? categoria = await db.Table<Categoria>()
                    .Where(c => c.idCategoria == idCategoria)
                    .FirstOrDefaultAsync();

                TxtCategoriaRelacionada.Text = categoria?.tituloCategoria ?? "Sem categoria";
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    private async void Contar_tempo_Clicked(object sender, EventArgs e)
    {
        bool configurado = CronometroService.ConfigurarContexto(
            NewPage1.OrigemCronometro.Sessao,
            idMeta: IdMeta,
            idSessao: IdSessao);

        if (!configurado)
        {
            await DisplayAlertAsync(
                "Cronômetro ativo",
                "Finalize o estudo atual antes de selecionar outra sessão.",
                "OK");

            return;
        }

        await Shell.Current.GoToAsync($"//{nameof(NewPage1)}");
    }

    private async void Salvar_Clicked_1(object sender, EventArgs e)
    {
        if (CronometroService.Iniciado && CronometroService.IdSessao == IdSessao)
        {
            await DisplayAlertAsync("Cronômetro ativo", "Pare o cronômetro antes de salvar a sessão.", "OK");
            return;
        }

        try
        {
            string titulo = TituloTXT.Text;

            if (string.IsNullOrWhiteSpace(titulo))
            {
                titulo = "Sem descrição";
            }

            string descricao = DescricaoTXT.Text;
            DateTime dataFim = DateTime.Now;

            await Models.Models_SessaoEstudo.FinalizarSessao(titulo, descricao, dataFim, IdSessao);

            await DisplayAlertAsync("Sucesso!", "Sessão finalizada com sucesso", "Ok");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    /// <summary>
    /// Saída da tela, usada pelo botão ‹ e pelo voltar do Android.
    /// Cronômetro rodando para esta sessão ou sessão já salva: só volta.
    /// Sessão sem tempo: apaga. Sessão com tempo: pergunta antes de apagar
    /// (o tempo já somado na meta é mantido).
    /// </summary>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            CronometroPreferences cronometro = new CronometroPreferences();
            cronometro.Carregar();

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                    .Where(mbox => mbox.idSessao == IdSessao)
                    .FirstOrDefaultAsync();

            if (cronometro.Iniciado && cronometro.IdSessao == IdSessao)
            {
                await Shell.Current.GoToAsync("..");
            }
            else if (sessao == null || sessao.DataFim != null) 
            {
                await Shell.Current.GoToAsync("..");
            }
            else if ((sessao.duracaoMinutos ?? 0) == 0)
            {
                if (!CronometroService.Iniciado && CronometroService.IdSessao == IdSessao)
                    CronometroService.Limpar();

                await Models.Models_SessaoEstudo.DeletarSessao(IdSessao);
                await Shell.Current.GoToAsync("..");
            }
            else 
            {
                bool resposta = await DisplayAlertAsync(
                "Sair da sessão?",
                "Se você sair agora, a sessão de estudo será apagada e não poderá ser recuperada. O tempo estudado acumulado na sua meta será mantido e salvo.\n\nDeseja realmente sair?",
                "Sair",
                "Continuar estudando"
            );

                if (resposta)
                {
                    if (!CronometroService.Iniciado && CronometroService.IdSessao == IdSessao) 
                        CronometroService.Limpar();

                    await Models.Models_SessaoEstudo.DeletarSessao(IdSessao);
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "ok");
        }
    }
    protected override bool OnBackButtonPressed()
    {
        // true impede o Android de voltar sozinho; quem decide e navega é o Button_Clicked.
        Button_Clicked(this, EventArgs.Empty);
        return true;
    }
}
