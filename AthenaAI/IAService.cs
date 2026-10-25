using ATHENA.AthenaAI;
using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ATHENA.AthenaAI
{
    internal class IAService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> Mensagens(string prompt)
        {
            IASettings settings = new IASettings();
            settings.Carregar();

            string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db");

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<MensagemChat>();

            var historico = await db.Table<MensagemChat>()
                .OrderBy(m => m.dataCriacao)
                .ToListAsync();

            string apiKey = await SecureStorage.Default.GetAsync("GeminiApiKey") ?? string.Empty;

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{settings.Model}:generateContent?key={apiKey}";

            var requestBody = new
            {
                generationConfig = new
                {
                    maxOutputTokens = 800
                },

                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = settings.SystemPrompt }
                    }
                },


                contents = historico.Select(m => new
                {
                    role = m.Papel,
                    parts = new[]
                    {
                        new { text = m.Conteudo }
                    }
                })
                .Append(new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                })
                .ToArray()

            };

            string Json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(Json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, content);
                string resposta = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using JsonDocument doc = JsonDocument.Parse(resposta);

                    var textResponse = doc.RootElement
                           .GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString();

                    if (string.IsNullOrEmpty(textResponse))
                        return "A IA não retornou uma resposta.";

                    await SalvarMensagem("user", prompt);
                    await SalvarMensagem("model", textResponse);

                    return textResponse;   
                }

                else
                {
                    int codigo = (int)response.StatusCode;

                    if (codigo == 503)
                        return "O modelo do Google está com alta demanda agora. Tente de novo em instantes ou escolha outro modelo no Perfil.";

                    if (codigo == 429)
                        return "O limite de uso gratuito foi atingido. Aguarde um pouco e tente de novo.";

                    return $"Erro na requisição: {response.StatusCode}\n{resposta}";
                }
            }
            catch (Exception ex)
            {
                return ($"Erro: {ex.Message}");
            }

        }

        public async static Task SalvarMensagem(string papel, string mensage)
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db");

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<MensagemChat>();

            MensagemChat? MensagemChat = await db.Table<MensagemChat>()
           .FirstOrDefaultAsync();

                MensagemChat = new MensagemChat
                {
                    Papel = papel,
                    Conteudo = mensage,
                    dataCriacao = DateTime.Now
                };

                await db.InsertAsync(MensagemChat);

            
        }
        public async static Task ApagarMensagem()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ATHENA.db");

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<MensagemChat>();

            MensagemChat? primeiraMensagem = await db.Table<MensagemChat>()
                .OrderBy(m => m.dataCriacao)
                .FirstOrDefaultAsync();

            if (primeiraMensagem != null &&
                DateTime.Now >= primeiraMensagem.dataCriacao.AddHours(24))
            {
                await db.DeleteAllAsync<MensagemChat>();
                primeiraMensagem = null;
            }
        }
    }
}

