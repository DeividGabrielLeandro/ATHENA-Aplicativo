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

            Usuario? usuario = await db.Table<Usuario>()
           .FirstOrDefaultAsync();

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    nome = "Usuario",
                    dataCriacao = DateTime.Now
                };

                await db.InsertAsync(usuario);

            }
        }

        public static async Task AtualizarUsuario(
           string? Nome,
           double? MetaDiaria,
           string? FotoPerfil = null)
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
            if (MetaDiaria != null)
            {
                usuario.metaDiaria = MetaDiaria;
            }

            if (FotoPerfil != null)
            {
                usuario.fotoPerfil = FotoPerfil;
            }



            await db.UpdateAsync(usuario);
        }
    }
}
