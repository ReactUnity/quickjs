using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public static class CodeGenUtils
    {
        public static string Concat(string sp, params string[] values)
        {
            return string.Join(sp, from value in values where !string.IsNullOrEmpty(value) select value);
        }

        public static string ConcatAsLiteral(string sp, params string[] values)
        {
            return string.Join(sp, from value in values where !string.IsNullOrEmpty(value) select $"\"{value}\"");
        }
    }
}