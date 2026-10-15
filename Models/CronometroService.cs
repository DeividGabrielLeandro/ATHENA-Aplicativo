using ATHENA.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace ATHENA.Models
{
    internal class CronometroService
    {

        public static NewPage1.OrigemCronometro Origem { get; private set; }
            = NewPage1.OrigemCronometro.Nenhum;

        public static int? IdMeta { get; private set; }
        public static int? IdSessao { get; private set; }

        public static bool Iniciado { get; private set; }
        public static bool Rodando { get; private set; }

        public static DateTime DataInicio { get; private set; }
        public static DateTime? DataFim { get; private set; }

        private static DateTime inicioTrecho;
        private static TimeSpan tempoLiquidoAcumulado;
        private static TimeSpan? tempoLiquidoFinal;
        private static TimeSpan? tempoBrutoFinal;

        public static TimeSpan TempoLiquido =>
            tempoLiquidoFinal ??
            (tempoLiquidoAcumulado +
             (Rodando ? DateTime.Now - inicioTrecho : TimeSpan.Zero));

        public static TimeSpan TempoBruto =>
            tempoBrutoFinal ??
            (Iniciado ? DateTime.Now - DataInicio : TimeSpan.Zero);

        public static TimeSpan TempoPausado =>
            TempoBruto - TempoLiquido;

        public static bool ConfigurarContexto(
    NewPage1.OrigemCronometro origem,
    int? idMeta = null,
    int? idSessao = null)
        {
            // Depois que o cronômetro começou, preserva o estudo atual.
            if (Iniciado)
                return false;

            Origem = origem;
            IdMeta = idMeta;
            IdSessao = idSessao;

            return true;
        }

        public static void GarantirEstudoLivre()
        {
            if (Origem == NewPage1.OrigemCronometro.Nenhum)
            {
                Origem = NewPage1.OrigemCronometro.Livre;
                IdMeta = null;
                IdSessao = null;
            }
        }

        public static void Iniciar()
        {
            if (Iniciado ||
                Origem == NewPage1.OrigemCronometro.Nenhum)
                return;

            DataInicio = DateTime.Now;
            DataFim = null;

            tempoLiquidoAcumulado = TimeSpan.Zero;
            tempoLiquidoFinal = null;
            tempoBrutoFinal = null;

            inicioTrecho = DateTime.Now;
            Iniciado = true;
            Rodando = true;
        }

        public static void Pausar()
        {
            if (!Iniciado || !Rodando)
                return;

            tempoLiquidoAcumulado += DateTime.Now - inicioTrecho;
            Rodando = false;
        }

        public static void Continuar()
        {
            if (!Iniciado || Rodando)
                return;

            inicioTrecho = DateTime.Now;
            Rodando = true;
        }

        public static NewPage1.ResultadoSessao? PrepararFinalizacao()
        {
            if (!Iniciado)
                return null;

            if (Rodando)
            {
                tempoLiquidoAcumulado += DateTime.Now - inicioTrecho;
                Rodando = false;
            }

            DataFim = DateTime.Now;
            tempoLiquidoFinal = tempoLiquidoAcumulado;
            tempoBrutoFinal = DataFim.Value - DataInicio;

            return new NewPage1.ResultadoSessao
            {
                DataInicio = DataInicio,
                DataFim = DataFim.Value,
                TempoLiquido = tempoLiquidoFinal.Value,
                TempoBruto = tempoBrutoFinal.Value
            };
        }

        public static void Limpar()
        {
            Origem = NewPage1.OrigemCronometro.Nenhum;
            IdMeta = null;
            IdSessao = null;

            Iniciado = false;
            Rodando = false;

            DataInicio = default;
            DataFim = null;

            inicioTrecho = default;
            tempoLiquidoAcumulado = TimeSpan.Zero;
            tempoLiquidoFinal = null;
            tempoBrutoFinal = null;
        }
    }

}


