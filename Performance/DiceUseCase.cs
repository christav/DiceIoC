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
        private static DiceIoC.Container container;
 
        static DiceUseCase()
        {
            container = new DiceIoC.Container()
                .Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()))
                .Register<IAuthenticator>(
                    c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IStockQuote>(
                    c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IDatabase>(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>()))
                .Register<IErrorHandler>(c => new ErrorHandler(c.Resolve<ILogger>()))
                .Register<ILogger>(DiceIoC.Container.Singleton(c => new Logger()));
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
        private static DiceIoC.Container container;

        static DiceSingletonUseCase()
        {
            container = new DiceIoC.Container()
                .Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()))
                .Register<IAuthenticator>(DiceIoC.Container.Singleton(
                    c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>())))
                .Register<IStockQuote>(DiceIoC.Container.Singleton(
                    c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>())))
                .Register<IDatabase>(DiceIoC.Container.Singleton(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>())))
                .Register<IErrorHandler>(DiceIoC.Container.Singleton(c => new ErrorHandler(c.Resolve<ILogger>())))
                .Register<ILogger>(DiceIoC.Container.Singleton(c => new Logger()));
        }
        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }
}
