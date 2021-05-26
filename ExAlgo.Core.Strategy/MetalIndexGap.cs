using System;
using System.Collections.Generic;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;


namespace ExAlgo.Core.Strategy
{
    public class MetalIndexGap : BaseStrategy, IMetalIndexGap
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private readonly Dictionary<string, string> InformationTechIndex;


        private readonly string NiftyFiftyIndex;
        private readonly string MetalIndex;


        public MetalIndexGap(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            this._quoteRepository = quoteRepository;
            this._orderProcessor = orderProcessor;

            InformationTechIndex = new Dictionary<string, string>();

            InformationTechIndex.Add("348929", "HINDALCO");
            InformationTechIndex.Add("3001089", "JSWSTEEL");
            InformationTechIndex.Add("895745", "TATASTEEL");
            InformationTechIndex.Add("5215745", "COALINDIA");

            NiftyFiftyIndex = "256265";
            MetalIndex = "263689";

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

            if (!InformationTechIndex.ContainsKey(tick.InstrumentToken.ToString()))
                return true;


            decimal informationTechnologyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(MetalIndex, out informationTechnologyOpenPrice);

            if (informationTechnologyOpenPrice == 0)
                return true;


            decimal niftyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyFiftyIndex, out niftyOpenPrice);

            if (niftyOpenPrice == 0)
                return true;

            var nifty50Change = ((niftyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyFiftyIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyFiftyIndex])) * 100;
            var nseITChange = ((informationTechnologyOpenPrice - _quoteRepository.LastDayClosePrice[MetalIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[MetalIndex])) * 100;
            var tickChange = ((tick.Open - _quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()]) / Math.Abs(_quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()])) * 100;


            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            if (tick.Open >= quotes.PivotPoint
                && quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.IMetalIndexGap)
                && nifty50Change > 0.5M
                && nseITChange > 0.5M
                && tickChange > 0.5M)
            {
                _orderProcessor.ExecuteScript(MapLongStrikePrice(tick, Contracts.Strategy.IMetalIndexGap, 0.2, 0.8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            if (tick.LastPrice <= quotes.PivotPoint
                && !quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.IMetalIndexGap)
                && nifty50Change < -0.5M
                && nseITChange < -0.5M
                && tickChange < -0.5M)
            {
                _orderProcessor.ExecuteScript(MapShortStrikePrice(tick, Contracts.Strategy.IMetalIndexGap, 0.2, .8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            return false;
        }
    }

    public interface IMetalIndexGap
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
