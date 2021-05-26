using System;
using System.Collections.Generic;
using System.Linq;
using ExAlgo.Core.Contracts;
using KiteConnect;
using Newtonsoft.Json;

namespace ExAlgo.Core.Zerodha
{
    public class ZerodhaClient
    {
        
        private Kite kiteClient;


        public ZerodhaClient(IAuthetication authetication)
        {
 
            kiteClient = authetication.Authorize();
        }

        public List<History> GetHistoricalData(Query query)
        {
            var reponse = kiteClient.GetHistoricalData(query.InstrumentToken, query.FromDate, query.ToDate, query.Interval, query.Continuous, query.Oi);
            var mapresponse = new List<History>();
            foreach (var hist in reponse)
            {
                mapresponse.Add(new History()
                {
                    TimeStamp = hist.TimeStamp,
                    Close = hist.Close,
                    Open = hist.Open,
                    Low = hist.Low,
                    Volume = hist.Volume,
                    High = hist.High
                });
            }
            return mapresponse;
        }


        public ZerodaOrderResponse PlaceOrder(StrikePrice strikePrice)
        {
            if (strikePrice.OrderVariety == Variety.CoverOrder)
                return PlaceCoverOrder(strikePrice);
            else
                return PlaceRegularOrder(strikePrice);

        }



        public ZerodaOrderResponse PlaceRegularOrder(StrikePrice strikePrice)
        {
            var orderType = strikePrice.ZerodaTradeOrderType == TradeOrderType.Limit ? "LIMIT" : "MARKET";
            var response = kiteClient.PlaceOrder("NSE",
                strikePrice.Instrument,
                strikePrice.OrderType == OrderType.Long ? "BUY" : "SELL",
                1,
                Price: (decimal)strikePrice.BuyPrice,
                Product: "MIS",
                OrderType: orderType,
                Validity: "DAY",
                DisclosedQuantity: 0,
                Variety: "regular"
                );

            var jsonresponse = JsonConvert.DeserializeObject<ZerodaOrderResponse>(JsonConvert.SerializeObject(response));
            return jsonresponse;
        }


        public ZerodaOrderResponse PlaceCoverOrder(StrikePrice strikePrice)
        {
            var orderType = strikePrice.ZerodaTradeOrderType == TradeOrderType.Limit ? "LIMIT" : "MARKET";
            var response = kiteClient.PlaceOrder("NSE",
                strikePrice.Instrument,
                strikePrice.OrderType == OrderType.Long ? "BUY" : "SELL",
                1,
                Price: (decimal)strikePrice.BuyPrice,
                Product: "MIS",
                OrderType: orderType,
                Validity: "DAY",
                DisclosedQuantity: 0,
                Variety: "co",
                TriggerPrice: (decimal)strikePrice.StopLoss
                );

            var jsonresponse = JsonConvert.DeserializeObject<ZerodaOrderResponse>(JsonConvert.SerializeObject(response));
            return jsonresponse;
        }


        public ZerodaOrderResponse UpdateOrderForStopLoss(StrikePrice strikePrice)
        {

            //var response = kiteClient.ModifyOrder(strikePrice.TrailingOrder.OrderId,
            //    Exchange: strikePrice.TrailingOrder.Exchange,
            //    TradingSymbol: strikePrice.TrailingOrder.Tradingsymbol,
            //    TransactionType: strikePrice.TrailingOrder.TransactionType, Quantity: strikePrice.TrailingOrder.Quantity.ToString(),
            //    Price: (decimal)strikePrice.StopLoss,
            //    Product: strikePrice.TrailingOrder.Product,
            //    OrderType: strikePrice.TrailingOrder.OrderType,
            //    Validity: strikePrice.TrailingOrder.Validity,
            //    Variety: strikePrice.TrailingOrder.Variety);


            //var jsonresponse = JsonConvert.DeserializeObject<ZerodaOrderResponse>(JsonConvert.SerializeObject(response));
            //return jsonresponse;
            return null;
        }


        public bool ExitOrderOnTarget(StrikePrice strikePrice)
        {
            try
            {
                var response = kiteClient.CancelOrder(OrderId: strikePrice.TrailingORderId,
                         Variety: "co");
            }
            catch
            {
                var response = kiteClient.CancelOrder(OrderId: strikePrice.OrderId,
                    ParentOrderId: strikePrice.TrailingORderId,
                    Variety: "co");
            }
            return true;
        }

        public List<Instrument> GetInstruments()
        {
            return kiteClient.GetInstruments("NSE");
        }

        public List<Order> GetOrders()
        {
            return kiteClient.GetOrders();
        }


        public bool HealthCheck()
        {
            try { return kiteClient.GetInstruments("NSE").Any(); }
            catch(Exception e) { return false; }

        }
    }
}
