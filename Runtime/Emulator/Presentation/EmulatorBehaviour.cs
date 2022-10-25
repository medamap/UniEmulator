using System;
using System.Collections;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace jp.megamin.UniEmulator.Runtime.Emulator.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class EmulatorBehaviour : MonoBehaviour
    {
        protected Emulator Emulator;
        [SerializeField] protected AudioClip audioClip;

        private string CurrentDirectory => Directory.GetCurrentDirectory();

        public string EmulatorName { get; set; } = null;

        private void CopyAssets()
        {
            var sourceDirectory = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, $"Machines/{EmulatorName}"));
            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                if (Path.GetExtension(file).EndsWith("meta")) continue;
                var destination = Path.Combine(CurrentDirectory, Path.GetFileName(file));
                if (File.Exists(destination)) continue;
                File.Copy(file, destination);
            }
        }

        private async void Start()
        {
            PreInitialize();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroy());

            if (string.IsNullOrEmpty(EmulatorName))
            {
                throw new Exception("EmulatorName is empty, please set this.EmulatorName.");
            }
            CopyAssets();
            
            await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroy());

            audioClip = GetComponent<AudioSource>().clip = AudioClip.Create("emulator", 4096, 2, 80000, true);
            var driveA = Path.Combine(CurrentDirectory, $"{EmulatorName}A.D88");
            var driveB = Path.Combine(CurrentDirectory, $"{EmulatorName}B.D88");

            Emulator = new Emulator(EmulatorName);
            if (File.Exists(driveA))
            {
                Emulator.EmuOpenFloppyDisk(0, driveA, 0);
            }
            if (File.Exists(driveB))
            {
                Emulator.EmuOpenFloppyDisk(1, driveB, 0);
            }
            PostInitialize();
        }

        private void PreInitialize() => OnPreInitialize();
        protected abstract void OnPreInitialize();
        private void PostInitialize() => OnPostInitialize();
        protected abstract void OnPostInitialize();

        private void Update() {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(keyCode)) {
                    Debug.Log($"{keyCode} down");
                    Emulator.EmulKeyDown(GetKeyCode(keyCode));
                }

                if (Input.GetKeyUp(keyCode)) {
                    Debug.Log($"{keyCode} up");
                    Emulator.EmulKeyUp(GetKeyCode(keyCode));
                }
            }
        }

        private int GetKeyCode(KeyCode keyCode) {
            switch (keyCode) {
                case KeyCode.A: return 0x41;
                case KeyCode.B: return 0x42;
                case KeyCode.C: return 0x43;
                case KeyCode.D: return 0x44;
                case KeyCode.E: return 0x45;
                case KeyCode.F: return 0x46;
                case KeyCode.Alpha0: case KeyCode.Keypad0: return 0x30;
                case KeyCode.Alpha1: case KeyCode.Keypad1: return 0x31;
                case KeyCode.Alpha2: case KeyCode.Keypad2: return 0x32;
                case KeyCode.Alpha3: case KeyCode.Keypad3: return 0x33;
                case KeyCode.Alpha4: case KeyCode.Keypad4: return 0x34;
                case KeyCode.Alpha5: case KeyCode.Keypad5: return 0x35;
                case KeyCode.Alpha6: case KeyCode.Keypad6: return 0x36;
                case KeyCode.Alpha7: case KeyCode.Keypad7: return 0x37;
                case KeyCode.Alpha8: case KeyCode.Keypad8: return 0x38;
                case KeyCode.Alpha9: case KeyCode.Keypad9: return 0x39;
                default: return 0x0;
            }
        }

        private void OnEnable() => Emulator?.OnEnable();
        private void OnDisable() => Emulator?.OnDisable();
        private void OnDestroy() => Emulator?.Dispose();
    }
}