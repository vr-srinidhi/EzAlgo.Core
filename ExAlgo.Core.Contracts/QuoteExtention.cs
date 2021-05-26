using System;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.Contracts
{
    public class QuoteExtention : Quote
    {
        public decimal EMA200 { get; set; }
        public Decimal? RsiClose { get; set; }

        public Decimal? RsiStreak { get; set; }

        public Decimal? PercentRank { get; set; }

        public Decimal? ConnorsRsi { get; set; }

        internal Decimal? Streak { get; set; }

        internal Decimal? PeriodGain { get; set; }

        public Decimal? ADL { get; set; }


    }
}
