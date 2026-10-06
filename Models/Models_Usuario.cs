using ATHENA.Database;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{
    internal class Models_Usuario
    {
        public static async Task LoginUsuario()
        {

            string dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ATHENA.db"
);

            var db = new SQLiteAsyncConnection(dbPath);

            await db.CreateTableAsync<Usuario>();

            var usuario = new Usuario
            {
                nome = "Usuario",
                dataCriacao = DateTime.Now
            };

            await db.InsertAsync(usuario);

        }

        public static async Task AtualizarUsuario(
           string? Nome,
           string? fotoPerfil)
        {
            string dbPath = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.LocalApplicationData),
         "ATHENA.db"
     );

            var db = new SQLiteAsyncConnection(dbPath);

            Usuario? usuario = await db.Table<Usuario>()
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return;
            }

            if (Nome != null)
            {
                usuario.nome = Nome;
            }

            if (fotoPerfil != null)
            {
                usuario.fotoPerfil = fotoPerfil;
            }

            await db.UpdateAsync(usuario);
        }
    }
}
