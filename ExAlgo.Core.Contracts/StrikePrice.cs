using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ExAlgo.Core.Contracts
{
    public class StrikePrice
    {
        public StrikePrice()
        {
            RevicedStopLoss = new Dictionary<DateTime, double>();
        }
        public string Instrument { get; set; }
        public double BuyPrice { get; set; }
        public double Target { get; set; }
        public double StopLoss { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime SellTime { get; set; }
        public DateTime ExchangeTime { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderResult Result { get; set; }

        public double GrossProfit
        {
            get
            {
                if (Result == OrderResult.NA)
                    return 0;
                if (Result == OrderResult.Profit)
                {
                    if (OrderType == OrderType.Long)
                        return this.Target * this.Qty - this.BuyPrice * this.Qty;
                    return this.BuyPrice * this.Qty - this.Target * this.Qty;
                }
                if (OrderType == OrderType.Long)
                    return this.StopLoss * this.Qty - this.BuyPrice * this.Qty;
                return this.BuyPrice * this.Qty - this.StopLoss * this.Qty;
            }
        }

        public int InstrumentToken { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType OrderType { get; set; }

        public int Qty { get; set; }

        public decimal? ConnorsRsi { get; set; }
        public decimal Ema200 { get; set; }

        public double BrokerageAndTxn = 250;

        public double NetProfit
        {
            get
            {
                return Result == OrderResult.Profit
                    ? GrossProfit - BrokerageAndTxn
                    : GrossProfit + -(BrokerageAndTxn);
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public Strategy OrderStrategy { get; set; }


        public Dictionary<DateTime, double> RevicedStopLoss { get; set; }

        public override string ToString()
        {
            var sellPrice = Result == OrderResult.Profit ? Target : StopLoss;
            return $"{OrderTime} : Transaction for {Instrument} with Buy Price {BuyPrice} sold at {sellPrice}, Result {Result.ToString()} Sell Time {SellTime}";
        }

        public DateTime PullDownTime { get; set; }


        public string OrderId { get; set; }
        public string TrailingORderId { get; set; }

        public TradeOrderType ZerodaTradeOrderType { get; set; }

        public Variety OrderVariety { get; set; }

        

    }

    public enum OrderResult
    {
        NA,
        Profit,
        Loss
    }

    public enum OrderType
    {
        Long,
        Short
    }

    public enum Strategy
    {
        Ema200CrossOver,
        ConnorsRsiAndEma200,
        MovingAverage6And200,
        MorningOpeningSwingTradeWithMoving,
        ConnorsRsiAndEma2001PercentLoss,
        ConnorsRsiAndEma200Point2PercentProfit,
        BankIndexGap,
        ITIndexGap,
        IPharmaIndexGap,
        IMetalIndexGap,
        BullishAndBearisEngulfing

    }

    public enum TradeOrderType
    {
        SLM,
        Market,
        Limit
    }

    public enum Variety
    {
        CoverOrder,
        Regular
    }
}

