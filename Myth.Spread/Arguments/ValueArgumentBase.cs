using System.Collections;
using System.Collections.Generic;

namespace Myth.Spread.Arguments {
    public abstract class ValueArgumentBase<T> : ArgumentBase {
        public T Value { get; }

        protected ValueArgumentBase(IEnumerable<string> args) {
            Value = ParseCommand(args);
        }

        protected abstract T ParseCommand(IEnumerable<string> args);
    }
}