using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateBridgeBindingInfo
    {
        // set of delegate types
        public HashSet<Type> types = new HashSet<Type>();

        // 反射生成的 MethodInfo, 用于 reflectbind 模式下注册委托映射
        public MethodInfo reflect;
        
        public Type returnType;
        public ParameterInfo[] parameters;
        public string requiredDefines;

        public DelegateBridgeBindingInfo(Type returnType, ParameterInfo[] parameters, string requiredDefines)
        {
            this.returnType = returnType;
            this.parameters = parameters;
            this.requiredDefines = requiredDefines;
        }

        public bool Equals(Type returnType, ParameterInfo[] parameters, string requiredDefines)
        {
            if (this.requiredDefines != requiredDefines)
            {
                return false;
            }
            
            if (returnType != this.returnType || parameters.Length != this.parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != this.parameters[i].ParameterType)
                {
                    return false;
                }

                if (parameters[i].IsOut != this.parameters[i].IsOut)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
