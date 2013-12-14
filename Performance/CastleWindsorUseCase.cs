using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

using Domain;

namespace Performance
{
    [System.ComponentModel.Description("Castle Windsor")]
    class CastleWindsorUseCase : UseCase
    {
        private static readonly WindsorContainer container;

        static CastleWindsorUseCase()
        {
            container = new WindsorContainer();
            container.Register(Component
                .For<IWebService>()
                .ImplementedBy<WebService>()
                .LifeStyle.Transient);
            container.Register(Component
                .For<IAuthenticator>()
                .ImplementedBy<Authenticator>()
                .LifeStyle.Transient);
            container.Register(Component
                .For<IStockQuote>()
                .ImplementedBy<StockQuote>()
                .LifeStyle.Transient);
            container.Register(Component
                .For<IDatabase>()
                .ImplementedBy<Database>()
                .LifeStyle.Transient);
            container.Register(Component
                .For<IErrorHandler>()
                .ImplementedBy<ErrorHandler>()
                .LifeStyle.Transient);
            container.Register(Component
                .For<ILogger>()
                .ImplementedBy<Logger>());
        }

        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
    }

    [System.ComponentModel.Description("Castle Windsor Singleton")]
    class CastleWindsorSingletonUseCase : UseCase
    {
        private static readonly WindsorContainer container;

        static CastleWindsorSingletonUseCase()
        {
            container = new WindsorContainer();
            container.Register(Component
                .For<IWebService>()
                .ImplementedBy<WebService>());
            container.Register(Component
                .For<IAuthenticator>()
                .ImplementedBy<Authenticator>());
            container.Register(Component
                .For<IStockQuote>()
                .ImplementedBy<StockQuote>());
            container.Register(Component
                .For<IDatabase>()
                .ImplementedBy<Database>());
            container.Register(Component
                .For<IErrorHandler>()
                .ImplementedBy<ErrorHandler>());
            container.Register(Component
                .For<ILogger>()
                .ImplementedBy<Logger>());
        }

        public override void Run()
        {
            var webApp = container.Resolve<IWebService>();
            webApp.Execute();
        }
        
    }
}
