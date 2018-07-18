using System;
namespace Myth.SpreadConsole.Pages
{
    class HelpPage : PageBase
    {
        public override string Title { get; } = "帮助页面";

        public override string Prompt => Title;

        public override string InputPrompt => "输入b返回或输出q退出...";

        protected override bool DoInput(string input)
        {
            return false;
        }

        protected override void DoShow()
        {
            Console.WriteLine("Nothing");
        }
    }
}
