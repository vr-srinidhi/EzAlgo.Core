using System;
namespace ExAlgo.Core.Contracts
{
    public class Data
    {
        public string order_id { get; set; }
    }

    public class ZerodaOrderResponse
    {
        public string status { get; set; }
        public Data data { get; set; }
    }
}
