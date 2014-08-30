using System.ComponentModel;
using DiceIoC;
using DiceIoC.Catalogs;
using Performance.Domain;

namespace Performance
{
    [Description("Dice")]
    class DiceUseCase : UseCase
    {
        private readonly static DiceIoC.Container container;
        private static readonly LifetimeContainer singleton = new LifetimeContainer();

        static DiceUseCase()
        {
            container = new Catalog()
                .Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()))
                .Register<IAuthenticator>(
                    c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IStockQuote>(
                    c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()))
                .Register<IDatabase>(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>()))
                .Register<IErrorHandler>(c => new ErrorHandler(c.Resolve<ILogger>()))
                .Register<ILogger>(c => new Logger(), singleton.Lifetime)
                .CreateContainer();
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
        private static readonly LifetimeContainer singleton = new LifetimeContainer();

        static DiceSingletonUseCase()
        {
            container = new Catalog()
                .With(() => singleton, r => {
                    r.Register<IWebService>(c => new WebService(c.Resolve<IAuthenticator>(), c.Resolve<IStockQuote>()));
                    r.Register<IAuthenticator>(c => new Authenticator(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()));
                    r.Register<IStockQuote>(c => new StockQuote(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>(), c.Resolve<IDatabase>()));
                    r.Register<IDatabase>(c => new Database(c.Resolve<ILogger>(), c.Resolve<IErrorHandler>()));
                    r.Register<IErrorHandler>(c => new ErrorHandler(c.Resolve<ILogger>()));
                    r.Register<ILogger>(c => new Logger());
                })
                .CreateContainer();
        }
        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }
}
