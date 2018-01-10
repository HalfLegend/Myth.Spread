using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Myth.Spread.Library
{
    public static class ReflectionExtension
    {
        public static T GetGenericValue<T>(this FieldInfo fieldInfo) {
            return (T) fieldInfo.GetValue(null);
        }
    }
}
