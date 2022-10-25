using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using jp.megamin.UniEmulator.Runtime.Dll.Infrastructure;
using jp.megamin.UniEmulator.Runtime.Emulator.Domain;
using UnityEngine;

namespace jp.megamin.UniEmulator.Runtime.Emulator.Presentation
{
    public class Emulator : IEmulator, IDisposable
    {
        public DllContainer DllContainer => _dllContainer;
        private DllContainer _dllContainer;

        private readonly string _emulatorName;

        private delegate void FnExec();
        private delegate void FnStopEmulation();
        private delegate void FnEmulKeyUp(int keyCode);
        private delegate void FnEmulKeyDown(int keyCode);
        private delegate IntPtr FnCheckResident();
        private delegate int FnGetEmulWidth(IntPtr ptr);
        private delegate int FnGetEmulHeight(IntPtr ptr);
        private delegate int FnSetEmulTexturePtr(IntPtr ptr, IntPtr texture);
        private delegate int FnSetEmulPause(IntPtr ptr, bool fPause);
        private delegate IntPtr FnGetRenderEventFunc();
        private delegate IntPtr FnEmuSendAudio(float[] data, int sz, int ch);
        private delegate IntPtr FnEmuOpenFloppyDisk(int drv, string filePath, int bank);

        private CancellationTokenSource _cancellationTokenSource;
        private IntPtr _emulator;
        private int width;
        private int height;
        public Texture2D Texture2D => _texture2D;
        private Texture2D _texture2D;
        public Emulator(string emulatorName)
        {
            _emulatorName = emulatorName;
            var emulatorPath = Path.Combine(Directory.GetCurrentDirectory(), $"{emulatorName}.dll");
            Debug.Log($"load {emulatorPath}");
            _dllContainer = new DllContainer(emulatorPath);
            _emulator = CheckResident();
            Debug.Log($"_emulator = {_emulator}");
            var width = GetEmulWidth(_emulator);
            var height = GetEmulHeight(_emulator);
            Debug.Log($"screen size = {width}, {height}");
            _texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            SetEmulTexturePtr(_emulator, _texture2D.GetNativeTexturePtr());
            SetEmulPause(_emulator, false);
            Exec();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void OnDisable() {
            Debug.Log("***OnDisable");
            if (_emulator == IntPtr.Zero) return;

            Debug.Log("**Disable:Pause On");
            SetEmulPause(_emulator, true);
        }

        public void OnEnable() {
            Debug.Log($"{System.IO.Directory.GetCurrentDirectory()}");
            Debug.Log($"{Application.dataPath}");
            Debug.Log("***OnEnable");
            if (_emulator == IntPtr.Zero) return;

            Debug.Log("**Enable:Pause Off");
            SetEmulPause(_emulator, false);
        }

        public void Exec() => _dllContainer?.GetDelegate<FnExec>("Exec")?.Invoke();
        public void StopEmulation() => _dllContainer?.GetDelegate<FnStopEmulation>("StopEmulation")?.Invoke();
        public void EmulKeyUp(int keyCode) => _dllContainer?.GetDelegate<FnEmulKeyUp>("EmulKeyUp")?.Invoke(keyCode);
        public void EmulKeyDown(int keyCode) => _dllContainer?.GetDelegate<FnEmulKeyDown>("EmulKeyDown")?.Invoke(keyCode);
        public IntPtr CheckResident() => (IntPtr)_dllContainer?.GetDelegate<FnCheckResident>("CheckResident")?.Invoke();
        public int GetEmulWidth(IntPtr ptr) => (int)_dllContainer?.GetDelegate<FnGetEmulWidth>("GetEmulWidth")?.Invoke(ptr);
        public int GetEmulHeight(IntPtr ptr) => (int)_dllContainer?.GetDelegate<FnGetEmulHeight>("GetEmulHeight")?.Invoke(ptr);
        public void SetEmulTexturePtr(IntPtr ptr, IntPtr texture) => _dllContainer?.GetDelegate<FnSetEmulTexturePtr>("SetEmulTexturePtr")?.Invoke(ptr, texture);
        public void SetEmulPause(IntPtr ptr, bool fPause) => _dllContainer?.GetDelegate<FnSetEmulPause>("SetEmulPause")?.Invoke(ptr, fPause);
        public IntPtr GetRenderEventFunc() => (IntPtr)_dllContainer?.GetDelegate<FnGetRenderEventFunc>("GetRenderEventFunc")?.Invoke();
        public void EmuSendAudio(float[] data, int sz, int ch) => _dllContainer?.GetDelegate<FnEmuSendAudio>("EmuSendAudio")?.Invoke(data, sz, ch);
        public void EmuOpenFloppyDisk(int drv, string filePath, int bank) => _dllContainer?.GetDelegate<FnEmuOpenFloppyDisk>("EmuOpenFloppyDisk")?.Invoke(drv, filePath, bank);

        public void Dispose()
        {
            Debug.Log("***Dispose");
            StopEmulation();
            _cancellationTokenSource.Dispose();
            Debug.Log($"dispose {_emulatorName}");
            _dllContainer?.Dispose();
            _dllContainer = null;
        }
    }
}
