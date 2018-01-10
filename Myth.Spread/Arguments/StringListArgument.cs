using System.Collections.Generic;

namespace Myth.Spread.Arguments {
    public abstract class StringListArgument : ListArgumentBase<string> {
        protected StringListArgument(IEnumerable<string> values) : base(values) { }

        protected override IEnumerable<string> ParseCommand(IEnumerable<string> args) {
            return args;
        }
    }
}