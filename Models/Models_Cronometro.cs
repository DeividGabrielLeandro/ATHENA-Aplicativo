using ATHENA.Database;
using ATHENA.Metas.xaml;
using ATHENA.Services;
using ATHENA.SessaoEstudo;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using static ATHENA.NewPage1;

namespace ATHENA.Models
{

    internal class Models_Cronometro
    {
        private static SQLiteAsyncConnection db;

        private static string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ATHENA.db"
        );

        static Models_Cronometro()
        {
            db = new SQLiteAsyncConnection(dbPath);
        }

        public static async Task AdicionaTempoMeta(NewPage1.ResultadoSessao resultadoSessao, int idMeta)
        {
            {
                string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
    );  

                var db = new SQLiteAsyncConnection(dbPath);

                Meta meta = await db.GetAsync<Meta>(idMeta);

                meta.minutosEstudados += (int)resultadoSessao.MinutosLiquidos;

                await db.UpdateAsync(meta);
            }
        }

        public static async Task AdicionaTempoEstudoLivre(NewPage1.ResultadoSessao resultadoSessao)
        {

            int minutosEstudados = ((int)resultadoSessao.MinutosLiquidos);
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
 );

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Sessao_Estudo>();

            var sessao = new Sessao_Estudo
            {
                tituloSessao = "Estudo livre",
                duracaoMinutos = minutosEstudados,
                idMeta = null,
            };

            await db.InsertAsync(sessao);
        }

        public static async Task Registro_Estudo(NewPage1.ResultadoSessao resultadoSessao)
        {


            double minutosEstudados = ((double)resultadoSessao.MinutosLiquidos);
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
 );

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Registro_Tempo>();

            var registro = new Registro_Tempo
            {
                tempoEstudo = minutosEstudados,
                data = DateTime.Now,
            };

            await db.InsertAsync(registro);
        }

        public async static Task SalvarEstudo(NewPage1.OrigemCronometro origem, int? idSessao, int? idMeta, NewPage1.ResultadoSessao resultadoSessao)
        {

            double minutos = (int)resultadoSessao.MinutosLiquidos;
            double minutosBruto = (int)resultadoSessao.MinutosBrutos;



            await db.RunInTransactionAsync(conn =>
            {
                if (origem == OrigemCronometro.Livre)
                {
                    var sessao = new Sessao_Estudo
                    {
                        tituloSessao = "Estudo livre",
                        duracaoMinutos = minutos,
                        idMeta = null
                    };

                    conn.Insert(sessao);

                }
                else if (origem == OrigemCronometro.Meta)
                {
                    if (idMeta == null)
                        throw new Exception("Nenhuma meta foi selecionada.");

                        var meta = conn.Find<Meta>(idMeta.Value);

                    if (meta == null)
                        throw new Exception("Meta não encontrada.");

                    meta.minutosEstudados = (meta.minutosEstudados ?? 0) + (int)minutos;

                    conn.Update(meta);


                }
                else if (origem == OrigemCronometro.Sessao)
                {
                    if (idMeta == null || idSessao == null)
                        throw new Exception("Os dados da sessão estão incompletos.");

                    var meta = conn.Find<Meta>(idMeta.Value);

                    if (meta == null)
                        throw new Exception("Meta não encontrada.");

                    meta.minutosEstudados = (meta.minutosEstudados ?? 0) + (int)minutos;

                    conn.Update(meta);

                    var sessao = conn.Find<Sessao_Estudo>(idSessao.Value);

                    if (sessao == null)
                        throw new Exception("Sessão não encontrada.");

                    sessao.tempoEstudadoMinutos = (sessao.tempoEstudadoMinutos ?? 0) + minutos;
                    sessao.duracaoMinutos = (sessao.duracaoMinutos ?? 0) + minutosBruto;

                    conn.Update(sessao);

                }
                else
                {
                    throw new Exception("Origem do cronômetro inválida.");
                }

                var registro = new Registro_Tempo
                {
                    tempoEstudo = minutos,
                    data = DateTime.Now
                };

                conn.Insert(registro);
            });
        }
    }
}

    