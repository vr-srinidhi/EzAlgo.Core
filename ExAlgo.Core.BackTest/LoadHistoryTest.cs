using System;
using System.Linq;
using ExAlgo.Core.Cache;
using KiteConnect;
using NUnit.Framework;

namespace ExAlgo.Core.BackTest
{
    public class LoadHistoryTest
    {
        Zerodha.ZerodhaClient zerodhaClient;
        QuoteRepository quoteRepository;
        Kite KiteClient;
        QuoteRepositoryManager QuoteRepositoryManager;

        [SetUp]
        public void Setup()
        {
            var authentication = new Zerodha.Authetication(new Zerodha.Configuration());
            zerodhaClient = new Zerodha.ZerodhaClient(authentication);
            KiteClient = authentication.Authorize();
            quoteRepository = new QuoteRepository();
            QuoteRepositoryManager = new QuoteRepositoryManager(zerodhaClient, quoteRepository);
        }


        [Test]
        public void LoadCurrentHistory()
        {
            QuoteRepositoryManager.LoadHistoricalData();

            foreach(var nse in Contracts.NSE.NationalStockExchange50)
            { 
                quoteRepository.QuotesContainers.TryGetValue(nse.Key, out var quotesContainer);
                var quotes = quotesContainer.QuoteExtentions;

                var pullDownTime = TimeRoundDown(DateTime.Now);
                var previousPullDownTime = TimeRoundDown(pullDownTime.AddMinutes(-5));

                var latestQuote = quotesContainer.QuoteExtentions.OrderByDescending(_ => _.Date).First();

                var openPrice = latestQuote.Close - latestQuote.Close * (1) / 100;
                var closePrice = latestQuote.Close + latestQuote.Close * (1) / 100;

                quotesContainer.OpeningPrice.TryAdd(Int64.Parse(previousPullDownTime.ToString("ddMMyyyyHHmm")), openPrice);
                quotesContainer.OpeningPrice.TryAdd(Int64.Parse(pullDownTime.ToString("ddMMyyyyHHmm")), closePrice);
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            QuoteRepositoryManager.LoadCurrentData();

            watch.Stop();

            foreach (var nse in Contracts.NSE.NationalStockExchange50)
            {
                quoteRepository.QuotesContainers.TryGetValue(nse.Key, out var quotesContainer);
                var quotes = quotesContainer.QuoteExtentions;

                var pullDownTime = TimeRoundDown(DateTime.Now);
                var previousPullDownTime = TimeRoundDown(pullDownTime);

                var latestQuote = quotesContainer.QuoteExtentions.OrderByDescending(_ => _.Date).Take(2).ToArray();

                var openPrice = latestQuote[1].Close - latestQuote[1].Close * (1) / 100;
                var closePrice = latestQuote[1].Close + latestQuote[1].Close * (1) / 100;

                Assert.AreEqual(latestQuote[0].Open, openPrice);
                Assert.AreEqual(latestQuote[0].Close, closePrice);

            }


        }


        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }
    }
}
