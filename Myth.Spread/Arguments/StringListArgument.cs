using System.Collections.Generic;

namespace Myth.Spread.Arguments
{
    public abstract class StringListArgument : ListArgumentBase<string>
    {
        protected StringListArgument(string[] values) : base(values)
        {
        }

        protected override ICollection<string> ParseCommand(string[] args)
        {
            return args;
        }
    }
}