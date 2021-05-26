using System;
using System.Collections.Generic;
using System.Linq;
using DateTimeExtensions;
using ExAlgo.Core.Contracts;
using NUnit.Framework;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.BackTest
{
    public class BullandBearEngulfing
    {
        Zerodha.ZerodhaClient zerodhaClient;
        List<StrikePrice> orderCollection;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);
            orderCollection = new List<StrikePrice>();
            
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
        public void BullandBearEngulfingBackTest()
        {
            var NSE = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "256265",
                FromDate = DateTime.Now.Date.AddDays(-60),
                ToDate = DateTime.Now.Date.AddDays(1),
                Interval = "15minute",
                Continuous = false,
                Oi = false
            });


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
                    InstrumentToken = "3861249",
                    FromDate = startDayTime.AddDays(-30),
                    ToDate = startDayTime,
                    Interval = "15minute",
                    Continuous = false,
                    Oi = false
                });


                var quotes = new List<QuoteExtention>();
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

                var NiftyToday = NSE.Where(_ => _.TimeStamp.Date == startDayTime.Date).First();
                var NiftyYesterday = NSE.Where(_ => _.TimeStamp.Date == PreviousWorkDay(startDayTime).Date).First();
                var ema200 = Indicator.GetEma(quotes, 200);
                var currentDayData = quotes.Where(_ => _.Date.Date == startDayTime.Date);
                var bearishTimeSegment = currentDayData.Where(_ => (((_.Close - _.Open) / Math.Abs(_.Open)) * 100) < 0).ToArray();

                int CounterIndex = 0;
                DateTime starttime = DateTime.Now;

                //List<QuoteExtention> ext

                while (CounterIndex < bearishTimeSegment.Count())
                {
                   var isTrue =  bearishTimeSegment[CounterIndex + 1].Date = TimeRoundUp(bearishTimeSegment[CounterIndex].Date);

                }

            }
        }


        private static DateTime TimeRoundUp(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(input.Minute % 15);
        }
    }
}
