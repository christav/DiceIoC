using System.ComponentModel;
using LightInject;
using Performance.Domain;

namespace Performance
{
    [Description("LightInject")]
    class LightInjectUseCase : UseCase
    {
        private readonly static ServiceContainer container;

        static LightInjectUseCase()
        {
            container = new ServiceContainer();
            container.Register<IWebService, WebService>();
            container.Register<IAuthenticator, Authenticator>();
            container.Register<IStockQuote, StockQuote>();
            container.Register<IDatabase, Database>();
            container.Register<IErrorHandler, ErrorHandler>();
            container.Register<ILogger, Logger>(new PerContainerLifetime());
        }

        public override void Run()
        {
            var webApp = container.GetInstance<IWebService>();
            webApp.Execute();
        }
    }

    [Description("LightInject Singleton")]
    class LightInjectSingletonUseCase: UseCase
    {
        private readonly static ServiceContainer container;

        static LightInjectSingletonUseCase()
        {
            container = new ServiceContainer();
            container.Register<IWebService, WebService>(new PerContainerLifetime());
            container.Register<IAuthenticator, Authenticator>(new PerContainerLifetime());
            container.Register<IStockQuote, StockQuote>(new PerContainerLifetime());
            container.Register<IDatabase, Database>(new PerContainerLifetime());
            container.Register<IErrorHandler, ErrorHandler>(new PerContainerLifetime());
            container.Register<ILogger, Logger>(new PerContainerLifetime());
        }

        public override void Run()
        {
            var webApp = container.GetInstance<IWebService>();
            webApp.Execute();
        }

    }
}
