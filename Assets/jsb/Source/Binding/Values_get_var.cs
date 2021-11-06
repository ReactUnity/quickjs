using System;
using System.Reflection;
using System.Collections.Generic;

namespace QuickJS.Binding
{
    using Native;

    // collect all built-in js-cs conversion helper methods
    public partial class Values
    {
        // cast js value to csharp value 
        // TypeCastGet ~ get/rebind: bool js_get_*(JSContext ctx, JSValue val, out T o);
        public static Dictionary<Type, MethodInfo> _JSCastMap = new Dictionary<Type, MethodInfo>();

        // replace the js value reference with another csharp value (for struct)
        public static Dictionary<Type, MethodInfo> _JSRebindMap = new Dictionary<Type, MethodInfo>();

        // cast csharp value to js value
        // TypeCastPush ~ push: JSValue js_push_primitive(JSContext ctx, T o)
        public static Dictionary<Type, MethodInfo> _CSCastMap = new Dictionary<Type, MethodInfo>();

        // construct a js value with given csharp value
        // TypeCastNew ~ new: JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, T o, int type_id, bool disposable)
        public static Dictionary<Type, MethodInfo> _JSNewMap = new Dictionary<Type, MethodInfo>();

        public delegate bool TypeCastGet<T>(JSContext ctx, JSValue val, out T o);

        public delegate JSValue TypeCastPush<T>(JSContext ctx, T o);

        public delegate JSValue TypeCastNew<T>(JSContext ctx, JSValue new_target, T o, int type_id, bool disposable);

        private static void init_cast_map()
        {
            var methods = typeof(Values).GetMethods();
            for (int i = 0, len = methods.Length; i < len; ++i)
            {
                register_type_caster(methods[i]);
            }
        }

        public static bool register_type_caster<T>(TypeCastGet<T> fn)
        {
            return register_type_caster(fn.Method);
        }

        public static bool register_type_caster<T>(TypeCastPush<T> fn)
        {
            return register_type_caster(fn.Method);
        }

        public static bool register_type_caster<T>(TypeCastNew<T> fn)
        {
            return register_type_caster(fn.Method);
        }

        public static bool register_type_caster(MethodInfo method)
        {
            if (!method.IsGenericMethodDefinition && method.IsStatic && method.IsPublic)
            {
                var parameters = method.GetParameters();

                if (parameters.Length < 2 || parameters[0].ParameterType != typeof(JSContext))
                {
                    return false;
                }

                if (parameters.Length == 5)
                {
                    if (parameters[1].ParameterType == typeof(JSValue))
                    {
                        // JSValue NewBridgeClassObject(JSContext ctx, JSValue new_target, T o, int type_id, bool disposable)
                        if (method.Name == "NewBridgeClassObject")
                        {
                            var type = parameters[2].ParameterType;
                            _JSNewMap[type] = method;
                            return true;
                        }
                    }
                }
                else if (parameters.Length == 3)
                {
                    // should only collect the method name with the expected signature, 
                    // bool js_get_*(JSContext ctx, JSValue val, out T o);
                    if (parameters[2].ParameterType.IsByRef && parameters[1].ParameterType == typeof(JSValue))
                    {
                        var type = parameters[2].ParameterType.GetElementType();
                        switch (method.Name)
                        {
                            case "js_rebind_this":
                                _JSRebindMap[type] = method;
                                return true;
                            case "js_get_primitive":
                            case "js_get_structvalue":
                            case "js_get_classvalue":
                                _JSCastMap[type] = method;
                                return true;
                        }
                    }
                }
                else if (parameters.Length == 2)
                {
                    // JSValue js_push_primitive(JSContext ctx, T o)
                    if (method.Name.StartsWith("js_push_"))
                    {
                        var type = parameters[1].ParameterType;

                        _CSCastMap[type] = method;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool js_rebind_var(JSContext ctx, JSValue this_obj, Type type, object o)
        {
            MethodInfo method;
            if (_JSRebindMap.TryGetValue(type, out method))
            {
                var parameters = new object[3] { ctx, this_obj, o };
                return (bool)method.Invoke(o, parameters);
            }
            return false;
        }

        /// <summary>
        /// convert csharp object `o` to jsvalue with o.GetType()
        /// </summary>
        public static JSValue js_push_var(JSContext ctx, object o)
        {
            if (o == null)
            {
                return JSApi.JS_UNDEFINED;
            }

            var type = o.GetType();

            if (type.BaseType == typeof(MulticastDelegate))
            {
                return js_push_delegate(ctx, o as Delegate);
            }

            if (type.IsEnum)
            {
                return js_push_primitive(ctx, Convert.ToInt32(o));
            }

            MethodInfo cast;
            do
            {
                if (_CSCastMap.TryGetValue(type, out cast))
                {
                    var parameters = new object[2] { ctx, o };
                    var rval = (JSValue)cast.Invoke(null, parameters);
                    return rval;
                }
                type = type.BaseType;
            } while (type != null);

            //NOTE: 2. fallthrough, push as object
            return js_push_classvalue(ctx, o);
        }

        public static JSValue js_new_var(JSContext ctx, JSValue new_target, Type type, object o, int type_id, bool disposable)
        {
            // most of NewBridgeClassObject are overrided for struct-type, no need to traverse their BaseType
            // all class-type can be directly tackled as 'object'
            MethodInfo cast;
            if (_JSNewMap.TryGetValue(type, out cast))
            {
                var parameters = new object[5] { ctx, new_target, o, type_id, disposable };
                var rval = (JSValue)cast.Invoke(null, parameters);
                return rval;
            }

            return NewBridgeClassObject(ctx, new_target, o, type_id, disposable);
        }

        public static bool js_get_var(JSContext ctx, JSValue val, out object o)
        {
            return GetObjectFallthrough(ctx, val, out o);
        }

        // type: expected type of object o
        public static bool js_get_var(JSContext ctx, JSValue val, Type type, out object o)
        {
            if (type.BaseType == typeof(MulticastDelegate))
            {
                Delegate d;
                var rs = js_get_delegate(ctx, val, type, out d);
                o = d;
                return rs;
            }

            var lookupType = type;
            MethodInfo cast;
            do
            {
                if (_JSCastMap.TryGetValue(lookupType, out cast))
                {
                    var parameters = new object[3] { ctx, val, null };
                    var rval = (bool)cast.Invoke(null, parameters);
                    o = parameters[2];
                    return rval;
                }
                lookupType = lookupType.BaseType;
            } while (lookupType != null);

            if (type.IsArray)
            {
                if (val.IsNullish())
                {
                    o = null;
                    return true;
                }

                if (type.GetArrayRank() == 1 && JSApi.JS_IsArray(ctx, val) == 1)
                {
                    var lengthVal = JSApi.JS_GetProperty(ctx, val, JSApi.JS_ATOM_length);
                    if (JSApi.JS_IsException(lengthVal))
                    {
                        o = null;
                        return WriteScriptError(ctx);
                    }

                    var elementType = type.GetElementType();
                    int length;
                    JSApi.JS_ToInt32(ctx, out length, lengthVal);
                    JSApi.JS_FreeValue(ctx, lengthVal);
                    var array = Array.CreateInstance(elementType, length);
                    for (var i = 0U; i < length; i++)
                    {
                        var eVal = JSApi.JS_GetPropertyUint32(ctx, val, i);
                        object e;
                        if (js_get_var(ctx, eVal, elementType, out e))
                        {
                            array.SetValue(e, i);
                            JSApi.JS_FreeValue(ctx, eVal);
                        }
                        else
                        {
                            o = null;
                            JSApi.JS_FreeValue(ctx, eVal);
                            return false;
                        }
                    }
                    o = array;
                    return true;
                }
            }

            if (type.IsEnum)
            {
                return js_get_enumvalue(ctx, val, type, out o);
            }

            if (type == typeof(void))
            {
                o = null;
                return true;
            }

            return GetObjectFallthrough(ctx, val, out o);
        }
    }
}
