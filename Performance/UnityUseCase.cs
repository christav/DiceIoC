using System.ComponentModel;
using Performance.Domain;
using Unity;
using Unity.Lifetime;

namespace Performance
{
	[Description("Unity")]
	public class UnityUseCase : UseCase
	{
		static readonly UnityContainer container;

		static UnityUseCase()
		{
			container = new UnityContainer();

			container.RegisterType<IWebService, WebService>();
			container.RegisterType<IAuthenticator, Authenticator>();
			container.RegisterType<IStockQuote, StockQuote>();
			container.RegisterType<IDatabase, Database>();
			container.RegisterType<IErrorHandler, ErrorHandler>();
			container.RegisterType<ILogger, Logger>(new ContainerControlledLifetimeManager());

		}

		public override void Run()
		{
			var webApp = container.Resolve<IWebService>();
			webApp.Execute();
		}
	}
}
