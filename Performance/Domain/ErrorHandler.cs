namespace Performance.Domain
{
	public class ErrorHandler : IErrorHandler
	{
	    readonly ILogger logger;

		public ErrorHandler(ILogger logger)
		{
			this.logger = logger;
		}

		public ILogger Logger { get { return logger; } }

		#region Behavior

		#endregion
	}
}
