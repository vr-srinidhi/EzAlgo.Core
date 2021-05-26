using System;

namespace ExAlgo.Core.Contracts
{
    public class History
    {
        public DateTime TimeStamp { get; set; }

        public Decimal Open { get; set; }

        public Decimal High { get; set; }

        public Decimal Low { get; set; }

        public Decimal Close { get; set; }

        public uint Volume { get; set; }
    }
}
