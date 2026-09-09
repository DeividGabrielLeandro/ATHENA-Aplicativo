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

            var CriaMeta = new Meta{
                tituloMeta = titulo,
                descricaoMeta = descricao,
                metaMinutos = minutos,
                prioridadeMeta = prioridade,
                status = status,};

            await db.InsertAsync(CriaMeta);
        }
        
    }
}
