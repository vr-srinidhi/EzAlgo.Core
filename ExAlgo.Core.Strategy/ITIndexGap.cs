using System;
using System.Collections.Generic;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;

namespace ExAlgo.Core.Strategy
{
    public class ITIndexGap: BaseStrategy, IITIndexGap
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private readonly Dictionary<string, string> InformationTechIndex;

        private readonly string NiftyFiftyIndex;
        private readonly string NiftyInformationTechIndex;


        public ITIndexGap(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            this._quoteRepository = quoteRepository;
            this._orderProcessor = orderProcessor;

            InformationTechIndex = new Dictionary<string, string>();
            
            InformationTechIndex.Add("1850625", "HCL TECHNOLOGIES");
            InformationTechIndex.Add("408065", "INFOSYS");
            InformationTechIndex.Add("2953217", "TATA CONSULTANCY SERV LT");
            InformationTechIndex.Add("3465729", "TECH MAHINDRA");
            InformationTechIndex.Add("969473", "WIPRO");

            NiftyFiftyIndex = "256265";
            NiftyInformationTechIndex = "259849";

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
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyInformationTechIndex, out informationTechnologyOpenPrice);

            if (informationTechnologyOpenPrice == 0)
                return true;


            decimal niftyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyFiftyIndex, out niftyOpenPrice);

            if (niftyOpenPrice == 0)
                return true;

            var nifty50Change = ((niftyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyFiftyIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyFiftyIndex])) * 100;
            var nseITChange = ((informationTechnologyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyInformationTechIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyInformationTechIndex])) * 100;
            var tickChange = ((tick.Open - _quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()]) / Math.Abs(_quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()])) * 100;

            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            if (tick.Open >= quotes.PivotPoint
                && quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.ITIndexGap)
                && nifty50Change > 0.5M
                && nseITChange > 0.5M
                && tickChange > 0.5M)
            {
                _orderProcessor.ExecuteScript(MapLongStrikePrice(tick, Contracts.Strategy.ITIndexGap, 0.20, 0.8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }

            if (tick.LastPrice <= quotes.PivotPoint
                && !quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.ITIndexGap)
                && nifty50Change <  -0.5M
                && nseITChange < -0.5M
                && tickChange < -0.5M)
            {
                _orderProcessor.ExecuteScript(MapShortStrikePrice(tick, Contracts.Strategy.ITIndexGap, 0.20, .8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            return false;
        }
    }

    public interface IITIndexGap
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
