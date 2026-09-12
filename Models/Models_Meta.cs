using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{
    internal class Models_Meta
    {
        public static async Task CriarMeta(
            string titulo,
            string descricao,
            int minutos,
            string prioridade,
            string status)
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Meta>();

            var CriaMeta = new Meta
            {
                tituloMeta = titulo,
                descricaoMeta = descricao,
                metaMinutos = minutos,
                prioridadeMeta = prioridade,
                status = status,
            };

            await db.InsertAsync(CriaMeta);
        }
        public static async Task EditarMeta(
            int idMeta,
            string? titulo,
            string? descricao,
            int? minutos,
            string? prioridade,
            string? status)
        {
            string dbPath = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.LocalApplicationData),
         "ATHENA.db"
     );

            var db = new SQLiteAsyncConnection(dbPath);

            Meta? meta = await db.Table<Meta>()
                .Where(m => m.idMeta == idMeta)
                .FirstOrDefaultAsync();

            if (meta == null)
                return;

            if (titulo != null)
                meta.tituloMeta = titulo;

            if (descricao != null)
                meta.descricaoMeta = descricao;

            if (minutos != null)
                meta.metaMinutos = minutos.Value;

            if (prioridade != null)
                meta.prioridadeMeta = prioridade;

            if (status != null)
                meta.status = status;

            await db.UpdateAsync(meta);
        }


        public static async Task DeletarMeta(int idMeta)
        {
            string dbPath = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.LocalApplicationData),
         "ATHENA.db"
     );

            var db = new SQLiteAsyncConnection(dbPath);

            Meta? meta = await db.Table<Meta>()
                .Where(m => m.idMeta == idMeta)
                .FirstOrDefaultAsync();

            if (meta == null)
                return;

            await db.DeleteAsync(meta);
        }


        public static async Task AdicionarRemoverData(
            int idMeta,
            DateTime? dataPrazo,
            char opcao
            )
        {

            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);


            Meta? meta = await db.Table<Meta>()
               .Where(m => m.idMeta == idMeta)
               .FirstOrDefaultAsync();

            if(meta == null) return;

            if (opcao == 'A')
            {
   
               meta.dataLimite = dataPrazo;

            }
        
            else if(opcao == 'R')
            {
               meta.dataLimite = null;
            }

            await db.UpdateAsync(meta);
        }
    }
}
