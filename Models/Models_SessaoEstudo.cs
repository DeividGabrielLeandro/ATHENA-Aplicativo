using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{
    internal class Models_SessaoEstudo
    {
        public static async Task<int> CriarSessao(int idMeta, DateTime dataInicio)
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Sessao_Estudo>();
            
                int quantidadeSessao = await db.Table<Sessao_Estudo>()
                    .Where(mbox => mbox.idMeta == idMeta)
                    .CountAsync();

                string tituloSessao = $"Sessão {quantidadeSessao + 1}";
           

            var sessao = new Sessao_Estudo
            {
                tituloSessao = tituloSessao,
                DataInicio = dataInicio,
                idMeta = idMeta,
                tempoEstudadoMinutos = 0,
            };

            await db.InsertAsync(sessao);

            return sessao.idSessao;
        }
        public static async Task FinalizarSessao(
           string? tituloSessao,
           string descricaoSessao,
           DateTime dataFim,
           double duracaoMinutosBruto,
           double duracaoMinutosLiquido,
           int idSessao
           )
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Sessao_Estudo>();

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                .Where(s => s.idSessao == idSessao)
                .FirstOrDefaultAsync();

            if (sessao == null)
                return;

            sessao.descricaoSessao = descricaoSessao;
            sessao.DataFim = dataFim;
            sessao.duracaoMinutos = duracaoMinutosBruto;
            sessao.tempoEstudadoMinutos = duracaoMinutosLiquido;

            if (tituloSessao != null)
                sessao.tituloSessao = tituloSessao;

            await db.UpdateAsync(sessao);
        }

        public static async Task SalvarPausaSessao(
            int idSessao,
            double duracaoMinutos,
            string? motivoPausa
            )
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Pausa_Sessao>();

            var Pausa = new Pausa_Sessao
            {
                idSessao = idSessao,
                duracaoMinutos = duracaoMinutos,
                motivoPausa = motivoPausa
            };

            await db.InsertAsync(Pausa);
        }

        public static async Task DeletarSessao(int idSessao)
        {
            string dbPath = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.LocalApplicationData),
         "ATHENA.db"
     );

            var db = new SQLiteAsyncConnection(dbPath);

            Sessao_Estudo? sessao = await db.Table<Sessao_Estudo>()
                .Where(m => m.idSessao == idSessao)
                .FirstOrDefaultAsync();

            if (sessao == null)
                return;

            await db.DeleteAsync(sessao);
        }

    }
}
