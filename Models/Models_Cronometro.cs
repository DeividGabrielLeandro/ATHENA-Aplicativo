using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{

    internal class Models_Cronometro
    {
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
    }
    }

    