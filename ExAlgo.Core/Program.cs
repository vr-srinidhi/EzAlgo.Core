using System;
using System.Diagnostics;
using System.Reflection;
using Autofac;
using ExAlgo.Core.BootStrap;
using ExAlgo.Core.Cache;
using ExAlgo.Core.Order;
using ExAlgo.Core.Processor;
using NLog;
using Topshelf;

namespace ExAlgo.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
            System.Console.WriteLine($"Starting ExAlgo {version} service...");
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new EzAlgoModules());
            containerBuilder.Register(_ => new HostService(_.Resolve<QuoteRepositoryManager>(), _.Resolve<IStrategy>(), _.Resolve<IOrderProcessor>()));
            var container = containerBuilder.Build();
            HostFactory.Run(x =>
            {
                x.UseNLog();
                x.Service<HostService>(_ =>
                {
                    _.ConstructUsing(name => container.Resolve<HostService>());
                    _.WhenStarted(x => x.OnStart());
                    _.WhenStopped(tc => tc.OnStop());
                }
                );


                x.StartAutomaticallyDelayed();
                x.RunAsLocalSystem();

                x.SetDescription("Datafactory to process rail reference data");
                x.SetDisplayName($"EzAlgoTrading.Standard {version}"); // To be able to see which version is running on the machine
                x.SetServiceName($"EzAlgoTrading.Standard"); // To make sure we only have one at a time
            });

            System.Console.ReadLine();
        }
    }
}
