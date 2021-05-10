﻿#if !JSB_UNITYLESS
namespace QuickJS.Unity
{
    [System.Flags]
    public enum JSScriptClassType
    {
        None = 0,
        MonoBehaviour = 1,
        Editor = 2,
        // EditorWindow,
    }

    public struct JSScriptClassPathHint
    {
        public readonly string sourceFile;
        public readonly string modulePath;
        public readonly string className;
        public readonly string classPath;
        public readonly JSScriptClassType classType;

        public JSScriptClassPathHint(string sourceFile, string modulePath, string className, JSScriptClassType classType)
        {
            this.sourceFile = sourceFile;
            this.modulePath = modulePath;
            this.className = className;
            this.classPath = JSBehaviourScriptRef.ToClassPath(modulePath, className);
            this.classType = classType;
        }

        public bool IsReferenced(JSBehaviourScriptRef scriptRef)
        {
            return scriptRef.sourceFile == sourceFile && scriptRef.modulePath == modulePath && scriptRef.className == className;
        }

        public string ToClassPath()
        {
            return classPath;
        }
    }
}
#endif