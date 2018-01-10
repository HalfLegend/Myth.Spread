namespace Myth.Spread.Arguments
{
    public abstract class ValueArgumentBase<T> : ArgumentBase
    {
        public T Value { get; }

        public ValueArgumentBase(string[] args)
        {
            Value = ParseCommand(args);
        }
        protected abstract T ParseCommand(string[] args);
    }
}