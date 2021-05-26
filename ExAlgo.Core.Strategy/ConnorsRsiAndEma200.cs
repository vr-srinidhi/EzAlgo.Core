using System;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Contracts;
using ExAlgo.Core.Order;
using KiteConnect;
using Newtonsoft.Json;
using NLog;

namespace ExAlgo.Core.Strategy
{
    public class ConnorsRsiAndEma200 : BaseStrategy, IConnorsRsiAndEma200
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ConnorsRsiAndEma200(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            _quoteRepository = quoteRepository;
            _orderProcessor = orderProcessor;
        }


        public bool IsTradeTimeOpen()
        {
            TimeSpan startTimeSpan = new TimeSpan(9, 30, 10);
            TimeSpan endTimeSpan = new TimeSpan(14, 30, 0);
            TimeSpan currentTimeSpan = DateTime.Now.TimeOfDay;
            return currentTimeSpan > startTimeSpan && currentTimeSpan < endTimeSpan;
        }

        public bool ProcessQuote(Tick tick)
        {
            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            var response = quotes.GetConnorsRsiResult(tick);
            if (response.IsTradable)
            {

                if (_orderProcessor.IsRunDownTimeTradeFoeTickExist(tick, Contracts.Strategy.ConnorsRsiAndEma200))
                {
                    //Logger.Info($"ConnorsRsiAndEma200 Order for {NSE50.NationalStockExchange50[tick.InstrumentToken.ToString()]} Already exist in timeframe, Object : {JsonConvert.SerializeObject(response)}");
                    return true;
                }

                //if (_orderProcessor.IsRunDownTimeTradeFoeTickExist(tick, Contracts.Strategy.ConnorsRsiAndEma2001PercentLoss))
                //{
                //    Logger.Info($"ConnorsRsiAndEma2001PercentLoss Order for {NSE.NationalStockExchange50[tick.InstrumentToken.ToString()]} Already exist in timeframe, Object : {JsonConvert.SerializeObject(response)}");
                //    return true;
                //}

                //if (_orderProcessor.IsRunDownTimeTradeFoeTickExist(tick, Contracts.Strategy.ConnorsRsiAndEma200Point2PercentProfit))
                //{
                //    Logger.Info($"ConnorsRsiAndEma200Point2PercentProfit Order for {NSE.NationalStockExchange50[tick.InstrumentToken.ToString()]} Already exist in timeframe, Object : {JsonConvert.SerializeObject(response)}");
                //    return true;
                //}

                var strikePrice = response.Order == ConnorsRsiStrategyOutput.OrderType.Long ?
                    MapLongStrikePrice(tick, Contracts.Strategy.ConnorsRsiAndEma200, stopLoss: 1, connorsRsiResult: response.ConnorsRsi, ema: response.Ema) :
                    MapShortStrikePrice(tick, Contracts.Strategy.ConnorsRsiAndEma200, stopLoss: 1, connorsRsiResult: response.ConnorsRsi, ema: response.Ema);
                _orderProcessor.ExecuteScript(strikePrice);

                //var strikeConnorsRsiAndEma200Point2PercentProfit = response.Order == ConnorsRsiStrategyOutput.OrderType.Long ?
                //    MapLongStrikePrice(tick, Contracts.Strategy.ConnorsRsiAndEma200Point2PercentProfit, stopLoss: 1, connorsRsiResult: response.ConnorsRsi, ema: response.Ema, profitMargin: .2) :
                //    MapShortStrikePrice(tick, Contracts.Strategy.ConnorsRsiAndEma200Point2PercentProfit, stopLoss: 1, connorsRsiResult: response.ConnorsRsi, ema: response.Ema, profitMargin: .2);
                //_orderProcessor.ExecuteScript(strikeConnorsRsiAndEma200Point2PercentProfit);
            }
            return true;
        }

    }


    public interface IConnorsRsiAndEma200
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
