namespace DiceIoC
{
    public interface ILifetime
    {
        object GetValue(Container c);
        object SetValue(object value, Container c);
    }
}
