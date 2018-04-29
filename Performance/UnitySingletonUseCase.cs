using System.ComponentModel;
using Performance.Domain;
using Unity;
using Unity.Lifetime;

namespace Performance
{
	[Description("UnitySingleton")]
	public class UnitySingletonUseCase : UseCase
	{
		static readonly UnityContainer container;

		static UnitySingletonUseCase()
		{
			container = new UnityContainer();

			container.RegisterType<IWebService, WebService>();
			container.RegisterType<IAuthenticator, Authenticator>(new ContainerControlledLifetimeManager());
			container.RegisterType<IStockQuote, StockQuote>(new ContainerControlledLifetimeManager());
			container.RegisterType<IDatabase, Database>(new ContainerControlledLifetimeManager());
			container.RegisterType<IErrorHandler, ErrorHandler>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());
		}

		public override void Run()
		{
			var webApp = container.Resolve<IWebService>();
			webApp.Execute();
		}
	}
}
