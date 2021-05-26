using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using ExAlgo.Core.Contracts;
using ExAlgo.Core.Zerodha;
using NLog;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.Cache
{
    public class QuoteRepositoryManager
    {

        private readonly ZerodhaClient _zerodhaClient;
        private readonly IQuoteRepository _quoteRepository;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        public QuoteRepositoryManager(Zerodha.ZerodhaClient zerodhaClient, IQuoteRepository quoteRepository)
        {
            _zerodhaClient = zerodhaClient;
            _quoteRepository = quoteRepository;
        }

        public void LoadHistoricalData()
        {
            Logger.Info($"Historical Data loaded at  {DateTime.Now}");
            foreach (var nse in NSE.NationalStockExchange50)
            {
                var history = _zerodhaClient.GetHistoricalData(new Query()
                {
                    InstrumentToken = nse.Key,
                    FromDate = DateTime.Now.AddDays(-30),
                    ToDate = DateTime.Now,
                    Interval = "15minute",
                    Continuous = false,
                    Oi = false
                });


                var historyInday = _zerodhaClient.GetHistoricalData(new Query()
                {
                    InstrumentToken = nse.Key,
                    FromDate = DateTime.Now.AddDays(-5),
                    ToDate = DateTime.Now,
                    Interval = "day",
                    Continuous = false,
                    Oi = false
                });

                //history.RemoveAt(history.Count-1);
                var quotes = new List<QuoteExtention>(); 
                    var pivotQuote = new List<QuoteExtention>();
                foreach (var hist in history)
                {
                    quotes.Add(new QuoteExtention()
                    {
                        Date = hist.TimeStamp,
                        Close = hist.Close,
                        Open = hist.Open,
                        Low = hist.Low,
                        Volume = hist.Volume,
                        High = hist.High
                    });


                    pivotQuote.Add(new QuoteExtention()
                    {
                        Date = hist.TimeStamp,
                        Close = hist.Close,
                        Open = hist.Open,
                        Low = hist.Low,
                        Volume = hist.Volume,
                        High = hist.High
                    });
                }

                //var crooner = Indicator.GetConnorsRsi(quotes);
                Indicator.GetConnorsRsi(quotes).ToList().ForEach(_ =>
                {
                    var quote = quotes.Find(x => x.Date == _.Date);
                    quote.ConnorsRsi = _.ConnorsRsi;
                    quote.RsiClose = _.RsiClose;
                    quote.RsiStreak = _.RsiStreak;
                    quote.PercentRank = _.PercentRank;
                });


                Indicator.GetAdl(quotes).ToList().ForEach(_ =>
                {
                    quotes.Find(x => x.Date == _.Date).ADL = _.Adl;
                });



                Indicator.GetEma(quotes, 200).ToList().ForEach(_ =>
                {
                    if (_.Ema != null) quotes.Find(x => x.Date == _.Date).EMA200 = _.Ema.Value;
                });




                var latestQuote = quotes.OrderByDescending(_ => _.Date).First();
                pivotQuote.Add(new QuoteExtention()
                {
                    Date = DateTime.Now.AddDays(1),
                    Close = latestQuote.Close,
                    Open = latestQuote.Open,
                    Low = latestQuote.Low,
                    Volume = latestQuote.Volume,
                    High = latestQuote.High
                });



                var lastDayQuote = historyInday.OrderByDescending(_ => _.TimeStamp).First();
                var change = ((lastDayQuote.High - lastDayQuote.Low) / Math.Abs(lastDayQuote.Low)) * 100;
                

                //var pivotPoint = Indicator.GetPivotPoints(pivotQuote, PeriodSize.Day).ToList().OrderByDescending(_ => _.Date).First();
                var pivotCollection = Indicator.GetPivotPoints(pivotQuote, PeriodSize.Day).ToList();
                
                var pivotPoint = pivotCollection.OrderByDescending(_ => _.Date).First();
                var previousPivotPoint = pivotCollection.OrderByDescending(_ => _.Date).Skip(1).First();
                var isUptrend = pivotPoint.PP > previousPivotPoint.PP ? true : false;
                _quoteRepository.HistoricalQuotes.TryAdd(nse.Key, history);
                _quoteRepository.QuotesContainers.TryAdd(nse.Key, new QuotesContainer
                {
                    QuoteExtentions = quotes,
                    PivotPoint = pivotPoint.PP,
                    Resistance1 = pivotPoint.R1,
                    Resistance2 = pivotPoint.R2,
                    Support1 = pivotPoint.S1,
                    Support2 = pivotPoint.S2,
                    LastPivotPoint = previousPivotPoint.PP,
                    IsUptrendPivot = isUptrend,
                    IsActiveStock = change >= 1.5M ? true : false
                }); 


                Logger.Info($"{nse.Value} Pivot Details , PivoPoint: {pivotPoint.PP},previousPivotPoint: {previousPivotPoint.PP},IsUptrendPivot : {isUptrend},  Resistance1: {pivotPoint.R1}, Support1: {pivotPoint.S1},  Points Change :{change}");
                _quoteRepository.LastDayClosePrice.TryAdd(nse.Key, lastDayQuote.Close);
            }

            foreach (var nse in NSE.NseIndices)
            {
                var historyInday = _zerodhaClient.GetHistoricalData(new Query()
                {
                    InstrumentToken = nse.Key,
                    FromDate = DateTime.Now.AddDays(-5),
                    ToDate = DateTime.Now,
                    Interval = "day",
                    Continuous = false,
                    Oi = false
                });

                var lastDayQuote = historyInday.OrderByDescending(_ => _.TimeStamp).First();
                _quoteRepository.LastDayClosePrice.TryAdd(nse.Key, lastDayQuote.Close);
            }
        }



        public void LoadCurrentData()
        {
            Thread.Sleep(3000);
            Logger.Info($"Current Data Load started  at  {DateTime.Now}");
            foreach (var nse in NSE.NationalStockExchange50)
            {
                //var history = _zerodhaClient.GetHistoricalData(new Query()
                //{
                //    InstrumentToken = nse.Key,
                //    FromDate = DateTime.Now.AddDays(-1),
                //    ToDate = DateTime.Now,
                //    Interval = "15minute",
                //    Continuous = false,
                //    Oi = false
                //});
                //var latest = history.OrderByDescending(_ => _.TimeStamp).Skip(1).First();

                _quoteRepository.QuotesContainers.TryGetValue(nse.Key, out var quotesContainer);
                if (quotesContainer != null)
                {

                    var currentPulldownTime = TimeRoundDown(DateTime.Now);
                    var previousPullDownTime = TimeRoundDown(currentPulldownTime.AddMinutes(-5));
                    quotesContainer.OpeningPrice.TryGetValue(Int64.Parse(currentPulldownTime.ToString("ddMMyyyyHHmm")), out var currentClosePrice);
                    quotesContainer.OpeningPrice.TryGetValue(Int64.Parse(previousPullDownTime.ToString("ddMMyyyyHHmm")), out var currentOpenPrice);

                    var quotes = quotesContainer.QuoteExtentions.OrderByDescending(_ => _.Date).Take(400).ToList();

                    if (quotes.All(_ => _.Date != previousPullDownTime))
                    {

                        if(currentClosePrice == 0 || currentOpenPrice == 0)
                        {

                            Logger.Info($"{nse.Value} Open {currentOpenPrice}, Close {currentClosePrice}");
                        }

                        var currentQuote = new QuoteExtention()
                        {
                            Date = previousPullDownTime,
                            Close = currentClosePrice,
                            Open = currentOpenPrice,
                            Low = 0,
                            Volume = 0,
                            High = 0
                        };


                        var his = new History
                        {
                            Close = currentClosePrice,
                            Open = currentOpenPrice,
                            Low = 0,
                            Volume = 0,
                            High = 0,
                            TimeStamp = previousPullDownTime
                        };
                        quotes.Add(currentQuote);


                        var latestQuoteExtention = quotes.Find(_ => _.Date == previousPullDownTime);
                        var connorsRsi = Indicator.GetConnorsRsi(quotes.OrderBy(_ => _.Date)).ToList()
                            .Find(_ => _.Date == previousPullDownTime);
                        var ema200 = Indicator.GetEma(quotes.OrderBy(_ => _.Date), 200).ToList()
                            .Find(_ => _.Date == previousPullDownTime);

                        latestQuoteExtention.ConnorsRsi = connorsRsi.ConnorsRsi;
                        latestQuoteExtention.RsiClose = connorsRsi.RsiClose;
                        latestQuoteExtention.RsiStreak = connorsRsi.RsiStreak;
                        latestQuoteExtention.PercentRank = connorsRsi.PercentRank;
                        latestQuoteExtention.EMA200 = ema200.Ema.Value;

                        _quoteRepository.HistoricalQuotes.TryGetValue(nse.Key, out var histCollection);
                        histCollection?.Add(his);
                        quotesContainer?.QuoteExtentions.Add(latestQuoteExtention);
                    }

                }
            }

            Logger.Info($"Current Data Load Closed  at  {DateTime.Now}");
        }


        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }


    }
}
