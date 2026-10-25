using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ATHENA.Categorias;

[QueryProperty(nameof(IdCategoria), "idCategoria")]
public partial class InterfaceCategoria : ContentPage
{
    public int IdCategoria { get; set; }

    private readonly SQLiteAsyncConnection db;
    private readonly List<int> idsMetasSelecionadas = new();
    private List<Meta> metasDaCategoria = new();
    private string filtroAtual = "Todas";

    public InterfaceCategoria()
    {
        InitializeComponent();

        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db");

        db = new SQLiteAsyncConnection(dbPath);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarInformacoes();
    }

    private async Task CarregarInformacoes()
    {
        try
        {
            Categoria? categoria = await db.Table<Categoria>()
                .FirstOrDefaultAsync(c => c.idCategoria == IdCategoria);

            // Esta mesma consulta alimenta o resumo e os cards.
            List<Meta> metas = await db.Table<Meta>()
                .Where(m => m.idCategoria == IdCategoria)
                .ToListAsync();

            int concluidas = metas.Count(m => EstaConcluida(m.status));
            int totalMinutos = metas.Sum(m => m.minutosEstudados ?? 0);
            int horas = totalMinutos / 60;
            int minutos = totalMinutos % 60;
            double progresso = metas.Count > 0
                ? (double)concluidas / metas.Count
                : 0;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                metasDaCategoria = metas;

                TituloCategoriaTXT.Text = categoria?.tituloCategoria
                    ?? "Categoria não encontrada";
                DescricaoCategoriaTXT.Text = categoria?.descricaoCategoria ?? "";
                TotalMetasCategoriaTXT.Text = metas.Count.ToString();
                MetasConcluidasCategoriaTXT.Text = concluidas.ToString();
                TempoEstudadoCategoriaTXT.Text = $"{horas}h {minutos:D2}min";
                BarraProgressoCategoria.Progress = progresso;
                ProgressoCategoriaTXT.Text = $"{progresso:P0}";

                MostrarMetas();
            });
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao carregar a categoria", ex.Message);
        }
    }

    private static bool EstaConcluida(string? status)
    {
        return string.Equals(status, "Concluida", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Concluída", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Concluído", StringComparison.OrdinalIgnoreCase);
    }

    private void MostrarMetas()
    {
        IEnumerable<Meta> resultado = metasDaCategoria;

        if (filtroAtual == "Concluidas")
        {
            resultado = resultado.Where(m => EstaConcluida(m.status));
        }
        else if (filtroAtual == "Pendentes")
        {
            resultado = resultado.Where(m =>
                string.Equals(m.status, "Em andamento", StringComparison.OrdinalIgnoreCase));
        }

        string busca = BuscaMetaTXT.Text?.Trim() ?? "";

        if (busca.Length > 0)
        {
            resultado = resultado.Where(m =>
                (m.tituloMeta ?? "").Contains(busca, StringComparison.CurrentCultureIgnoreCase));
        }

        List<Meta> metasVisiveis = resultado.ToList();

        // listaMeta agora é um VerticalStackLayout no XAML.
        // BindableLayout cria um card visível para cada Meta desta coleção.
        BindableLayout.SetItemsSource(listaMeta, metasVisiveis);

        EstadoVazioMetas.IsVisible = metasVisiveis.Count == 0;

        bool categoriaVazia = metasDaCategoria.Count == 0;
        EstadoVazioTituloTXT.Text = categoriaVazia
            ? "Esta categoria ainda não possui metas"
            : "Nenhuma meta encontrada";
        EstadoVazioDescricaoTXT.Text = categoriaVazia
            ? "Use ADICIONAR METAS para colocar metas nesta categoria."
            : "Ajuste o nome pesquisado ou escolha o filtro Todas.";

        Button selecionado = filtroAtual == "Concluidas"
            ? BtnFiltroConcluidas
            : filtroAtual == "Pendentes"
                ? BtnFiltroPendentes
                : BtnFiltroTodas;

        SelecionarFiltro(selecionado);
    }

    private async Task MostrarErroSeguro(string titulo, string mensagem)
    {
        System.Diagnostics.Debug.WriteLine($"[ATHENA] {titulo}: {mensagem}");

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await DisplayAlertAsync(titulo, mensagem, "OK");
        });
    }

    private Color ObterCor(string nome)
    {
        return (Color)Application.Current!.Resources[nome];
    }

    private void AplicarEstadoNormal(Button botao)
    {
        botao.BackgroundColor = ObterCor("AthenaCard");
        botao.BorderColor = ObterCor("AthenaBorderSoft");
        botao.TextColor = ObterCor("AthenaTextSecondary");
    }

    private void SelecionarFiltro(Button botaoSelecionado)
    {
        AplicarEstadoNormal(BtnFiltroTodas);
        AplicarEstadoNormal(BtnFiltroPendentes);
        AplicarEstadoNormal(BtnFiltroConcluidas);

        if (botaoSelecionado == BtnFiltroConcluidas)
        {
            botaoSelecionado.BackgroundColor = ObterCor("AthenaSuccessBackground");
            botaoSelecionado.BorderColor = ObterCor("AthenaSuccess");
            botaoSelecionado.TextColor = ObterCor("AthenaSuccess");
        }
        else
        {
            botaoSelecionado.BackgroundColor = ObterCor("AthenaBlueBackground");
            botaoSelecionado.BorderColor = ObterCor("AthenaBlue");
            botaoSelecionado.TextColor = ObterCor("AthenaTextPrimary");
        }
    }

    private async void ButtonCriarMeta_Clicked(object sender, EventArgs e)
    {
        try
        {
            PainelAdicionarMetas.IsVisible = false;
            await Shell.Current.GoToAsync($"{nameof(NewPage2)}?idCategoria={IdCategoria}");
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao abrir a criação de meta", ex.Message);
        }
    }

    private async void EscolherMeta_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button botao || botao.BindingContext is not Meta meta)
            return;

        try
        {
            await Shell.Current.GoToAsync($"{nameof(InterfaceMeta)}?idMeta={meta.idMeta}");
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao abrir a meta", ex.Message);
        }
    }

    private async void BtnAdicionarMetas_Clicked(object sender, EventArgs e)
    {
        try
        {
            List<Meta> metas = await db.Table<Meta>()
                .Where(m => m.idCategoria == null)
                .ToListAsync();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                listaMetaEscolha.SelectedItems.Clear();
                idsMetasSelecionadas.Clear();
                listaMetaEscolha.ItemsSource = metas;
                BtnConfirmarMetas.IsEnabled = false;
                MetasSelecionadasTXT.Text = "Nenhuma meta selecionada";
                PainelAdicionarMetas.IsVisible = true;
            });
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao carregar metas disponíveis", ex.Message);
        }
    }

    private void BtnFecharAdicionarMetas_Clicked(object sender, EventArgs e)
    {
        PainelAdicionarMetas.IsVisible = false;
    }

    private void BtnFiltroConcluidas_Clicked(object sender, EventArgs e)
    {
        filtroAtual = "Concluidas";
        MostrarMetas();
    }

    private void BtnFiltroPendentes_Clicked(object sender, EventArgs e)
    {
        filtroAtual = "Pendentes";
        MostrarMetas();
    }

    private void BtnFiltroTodas_Clicked(object sender, EventArgs e)
    {
        filtroAtual = "Todas";
        MostrarMetas();
    }

    private void BuscaMetaTXT_Completed(object sender, EventArgs e)
    {
        MostrarMetas();
    }

    private async void BtnEditarCategoria_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(EditarCategoria)}?idCategoria={IdCategoria}");
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao abrir a edição", ex.Message);
        }
    }

    private void listaMetaEscolha_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        idsMetasSelecionadas.Clear();

        foreach (var item in e.CurrentSelection)
        {
            if (item is Meta meta)
                idsMetasSelecionadas.Add(meta.idMeta);
        }

        BtnConfirmarMetas.IsEnabled = idsMetasSelecionadas.Count > 0;
        MetasSelecionadasTXT.Text = idsMetasSelecionadas.Count switch
        {
            0 => "Nenhuma meta selecionada",
            1 => "1 meta selecionada",
            _ => $"{idsMetasSelecionadas.Count} metas selecionadas"
        };
    }

    private async void BtnConfirmarMetas_Clicked(object sender, EventArgs e)
    {
        if (idsMetasSelecionadas.Count == 0)
            return;

        BtnConfirmarMetas.IsEnabled = false;

        try
        {
            foreach (int idMeta in idsMetasSelecionadas.ToList())
            {
                await Models_Categoria.AdicionarMetaCategoria(idMeta, IdCategoria);
            }

            PainelAdicionarMetas.IsVisible = false;
            filtroAtual = "Todas";
            BuscaMetaTXT.Text = "";
            await CarregarInformacoes();
            await DisplayAlertAsync("Sucesso", "Metas adicionadas à categoria", "OK");
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao adicionar as metas", ex.Message);
        }
        finally
        {
            BtnConfirmarMetas.IsEnabled = idsMetasSelecionadas.Count > 0;
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button botao || botao.BindingContext is not Meta meta)
            return;

        PainelOpcoesMeta.BindingContext = meta;
        PainelOpcoesMeta.IsVisible = true;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        PainelAdicionarMetas.IsVisible = false;
        PainelOpcoesMeta.IsVisible = false;
        await Shell.Current.GoToAsync("..");
    }

    private void BtnFundoOpcoesMeta_Clicked(object sender, EventArgs e)
    {
        PainelOpcoesMeta.IsVisible = false;
    }

    private void BtnFecharOpcoesMeta_Clicked(object sender, EventArgs e)
    {
        PainelOpcoesMeta.IsVisible = false;
    }

    private async void BtnRemoverMetaCategoria_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button botao || botao.BindingContext is not Meta meta)
            return;

        BtnRemoverMetaCategoria.IsEnabled = false;

        try
        {
            await Models_Categoria.RemoverMetaCategoria(meta.idMeta, IdCategoria);
            PainelOpcoesMeta.IsVisible = false;
            await CarregarInformacoes();
            await DisplayAlertAsync("Sucesso", "Meta removida da categoria", "OK");
        }
        catch (Exception ex)
        {
            await MostrarErroSeguro("Erro ao remover a meta", ex.Message);
        }
        finally
        {
            BtnRemoverMetaCategoria.IsEnabled = true;
        }
    }
}
