using System;
using System.Linq;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;
using NLog;

namespace ExAlgo.Core.Strategy
{
    public class BullishAndBearisEngulfingRunTime : BaseStrategy, IBullishAndBearisEngulfingRunTime
    {

        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        public BullishAndBearisEngulfingRunTime(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            _quoteRepository = quoteRepository;
            _orderProcessor = orderProcessor;
        }

        public bool IsTradeTimeOpen()
        {
            TimeSpan startTimeSpan = new TimeSpan(10, 00, 00);
            TimeSpan endTimeSpan = new TimeSpan(13, 30, 0);
            TimeSpan currentTimeSpan = DateTime.Now.TimeOfDay;
            var response = currentTimeSpan > startTimeSpan && currentTimeSpan < endTimeSpan;

            if(response)
            {
                //var pulldownTime = TimeRoundDown(DateTime.Now);
                if ((DateTime.Now.Minute % 15) == 0 &&
                    DateTime.Now.Second <= 3)
                {
                    response = true;
                }
                else
                {
                    response = false;
                }
            }

            return response;
        }

        public bool ProcessQuote(Tick tick)
        {
           _quoteRepository.HistoricalQuotes.TryGetValue(tick.InstrumentToken.ToString(),out var histories);
            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];

            var pulldownTime = TimeRoundDown(DateTime.Now.AddMinutes(-5));

            var key = Int64.Parse(pulldownTime.ToString("ddMMyyyyHHmm"));

            if (!quotes.OpeningPrice.TryGetValue(key, out var lastOpenPrice))
                return true;

            var last2TradingBlock = histories.OrderByDescending(_ => _.TimeStamp).Take(2);
            
            decimal last2Days = 0;
            foreach (var prev in last2TradingBlock.Take(2))
            {
                if (prev.Open > prev.Close)
                {
                    last2Days += (prev.Open - prev.Close) / Math.Abs(prev.Close) * 100;
                }
                else
                {
                    last2Days += (prev.Close - prev.Open) / Math.Abs(prev.Open) * 100;
                }
            }

            if (lastOpenPrice > tick.LastPrice &&
                ((lastOpenPrice - tick.LastPrice) / Math.Abs(tick.LastPrice) * 100) > 0.45M &&
                ((lastOpenPrice - tick.LastPrice) / Math.Abs(tick.LastPrice) * 100) < 1M &&
                ((lastOpenPrice - tick.LastPrice) / Math.Abs(tick.LastPrice) * 100) > last2Days &&
                !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick,Contracts.Strategy.BullishAndBearisEngulfing) 
                )
            {
                var orderQuote = MapShortStrikePrice(tick, Contracts.Strategy.BullishAndBearisEngulfing, stopLoss: 1, profitMargin: 0.2);
                _orderProcessor.ExecuteScript(orderQuote);
            }
            else if (tick.LastPrice > lastOpenPrice &&
                ((tick.LastPrice - lastOpenPrice) / Math.Abs(lastOpenPrice) * 100) > 0.45M &&
                ((tick.LastPrice - lastOpenPrice) / Math.Abs(lastOpenPrice) * 100) < 1M &&
                ((tick.LastPrice - lastOpenPrice) / Math.Abs(lastOpenPrice) * 100) > last2Days &&
                !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.BullishAndBearisEngulfing)
                )   
            {
                var orderQuote = MapLongStrikePrice(tick, Contracts.Strategy.BullishAndBearisEngulfing, stopLoss: 1, profitMargin: 0.2);
                _orderProcessor.ExecuteScript(orderQuote);
            }
            return true;
        }

        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }
    }

    public interface IBullishAndBearisEngulfingRunTime
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
