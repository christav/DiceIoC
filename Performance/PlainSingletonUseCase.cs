using System;
using Domain;
using System.ComponentModel;

namespace Performance
{
	[Description("No IOC Container Singleton")]
	public class PlainSingletonUseCase : UseCase
	{
		static Authenticator authenticator;
		static StockQuote stockQuote;

		static PlainSingletonUseCase()
		{
			var logger = new Logger();
			var errorHandler = new ErrorHandler(logger);
			var database = new Database(logger, errorHandler);
			stockQuote = new StockQuote(logger, errorHandler, database);
			authenticator = new Authenticator(logger, errorHandler, database);
		}

		public override void Run()
		{

			var app = new WebService(authenticator, stockQuote);

			app.Execute();
		}
	}
}
