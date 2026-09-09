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
    }
}
    