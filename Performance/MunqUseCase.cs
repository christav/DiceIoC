using System.ComponentModel;
using Domain;
using Munq;
using Munq.LifetimeManagers;

namespace Performance
{
    [Description("Munq")]
    class MunqUseCase : UseCase
    {
        private static IocContainer container;
        private static ILifetimeManager singleton;

        static MunqUseCase()
        {
            container = new IocContainer();
            singleton = new ContainerLifetime();

            container.Register<IWebService>(
                c => new WebService(
                         c.Resolve<IAuthenticator>(),
                         c.Resolve<IStockQuote>()));

            container.Register<IAuthenticator>(
                c => new Authenticator(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>(),
                         c.Resolve<IDatabase>()));

            container.Register<IStockQuote>(
                c => new StockQuote(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>(),
                         c.Resolve<IDatabase>()));

            container.Register<IDatabase>(
                c => new Database(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>()));

            container.Register<IErrorHandler>(
                c => new ErrorHandler(c.Resolve<ILogger>()));

            container.RegisterInstance<ILogger>(new Logger())
                     .WithLifetimeManager(singleton);
        }

        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }

    [Description("Munq Autowire")]
    class MunqAutowireUseCase : UseCase
    {
        private static IocContainer container;
        private static ILifetimeManager singleton;

        static MunqAutowireUseCase()
        {
            container = new IocContainer();
            singleton = new ContainerLifetime();

            container.Register<IAuthenticator, Authenticator>();
            container.Register<IStockQuote, StockQuote>();
            container.Register<IDatabase, Database>();
            container.Register<IErrorHandler, ErrorHandler>();
            container.Register<ILogger, Logger>()
                     .WithLifetimeManager(singleton);
        }
        public override void Run()
        {
            var webApp = container.Resolve<WebService>();
            webApp.Execute();
        }
    }

    [Description("Munq Singleton")]
    class MunqSingletonUseCase : UseCase
    {
        private static IocContainer container;
        private static ILifetimeManager singleton;

        static MunqSingletonUseCase()
        {
            container = new IocContainer();
            singleton = new ContainerLifetime();

            container.Register<IWebService>(
                c => new WebService(
                         c.Resolve<IAuthenticator>(),
                         c.Resolve<IStockQuote>()))
                         .WithLifetimeManager(singleton);

            container.Register<IAuthenticator>(
                c => new Authenticator(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>(),
                         c.Resolve<IDatabase>()))
                         .WithLifetimeManager(singleton);

            container.Register<IStockQuote>(
                c => new StockQuote(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>(),
                         c.Resolve<IDatabase>()))
                         .WithLifetimeManager(singleton);

            container.Register<IDatabase>(
                c => new Database(
                         c.Resolve<ILogger>(),
                         c.Resolve<IErrorHandler>()))
                         .WithLifetimeManager(singleton);

            container.Register<IErrorHandler>(
                c => new ErrorHandler(c.Resolve<ILogger>())).WithLifetimeManager(singleton);

            container.RegisterInstance<ILogger>(new Logger())
                     .WithLifetimeManager(singleton);
        }

        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }        
    }
}
