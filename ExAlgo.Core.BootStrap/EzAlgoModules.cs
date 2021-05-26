using System;
using System.Collections.Generic;
using Autofac;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using ExAlgo.Core.Processor;
using ExAlgo.Core.Strategy;
using ExAlgo.Core.Zerodha;

namespace ExAlgo.Core.BootStrap
{
    public class EzAlgoModules : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Authetication>().As<IAuthetication>().SingleInstance();
            builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();
            builder.RegisterType<QuoteRepository>().As<IQuoteRepository>().SingleInstance();
            builder.Register(_ => new ZerodhaClient(_.Resolve<IAuthetication>()));
            builder.Register(_ => new QuoteRepositoryManager(_.Resolve<ZerodhaClient>(), _.Resolve<IQuoteRepository>())).SingleInstance();
            builder.RegisterType<Processor.Strategy>().As<IStrategy>().SingleInstance();
            builder.RegisterType<ConnorsRsiAndEma200>().As<IConnorsRsiAndEma200>().SingleInstance();
            builder.RegisterType<OrderProcessor>().As<IOrderProcessor>().SingleInstance();

            builder.RegisterType<MovingAverage200And6Cross>().As<IMovingAverage200And6Cross>().SingleInstance();

            builder.RegisterType<BankIndexGap>().As<IBankIndexGap>().SingleInstance();
            builder.RegisterType<ITIndexGap>().As<IITIndexGap>().SingleInstance();

            builder.RegisterType<PharmaIndexGap>().As<IPharmaIndexGap>().SingleInstance();
            builder.RegisterType<MetalIndexGap>().As<IMetalIndexGap>().SingleInstance();
            builder.RegisterType<BullishAndBearisEngulfingRunTime>().As<IBullishAndBearisEngulfingRunTime>().SingleInstance();

            builder.RegisterType<OpenQuoteUpdater>().As<IOpenQuoteUpdater>().SingleInstance();

        }

    }
}
