using System;
using System.Collections.Generic;

namespace Myth.SpreadConsole.Pages
{
    abstract class ItemPage : PageBase 
    {
        PageBase[] _pages;
        public ItemPage(params PageBase[] pages)
        {
            _pages = pages;
        }

        protected override void DoShow()
        {
            for (int i = 0; i < _pages.Length; i++){
                Console.WriteLine("{0,4}  {1}", i + 1, _pages[i].Prompt);
            }

            int input;
            do
            {
                input = Console.Read() - '0';
            }
            while (input > 0 && input <= _pages.Length);

            _pages[input - 1].Show();
        }
    }
}
