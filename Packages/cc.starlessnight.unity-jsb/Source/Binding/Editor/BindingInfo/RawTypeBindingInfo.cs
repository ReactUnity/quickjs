using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuickJS.Binding
{
    public class RawTypeBindingInfo
    {
        public readonly Type type;
        public readonly MethodInfo method;

        public RawTypeBindingInfo(Type type, MethodInfo method)
        {
            this.type = type;
            this.method = method;
        }
    }
}