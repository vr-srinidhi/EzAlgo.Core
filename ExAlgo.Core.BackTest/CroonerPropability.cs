using System;
using System.Collections.Generic;
using System.Linq;
using ExAlgo.Core.Contracts;
using KiteConnect;
using NUnit.Framework;
using Skender.Stock.Indicators;

namespace ExAlgo.Core.BackTest
{
    public class CroonerPropability
    {
        public CroonerPropability()
        {
        }

        Zerodha.ZerodhaClient zerodhaClient;
        Kite KiteClient;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);
            KiteClient = authentication.Authorize();
        }

        [Test]
        public void CroonerTest()
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

            var crooner = Indicator.GetConnorsRsi(quotes);
            var last3Set = crooner.OrderByDescending(_ => _.Date).Take(3);


            var latest = quotes.OrderByDescending(_ => _.Date).First();
            var nextPrice = latest.Close + latest.Close * (.1M) / 100;

            var nextQuote = new QuoteExtention()
            {
                Date = latest.Date.AddMinutes(15),
                Close = nextPrice,
                Open = latest.Close,
                Low = latest.Low,
                Volume = latest.Volume+ 100,
                High = nextPrice+1
            };

            quotes.Add(nextQuote);

            var crooner1 = Indicator.GetConnorsRsi(quotes);
            var last3Set1 = crooner1.OrderByDescending(_ => _.Date).Take(3);

        }
    }
}
