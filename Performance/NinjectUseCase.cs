using System.ComponentModel;
using Domain;
using Ninject;

namespace Performance
{
    [Description("Ninject")]
    class NinjectUseCase : UseCase
    {
        private static readonly IKernel kernel;

        static NinjectUseCase()
        {
            kernel = new StandardKernel();
            kernel.Bind<IWebService>().To<WebService>();
            kernel.Bind<IAuthenticator>().To<Authenticator>();
            kernel.Bind<IStockQuote>().To<StockQuote>();
            kernel.Bind<IDatabase>().To<Database>();
            kernel.Bind<IErrorHandler>().To<ErrorHandler>();
            kernel.Bind<ILogger>().To<Logger>().InSingletonScope();
        }

        public override void Run()
        {
            var webApp = kernel.Get<IWebService>();
            webApp.Execute();
        }
    }

    [Description("Ninject Singleton")]
    class NinjectSingletonUseCase : UseCase
    {
        private static readonly IKernel kernel;

        static NinjectSingletonUseCase()
        {
            kernel = new StandardKernel();
            kernel.Bind<IWebService>().To<WebService>().InSingletonScope();
            kernel.Bind<IAuthenticator>().To<Authenticator>().InSingletonScope();
            kernel.Bind<IStockQuote>().To<StockQuote>().InSingletonScope();
            kernel.Bind<IDatabase>().To<Database>().InSingletonScope();
            kernel.Bind<IErrorHandler>().To<ErrorHandler>().InSingletonScope();
            kernel.Bind<ILogger>().To<Logger>().InSingletonScope();
        }

        public override void Run()
        {
            var webApp = kernel.Get<IWebService>();
            webApp.Execute();
        }
        
    }
}
