using System;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;

namespace ExAlgo.Core.Strategy
{
    public class MovingAverage200And6Cross : BaseStrategy, IMovingAverage200And6Cross
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;

        public MovingAverage200And6Cross(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            this._quoteRepository = quoteRepository;
            this._orderProcessor = orderProcessor;
        }

        public bool IsTradeTimeOpen()
        {
            TimeSpan startTimeSpan = new TimeSpan(9, 15, 00);
            TimeSpan endTimeSpan = new TimeSpan(9, 15, 30);
            TimeSpan currentTimeSpan = DateTime.Now.TimeOfDay;
            return currentTimeSpan > startTimeSpan && currentTimeSpan < endTimeSpan;
        }

        public bool ProcessQuote(Tick tick)
        {
            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            //if (tick.Open >= quotes.PivotPoint
            //    && quotes.IsUptrendPivot
            //    && quotes.IsActiveStock
            //    && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.MovingAverage6And200))
            //{
            //    _orderProcessor.ExecuteScript(MapLongStrikePrice(tick, Contracts.Strategy.MovingAverage6And200, 0.3, 1.5));
            //    return true;
            //}
            if (tick.LastPrice <= quotes.PivotPoint
                && !quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.MovingAverage6And200))
            {
                _orderProcessor.ExecuteScript(MapShortStrikePrice(tick, Contracts.Strategy.MovingAverage6And200, 0.3, 1.5));
                return true;
            }
            return false;
        }
    }

    public interface IMovingAverage200And6Cross
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
