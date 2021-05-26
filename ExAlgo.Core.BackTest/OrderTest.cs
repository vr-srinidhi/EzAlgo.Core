using System;
using System.Collections.Generic;
using System.Linq;
using ExAlgo.Core.Strategy;
using KiteConnect;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ExAlgo.Core.BackTest
{
    public class OrderTest : BaseStrategy
    {

        Zerodha.ZerodhaClient zerodhaClient;
        Kite KiteClient;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);
            KiteClient = authentication.Authorize();
            //orderCollection = new List<StrikePrice>();
            //var instruments = zerodhaClient.GetInstruments();
            //String str = "";
            //foreach(var nse in NSE50.NationalStockExchange50)
            //{
            //    var inst = instruments.First(_ => _.InstrumentToken.ToString() == nse.Key);
            //    {
            //        str = str + $"_nse50.Add(\"{inst.InstrumentToken}\",\"{inst.TradingSymbol}\"); \\n ";
            //    }
            //}
            //File.WriteAllText($@"Instruments-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(instruments));
        }

        [Test]
        public void OrderProcessorTest()
        {
            var tick = new KiteConnect.Tick
            {
                Open = 5174,
                Close = 5174,
                Volume = 5174,
                High = 5174,
                Low = 5174,
                LastPrice = 5174,
                InstrumentToken = Convert.ToUInt32("225537")
                

            };

            var longTrade = MapLongStrikePrice(tick, Contracts.Strategy.BankIndexGap, stopLoss: 1);
            var response = zerodhaClient.PlaceOrder(longTrade);
            var str = JsonConvert.SerializeObject(response);
            //var responseStatus = response["status"];

            var jsonresponse = JsonConvert.DeserializeObject<Root>(JsonConvert.SerializeObject(response));
            var orderId = jsonresponse.data.order_id;
        }


        [Test]
        public void CoverOrderTest()
        {
            var tick = new KiteConnect.Tick
            {
                Open = 5000,
                Close = 5000,
                Volume = 5000,
                High = 5000,
                Low = 5000,
                LastPrice = 5000,
                InstrumentToken = Convert.ToUInt32("225537")
            };

            var strikePrice = MapLongStrikePrice(tick, Contracts.Strategy.BankIndexGap, stopLoss: 1);

            var orderType = "LIMIT";

            var response = KiteClient.PlaceOrder("NSE",
                strikePrice.Instrument,
                "SELL",
                1,
                Price: (decimal)strikePrice.BuyPrice,
                Product: "MIS",
                OrderType: orderType,
                Validity: "DAY",
                DisclosedQuantity: 0,
                Variety: "co",
                TriggerPrice:(decimal)strikePrice.Target
                );

            var jsonresponse = JsonConvert.DeserializeObject<Root>(JsonConvert.SerializeObject(response));
        }



        [Test]
        public void GetOrderTest()
        {
            
            var orderType = "LIMIT";

            var response = KiteClient.GetOrderHistory("210517000705926");
            var orders = KiteClient.GetOrders();
            //KiteClient.ModifyOrder()
            var jsonresponse = JsonConvert.DeserializeObject<Root>(JsonConvert.SerializeObject(response));
        }






    }

    public class Data
    {
        public string order_id { get; set; }
    }

    public class Root
    {
        public string status { get; set; }
        public Data data { get; set; }
    }
}
