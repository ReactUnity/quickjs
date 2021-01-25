using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJS.Unity
{
    using UnityEngine;
    using UnityEditor;
    using QuickJS.Native;

    public unsafe static class ReflectBindDelegateGen
    {
        public static void ActionCall(ScriptDelegate fn)
        {
            var ctx = fn.ctx;
            var rval = fn.Invoke(ctx);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1>(ScriptDelegate fn, T1 a1)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[1];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 1, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2>(ScriptDelegate fn, T1 a1, T2 a2)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[2];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 2, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2, T3>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[3];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T3>(ctx, a3);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 3, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2, T3, T4>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3, T4 a4)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[4];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T3>(ctx, a3);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[3] = ReflectBindValueOp.js_push_tvar<T4>(ctx, a4);
            if (argv[3].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 4, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            JSApi.JS_FreeValue(ctx, argv[3]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static void ActionCall<T1, T2, T3, T4, T5>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3, T4 a4, T5 a5)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[5];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T3>(ctx, a3);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[3] = ReflectBindValueOp.js_push_tvar<T4>(ctx, a4);
            if (argv[3].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[4] = ReflectBindValueOp.js_push_tvar<T5>(ctx, a5);
            if (argv[4].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                JSApi.JS_FreeValue(ctx, argv[2]);
                JSApi.JS_FreeValue(ctx, argv[3]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 5, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            JSApi.JS_FreeValue(ctx, argv[3]);
            JSApi.JS_FreeValue(ctx, argv[4]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            JSApi.JS_FreeValue(ctx, rval);
        }

        public static RT FuncCall<RT>(ScriptDelegate fn)
        {
            var ctx = fn.ctx;
            var rval = fn.Invoke(ctx);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, RT>(ScriptDelegate fn, T1 a1)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[1];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 1, argv);
            if (rval.IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            JSApi.JS_FreeValue(ctx, argv[0]);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, T2, RT>(ScriptDelegate fn, T1 a1, T2 a2)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[2];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 2, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }

        public static RT FuncCall<T1, T2, T3, RT>(ScriptDelegate fn, T1 a1, T2 a2, T3 a3)
        {
            var ctx = fn.ctx;
            var argv = stackalloc JSValue[3];
            argv[0] = ReflectBindValueOp.js_push_tvar<T1>(ctx, a1);
            if (argv[0].IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            argv[1] = ReflectBindValueOp.js_push_tvar<T2>(ctx, a2);
            if (argv[1].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                throw new Exception(ctx.GetExceptionString());
            }
            argv[2] = ReflectBindValueOp.js_push_tvar<T3>(ctx, a3);
            if (argv[2].IsException())
            {
                JSApi.JS_FreeValue(ctx, argv[0]);
                JSApi.JS_FreeValue(ctx, argv[1]);
                throw new Exception(ctx.GetExceptionString());
            }
            var rval = fn.Invoke(ctx, 3, argv);
            JSApi.JS_FreeValue(ctx, argv[0]);
            JSApi.JS_FreeValue(ctx, argv[1]);
            JSApi.JS_FreeValue(ctx, argv[2]);
            if (rval.IsException())
            {
                throw new Exception(ctx.GetExceptionString());
            }
            RT ret0;
            var succ = ReflectBindValueOp.js_get_tvar<RT>(ctx, rval, out ret0);
            JSApi.JS_FreeValue(ctx, rval);
            if (succ)
            {
                return ret0;
            }
            else
            {
                throw new Exception("js exception caught");
            }
        }
    }
}