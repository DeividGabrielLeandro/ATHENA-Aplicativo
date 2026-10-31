using SQLite;

namespace ATHENA.Database
{

    [Table("Usuario")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [MaxLength(100), NotNull]
        public string nome { get; set; }

        [MaxLength(255)]
        public string? email { get; set; }

        public double? metaDiaria { get; set; } = 60;
        public string? fotoPerfil { get; set; }

        public DateTime dataCriacao { get; set; } = DateTime.Now;
    }

    [Table("Registro_Tempo")]
    public class Registro_Tempo
    {
        public DateTime data { get; set; }
        public double tempoEstudo { get; set; }
    }



    [Table("Meta")]
    public class Meta
    {
        [AutoIncrement, PrimaryKey]
        public int idMeta { get; set; }

        public int? idUsuario { get; set; }

        public int? idSessao { get; set; }

        public int? idCategoria { get; set; }

        [MaxLength(150), NotNull]
        public string tituloMeta { get; set; }

        [MaxLength(1000)]
        public string? descricaoMeta { get; set; }

        public int metaMinutos { get; set; } = 0; //Objetivo de tempo a ser estudado

        public int? minutosEstudados { get; set; } = 0; //Total estudado na meta, somando todas as origens

        [MaxLength(20), NotNull]
        public string prioridadeMeta { get; set; } = "Sem prioridade"; //Sem prioridade, Baixa, Média ou Alta

        [MaxLength(20), NotNull]
        public string status { get; set; } = "Em andamento"; //Em andamento ou Concluida

        [NotNull]
        public DateTime dataCriacao { get; set; } = DateTime.Now;

        public DateTime? dataLimite { get; set; }

        public DateTime? dataConclusao { get; set; }

        [Ignore]
        public double Progresso
        {
            get
            {
                if (metaMinutos <= 0)
                    return 0;

                double progresso =
                    (double)(minutosEstudados ?? 0) / metaMinutos;

                return Math.Min(progresso, 1);
            }
        }

        [Ignore]
        public string TextoDataLimite
        {
            get
            {
                if (dataLimite == null)
                    return "Não possui prazo";

                return dataLimite.Value.ToString("dd/MM/yyyy");
            }
        }
    }


    [Table("Categoria")]
    public class Categoria
    {
        [AutoIncrement,PrimaryKey]
        public int idCategoria { get; set; }

        public int? idUsuario { get; set; }

        public int? idMeta { get; set; }

        [MaxLength(100), NotNull]
        public string tituloCategoria { get; set; }

        [MaxLength(500)]
        public string? descricaoCategoria { get; set; }

        public DateTime dataCriacao { get; set; } = DateTime.Now;
    }

    [Table("SessaoEstudo")]
    public class Sessao_Estudo
    {
        //Uma meta pode ser dividida em sessões. Ex.: Sessão 1 - 20 min estudados.
        //O estudo livre também é gravado aqui, sem meta (idMeta nulo).

        [PrimaryKey, AutoIncrement]
        public int idSessao { get; set; }

        public int? idUsuario { get; set; }

        public int? idMeta { get; set; }

        [MaxLength(100)]
        public string? tituloSessao { get; set; }

        [MaxLength(500)]
        public string? descricaoSessao { get; set; }

        public DateTime? DataInicio { get; set; }

        public DateTime? DataFim { get; set; } //Nulo enquanto a sessão não foi salva

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public double? duracaoMinutos { get; set; } //Tempo bruto, incluindo as pausas

        public double? tempoEstudadoMinutos { get; set; } //Tempo líquido, sem as pausas

        [Ignore]
        public string TempoLiquidoFormatado
        {
            get
            {
                TimeSpan tempo = TimeSpan.FromMinutes(tempoEstudadoMinutos ?? 0);
                return tempo.ToString(@"hh\:mm\:ss");
            }
        }

        [Ignore]
        public string TempoBrutoFormatado
        {
            get
            {
                TimeSpan tempo = TimeSpan.FromMinutes(duracaoMinutos ?? 0);
                return tempo.ToString(@"hh\:mm\:ss");
            }
        }

        [Ignore]
        public string TempoPausaFormatado
        {
            get
            {
                double pausa = (duracaoMinutos ?? 0) - (tempoEstudadoMinutos ?? 0);

                TimeSpan tempo = TimeSpan.FromMinutes(pausa);
                return tempo.ToString(@"hh\:mm\:ss");
            }
        }
    }

    [Table("PausaSessao")]
    public class Pausa_Sessao
    {
        [AutoIncrement, PrimaryKey]
        public int idPausa { get; set; }

        [NotNull]
        public int idSessao { get; set; }


        public double duracaoMinutos { get; set; }

        [MaxLength(255)]
        public string? motivoPausa { get; set; }
    }


    [Table("MensagemChat")]
    public class MensagemChat
    {
        [AutoIncrement, PrimaryKey]
        public int id { get; set; }

        [NotNull]
        public string Papel { get; set; } //"user" ou "model", como a API do Gemini espera

        public string Conteudo { get; set; }

        public DateTime dataCriacao { get; set; }
    }
}
