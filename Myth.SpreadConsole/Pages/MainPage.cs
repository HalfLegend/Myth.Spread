using System;
namespace Myth.SpreadConsole.Pages
{
    class MainPage : ItemPage
    {
        public MainPage() : base(new HelpPage())
        {
        }

        public override string Title => "主页";

        public override string Prompt => throw new NotImplementedException();

        public override string InputPrompt => throw new NotImplementedException();

        protected override bool DoInput(string input)
        {
            throw new NotImplementedException();
        }
    }
}
