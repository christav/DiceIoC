using System;
using System.ComponentModel;
using Performance.Domain;

namespace Performance
{
    [Description("Activator.CreateInstance")]
    class ActivatorUseCase : UseCase
    {
        public override void Run()
        {
            var logger = Activator.CreateInstance<Logger>();

            var app = (IWebService)Activator.CreateInstance(typeof(WebService), 
                (IAuthenticator)Activator.CreateInstance(typeof(Authenticator),
                    logger,
                    (IErrorHandler)Activator.CreateInstance(typeof(ErrorHandler),
                        logger
                    ),
                    (IDatabase)Activator.CreateInstance(typeof(Database),
                        logger,
                        (IErrorHandler)Activator.CreateInstance(typeof(ErrorHandler),
                            logger
                        )
                    )
                ),
                (IStockQuote)Activator.CreateInstance(typeof(StockQuote),
                    logger,
                    (IErrorHandler)Activator.CreateInstance(typeof(ErrorHandler),
                        logger
                    ),
                    (IDatabase)Activator.CreateInstance(typeof(Database),
                        logger,
                        (IErrorHandler)Activator.CreateInstance(typeof(ErrorHandler),
                            logger
                        )
                    )
                )
            );

            app.Execute();
        }
    }
}
