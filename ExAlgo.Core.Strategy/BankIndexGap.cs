using System;
using System.Collections.Generic;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using KiteConnect;


namespace ExAlgo.Core.Strategy
{
    public class BankIndexGap: BaseStrategy, IBankIndexGap
    {
        private readonly IQuoteRepository _quoteRepository;
        private readonly IOrderProcessor _orderProcessor;
        private readonly Dictionary<string, string> BankIndex;

        private readonly string NiftyFiftyIndex;
        private readonly string NiftyBankIndex;



        public BankIndexGap(IQuoteRepository quoteRepository, IOrderProcessor orderProcessor)
        {
            this._quoteRepository = quoteRepository;
            this._orderProcessor = orderProcessor;

            BankIndex = new Dictionary<string, string>();
            BankIndex.Add("1510401", "AXIS BANK");
            BankIndex.Add("341249", "HDFC BANK");
            BankIndex.Add("1270529", "ICICI BANK.");
            BankIndex.Add("1346049", "INDUSIND BANK");
            BankIndex.Add("779521", "STATE BANK OF INDIA");
            BankIndex.Add("492033", "KOTAK MAHINDRA BANK");

            NiftyFiftyIndex = "256265";
            NiftyBankIndex = "260105";
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

            if (!BankIndex.ContainsKey(tick.InstrumentToken.ToString()))
                return true;


            decimal informationTechnologyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyBankIndex, out informationTechnologyOpenPrice);

            if (informationTechnologyOpenPrice == 0)
                return true;


            decimal niftyOpenPrice;
            _quoteRepository.IndexOpenPrice.TryGetValue(NiftyFiftyIndex, out niftyOpenPrice);

            if (niftyOpenPrice == 0)
                return true;

            var nifty50Change = ((niftyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyFiftyIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyFiftyIndex])) * 100;
            var nseITChange = ((informationTechnologyOpenPrice - _quoteRepository.LastDayClosePrice[NiftyBankIndex]) / Math.Abs(_quoteRepository.LastDayClosePrice[NiftyBankIndex])) * 100;
            var tickChange = ((tick.Open - _quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()]) / Math.Abs(_quoteRepository.LastDayClosePrice[tick.InstrumentToken.ToString()])) * 100;


            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            if (tick.Open >= quotes.PivotPoint
                && quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.BankIndexGap)
                && nifty50Change > 0.5M
                && nseITChange > 0.5M
                && tickChange > 0.5M)
            {
                _orderProcessor.ExecuteScript(MapLongStrikePrice(tick, Contracts.Strategy.BankIndexGap, 0.2, 0.8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            if (tick.LastPrice <= quotes.PivotPoint
                && !quotes.IsUptrendPivot
                && quotes.IsActiveStock
                && !_orderProcessor.IsSimilarTradeAlreadyDoneForTheDay(tick, Contracts.Strategy.BankIndexGap)
                && nifty50Change < -0.5M
                && nseITChange < -0.5M
                && tickChange < -0.5M)
            {
                _orderProcessor.ExecuteScript(MapShortStrikePrice(tick, Contracts.Strategy.BankIndexGap, 0.2, .8, tradeOrderType: Contracts.TradeOrderType.Market));
                return true;
            }
            return false;
        }
    }


    public interface IBankIndexGap
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
