using System;
namespace ExAlgo.Core.Contracts
{
    public class Query
    {
        public string InstrumentToken { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Interval { get; set; }
        public bool Continuous { get; set; }
        public bool Oi { get; set; }
    }
}
