using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DiceIoC;
using Domain;

namespace Performance
{
    [Description("Dice")]
    class DiceUseCase : UseCase
    {
        private readonly static DiceIoC.Container container;
 
        static DiceUseCase()
        {
            var catalog = new Catalog()
                .Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()))
                .Register<IAuthenticator>(
                    c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IStockQuote>(
                    c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IDatabase>(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>()))
                .Register<IErrorHandler>(c => new ErrorHandler(c.Resolve<ILogger>()))
                .Register<ILogger>(c => new Logger(), Singleton.Lifetime());
            container = catalog.CreateContainer();
        }
        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }

    [Description("DiceSingleton")]
    class DiceSingletonUseCase : UseCase
    {
        private readonly static DiceIoC.Container container;

        static DiceSingletonUseCase()
        {
            container = new Catalog()
                .Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()))
                .Register<IAuthenticator>(
                    c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()),
                    Singleton.Lifetime())
                .Register<IStockQuote>(
                    c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()),
                    Singleton.Lifetime())
                .Register<IDatabase>(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>()),
                    Singleton.Lifetime())
                .Register<IErrorHandler>(c => new ErrorHandler(c.Resolve<ILogger>()), Singleton.Lifetime())
                .Register<ILogger>(c => new Logger(), Singleton.Lifetime())
                .CreateContainer();
        }
        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }
}
