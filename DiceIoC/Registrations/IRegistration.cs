using System;
using System.Linq.Expressions;
 
namespace DiceIoC.Registrations
{
    using FactoryExpression = Expression<Func<Container, object>>;

    /// <summary>
    /// Represents a registration for a type
    /// </summary>
    public interface IRegistration
    {
        FactoryExpression GetFactory();

        FactoryExpression GetFactory(Type serviceType);
    }
}
