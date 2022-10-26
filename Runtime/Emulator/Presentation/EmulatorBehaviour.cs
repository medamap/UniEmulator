using System;
using System.IO;
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

        private bool IsPressShift => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); 
        
        private int GetKeyCode(KeyCode keyCode) {
            switch (keyCode) {
                // 0x100: shift
                // 0x200: kana
                // 0x400: alphabet
                // 0x800: ALPHABET
                case KeyCode.LeftShift: case KeyCode.RightShift: return 0x10; // Shift
                case KeyCode.Backspace: return 0x08; // BS
                case KeyCode.Tab: return 0x09; // Tab
                case KeyCode.Return: return 0x0d; // Enter
                case KeyCode.Escape: return 0x1b; // Escape
                case KeyCode.Space: // ' '
                case KeyCode.Exclaim: return 0x21; // '!'
                case KeyCode.DoubleQuote: return 0x22; // '"'
                case KeyCode.Hash: return 0x23; // '#'
                case KeyCode.Dollar: return 0x24; // '$'
                case KeyCode.Percent: return 0x25; // '%'
                case KeyCode.Ampersand: return 0x26; // '&'
                case KeyCode.Quote: return 0x27; // '''
                case KeyCode.LeftParen: return 0x28; // '('
                case KeyCode.RightParen: return 0x29; // ')'
                case KeyCode.Asterisk: case KeyCode.KeypadMultiply: return 0x2a; // '*'
                case KeyCode.Plus: case KeyCode.KeypadPlus: return 0x2b; // '+'
                case KeyCode.Comma: return 0x2c; // ','
                case KeyCode.Minus: case KeyCode.KeypadMinus: return 0x2d; // '-'
                case KeyCode.Period: case KeyCode.KeypadPeriod: return 0x2e; // '.'
                case KeyCode.Slash: case KeyCode.KeypadDivide: return 0x2f; // '/'
                case KeyCode.Alpha0: case KeyCode.Keypad0: return 0x30; // '0'
                case KeyCode.Alpha1: case KeyCode.Keypad1: return 0x31; // '1'
                case KeyCode.Alpha2: case KeyCode.Keypad2: return 0x32; // '2'
                case KeyCode.Alpha3: case KeyCode.Keypad3: return 0x33; // '3'
                case KeyCode.Alpha4: case KeyCode.Keypad4: return 0x34; // '4'
                case KeyCode.Alpha5: case KeyCode.Keypad5: return 0x35; // '5'
                case KeyCode.Alpha6: case KeyCode.Keypad6: return 0x36; // '6'
                case KeyCode.Alpha7: case KeyCode.Keypad7: return 0x37; // '7'
                case KeyCode.Alpha8: case KeyCode.Keypad8: return 0x38; // '8'
                case KeyCode.Alpha9: case KeyCode.Keypad9: return 0x39; // '9'
                case KeyCode.Colon: return 0x3a; // ':'
                case KeyCode.Semicolon: return 0x3b; // ';'
                case KeyCode.Less: return 0x3c; // '<'
                case KeyCode.Equals: return 0x3b; // '='
                case KeyCode.Greater: return 0x3e; // '>'
                case KeyCode.Question: return 0x3f; // '?'
                case KeyCode.At: return 0x40; // '@'
                case KeyCode.A: return 0x41; // 'A' 'a'
                case KeyCode.B: return 0x42; // 'B' 'b'
                case KeyCode.C: return 0x43; // 'C' 'c'
                case KeyCode.D: return 0x44; // 'D' 'd'
                case KeyCode.E: return 0x45; // 'E' 'e'
                case KeyCode.F: return 0x46; // 'F' 'f'
                case KeyCode.G: return 0x47; // 'G' 'g'
                case KeyCode.H: return 0x48; // 'H' 'h'
                case KeyCode.I: return 0x49; // 'I' 'i'
                case KeyCode.J: return 0x4a; // 'J' 'j'
                case KeyCode.K: return 0x4b; // 'K' 'k'
                case KeyCode.L: return 0x4c; // 'L' 'l'
                case KeyCode.M: return 0x4d; // 'M' 'm'
                case KeyCode.N: return 0x4e; // 'N' 'n'
                case KeyCode.O: return 0x4f; // 'O' 'o'
                case KeyCode.P: return 0x50; // 'P' 'p'
                case KeyCode.Q: return 0x51; // 'Q' 'q'
                case KeyCode.R: return 0x52; // 'R' 'r'
                case KeyCode.S: return 0x53; // 'S' 's'
                case KeyCode.T: return 0x54; // 'T' 't'
                case KeyCode.U: return 0x55; // 'U' 'u'
                case KeyCode.V: return 0x56; // 'V' 'v'
                case KeyCode.W: return 0x57; // 'W' 'w'
                case KeyCode.X: return 0x58; // 'X' 'x'
                case KeyCode.Y: return 0x59; // 'Y' 'y'
                case KeyCode.Z: return 0x5a; // 'Z' 'z'
                case KeyCode.LeftBracket: return 0x5b; // '['
                case KeyCode.Backslash: return 0x5c; // '\'
                case KeyCode.RightBracket: return 0x5d; // ']'
                case KeyCode.Caret: return 0x5e; // '^'
                case KeyCode.Underscore: return 0x5f; // '_'
                case KeyCode.BackQuote: return 0x60; // '`'
                case KeyCode.LeftCurlyBracket: return 0x7b; // '{'
                case KeyCode.Pipe: return 0x7c; // '|'
                case KeyCode.RightCurlyBracket : return 0x7d; // '}'
                case KeyCode.Tilde: return 0x7e; // '~'
                default: return 0x0;
            }
        }

        private void OnEnable() => Emulator?.OnEnable();
        private void OnDisable() => Emulator?.OnDisable();
        private void OnDestroy() => Emulator?.Dispose();
    }
}