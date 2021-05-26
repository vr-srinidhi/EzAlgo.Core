using System;
using ExAlgo.Core.Contracts;
using KiteConnect;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.Strategy
{
    public class BaseStrategy
    {
        public StrikePrice MapLongStrikePrice(Tick tick,
            Contracts.Strategy strategy,
            double profitMargin = 0.15,
            double stopLoss = 0.3,
            ConnorsRsiResult connorsRsiResult = null,
            decimal ema = 0,
            Contracts.TradeOrderType tradeOrderType = TradeOrderType.Limit
            
            )
        {
            var buyPrice = (double)tick.LastPrice;
            //Math.DivRem(500000, (int)buyPrice, out var quantity);
            var quantity = Convert.ToInt32(Math.Floor(500000 / buyPrice));

            var target = buyPrice + buyPrice * (profitMargin) / 100;
            var stpLoss = buyPrice - buyPrice * (stopLoss) / 100;

            return new StrikePrice
            {
                InstrumentToken = (int)tick.InstrumentToken,
                OrderType = OrderType.Long,
                BuyPrice = buyPrice,
                Target = (float)Math.Round((decimal)target / (decimal).05, MidpointRounding.AwayFromZero) * .05,
                StopLoss = (float)Math.Round((decimal)stpLoss / (decimal).05, MidpointRounding.AwayFromZero) * .05,
                OrderStrategy = strategy,
                Instrument = NSE.NationalStockExchange50[tick.InstrumentToken.ToString()],
                OrderTime = DateTime.Now,
                Qty = quantity,
                ConnorsRsi = connorsRsiResult?.ConnorsRsi ?? 0,
                Ema200 = ema,
                PullDownTime = TimeRoundDown(DateTime.Now),
                ZerodaTradeOrderType = tradeOrderType,
                OrderVariety = Variety.CoverOrder,
                ExchangeTime = tick.Timestamp.Value
            };
        }

        public StrikePrice MapShortStrikePrice(Tick tick,
            Contracts.Strategy strategy,
            double profitMargin = 0.15,
            double stopLoss = 0.3,
            ConnorsRsiResult connorsRsiResult = null,
            decimal ema = 0,
            Contracts.TradeOrderType tradeOrderType = TradeOrderType.Limit)
        {
            var buyPrice = (double)tick.LastPrice;
            var quantity = Convert.ToInt32(Math.Floor(500000 / buyPrice));

            var target = buyPrice - buyPrice * (profitMargin) / 100;
            var stpLoss = buyPrice + buyPrice * (stopLoss) / 100;

            return new StrikePrice
            {
                InstrumentToken = (int)tick.InstrumentToken,
                OrderType = OrderType.Short,
                BuyPrice = buyPrice,
                Target = (float)Math.Round((decimal)target / (decimal).05, MidpointRounding.AwayFromZero) * .05,
                StopLoss = (float)Math.Round((decimal)stpLoss / (decimal).05, MidpointRounding.AwayFromZero) * .05,
                OrderStrategy = strategy,
                Instrument = NSE.NationalStockExchange50[tick.InstrumentToken.ToString()],
                OrderTime = DateTime.Now,
                Qty = quantity,
                ConnorsRsi = connorsRsiResult?.ConnorsRsi ?? 0,
                Ema200 = ema,
                PullDownTime = TimeRoundDown(DateTime.Now),
                ZerodaTradeOrderType = tradeOrderType,
                OrderVariety = Variety.CoverOrder,
                ExchangeTime = tick.Timestamp.Value
            };
        }


        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }
    }
}
