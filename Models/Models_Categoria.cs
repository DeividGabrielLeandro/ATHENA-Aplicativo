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
    }
}
