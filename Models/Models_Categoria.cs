using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{
    internal class Models_Categoria
    {

        public static async Task CriarCategoria(
            string tituloCategoria,
            string descricaoCategoria)
        {
            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Categoria>();

            var CriaCategoria = new Categoria
            {
                tituloCategoria = tituloCategoria,
                descricaoCategoria = descricaoCategoria
            };

            await db.InsertAsync(CriaCategoria);
        }

        public static async Task AdicionarMetaCategoria(
 int idMeta,
 int idCategoria)
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

            Categoria? categoria = await db.Table<Categoria>()
                .Where(c => c.idCategoria == idCategoria)
                .FirstOrDefaultAsync();

            if (categoria == null)
                return;

            meta.idCategoria = idCategoria;

            await db.UpdateAsync(meta);


}



        public static async Task DeletarCategoria(int idCategoria)
        {
            string dbPath = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.LocalApplicationData),
         "ATHENA.db"
     );

            var db = new SQLiteAsyncConnection(dbPath);


           var metas  = await db.Table<Meta>()
               .Where(m => m.idCategoria == idCategoria)
               .ToListAsync();

            foreach(var meta in metas)
            {
                meta.idCategoria = null;
                await db.UpdateAsync(meta);
            }

            Categoria? categoria = await db.Table<Categoria>()
                .Where(m => m.idCategoria == idCategoria)
                .FirstOrDefaultAsync();

            if (categoria == null)
                return;

            await db.DeleteAsync(categoria);
        }
    }
}
