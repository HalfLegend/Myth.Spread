using System;
using Myth.SpreadConsole.Framework;

namespace Myth.SpreadConsole.Pages
{
    delegate void PageEventHandler(PageBase page, object eventArgs);
    delegate void InputEventHandler(PageBase page, string input);
    abstract class PageBase
    {
        public PageBase()
        {
            OnShowing += (page, eventArgs) => Console.Clear();
        }

        public abstract string Title { get; }
        public abstract string Prompt { get; }

        public abstract string InputPrompt { get; }
        public void Show()
        {
            OnShowing(this, null);
            DoShow();
            OnShowed?.Invoke(this, null);

            while (!HandleInput()) { }
        }

        bool HandleInput()
        {
            Console.WriteLine(InputPrompt);
            string input = Console.ReadLine().Trim();
            if (input == "q" || input == "Q")
            {
                Environment.Exit(0);
            }else if(input == "b" || input == "B"){
                NavigationManager.Back();
                return true;
            }
            OnInputing?.Invoke(this, input);
            bool handled = DoInput(input);
            OnInputing?.Invoke(this, input);
            return handled;
        }

        protected abstract bool DoInput(string input);

        protected abstract void DoShow();

        public event PageEventHandler OnShowing;
        public event PageEventHandler OnShowed;
        public event InputEventHandler OnInputing;
        public event InputEventHandler OnInputted;
    }
}
