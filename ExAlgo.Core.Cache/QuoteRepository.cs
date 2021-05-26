using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ExAlgo.Core.Contracts;

namespace ExAlgo.Core.Cache
{
    public class QuoteRepository : IQuoteRepository
    {
        public QuoteRepository()
        {
            HistoricalQuotes = new ConcurrentDictionary<string, List<History>>();
            QuotesContainers = new ConcurrentDictionary<string, QuotesContainer>();
            LastDayClosePrice = new Dictionary<string, decimal>();
            IndexOpenPrice = new ConcurrentDictionary<string, decimal>();

        }

        public ConcurrentDictionary<string, List<History>> HistoricalQuotes { get; set; }
        public ConcurrentDictionary<string, QuotesContainer> QuotesContainers { get; set; }
        public Dictionary<string, decimal> LastDayClosePrice { get; set; }
        public ConcurrentDictionary<string,decimal> IndexOpenPrice { get; set; }
    }

    public interface IQuoteRepository
    {
        ConcurrentDictionary<string, List<History>> HistoricalQuotes { get; set; }
        ConcurrentDictionary<string, QuotesContainer> QuotesContainers { get; set; }
        Dictionary<string, decimal> LastDayClosePrice { get; set; }
        ConcurrentDictionary<string, decimal> IndexOpenPrice { get; set; }
    }
}
