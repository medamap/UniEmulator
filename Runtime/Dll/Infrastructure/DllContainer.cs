using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace jp.megamin.UniEmulator.Runtime.Dll.Infrastructure
{

    public class DllContainer : IDisposable
    {
        private IntPtr _dll = IntPtr.Zero;
        
        delegate void FnUnityPluginLoad(IntPtr unityInterfaces);
        delegate void FnUnityPluginUnload();

        public DllContainer(string dllpath)
        {
            _dll = LoadLibrary(dllpath);

            if (_dll != IntPtr.Zero)
            {
                Debug.Log($"Load dll is successes. {_dll}");
                GetDelegate<FnUnityPluginLoad>("UnityPluginLoad")?.Invoke(GetUnityInterface());
            }
            else
            {
                Debug.Log($"Load dll is failed.");
            }
        }

        public void Dispose()
        {
            if (_dll != IntPtr.Zero)
            {
                Debug.Log($"Unload A");
                GetDelegate<FnUnityPluginUnload>("UnityPluginUnload")?.Invoke();
                Debug.Log($"Unload B");
                var freeFlag = FreeLibrary(_dll);
                Debug.Log($"Unload C {freeFlag}");
                _dll = IntPtr.Zero;
                Debug.Log($"Unload D");
            }
        }

        [DllImport("UnityInterfaceGetter")] private static extern IntPtr GetUnityInterface();

        public TDelegate GetDelegate<TDelegate>(string procName)
        {
            if (_dll != IntPtr.Zero)
            {
                return Marshal.GetDelegateForFunctionPointer<TDelegate>(GetProcAddress(_dll, procName));
            }
            return default(TDelegate);
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
    }
}
