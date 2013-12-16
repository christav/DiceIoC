namespace DiceIoC
{
    //
    // The following interfaces are uses as markers in register
    // calls to indicate that you are registering an open generic.
    // T0 represents your first generic argument, T1 the second, etc.
    // (We stop at 16 because that's how many variants of Action<T>
    // and Func<T> there are. If you need more you're doing something
    // wrong. :-) )
    //
    // Consider the classic "generic repository" implementation. Say that
    // your interface takes two generic parameters - the type of value
    // in the repository and a database provider, like so:
    //
    //    public interface IRepository<TData, TDataProvider> { ... }
    //
    // Now you want to use the DefaultRespository interface when you
    // ask for an IRepository. To register that, do:
    //
    //    catalog.Register<IRepository<T0, T1>(c => new DefaultRepository<T0, T1>(...));
    //
    // The T0, T1, etc at resolve time will be replaced with the parameters
    // passed in the actual resolve call. So, in this call:
    //
    //    var repository = container.Resolve<IRepository<User, InMemoryDb>();
    //
    // T0 would be replaced by User, and T1 would be replaced by InMemoryDb.
    //
    // If you have a generic constraint, you can still do open registration by
    // creating a new type that satisfies your constraints but implements the
    // corresponding positional marker interface. Then use your new type when
    // registering.
    //
    // For example, suppose that there were constraints on the generic parameters
    // in the IRepositoryInterface:
    //
    //    public interface IRepository<TData, TDataProvider> where
    //        TData: new(),
    //        TDataProvider: IDataProvider
    //    { ... }
    //
    // IRepository<T0, T1> won't compile now due to unsatisified constraints. But you
    // can do:
    //
    //    class T0Data : T0 { public T0Data() { } }
    //    interface T1DataProvider: T1, IDataProvider 
    //    {
    //        DbConnection Open() { throw new NotImplementedException(); }
    //        ... etc for each method in IDataProvider
    //    }
    //
    // Now, you can register your types:
    //
    //    container.Register<IRepository<T0Data, T1DataProvider>>(c => new DefaultRepository<T0Data, T1DataProvider(...));
    //
    // The catalog will take care of matching up the types as before.
    //

// ReSharper disable InconsistentNaming
    // Known deviation - these are interfaces only to make it easier to derive
    // types from to satisfy constraints
    public interface T0 {}
    public interface T1 {}
    public interface T2 {}
    public interface T3 {}
    public interface T4 {}
    public interface T5 {}
    public interface T6 {}
    public interface T7 {}
    public interface T8 {}
    public interface T9 {}
    public interface T10 {}
    public interface T11 {}
    public interface T12 {}
    public interface T13 {}
    public interface T14 {}
    public interface T15 {}
    public interface T16 {}
// ReSharper restore InconsistentNaming

}
