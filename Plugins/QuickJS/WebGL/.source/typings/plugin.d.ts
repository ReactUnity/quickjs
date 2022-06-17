export { };

declare global {
  var state: PluginState;

  export declare type PluginState = {
    stringify: ((bytes: any) => string);
    stringifyBuffer: ((bytes: any, bufferSize) => string);
    bufferify: ((str: string) => [number, number]);
    dynCall: ((bytes: any) => string);
    runtimes: Record<string, PluginRuntime | undefined>;
    contexts: Record<string, PluginContext | undefined>;
    lastRuntimeId: number;
    lastContextId: number;
    atoms?: PluginHeap<string, JSAtom>;
    createHeap: <T>(isAtom: boolean) => PluginHeap<T>;

    getRuntime: (ctx: JSRuntime) => PluginRuntime;
    getContext: (ctx: JSContext) => PluginContext;
  }

  export declare type PluginRuntime = {
    id: number;
    opaque?: any;
    contexts: Record<string, PluginContext | undefined>;
  };

  export declare type PluginContext = {
    id: number;
    opaque?: any;
    runtimeId: number;
    globalId?: number;

    objects: PluginHeap;

    iframe: HTMLIFrameElement;
    window: Window;
    evaluate: ((script: string) => any);
    execute: ((script: string) => any);

    lastException?: Error;
  };

  export declare type PluginHeap<T = any, PtrType = JSValue> = {
    record: Record<string | number, PluginHeapObject>;
    get: ((ref: PtrType) => T);
    push: ((obj: T, ptr: PtrType) => number);
    ref: ((obj: PtrType, diff: number, ptr: PtrType) => number);
    refIndex: ((obj: number, diff: number, ptr: PtrType) => number);
    lastId: number;
  };

  export declare type PluginHeapObject = {
    refCount: number;
    tag: Tags;
    value: any;
  };


  const enum JSPropFlags {
    /* flags for object properties */
    JS_PROP_CONFIGURABLE = (1 << 0),
    JS_PROP_WRITABLE = (1 << 1),
    JS_PROP_ENUMERABLE = (1 << 2),
    JS_PROP_C_W_E = (JS_PROP_CONFIGURABLE | JS_PROP_WRITABLE | JS_PROP_ENUMERABLE),
    JS_PROP_LENGTH = (1 << 3) /* used internally in Arrays */,
    JS_PROP_TMASK = (3 << 4) /* mask for NORMAL, GETSET, VARREF, AUTOINIT */,
    JS_PROP_NORMAL = (0 << 4),
    JS_PROP_GETSET = (1 << 4),
    JS_PROP_VARREF = (2 << 4) /* used internally */,
    JS_PROP_AUTOINIT = (3 << 4) /* used internally */,

    /* flags for JS_DefineProperty */
    JS_PROP_HAS_SHIFT = 8,
    JS_PROP_HAS_CONFIGURABLE = (1 << 8),
    JS_PROP_HAS_WRITABLE = (1 << 9),
    JS_PROP_HAS_ENUMERABLE = (1 << 10),
    JS_PROP_HAS_GET = (1 << 11),
    JS_PROP_HAS_SET = (1 << 12),
    JS_PROP_HAS_VALUE = (1 << 13),

    /* throw an exception if false would be returned
       (JS_DefineProperty/JS_SetProperty) */
    JS_PROP_THROW = (1 << 14),

    /* throw an exception if false would be returned in strict mode
       (JS_SetProperty) */
    JS_PROP_THROW_STRICT = (1 << 15),

    JS_PROP_NO_ADD = (1 << 16) /* internal use */,
    JS_PROP_NO_EXOTIC = (1 << 17) /* internal use */,

    // custom values
    CONST_VALUE = JS_PROP_HAS_VALUE | JS_PROP_ENUMERABLE,
    DEFAULT = JS_PROP_CONFIGURABLE | JS_PROP_ENUMERABLE,
    NONE = 0,
  }

  const enum Tags {
    JS_TAG_FIRST = -11, /* first negative tag */
    JS_TAG_BIG_DECIMAL = -11,
    JS_TAG_BIG_INT = -10,
    JS_TAG_BIG_FLOAT = -9,
    JS_TAG_SYMBOL = -8,
    JS_TAG_STRING = -7,
    JS_TAG_MODULE = -3, /* used internally */
    JS_TAG_FUNCTION_BYTECODE = -2, /* used internally */
    JS_TAG_OBJECT = -1,
    JS_TAG_INT = 0,
    JS_TAG_BOOL = 1,
    JS_TAG_NULL = 2,
    JS_TAG_UNDEFINED = 3,
    JS_TAG_EXCEPTION = 6,
    JS_TAG_FLOAT64 = 7,
  }

  const enum Constants {
    VERSION = 0x010704,
    CS_JSB_VERSION = 0xa,

    JS_WRITE_OBJ_BYTECODE = 1 << 0, /* allow function/module */
    JS_WRITE_OBJ_BSWAP = 1 << 1, /* byte swapped output */
    JS_WRITE_OBJ_SAB = 1 << 2, /* allow SharedArrayBuffer */
    JS_WRITE_OBJ_REFERENCE = 1 << 3, /* allow object references to encode arbitrary object graph */
    JS_READ_OBJ_BYTECODE = 1 << 0, /* allow function/module */
    JS_READ_OBJ_ROM_DATA = 1 << 1, /* avoid duplicating 'buf' data */
    JS_READ_OBJ_SAB = 1 << 2, /* allow SharedArrayBuffer */
    JS_READ_OBJ_REFERENCE = 1 << 3, /* allow object references */
  }

  const enum JSEvalFlags {
    JS_EVAL_TYPE_GLOBAL = (0 << 0) /* global code (default) */,
    JS_EVAL_TYPE_MODULE = (1 << 0) /* module code */,
    JS_EVAL_TYPE_DIRECT = (2 << 0) /* direct call (internal use) */,
    JS_EVAL_TYPE_INDIRECT = (3 << 0) /* indirect call (internal use) */,
    JS_EVAL_TYPE_MASK = (3 << 0),

    JS_EVAL_FLAG_STRICT = (1 << 3) /* force 'strict' mode */,
    JS_EVAL_FLAG_STRIP = (1 << 4) /* force 'strip' mode */,

    /* compile but do not run. The result is an object with a
       JS_TAG_FUNCTION_BYTECODE or JS_TAG_MODULE tag. It can be executed
       with JS_EvalFunction(). */
    JS_EVAL_FLAG_COMPILE_ONLY = (1 << 5),

    /* don't include the stack frames before this eval in the Error() backtraces */
    JS_EVAL_FLAG_BACKTRACE_BARRIER = (1 << 6),
  }
}