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
    public class MovingAverage200And6Cross_BackTest : BaseStrategy
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

        }

        public DateTime PreviousWorkDay(DateTime date)
        {
            do
            {
                date = date.AddDays(-1);
            }
            while (IsWeekend(date) || IsHoliday(date));

            return date;
        }

        private bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                   date.DayOfWeek == DayOfWeek.Sunday;
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


        [Test]
        public void MovingAverage200And6CrossBackTest()
        {

            var NSE = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "256265",
                FromDate = DateTime.Now.Date.AddDays(-60),
                ToDate = DateTime.Now.Date.AddDays(1),
                Interval = "day",
                Continuous = false,
                Oi = false
            });

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

                int counter = 45;
                DateTime startDayTime = DateTime.Now.AddDays(-counter);



                while (startDayTime.Date <= DateTime.Now.Date)
                {

                    if (!startDayTime.IsWorkingDay() || IsHoliday(startDayTime.Date))
                    {
                        counter--;
                        startDayTime = DateTime.Now.AddDays(-counter);
                        continue;
                    }







                    var history = zerodhaClient.GetHistoricalData(new Query()
                    {
                        InstrumentToken = nse.Key,
                        FromDate = startDayTime.AddDays(-30),
                        ToDate = startDayTime,
                        Interval = "15minute",
                        Continuous = false,
                        Oi = false
                    });






                    var initialHistory = history.ToList().Where(_ => _.TimeStamp <= DateTime.Now.AddDays(-90)
                    && _.TimeStamp >= startDayTime);


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


                    var pivotCollection = Indicator.GetPivotPoints(quotes, PeriodSize.Day).ToList();
                    var pivotPoint = pivotCollection.OrderByDescending(_ => _.Date).First();
                    var previousPivotPoint = pivotCollection.Where(_ => _.Date.Date == PreviousWorkDay(startDayTime).Date).First();
                    var isUptrend = pivotPoint.PP > previousPivotPoint.PP ? true : false;


                    var NiftyToday = NSE.Where(_ => _.TimeStamp.Date == startDayTime.Date).First();
                    var NiftyYesterday = NSE.Where(_ => _.TimeStamp.Date == PreviousWorkDay(startDayTime).Date).First();

                    bool GapUp;

                    var container = new QuotesContainer
                    {
                        QuoteExtentions = quotes,
                        PivotPoint = pivotPoint.PP,
                        Resistance1 = pivotPoint.R1,
                        Resistance2 = pivotPoint.R2,
                        Support1 = pivotPoint.S1,
                        Support2 = pivotPoint.S2,
                        LastPivotPoint = previousPivotPoint.PP,
                        IsUptrendPivot = isUptrend
                    };

                    GapUp = NiftyToday.Open > NiftyYesterday.Close;
                    bool isTradable = false;

                    var morningTick = history.
                                        Where(_ => _.TimeStamp.Date == startDayTime.Date).
                                        OrderBy(_ => _.TimeStamp).First();
                    var tick = new KiteConnect.Tick
                    {
                        Open = morningTick.Open,
                        Close = morningTick.Close,
                        Volume = morningTick.Volume,
                        High = morningTick.High,
                        Low = morningTick.Low,
                        LastPrice = morningTick.Open,
                        InstrumentToken = Convert.ToUInt32(nse.Key)

                    };

                    StrikePrice price = null;
                    if (GapUp && pivotPoint.PP > previousPivotPoint.PP && tick.LastPrice > pivotPoint.PP)
                    {
                        price = MapLongStrikePrice(tick, Contracts.Strategy.MorningOpeningSwingTradeWithMoving, .5, 2);
                        isTradable = true;
                    }

                    else if (!GapUp && pivotPoint.PP < previousPivotPoint.PP && tick.LastPrice < pivotPoint.PP)
                    {
                        price = MapLongStrikePrice(tick, Contracts.Strategy.MorningOpeningSwingTradeWithMoving, .5, 2);
                        isTradable = true;
                    }

                    History target = null;
                    History stopLoss = null;

                    if (isTradable)
                    {


                        if (price.OrderType == OrderType.Long)
                        {
                            target = history.Where(_ => (decimal)price.Target <= _.High && _.TimeStamp.Date == startDayTime.Date)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            stopLoss = history.Where(_ => (decimal)price.StopLoss >= _.Low && _.TimeStamp.Date == startDayTime.Date)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                        }
                        else if (price.OrderType == OrderType.Short)
                        {
                            target = history.Where(_ => (decimal)price.Target >= _.Low && _.TimeStamp.Date == startDayTime.Date)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                            stopLoss = history.Where(_ => (decimal)price.StopLoss <= _.High && _.TimeStamp.Date == startDayTime.Date)?.OrderBy(_ => _.TimeStamp)?.FirstOrDefault();
                        }



                        if (target != null)
                        {
                            if (stopLoss == null)
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }

                            else if (target.TimeStamp.IsBefore(new DateTimeExtensions.TimeOfDay.Time(stopLoss.TimeStamp.Hour, stopLoss.TimeStamp.Minute, stopLoss.TimeStamp.Second)))
                            {
                                price.Result = OrderResult.Profit;
                                price.SellTime = target.TimeStamp;
                            }
                            else
                            {
                                price.Result = OrderResult.Loss;
                                price.SellTime = target.TimeStamp;

                            }

                            //orderCollection.Add(price);
                        }

                        orderCollection.Add(price);
                    }

                    counter--;
                    startDayTime = DateTime.Now.AddDays(-counter);
                    continue;



                }
            }


            File.WriteAllText($@"BackTestMovingAverage-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(orderCollection));

            //var initialHistory = history.Find(_ => _.TimeStamp.)

        }
    }
}
