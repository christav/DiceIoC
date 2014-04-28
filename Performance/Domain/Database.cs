namespace Performance.Domain
{
	public class Database : IDatabase
	{
	    readonly ILogger logger;
	    readonly IErrorHandler handler;

		public Database(ILogger logger, IErrorHandler handler)
		{
			this.logger = logger;
			this.handler = handler;
		}

		public ILogger Logger { get { return logger; } }
		public IErrorHandler ErrorHandler { get { return handler; } }

		#region Behavior

		#endregion
	}
}
