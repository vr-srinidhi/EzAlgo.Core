using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExAlgo.Core.Contracts;
using ExAlgo.Core.Zerodha;
using KiteConnect;
using Newtonsoft.Json;
using NLog;

namespace ExAlgo.Core.Order
{
    public class OrderProcessor : IOrderProcessor
    {
        private readonly Ticker _ticker;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly ZerodhaClient zerodhaClient;

        List<StrikePrice> orderLedger { get; set; }
        List<StrikePrice> Orderconfirmation { get; set; }
        Dictionary<string, StrikePrice> TerminalLedger;

        public OrderProcessor(ZerodhaClient zerodhaClient)
        {
            orderLedger = new List<StrikePrice>();
            Orderconfirmation = new List<StrikePrice>();
            this.zerodhaClient = zerodhaClient;
            TerminalLedger = new Dictionary<string, StrikePrice>();
        }

        public bool ExecuteScript(StrikePrice strikePrice)
        {

            if (!orderLedger.Any(_ => _.InstrumentToken == strikePrice.InstrumentToken &&
            _.OrderType == strikePrice.OrderType &&
            _.OrderStrategy == strikePrice.OrderStrategy))
            {
                orderLedger.Add(strikePrice);
                Logger.Info($"Recieved order for {JsonConvert.SerializeObject(strikePrice)}");


                //if (TerminalLedger.Count() <= 5 && strikePrice.OrderStrategy == Strategy.ConnorsRsiAndEma200)
                //{
                //    var respose = zerodhaClient.PlaceOrder(strikePrice);
                //    strikePrice.OrderId = respose.data.order_id;
                //    TerminalLedger.Add(respose.data.order_id, strikePrice);
                //    Logger.Info($"Order Placed in Terminal, Order ID :  {respose.data.order_id}");
                //}

                return true;
            }


            return false;
        }




        public void TickRecieved(Tick tickData)
        {
                if (orderLedger.Any(_ => _.InstrumentToken == tickData.InstrumentToken))
            {
                var strikePrices = orderLedger.Where(_ => _.InstrumentToken == tickData.InstrumentToken);
                var tempstrikePrice = new List<StrikePrice>();
                foreach (var str in strikePrices)
                {
                    var shouldCloseORder = false;   
                    var strikePrice = str;
                    if (strikePrice.OrderType == OrderType.Long)
                    {
                        if (tickData.LastPrice >= (decimal)strikePrice.Target)
                        {
                            if (!string.IsNullOrEmpty(strikePrice.TrailingORderId))
                            {
                                zerodhaClient.ExitOrderOnTarget(strikePrice);
                            }
                            strikePrice.Result = OrderResult.Profit;
                            strikePrice.SellTime = DateTime.Now;
                            shouldCloseORder = true;
                        }
                        else if (tickData.LastPrice <= (decimal)strikePrice.StopLoss)
                        {

                            strikePrice.Result = OrderResult.Loss;
                            strikePrice.SellTime = DateTime.Now;
                            shouldCloseORder = true;
                        }
                    }

                    if (strikePrice.OrderType == OrderType.Short)
                    {
                        if (tickData.LastPrice <= (decimal)strikePrice.Target)
                        {
                            if (!string.IsNullOrEmpty(strikePrice.TrailingORderId))
                            {
                                zerodhaClient.ExitOrderOnTarget(strikePrice);
                            }
                            strikePrice.Result = OrderResult.Profit;
                            strikePrice.SellTime = DateTime.Now;
                            shouldCloseORder = true;
                        }
                        else if (tickData.LastPrice >= (decimal)strikePrice.StopLoss)
                        {

                            strikePrice.Result = OrderResult.Loss;
                            strikePrice.SellTime = DateTime.Now;
                            shouldCloseORder = true;
                        }
                    }

                    if (shouldCloseORder)
                    {
                        Logger.Info($"Order executed for {strikePrice.Instrument} - {JsonConvert.SerializeObject(strikePrice)}");
                        tempstrikePrice.Add(strikePrice);
                        Orderconfirmation.Add(strikePrice);
                    }

                }

                foreach (var temp in tempstrikePrice)
                {
                    orderLedger.Remove(temp);
                }
            }

        }




        public void WriteConfirmation()
        {
            Logger.Info(JsonConvert.SerializeObject(Orderconfirmation));
            File.WriteAllText($@"Orderconfirmation-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(Orderconfirmation));
            File.WriteAllText($@"OrderLedger-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(orderLedger));
            Orderconfirmation.Clear();
        }

        public bool IsSimilarTradeAlreadyDoneForTheDay(Tick tick, Strategy strategy)
        {
            return Orderconfirmation.Any(_ => _.InstrumentToken == tick.InstrumentToken && _.OrderStrategy == strategy);
        }


        public bool IsRunDownTimeTradeFoeTickExist(Tick tick, Strategy strategy)
        {
            var timeRoundDown = TimeRoundDown(DateTime.Now);
            return Orderconfirmation.Any(_ => _.InstrumentToken == tick.InstrumentToken
                                              && _.OrderStrategy == strategy
                                              && DateTime.Compare(_.PullDownTime, timeRoundDown) == 0);
        }


        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }


        public void OrderUpdateRecieved(KiteConnect.Order order)
        {
            if(TerminalLedger.ContainsKey(order.OrderId))
            {
                var strikePrice = TerminalLedger[order.OrderId];
                StrikePrice price = new StrikePrice()
                {
                    BuyPrice = strikePrice.Target,
                    Instrument = strikePrice.Instrument,
                    InstrumentToken = strikePrice.InstrumentToken,
                    OrderType = strikePrice.OrderType == OrderType.Long ? OrderType.Short : OrderType.Long,
                    OrderStrategy = strikePrice.OrderStrategy,
                    OrderTime = DateTime.Now,
                    Qty = strikePrice.Qty,
                    PullDownTime = TimeRoundDown(DateTime.Now),
                    ZerodaTradeOrderType = TradeOrderType.Limit,
                    OrderVariety = Variety.Regular
                };
                var respose = zerodhaClient.PlaceOrder(price);
                strikePrice.OrderId = respose.data.order_id;
                Logger.Info($"Sale Order Placed in Terminal, Order ID :  {order.OrderId}, Sale OrderId {respose.data.order_id}");

            }
        }


        public void UpdateTrigerOrderId(KiteConnect.Order order)
        {
            if(TerminalLedger.TryGetValue(order.ParentOrderId, out var strikePrice))
            {
                strikePrice.TrailingORderId = order.OrderId;
                //strikePrice.TrailingOrder = order;
            }
        }

    }

    public interface IOrderProcessor
    {
        bool ExecuteScript(StrikePrice strikePrice);
        void TickRecieved(Tick tickData);
        void WriteConfirmation();
        bool IsSimilarTradeAlreadyDoneForTheDay(Tick tick, Strategy strategy);
        bool IsRunDownTimeTradeFoeTickExist(Tick tick, Strategy strategy);
        void OrderUpdateRecieved(KiteConnect.Order order);
        //bool PlaceOrder(StrikePrice strikePrice);
        void UpdateTrigerOrderId(KiteConnect.Order order);
    }

}



