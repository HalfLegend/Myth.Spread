using System;
using System.Collections;
using System.Collections.Generic;

namespace Myth.Spread.Arguments {
    public abstract class ListArgumentBase<T> : ValueArgumentBase<IEnumerable<T>>, IEnumerable<T> {
        protected ListArgumentBase(IEnumerable<string> args) : base(args) { }

        public IEnumerator<T> GetEnumerator() {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return Value.GetEnumerator();
        }
    }
}