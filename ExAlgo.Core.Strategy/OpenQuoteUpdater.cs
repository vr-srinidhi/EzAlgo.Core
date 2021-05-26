using System;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Contracts;
using KiteConnect;
using NLog;

namespace ExAlgo.Core.Strategy
{
    public class OpenQuoteUpdater : IOpenQuoteUpdater
    {
        private readonly IQuoteRepository _quoteRepository;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public OpenQuoteUpdater(IQuoteRepository quoteRepository)
        {
            this._quoteRepository = quoteRepository;
        }

        public bool IsTradeTimeOpen()
        {
            TimeSpan startTimeSpan = new TimeSpan(9, 15, 00);
            TimeSpan endTimeSpan = new TimeSpan(15, 30, 30);
            TimeSpan currentTimeSpan = DateTime.Now.TimeOfDay;
            var response = currentTimeSpan > startTimeSpan && currentTimeSpan < endTimeSpan;

            if (response)
            {
                if ((DateTime.Now.Minute % 15) == 0 &&
                    DateTime.Now.Second <= 3)
                {
                    response = true;
                }
                else
                {
                    response = false;
                }
            }
            return response;
        }

        public bool ProcessQuote(Tick tick)
        {
            var quotes = _quoteRepository.QuotesContainers[tick.InstrumentToken.ToString()];
            var key = Int64.Parse(DateTime.Now.ToString("ddMMyyyyHHmm"));
            if (!quotes.OpeningPrice.ContainsKey(key))
            {
                quotes.OpeningPrice.TryAdd(key, tick.LastPrice);
                Logger.Info($"Recieved Opening tick for{Contracts.NSE.NationalStockExchange50[tick.InstrumentToken.ToString()]}");
            }
            return true;
        }


        private static DateTime TimeRoundDown(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0).AddMinutes(-input.Minute % 15);
        }
    }
    public interface IOpenQuoteUpdater
    {
        bool ProcessQuote(Tick tick);
        bool IsTradeTimeOpen();
    }
}
