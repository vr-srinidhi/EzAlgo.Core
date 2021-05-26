using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExAlgo.Core.Contracts;
using KiteConnect;
using Newtonsoft.Json;
using NUnit.Framework;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.BackTest
{
    public class Tests
    {

        Zerodha.ZerodhaClient zerodhaClient;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);

            var instruments = zerodhaClient.GetInstruments();
            File.WriteAllText($@"Instruments-{DateTime.Now:dddd-dd-MMMM-yyyy-HH-mm-ss}.json", JsonConvert.SerializeObject(instruments));
        }

        [Test]
        public void Test1()
        {
            var history = zerodhaClient.GetHistoricalData(new Query()
            {
                InstrumentToken = "3861249",
                FromDate = DateTime.Now.AddDays(-30),
                ToDate = DateTime.Now,
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


            var latestQuote = quotes.OrderByDescending(_ => _.Date).First();
            var pivotPoint = Indicator.GetPivotPoints(quotes, PeriodSize.Day).ToList();

            quotes.Add(new QuoteExtention()
            {
                Date = DateTime.Now.AddDays(1),
                Close = latestQuote.Close,
                Open = latestQuote.Open,
                Low = latestQuote.Low,
                Volume = latestQuote.Volume,
                High = latestQuote.High
            });

            var pivotPoint1 = Indicator.GetPivotPoints(quotes, PeriodSize.Day).ToList();


            var quotes1 = new List<QuoteExtention>();
            foreach (var hist in history)
            {
                quotes1.Add(new QuoteExtention()
                {
                    Date = hist.TimeStamp,
                    Close = hist.Close,
                    Open = hist.Open,
                    Low = hist.Low,
                    Volume = hist.Volume,
                    High = hist.High
                });
            }


            quotes1.Add(new QuoteExtention()
            {
                Date = DateTime.Now.AddDays(1),
                Close = 700,
                Open = 700,
                Low = 700,
                Volume = 700,
                High = 7000
            });

            var pivotPoint2 = Indicator.GetPivotPoints(quotes1, PeriodSize.Day).ToList();

            Assert.True(pivotPoint1.Last().PP == pivotPoint2.Last().PP);
            Assert.True(pivotPoint1.Last().R1 == pivotPoint2.Last().R1);
            Assert.True(pivotPoint1.Last().R2 == pivotPoint2.Last().R2);

            Assert.True(pivotPoint1.Last().S1 == pivotPoint2.Last().S1);
            Assert.True(pivotPoint1.Last().S2 == pivotPoint2.Last().S2);


            System.Console.WriteLine(DateTime.Now.ToString("dddd-dd-MMMM-yyyy-HH-mm-ss"));



            System.Console.WriteLine(DateTime.Now.AddMinutes(3).ToString("dddd-dd-MMMM-yyyy-HH-mm-ss"));
            

            System.Console.ReadLine();

        }



    }
}