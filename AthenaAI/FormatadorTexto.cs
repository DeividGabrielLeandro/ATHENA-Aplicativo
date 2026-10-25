using System.Text;
using System.Text.RegularExpressions;

namespace ATHENA.AthenaAI;


public static class FormatadorTexto
{
    public static FormattedString Converter(string? texto)
    {
        FormattedString resultado = new FormattedString();

        if (string.IsNullOrEmpty(texto))
            return resultado;

        string[] linhas = texto.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < linhas.Length; i++)
        {
            string linha = linhas[i];
            bool ehTitulo = false;

            // "# Título" vira uma linha em negrito.
            Match titulo = Regex.Match(linha, @"^\s{0,3}#{1,6}\s+(.*)$");

            if (titulo.Success)
            {
                linha = titulo.Groups[1].Value;
                ehTitulo = true;
            }
            else
            {
                // "* item" ou "- item" vira "• item".
                Match item = Regex.Match(linha, @"^(\s*)[\*\-]\s+(.*)$");

                if (item.Success)
                    linha = item.Groups[1].Value + "• " + item.Groups[2].Value;
            }

            AdicionarLinha(resultado, linha, ehTitulo);

            if (i < linhas.Length - 1)
                resultado.Spans.Add(new Span { Text = "\n" });
        }

        return resultado;
    }

    private static void AdicionarLinha(FormattedString destino, string linha, bool ehTitulo)
    {
        bool negrito = false;
        bool italico = false;
        bool sublinhado = false;
        bool riscado = false;
        bool codigo = false;

        StringBuilder atual = new StringBuilder();

        // Fecha o pedaço de texto acumulado e cria um Span com o estilo que está ligado.
        void Fechar()
        {
            if (atual.Length == 0)
                return;

            Span span = new Span { Text = atual.ToString() };

            if (negrito || ehTitulo) span.FontAttributes |= FontAttributes.Bold;
            if (italico) span.FontAttributes |= FontAttributes.Italic;
            if (sublinhado) span.TextDecorations |= TextDecorations.Underline;
            if (riscado) span.TextDecorations |= TextDecorations.Strikethrough;
            if (codigo) span.BackgroundColor = Color.FromArgb("#33FFFFFF");

            destino.Spans.Add(span);
            atual.Clear();
        }

        int i = 0;

        while (i < linha.Length)
        {
            // Dentro de `código` nada é formatado: só procura o ` que fecha.
            if (codigo)
            {
                if (linha[i] == '`')
                {
                    Fechar();
                    codigo = false;
                }
                else
                {
                    atual.Append(linha[i]);
                }

                i++;
                continue;
            }

            // **negrito**
            if (Comeca(linha, i, "**"))
            {
                if (EhMarcador(linha, i, "**", negrito))
                {
                    Fechar();
                    negrito = !negrito;
                }
                else
                {
                    atual.Append("**");
                }

                i += 2;
                continue;
            }

            // ++sublinhado++
            if (Comeca(linha, i, "++"))
            {
                if (EhMarcador(linha, i, "++", sublinhado))
                {
                    Fechar();
                    sublinhado = !sublinhado;
                }
                else
                {
                    atual.Append("++");
                }

                i += 2;
                continue;
            }

            // ~~riscado~~
            if (Comeca(linha, i, "~~"))
            {
                if (EhMarcador(linha, i, "~~", riscado))
                {
                    Fechar();
                    riscado = !riscado;
                }
                else
                {
                    atual.Append("~~");
                }

                i += 2;
                continue;
            }

            // *itálico*
            if (linha[i] == '*')
            {
                if (EhMarcador(linha, i, "*", italico))
                {
                    Fechar();
                    italico = !italico;
                }
                else
                {
                    atual.Append('*');
                }

                i++;
                continue;
            }

            // `código`
            if (linha[i] == '`')
            {
                if (EhMarcador(linha, i, "`", false))
                {
                    Fechar();
                    codigo = true;
                }
                else
                {
                    atual.Append('`');
                }

                i++;
                continue;
            }

            atual.Append(linha[i]);
            i++;
        }

        Fechar();
    }

   
    private static bool Comeca(string linha, int posicao, string marcador)
    {
        return string.CompareOrdinal(linha, posicao, marcador, 0, marcador.Length) == 0;
    }

 
    private static bool EhMarcador(string linha, int posicao, string marcador, bool jaAberto)
    {
        
        if (jaAberto)
            return true;

        int depois = posicao + marcador.Length;

        if (depois >= linha.Length || char.IsWhiteSpace(linha[depois]))
            return false;

        return linha.IndexOf(marcador, depois, StringComparison.Ordinal) >= 0;
    }
}