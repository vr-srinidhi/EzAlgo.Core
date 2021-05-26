using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DateTimeExtensions;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Contracts;
using ExAlgo.Core.Strategy;
using Newtonsoft.Json;
using NUnit.Framework;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.BackTest
{
    public class BullishAndBearisCandle:BaseStrategy
    {
        Zerodha.ZerodhaClient zerodhaClient;
        List<StrikePrice> orderCollection;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);
            orderCollection = new List<StrikePrice>();
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
        public void BullishAndBearisCandleBackTest()
        {
            var NSE = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "256265",
                FromDate = DateTime.Now.Date.AddDays(-4),
                ToDate = DateTime.Now.Date.AddDays(1),
                Interval = "15minute",
                Continuous = false,
                Oi = false
            });

            List<StrikePrice> orderCollection = new List<StrikePrice>();


            var _nse50 = new Dictionary<string, string>();
            _nse50.Add("3861249", "ADANI PORT & SEZ");
            _nse50.Add("60417", "ASIAN PAINTS");
            _nse50.Add("1510401", "AXIS BANK");
            _nse50.Add("4267265", "BAJAJ AUTO");
            _nse50.Add("4268801", "BAJAJ FINSERV.");
            _nse50.Add("81153", "BAJAJ FINANCE");
            _nse50.Add("2714625", "BHARTI AIRTEL");
            _nse50.Add("134657", "BHARAT PETROLEUM CORP  LT");
            _nse50.Add("140033", "BRITANNIA INDUSTRIES");
            _nse50.Add("177665", "CIPLA");
            _nse50.Add("5215745", "COAL INDIA");
            _nse50.Add("2800641", "DIVI'S LABORATORIES");
            _nse50.Add("225537", "DR. REDDY'S LABORATORIES");
            _nse50.Add("232961", "EICHER MOTORS");
            _nse50.Add("1207553", "GAIL (INDIA)");

            foreach (var nse in _nse50)
            {


                var history = zerodhaClient.GetHistoricalData(new Query()
                {
                    InstrumentToken = nse.Key.ToString(),
                    FromDate = DateTime.Now.Date.AddDays(-60),
                    ToDate = DateTime.Now.Date,
                    Interval = "15minute",
                    Continuous = false,
                    Oi = false
                });


                var distinctDays = history.Select(_ => _.TimeStamp.Date).Distinct();


                foreach (var day in distinctDays)
                {
                    var historyOfDay = history.Where(_ => _.TimeStamp.Date == day);
                    foreach (var hist in historyOfDay)
                    {
                        if (hist.TimeStamp.IsAfter(new DateTimeExtensions.TimeOfDay.Time(14, 30)) || hist.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(9, 20)))
                            continue;

                        StrikePrice price = null;
                        bool isTradable = false;
                        if (((hist.High - hist.Open) / Math.Abs(hist.Open) * 100) < 0.05M &&
                            hist.Open > hist.Close)
                        {

                            var nextHist = historyOfDay.Where(_ => _.TimeStamp > hist.TimeStamp).OrderBy(_ => _.TimeStamp).FirstOrDefault();
                            var prevHist = historyOfDay.Where(_ => _.TimeStamp < hist.TimeStamp).OrderByDescending(_ => _.TimeStamp).FirstOrDefault();

                            if (prevHist.Close > prevHist.Open)
                                continue;

                            var tick = new KiteConnect.Tick
                            {
                                Open = hist.Open,
                                Close = hist.Close,
                                Volume = hist.Volume,
                                High = hist.High,
                                Low = hist.Low,
                                LastPrice = nextHist.Open,
                                InstrumentToken = Convert.ToUInt32(nse.Key)

                            };

                            price = MapShortStrikePrice(tick, Contracts.Strategy.MorningOpeningSwingTradeWithMoving, 0.15, .9);
                            price.OrderTime = hist.TimeStamp;

                            isTradable = true;
                            History target = null;
                            History stopLoss = null;

                            if (price.OrderType == OrderType.Long)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }
                            else if (price.OrderType == OrderType.Short)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }




                            if(stopLoss == null && target == null)
                            {
                                orderCollection.Add(price);
                                continue;
                            }


                            if (stopLoss == null && target != null)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }

                            else if (target == null && stopLoss != null)
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;
                            }

                            else if (target.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(stopLoss.TimeStamp.Hour, stopLoss.TimeStamp.Minute, stopLoss.TimeStamp.Second)) || target.TimeStamp == stopLoss.TimeStamp)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }
                            else
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;

                            }

                            orderCollection.Add(price);
                        }
                    }
                }
            }
            orderCollection = orderCollection.OrderByDescending(_ => _.OrderTime).ToList();

            File.WriteAllText($@"BullishAndBearisCandleB-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(orderCollection));
        }


        [Test]
        public void BullishAndBearisEngulfing()
        {
            var NSE = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "256265",
                FromDate = DateTime.Now.Date.AddDays(-4),
                ToDate = DateTime.Now.Date.AddDays(1),
                Interval = "15minute",
                Continuous = false,
                Oi = false
            });

            List<StrikePrice> orderCollection = new List<StrikePrice>();

            Dictionary<string, string> _nse50 = GetTradingInstruments();

            foreach (var nse in _nse50)
            {
                List<History> history = GetInstrumentHistory(nse,DateTime.Now,90);

                var distinctDays = history.Select(_ => _.TimeStamp.Date).Distinct();


                foreach (var day in distinctDays)
                {
                    var historyOfDay = history.Where(_ => _.TimeStamp.Date == day);
                    foreach (var hist in historyOfDay)
                    {
                        if (hist.TimeStamp.IsAfter(new DateTimeExtensions.TimeOfDay.Time(14, 30)) || hist.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(9, 31)))
                            continue;

                        StrikePrice price = null;
                        bool isTradable = false;

                        var prevHist = historyOfDay.Where(_ => _.TimeStamp < hist.TimeStamp).OrderByDescending(_ => _.TimeStamp).Take(2);


                        decimal last2Days = 0;
                        foreach (var prev in prevHist)
                        {
                            if (prev.Open > prev.Close)
                            {
                                last2Days += (prev.Open - prev.Close) / Math.Abs(prev.Close) * 100;
                            }
                            else
                            {

                                last2Days += (prev.Close - prev.Open) / Math.Abs(prev.Open) * 100;

                            }

                        }

                        if (hist.Open > hist.Close &&
                            ((hist.Open - hist.Close) / Math.Abs(hist.Close) * 100) > 0.45M &&
                            ((hist.Open - hist.Close) / Math.Abs(hist.Close) * 100) < 1M &&
                            ((hist.Open - hist.Close) / Math.Abs(hist.Close) * 100) > last2Days &&
                            !orderCollection.Any(_ => _.InstrumentToken.ToString() == nse.Key && _.OrderTime.Date == day.Date))
                        {

                            var nextHist = historyOfDay.Where(_ => _.TimeStamp > hist.TimeStamp).OrderBy(_ => _.TimeStamp).FirstOrDefault();



                            var tick = new KiteConnect.Tick
                            {
                                Open = hist.Open,
                                Close = hist.Close,
                                Volume = hist.Volume,
                                High = hist.High,
                                Low = hist.Low,
                                LastPrice = nextHist.Open,
                                InstrumentToken = Convert.ToUInt32(nse.Key)

                            };

                            price = MapShortStrikePrice(tick, Contracts.Strategy.MorningOpeningSwingTradeWithMoving, 0.2, 1.2);
                            price.OrderTime = hist.TimeStamp;

                            isTradable = true;
                            History target = null;
                            History stopLoss = null;

                            if (price.OrderType == OrderType.Long)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }
                            else if (price.OrderType == OrderType.Short)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }




                            if (stopLoss == null && target == null)
                            {
                                orderCollection.Add(price);
                                continue;
                            }


                            if (stopLoss == null && target != null)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }

                            else if (target == null && stopLoss != null)
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;
                            }

                            else if (target.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(stopLoss.TimeStamp.Hour, stopLoss.TimeStamp.Minute, stopLoss.TimeStamp.Second)) || target.TimeStamp == stopLoss.TimeStamp)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }
                            else
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;

                            }

                            orderCollection.Add(price);
                        }
                    }
                }
            }
            orderCollection = orderCollection.OrderByDescending(_ => _.OrderTime).ToList();

            var sum = orderCollection.Sum(_ => _.NetProfit);


            Console.WriteLine($"Total Profit Amount: {orderCollection.Sum(_ => _.NetProfit)}");
            Console.WriteLine($"Total Profit Count: {orderCollection.Count(_ => _.Result == OrderResult.Profit)}");
            Console.WriteLine($"Total Loss Count: {orderCollection.Count(_ => _.Result == OrderResult.Loss)}");
            File.WriteAllText($@"BullishAndBearisEngulfing-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(orderCollection));
        }



        [Test]
        public void VVampBearishBackTest()
        {
            var NSE = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "256265",
                FromDate = DateTime.Now.Date.AddDays(-4),
                ToDate = DateTime.Now.Date.AddDays(1),
                Interval = "15minute",
                Continuous = false,
                Oi = false
            });

            List<StrikePrice> orderCollection = new List<StrikePrice>();

            Dictionary<string, string> _nse50 = GetTradingInstruments();

            foreach (var nse in _nse50)
            {
                int counter = 60;
                DateTime startDayTime = DateTime.Now.AddDays(-counter);
                while (startDayTime.Date <= DateTime.Now.Date)
                {

                    if (!startDayTime.IsWorkingDay() || IsHoliday(startDayTime.Date))
                    {
                        counter--;
                        startDayTime = DateTime.Now.AddDays(-counter);
                        continue;
                    }

                    List<History> history = GetInstrumentHistory(nse, startDayTime,30);
                    var distinctDays = history.Select(_ => _.TimeStamp.Date).Distinct();

                    var quotes = new List<QuoteExtention>();
                    var pivotQuote = new List<QuoteExtention>();
                    foreach (var hist in history)
                    {
                        quotes.Add(new QuoteExtention()
                        {
                            Date = hist.TimeStamp,
                            Close = hist.Close,
                            Open = hist.Open,
                            Low = hist.Low,
                            Volume = hist.Volume,
                            High = hist.High
                        });
                    }

                    var vvmPact = Indicator.GetVwap(quotes, startDayTime.Date);
                    var pivot = Indicator.GetPivotPoints(quotes, PeriodSize.Day);
                    var macd = Indicator.GetMacd(quotes);

                    var historyOfDay = history.Where(_ => _.TimeStamp.Date == startDayTime.Date);
                    foreach (var hist in historyOfDay)
                    {

                        var prevHist = history.Where(_ => _.TimeStamp < hist.TimeStamp.Date).OrderByDescending(_ => _.TimeStamp).First();
                        var volumeIndex = vvmPact.Where(_ => _.Date == hist.TimeStamp).First();
                        var pivotPoint = pivot.Where(_ => _.Date == hist.TimeStamp).First();
                        var previousPivotPoint = pivot.Where(_ => _.Date == prevHist.TimeStamp).First();
                        var macdPoint = macd.Where(_ => _.Date == hist.TimeStamp).First();

                        if (hist.TimeStamp.IsAfter(new DateTimeExtensions.TimeOfDay.Time(12, 30)) || hist.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(9, 30)))
                            continue;


                        if(hist.Open > volumeIndex.Vwap &&
                            hist.Close < volumeIndex.Vwap &&
                            macdPoint.Macd < macdPoint.Signal &&
                            !orderCollection.Any(_ => _.InstrumentToken.ToString() == nse.Key && _.OrderTime.Date == startDayTime.Date) &&
                            pivotPoint.PP < previousPivotPoint.PP)
                        {
                            var nextHist = historyOfDay.Where(_ => _.TimeStamp > hist.TimeStamp).OrderBy(_ => _.TimeStamp).FirstOrDefault();

                            var tick = new KiteConnect.Tick
                            {
                                Open = hist.Open,
                                Close = hist.Close,
                                Volume = hist.Volume,
                                High = hist.High,
                                Low = hist.Low,
                                LastPrice = nextHist.Open,
                                InstrumentToken = Convert.ToUInt32(nse.Key)

                            };
                            StrikePrice price = null;
                            bool isTradable = false;

                            price = MapShortStrikePrice(tick, Contracts.Strategy.MorningOpeningSwingTradeWithMoving, 0.2, .8);
                            price.OrderTime = hist.TimeStamp;

                            isTradable = true;
                            History target = null;
                            History stopLoss = null;

                            if (price.OrderType == OrderType.Long)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }
                            else if (price.OrderType == OrderType.Short)
                            {
                                target = historyOfDay.Where(_ => (decimal)price.Target >= _.Low && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                                stopLoss = historyOfDay.Where(_ => (decimal)price.StopLoss <= _.High && _.TimeStamp > hist.TimeStamp)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            }




                            if (stopLoss == null && target == null)
                            {
                                orderCollection.Add(price);
                                continue;
                            }


                            if (stopLoss == null && target != null)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }

                            else if (target == null && stopLoss != null)
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;
                            }

                            else if (target.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(stopLoss.TimeStamp.Hour, stopLoss.TimeStamp.Minute, stopLoss.TimeStamp.Second)) || target.TimeStamp == stopLoss.TimeStamp)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }
                            else
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = stopLoss.TimeStamp;

                            }

                            orderCollection.Add(price);

                        }
                    }
                    counter--;
                    startDayTime = DateTime.Now.AddDays(-counter);
                    continue;
                }



            }
            orderCollection = orderCollection.OrderByDescending(_ => _.OrderTime).ToList();

            var sum = orderCollection.Sum(_ => _.NetProfit);


            Console.WriteLine($"Total Profit Amount: {orderCollection.Sum(_ => _.NetProfit)}");
            Console.WriteLine($"Total Profit Count: {orderCollection.Count(_ => _.Result == OrderResult.Profit)}");
            Console.WriteLine($"Total Loss Count: {orderCollection.Count(_ => _.Result == OrderResult.Loss)}");
            File.WriteAllText($@"VVampBearishBackTest-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(orderCollection));
        }

        private List<History> GetInstrumentHistory(KeyValuePair<string, string> nse, DateTime dateTime, int duration = 60)
        {
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 15, 30, 0);
            return zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = nse.Key.ToString(),
                FromDate = dateTime.Date.AddDays(-duration),
                ToDate = dateTime,
                Interval = "15minute",
                Continuous = false,
                Oi = false
            });
        }

        private static Dictionary<string, string> GetTradingInstruments()
        {

            return NSE.NationalStockExchange50;

            //var _nse50 = new Dictionary<string, string>();
            //_nse50.Add("3861249", "ADANI PORT & SEZ");
            //_nse50.Add("60417", "ASIAN PAINTS");
            //_nse50.Add("1510401", "AXIS BANK");
            //_nse50.Add("4267265", "BAJAJ AUTO");
            //_nse50.Add("4268801", "BAJAJ FINSERV.");
            //_nse50.Add("81153", "BAJAJ FINANCE");
            //_nse50.Add("2714625", "BHARTI AIRTEL");
            //_nse50.Add("134657", "BHARAT PETROLEUM CORP  LT");
            //_nse50.Add("140033", "BRITANNIA INDUSTRIES");
            //_nse50.Add("177665", "CIPLA");
            //_nse50.Add("5215745", "COAL INDIA");
            //_nse50.Add("2800641", "DIVI'S LABORATORIES");
            //_nse50.Add("225537", "DR. REDDY'S LABORATORIES");
            //_nse50.Add("232961", "EICHER MOTORS");
            //_nse50.Add("1207553", "GAIL (INDIA)");
            //return _nse50;
        }

        private bool IsHoliday(DateTime date)
        {
            List<DateTime> dateCollection = new List<DateTime>();
            dateCollection.Add(new DateTime(2021, 1, 26));
            dateCollection.Add(new DateTime(2021, 3, 11));
            dateCollection.Add(new DateTime(2021, 3, 29));
            dateCollection.Add(new DateTime(2021, 4, 2));
            dateCollection.Add(new DateTime(2021, 4, 14));
            dateCollection.Add(new DateTime(2021, 4, 21));


            return dateCollection.Contains(date.Date);
        }
    }
}
