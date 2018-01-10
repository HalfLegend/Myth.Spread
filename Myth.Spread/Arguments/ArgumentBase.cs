namespace Myth.Spread.Arguments {
    public abstract class ArgumentBase {
        protected ArgumentBase() { }

        public abstract string Prompt { get; }
    }
}