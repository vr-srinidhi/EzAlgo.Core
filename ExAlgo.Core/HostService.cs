using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using ExAlgo.Core.Processor;
using FluentScheduler;
using NLog;

namespace ExAlgo.Core
{
    public class HostService
    {
        private readonly IStrategy _strategy;
        private readonly QuoteRepositoryManager _quoteRepositoryManager;
        
        private readonly IOrderProcessor _orderProcessor;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public HostService(QuoteRepositoryManager quoteRepositoryManager,
            IStrategy strategy,
            IOrderProcessor orderProcessor
            )
        {
            _quoteRepositoryManager = quoteRepositoryManager;
            _orderProcessor = orderProcessor;
            this._strategy = strategy;
            JobManager.Initialize();
        }

        public void OnStart()
        {
            //_quoteRepositoryManager.LoadHistoricalData();
            //_strategy.RegisterAndProcessTicks();
            JobManager.AddJob(_quoteRepositoryManager.LoadHistoricalData, _ => _.ToRunOnceAt(9, 5));
            JobManager.AddJob(_quoteRepositoryManager.LoadCurrentData, _ => _.ToRunOnceAt(9, 30).AndEvery(15).Minutes());
            JobManager.AddJob(_strategy.RegisterAndProcessTicks, _ => _.ToRunOnceAt(9, 12));
            JobManager.AddJob(_orderProcessor.WriteConfirmation, _ => _.ToRunOnceAt(15, 30));
            JobManager.AddJob(_strategy.UnRegisterNseIndicex, _ => _.ToRunOnceAt(9, 16));

            Logger.Info($"Scheduler registered..");

            Logger.Info($"LoadHistoricalData scheduled at ..9:13");
            Logger.Info($"LoadCurrentData scheduled at ..9:30");
            Logger.Info($"RegisterAndProcessTicks scheduled at ..9:15");
            Logger.Info($"WriteConfirmation scheduled at ..15:30");
        }

        public void OnStop()
        {
            
            Logger.Info($"Service Stopped..");
        }
    }
}
