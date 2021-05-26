using System;
using System.Linq;
using System.Threading;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Contracts;
using ExAlgo.Core.Order;
using ExAlgo.Core.Strategy;
using ExAlgo.Core.Zerodha;
using KiteConnect;
using Newtonsoft.Json;
using NLog;

namespace ExAlgo.Core.Processor
{
    public class Strategy : IStrategy
    {

        private readonly IConfiguration _configuration;
        private readonly IConnorsRsiAndEma200 connorsRsiAndEma200;
        private readonly IMovingAverage200And6Cross movingAverage200And6Cross;
        private readonly IOrderProcessor orderProcessor;
        private readonly IITIndexGap iTIndexGap;
        private readonly IBankIndexGap bankIndexGap;
        private readonly IPharmaIndexGap pharmaIndexGap;
        private readonly IMetalIndexGap metalIndexGap;
        private readonly IQuoteRepository quoteRepository;
        private readonly IBullishAndBearisEngulfingRunTime bullishAndBearisEngulfingRunTime;
        private readonly IOpenQuoteUpdater openQuoteUpdater;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private  Ticker _ticker;

        public Strategy(IConfiguration configuration,
            IConnorsRsiAndEma200 connorsRsiAndEma200,
            IMovingAverage200And6Cross movingAverage200And6Cross,
            IOrderProcessor orderProcessor,
            IITIndexGap iTIndexGap,
            IBankIndexGap bankIndexGap,
            IPharmaIndexGap pharmaIndexGap,
            IMetalIndexGap metalIndexGap,
            IQuoteRepository quoteRepository,
            IBullishAndBearisEngulfingRunTime bullishAndBearisEngulfingRunTime,
            IOpenQuoteUpdater openQuoteUpdater)
        {
            
            _configuration = configuration;
            this.connorsRsiAndEma200 = connorsRsiAndEma200;
            this.movingAverage200And6Cross = movingAverage200And6Cross;
            this.orderProcessor = orderProcessor;
            this.iTIndexGap = iTIndexGap;
            this.bankIndexGap = bankIndexGap;
            this.pharmaIndexGap = pharmaIndexGap;
            this.metalIndexGap = metalIndexGap;
            this.quoteRepository = quoteRepository;
            this.bullishAndBearisEngulfingRunTime = bullishAndBearisEngulfingRunTime;
            this.openQuoteUpdater = openQuoteUpdater;
        }

        public void RegisterAndProcessTicks()
        {
            _ticker = new Ticker(_configuration.ZerodhaApikey, _configuration.ZerodhaAccessToken);
            _ticker.Subscribe(NSE.NationalStockExchange50.Keys.Concat(NSE.NseIndices.Keys).Select(uint.Parse).ToArray());
            _ticker.SetMode(NSE.NationalStockExchange50.Keys.Select(uint.Parse).ToArray(), "quote");
            _ticker.OnTick += TickRecieved;
            _ticker.OnOrderUpdate += OnOrderUpdate;

            _ticker.EnableReconnect(Interval: 5, Retries: 50);
            _ticker.Connect();
        }



        private void TickRecieved(Tick tickData)
        {
            tickData.Timestamp = DateTime.Now;
            var instrumentTok = tickData.InstrumentToken.ToString();
            if (NSE.NseIndices.ContainsKey(instrumentTok))
            {
                if(!quoteRepository.IndexOpenPrice.ContainsKey(instrumentTok))
                    quoteRepository.IndexOpenPrice.TryAdd(instrumentTok, tickData.Open);

                return;
            }

            if (openQuoteUpdater.IsTradeTimeOpen())
            {
                openQuoteUpdater.ProcessQuote(tickData);
            }

            if (connorsRsiAndEma200.IsTradeTimeOpen())
            {
                connorsRsiAndEma200.ProcessQuote(tickData);
            }

            if (iTIndexGap.IsTradeTimeOpen())
            {
                iTIndexGap.ProcessQuote(tickData);
            }

            if (bullishAndBearisEngulfingRunTime.IsTradeTimeOpen())
            {
                bullishAndBearisEngulfingRunTime.ProcessQuote(tickData);
            }

            if (bankIndexGap.IsTradeTimeOpen())
            {
                bankIndexGap.ProcessQuote(tickData);
            }

            if (pharmaIndexGap.IsTradeTimeOpen())
            {
                pharmaIndexGap.ProcessQuote(tickData);
            }

            if (metalIndexGap.IsTradeTimeOpen())
            {
                metalIndexGap.ProcessQuote(tickData);
            }
            orderProcessor.TickRecieved(tickData);

        }

        public void UnRegisterNseIndicex()
        {
            _ticker.UnSubscribe(NSE.NseIndices.Keys.Select(uint.Parse).ToArray());
        }


        private  void OnOrderUpdate(KiteConnect.Order OrderData)
        {


            if(!string.IsNullOrEmpty(OrderData.ParentOrderId))
            {
                orderProcessor.UpdateTrigerOrderId(OrderData);
            }
            Logger.Info($"Order CallBack :  {JsonConvert.SerializeObject(OrderData)}");
        }
        }


    public interface IStrategy
    {

        void RegisterAndProcessTicks();
        void UnRegisterNseIndicex();
        
    }
}
