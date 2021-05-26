using System;
using System.Collections.Generic;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;


namespace ExAlgo.Core.Strategy
{
    public class PharmaIndexGap:BaseStrategy, IPharmaIndexGap
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private readonly Dictionary<string, string> InformationTechIndex;

        private readonly string NiftyFiftyIndex;
        private readonly string PharmaIndex;


        public PharmaIndexGap(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            this._quoteRepository = quoteRepository;
            this._orderProcessor = orderProcessor;

            InformationTechIndex = new Dictionary<string, string>();

            InformationTechIndex.Add("177665", "CIPLA");
            InformationTechIndex.Add("225537", "DRREDDY");
            InformationTechIndex.Add("2800641", "DIVISLAB");
            InformationTechIndex.Add("857857", "SUNPHARMA");

            NiftyFiftyIndex = "256265";
            PharmaIndex = "262409";
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
            _quoteRepository.IndexOpenPrice.TryGetValue(PharmaIndex, out informationTechnologyOpenPrice);

            if (informationTechnologyOpenPrice == 0)
                return true;


            decimal niftyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyFiftyIndex, out niftyOpenPrice);

            if (niftyOpenPrice == 0)
                return true;

            var nifty50Change = ((niftyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyFiftyIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyFiftyIndex])) * 100;
            var nseITChange = ((informationTechnologyOpenPrice - _quoteRepository.LastDayClosePrice[PharmaIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[PharmaIndex])) * 100;
            var tickChange = ((tick.Open - _quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()]) / Math.Abs(_quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()])) * 100;


            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            if (tick.Open >= quotes.PivotPoint
                && quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.IPharmaIndexGap)
                && nifty50Change > 0.5M
                && nseITChange > 0.5M
                && tickChange > 0.5M)
            {
                _orderProcessor.ExecuteScript(MapLongStrikePrice(tick, Contracts.Strategy.IPharmaIndexGap, 0.2, 0.8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            if (tick.LastPrice <= quotes.PivotPoint
                && !quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.IPharmaIndexGap)
                && nifty50Change < -0.5M
                && nseITChange < -0.5M
                && tickChange < -0.5M)
            {
                _orderProcessor.ExecuteScript(MapShortStrikePrice(tick, Contracts.Strategy.IPharmaIndexGap, 0.2, 0.8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            return false;
        }
    }

    public interface IPharmaIndexGap
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
