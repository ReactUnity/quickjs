﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AOT;
using QuickJS.Native;
using System.Threading;
using System.Reflection;
using QuickJS.Binding;
using QuickJS.Utils;

namespace QuickJS
{
    using UnityEngine;

    public partial class ScriptRuntime
    {
        public event Action<ScriptRuntime> OnDestroy;
        public event Action<ScriptRuntime> OnAfterDestroy;

        private JSRuntime _rt;
        private IScriptLogger _logger;
        private List<ScriptContext> _contexts = new List<ScriptContext>();
        private ScriptContext _mainContext;
        private Queue<JSAction> _pendingGC = new Queue<JSAction>();

        private int _mainThreadId;
        private uint _class_id_alloc = JSApi.__JSB_GetClassID();

        private IFileResolver _fileResolver;
        private IFileSystem _fileSystem;
        private ObjectCache _objectCache = new ObjectCache();
        private TypeDB _typeDB;
        private TimerManager _timerManager;
        private IO.ByteBufferAllocator _byteBufferAllocator;
        private GameObject _container;
        private bool _isValid; // destroy 调用后立即 = false

        public ScriptRuntime()
        {
            _isValid = true;
            _fileResolver = new FileResolver();
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _timerManager = new TimerManager();
            _rt = JSApi.JS_NewRuntime();
            JSApi.JS_SetModuleLoaderFunc(_rt, module_normalize, module_loader, IntPtr.Zero);
            _mainContext = CreateContext();
        }

        public GameObject GetContainer()
        {
            if (_container == null && _isValid)
            {
                _container = new GameObject("JSRuntimeContainer");
                _container.hideFlags = HideFlags.HideInHierarchy;
                Object.DontDestroyOnLoad(_container);
            }
            return _container;
        }

        public void AddSearchPath(string path)
        {
            _fileResolver.AddSearchPath(path);
        }

        public void Initialize(IFileSystem fileSystem, IScriptRuntimeListener runner,
            IScriptLogger logger,
            IO.ByteBufferAllocator byteBufferAllocator = null, int step = 30)
        {
            if (logger == null)
            {
                throw new NullReferenceException(nameof(logger));
            }
            if (runner == null)
            {
                throw new NullReferenceException(nameof(runner));
            }
            if (fileSystem == null)
            {
                throw new NullReferenceException(nameof(fileSystem));
            }
            _byteBufferAllocator = byteBufferAllocator;
            _fileSystem = fileSystem;
            _logger = logger;
            var e = _InitializeStep(_mainContext, runner, step);
            while (e.MoveNext()) ;
        }

        private IEnumerator _InitializeStep(ScriptContext context, IScriptRuntimeListener runner, int step)
        {
            var register = new TypeRegister(this, context);
            var regArgs = new object[] { register };
            var bindingTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int assemblyIndex = 0, assemblyCount = assemblies.Length;
                assemblyIndex < assemblyCount;
                assemblyIndex++)
            {
                var assembly = assemblies[assemblyIndex];
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }

                    var exportedTypes = assembly.GetExportedTypes();
                    for (int i = 0, size = exportedTypes.Length; i < size; i++)
                    {
                        var type = exportedTypes[i];
#if UNITY_EDITOR
                        if (type.IsDefined(typeof(JSAutoRunAttribute), false))
                        {
                            try
                            {
                                var run = type.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                                if (run != null)
                                {
                                    run.Invoke(null, null);
                                }
                            }
                            catch (Exception exception)
                            {
                                _logger.Error(exception);
                            }

                            continue;
                        }
#endif
                        var attributes = type.GetCustomAttributes(typeof(JSBindingAttribute), false);
                        if (attributes.Length == 1)
                        {
                            var jsBinding = attributes[0] as JSBindingAttribute;
                            if (jsBinding.Version == 0 || jsBinding.Version == ScriptEngine.VERSION)
                            {
                                bindingTypes.Add(type);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Write(LogLevel.Error, "assembly: {0}, {1}", assembly, e);
                }
            }

            var numRegInvoked = bindingTypes.Count;
            for (var i = 0; i < numRegInvoked; ++i)
            {
                var type = bindingTypes[i];
                var reg = type.GetMethod("Bind");
                if (reg != null)
                {
                    reg.Invoke(null, regArgs);

                    if (i % step == 0)
                    {
                        yield return null;
                    }
                }
            }

            register.RegisterType(typeof(ScriptBridge));
            runner.OnBind(this, register);
            TimerManager.Bind(register);
            ScriptContext.Bind(register);
            _typeDB = register.Finish();
            runner.OnComplete(this);
        }

        public IO.ByteBufferAllocator GetByteBufferAllocator()
        {
            return _byteBufferAllocator;
        }

        public TimerManager GetTimerManager()
        {
            return _timerManager;
        }

        public IScriptLogger GetLogger()
        {
            return _logger;
        }

        public TypeDB GetTypeDB()
        {
            return _typeDB;
        }

        public Utils.ObjectCache GetObjectCache()
        {
            return _objectCache;
        }

        public JSClassID NewClassID()
        {
            return _class_id_alloc++;
        }

        public ScriptContext CreateContext()
        {
            var context = new ScriptContext(this);
            _contexts.Add(context);
            context.OnDestroy += OnContextDestroy;
            context.RegisterBuiltins();
            return context;
        }

        private void OnContextDestroy(ScriptContext context)
        {
            _contexts.Remove(context);
        }

        public ScriptContext GetMainContext()
        {
            return _mainContext;
        }

        public ScriptContext GetContext(JSContext ctx)
        {
            for (int i = 0, count = _contexts.Count; i < count; i++)
            {
                var context = _contexts[i];
                if (context.IsContext(ctx))
                {
                    return context;
                }
            }

            return null;
        }

        private static void _FreeValueAction(ScriptRuntime rt, JSValue value)
        {
            JSApi.JS_FreeValueRT(rt, value);
        }

        private static void _FreeValueAndDelegationAction(ScriptRuntime rt, JSValue value)
        {
            var cache = rt.GetObjectCache();
            cache.RemoveDelegate(value);
            JSApi.JS_FreeValueRT(rt, value);
        }

        public void FreeDelegationValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _objectCache.RemoveDelegate(value);
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAndDelegationAction,
                };
                lock (_pendingGC)
                {
                    _pendingGC.Enqueue(act);
                }
            }
        }

        public void FreeValue(JSValue value)
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                if (_rt != JSRuntime.Null)
                {
                    JSApi.JS_FreeValueRT(_rt, value);
                }
            }
            else
            {
                var act = new JSAction()
                {
                    value = value,
                    callback = _FreeValueAction,
                };
                lock (_pendingGC)
                {
                    _pendingGC.Enqueue(act);
                }
            }
        }

        public void FreeValues(JSValue[] values)
        {
            if (values == null)
            {
                return;
            }
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                for (int i = 0, len = values.Length; i < len; i++)
                {
                    JSApi.JS_FreeValueRT(_rt, values[i]);
                }
            }
            else
            {
                lock (_pendingGC)
                {
                    for (int i = 0, len = values.Length; i < len; i++)
                    {
                        var act = new JSAction()
                        {
                            value = values[i],
                            callback = _FreeValueAction,
                        };
                        _pendingGC.Enqueue(act);
                    }
                }
            }
        }

        public void EvalMain(string fileName)
        {
            string resolvedPath;
            if (_fileResolver.ResolvePath(_fileSystem, fileName, out resolvedPath))
            {
                var source = _fileSystem.ReadAllBytes(resolvedPath);
                var input_bytes = GetShebangNullTerminatedBytes(source);
                _mainContext.EvalMain(input_bytes, resolvedPath);
            }
        }

        // main loop
        public void Update(float deltaTime)
        {
            if (_pendingGC.Count != 0)
            {
                CollectPendingGarbage();
            }

            ExecutePendingJob();

            // poll here;
            var ms = (int)(deltaTime * 1000f);
            _timerManager.Update(ms);

            if (_byteBufferAllocator != null)
            {
                _byteBufferAllocator.Drain();
            }
        }

        public void ExecutePendingJob()
        {
            JSContext ctx;
            while (true)
            {
                var err = JSApi.JS_ExecutePendingJob(_rt, out ctx);
                if (err == 0)
                {
                    break;
                }

                if (err < 0)
                {
                    ctx.print_exception();
                }
            }
        }

        private void CollectPendingGarbage()
        {
            lock (_pendingGC)
            {
                while (true)
                {
                    if (_pendingGC.Count == 0)
                    {
                        break;
                    }

                    var action = _pendingGC.Dequeue();
                    action.callback(this, action.value);
                }
            }
        }

        public void Destroy()
        {
            _isValid = false;
            try
            {
                OnDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            _timerManager.Destroy();
            _objectCache.Clear();
            _typeDB.Destroy();
            GC.Collect();
            CollectPendingGarbage();

            for (int i = 0, count = _contexts.Count; i < count; i++)
            {
                var context = _contexts[i];
                context.Destroy();
            }

            _contexts.Clear();

            if (_container != null)
            {
                Object.DestroyImmediate(_container);
                _container = null;
            }

            JSApi.JS_FreeRuntime(_rt);
            _rt = JSRuntime.Null;

            try
            {
                OnAfterDestroy?.Invoke(this);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

        }

        public static implicit operator JSRuntime(ScriptRuntime se)
        {
            return se._rt;
        }
    }
}