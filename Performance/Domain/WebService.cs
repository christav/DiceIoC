namespace Performance.Domain
{
	public class WebService : IWebService
	{
	    readonly IAuthenticator authenticator;
	    readonly IStockQuote quotes;

		public WebService(IAuthenticator authenticator, IStockQuote quotes)
		{
			this.authenticator = authenticator;
			this.quotes = quotes;
		}

		public IAuthenticator Authenticator { get { return authenticator; } }
		public IStockQuote StockQuote { get { return quotes; } }

		#region Behavior

		public void Execute()
		{
		}

		#endregion
	}
}