using System;
using System.Collections.Generic;
using Myth.SpreadConsole.Pages;

namespace Myth.SpreadConsole.Framework
{
    static class NavigationManager
    {
        static readonly Stack<Type> _pageStack = new Stack<Type>();

        public static void Navigate<T>() where T : PageBase
        {
            _pageStack.Push(typeof(T));
            Activator.CreateInstance<T>().Show();
        }

        public static void Back()
        {
            _pageStack.Pop();
            ((PageBase)Activator.CreateInstance(_pageStack.Peek())).Show();
        }
    }
}
