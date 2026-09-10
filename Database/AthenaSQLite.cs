using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SQLite;

namespace ATHENA.Database
{

    [Table("Usuario")]
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; } //Id do cliente


        [MaxLength(255), Unique, NotNull]
        public string googleID {  get; set; } //Fornecido pelo google

        [MaxLength(100), NotNull]
        public string nome { get; set; } //Nome de exibição

        [MaxLength(255), NotNull]
        public string email { get; set; } //Email da conta

        [MaxLength(500)]
        public string fotoURL { get; set; } //Foto de perfil

        public DateTime dataCriacao { get; set; } = DateTime.Now; //Criação da conta
    }



    [Table("Meta")]
    public class Meta
    {
        [AutoIncrement, PrimaryKey]
        public int idMeta { get; set; } //Id da meta

        //[NotNull]
        public int? idUsuario { get; set; } // Liga a meta ao idUsuario

        public int? idCategoria { get; set; } //Para poder adicionar meta à categoria

        [MaxLength(150), NotNull]
        public string tituloMeta { get; set; } //Título da meta

        [MaxLength(1000)]
        public string descricaoMeta { get; set; } //Descrição da meta, pode ser nula

        public int metaMinutos { get; set; } = 0; //Define um objetivo de tempo à ser estudado

        public int? minutosEstudados { get; set; } = 0;//Total de tempo estudado na meta

        [MaxLength(20), NotNull]
        public string prioridadeMeta { get; set; } = "Sem prioridade"; //Sem prioridade, Baixa, Média e Alta

        [MaxLength(20), NotNull]
        public string status { get; set; } = "Em andamento"; //Status Concluído ou Em andamento

        [NotNull]
        public DateTime dataCriacao { get; set; } = DateTime.Now; //Data de criação da meta

        public DateTime? dataLimite { get; set; } //Pode ser adicionado uma data limite para completar a meta

        public DateTime? dataConclusao { get; set; } //Data da conclusão

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
        public int idCategoria { get; set; } //Id categoria

        [NotNull]
        public int idUsuario { get; set; } //Facilita pesquisas

        [NotNull]
        public int idMeta { get; set; } //Uma categoria pode ter várias metas

        [MaxLength(100), NotNull]
        public string tituloCategoria { get; set; } //Titulo

        [MaxLength(500)]
        public string descricaoCategoria { get; set; } //Descrição

        public DateTime dataCriacao { get; set; } = DateTime.Now; //Data criaçao
    }

    [Table("SessaoEstudo")]
    public class SessaoEstudo
    {
        //A meta de estudo pode ser dividido em sessões. ex: sessão 1 - 20min estudados

        [PrimaryKey, AutoIncrement]
        public int idSessao { get; set; }

        //[NotNull]
        public int idUsuario { get; set; } //Facilita pesquisas

        [NotNull]
        public int idMeta { get; set; } //Uma meta pode ter várias sessões

        [MaxLength(100), NotNull]
        public string tituloSessao { get; set; } //Titulo

        [MaxLength(500)]
        public string descricaoSessao { get; set; } //Descrição

        public DateTime? DataInicio { get; set; } //Inicio da sessão

        public DateTime? DataFim { get; set; } //Fim da sessão

        public DateTime DataCriacao { get; set; } = DateTime.Now; //Data criação

        public int? duracaoMinutos { get; set; } //Duração

        public int? tempoEstudadoMinutos { get; set; } //Tempo realmente estudado

        [MaxLength(20), NotNull]
        public string status { get; set; } = "Em andamento"; //Em andamento, concluío
    }

    [Table("PausaSessao")]
    public class PausaSessao
    {
        [AutoIncrement, PrimaryKey]
        public int idPausa { get; set; } //Id pausa

        [NotNull]
        public int idSessao { get; set; } //Id sessão

        public DateTime inicioPausa { get; set; } //Hora do início da pausa

        public DateTime? fimPausa { get; set; } //Hora do fim da pausagit log --oneline -4git log --oneline -4git log --oneline -4git log --oneline -4

        public int duracaoMinutos { get; set; } //Duração da pausa

        [MaxLength(255)]
        public string motivoPausa { get; set; } //Motivo
    }
}
