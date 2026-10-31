using ATHENA.Database;
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

        /// <summary>
        /// Salva o resultado do cronômetro numa única transação: ou todas as gravações acontecem, ou nenhuma.
        /// Meta e Sessão somam o tempo na meta (a Sessão também soma na própria sessão);
        /// o estudo livre vira uma sessão sem meta. Toda origem gera um Registro_Tempo para as estatísticas do dia.
        /// </summary>
        public async static Task SalvarEstudo(NewPage1.OrigemCronometro origem, int? idSessao, int? idMeta, NewPage1.ResultadoSessao resultadoSessao)
        {

            double minutos = resultadoSessao.MinutosLiquidos;
            double minutosBruto = resultadoSessao.MinutosBrutos;



            await db.RunInTransactionAsync(conn =>
            {
                if (origem == OrigemCronometro.Livre)
                {
                    var sessao = new Sessao_Estudo
                    {
                        tituloSessao = "Estudo livre",
                        DataInicio = resultadoSessao.DataInicio,
                        DataFim = resultadoSessao.DataFim,
                        duracaoMinutos = minutosBruto,
                        tempoEstudadoMinutos = minutos,
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

                    meta.minutosEstudados = (meta.minutosEstudados ?? 0) + minutos;

                    conn.Update(meta);


                }
                else if (origem == OrigemCronometro.Sessao)
                {
                    if (idMeta == null || idSessao == null)
                        throw new Exception("Os dados da sessão estão incompletos.");

                    var meta = conn.Find<Meta>(idMeta.Value);

                    if (meta == null)
                        throw new Exception("Meta não encontrada.");

                    meta.minutosEstudados = (meta.minutosEstudados ?? 0) + minutos;

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

    