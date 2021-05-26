using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ExAlgo.Core.Contracts;
using KiteConnect;
using NLog;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.Cache
{
    public class QuotesContainer
    {
        public List<QuoteExtention> QuoteExtentions { get; set; }
        public ConcurrentDictionary<Int64,  decimal> OpeningPrice { get; set; }
        public bool IsEMA200CrossOverTrigerred = false;
        public bool IsEMA200CrossUnderTrigerred = false;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public bool IsFirstTickProcessed = false;


        public QuotesContainer()
        {
            OpeningPrice = new ConcurrentDictionary<long, decimal>();
        }

        public bool IsPriceOverEmaLastDay()
        {
            var lastDayQuotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(25);
            return !lastDayQuotes.Any(_ => _.Close < _.EMA200);
        }

        public bool IsPriceUnderEmaLastDay()
        {
            var lastDayQuotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(25);
            return !lastDayQuotes.Any(_ => _.Close > _.EMA200);
        }

        public bool EmaCrossUnder(Tick tick)
        {
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            return tick.LastPrice < quote[0].EMA200;
        }

        public bool EmaCrossOver(Tick tick)
        {
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            return tick.LastPrice > quote[0].EMA200;
        }


        public ConnorsRsiStrategyOutput GetConnorsRsiResult(Tick tick)
        {

            var quotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(500).ToList();
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).First();
            var response = new ConnorsRsiStrategyOutput();
            var quoteDate = DateTime.Now;
            quotes.Add(new QuoteExtention
            {
                Date = quoteDate,
                Close = tick.LastPrice,
                Open = tick.Open,
                Low = tick.Low,
                Volume = tick.Volume,
                High = tick.High
            });
            var rsi = Indicator.GetConnorsRsi(quotes.OrderBy(_ => _.Date)).ToList().First(_ => _.Date == quoteDate);
            //var ema200 = Indicator.GetEma(quotes.OrderBy(_ => _.Date),200).ToList().First(_ => _.Date == quoteDate);
            var diff = (double)quote.EMA200 * (0.5) / 100;
            response.ConnorsRsi = rsi;



            if (rsi.ConnorsRsi < 10 && quote.EMA200 < tick.LastPrice && (((double)tick.LastPrice - (double)quote.EMA200) > diff))
            {
                response.IsTradable = true;
                response.Order = ConnorsRsiStrategyOutput.OrderType.Long;
                response.Ema = quote.EMA200;
            }
            else if (rsi.ConnorsRsi > 90 && quote.EMA200 > tick.LastPrice && ((double)quote.EMA200 - (double)tick.LastPrice) > diff)
            {
                response.IsTradable = true;
                response.Order = ConnorsRsiStrategyOutput.OrderType.Short;
                response.Ema = quote.EMA200;
            }

            return response;
        }


        public bool MovingAverageCrossOverAboveEMA200(int interval, Tick tick)
        {

            var quotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(120).ToList();
            var quoteDate = DateTime.Now;
            quotes.Add(new QuoteExtention
            {
                Date = quoteDate,
                Close = tick.LastPrice,
                Open = tick.Open,
                Low = tick.Low,
                Volume = tick.Volume,
                High = tick.High
            });
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            var emaInterval = Indicator.GetEma(quotes.OrderBy(_ => _.Date), interval).ToList().First(_ => _.Date == quoteDate);
            return tick.LastPrice > emaInterval.Ema;
        }


        public bool MovingAverageEarlyTradePriceAboveEma(Tick tick)
        {

            var quotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(500).ToList();
            var quoteDate = DateTime.Now;
            quotes.Add(new QuoteExtention
            {
                Date = quoteDate,
                Close = tick.LastPrice,
                Open = tick.Open,
                Low = tick.Low,
                Volume = tick.Volume,
                High = tick.High
            });
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            var emaInterval200 = Indicator.GetEma(quotes.OrderBy(_ => _.Date), 200).ToList().First(_ => _.Date == quoteDate);
            var diffinPercent = ((tick.LastPrice - emaInterval200.Ema) * 100) / emaInterval200.Ema;
            return tick.LastPrice > emaInterval200.Ema && diffinPercent > 1;
        }


        public bool MovingAverageEarlyTradePriceBelowEma(Tick tick)
        {

            var quotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(500).ToList();
            var quoteDate = DateTime.Now;
            quotes.Add(new QuoteExtention
            {
                Date = quoteDate,
                Close = tick.LastPrice,
                Open = tick.Open,
                Low = tick.Low,
                Volume = tick.Volume,
                High = tick.High
            });
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            var emaInterval200 = Indicator.GetEma(quotes.OrderBy(_ => _.Date), 200).ToList().First(_ => _.Date == quoteDate);
            var diffinPercent = ((emaInterval200.Ema - tick.LastPrice) * 100) / tick.LastPrice;
            return tick.LastPrice < emaInterval200.Ema && diffinPercent > 1;
        }


        public bool MovingAverageCrossUnderAboveEMA200(int interval, Tick tick)
        {
            var quotes = QuoteExtentions.OrderByDescending(_ => _.Date).Take(120).ToList();
            var quoteDate = DateTime.Now;
            quotes.Add(new QuoteExtention
            {
                Date = quoteDate,
                Close = tick.LastPrice,
                Open = tick.Open,
                Low = tick.Low,
                Volume = tick.Volume,
                High = tick.High
            });
            var quote = QuoteExtentions.OrderByDescending(_ => _.Date).Take(1).ToArray();
            var emaInterval = Indicator.GetEma(quotes.OrderBy(_ => _.Date), interval).ToList().First(_ => _.Date == quoteDate);
            return tick.LastPrice < emaInterval.Ema;
        }

        public DateTime PreviousWorkDay()
        {
            var date = DateTime.Now;
            do
            {
                date = date.AddDays(-1);
            }
            while (IsWeekend(date));

            return date;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
        }


        public decimal? PivotPoint { get; set; }

        public decimal? Resistance1 { get; set; }
        public decimal? Resistance2 { get; set; }

        public decimal? Support1 { get; set; }
        public decimal? Support2 { get; set; }

        public bool IsUptrendPivot { get; set; }
        public decimal? LastPivotPoint { get; set; }

        public bool IsActiveStock { get; set; }
    }

    public class ConnorsRsiStrategyOutput
    {
        public enum OrderType
        {
            None,
            Long,
            Short
        }
        public ConnorsRsiStrategyOutput()
        {
            IsTradable = false;
        }
        public OrderType Order { get; set; }
        public bool IsTradable { get; set; }
        public ConnorsRsiResult ConnorsRsi { get; set; }
        public decimal Ema { get; set; }

        
    }
}
