using System;
using StructureMap;
using Domain;

namespace Performance
{
	[System.ComponentModel.Description("StructureMapSingleton")]
	public class StructureMapSingletonUseCase : UseCase
	{
		static Container container;

		static StructureMapSingletonUseCase()
		{
			container = new Container();
			container.Configure(
				x => x.For<IWebService>()
					  .Use<WebService>());

			container.Configure(
				x => x.For<IAuthenticator>().Singleton()
						.Use<Authenticator>());

			container.Configure(
				x => x.For<IStockQuote>().Singleton()
					.Use<StockQuote>());

			container.Configure(
				x => x.For<IDatabase>().Singleton()
					.Use<Database>());

			container.Configure(
				x => x.For<IErrorHandler>().Singleton()
					.Use<ErrorHandler>());

			container.Configure(
				x => x.For<ILogger>().Singleton()
					.Use(c => new Logger()));
		}

		public override void Run()
		{
			var webApp = container.GetInstance<IWebService>();
			webApp.Execute();
		}
	}
}
