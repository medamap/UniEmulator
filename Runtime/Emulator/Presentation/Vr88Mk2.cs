using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Vr88Mk2 : MonoBehaviour
{
    [DllImport("pc8801mk2")] private static extern IntPtr Init();
    [DllImport("pc8801mk2")] private static extern void Exec();
    [DllImport("pc8801mk2")] private static extern void StopEmulation();
    [DllImport("pc8801mk2")] private static extern void EmulKeyUp(int keyCode);
    [DllImport("pc8801mk2")] private static extern void EmulKeyDown(int keyCode);
    [DllImport("pc8801mk2")] private static extern IntPtr CheckResident();
    [DllImport("pc8801mk2")] private static extern int GetEmulWidth(IntPtr ptr);
    [DllImport("pc8801mk2")] private static extern int GetEmulHeight(IntPtr ptr);
    [DllImport("pc8801mk2")] private static extern void SetEmulTexturePtr(IntPtr ptr, IntPtr texture);
    [DllImport("pc8801mk2")] private static extern void SetEmulPause(IntPtr ptr, bool fPause);
    [DllImport("pc8801mk2")] private static extern IntPtr GetRenderEventFunc();
    [DllImport("pc8801mk2")] private static extern void EmuSendAudio(float[] data, int sz, int ch);
    [DllImport("pc8801mk2")] private static extern void EmuOpenFloppyDisk(int drv, string filePath, int bank);

    private IntPtr _emul = IntPtr.Zero;

    [SerializeField] private AudioClip audioClip;

    void Start() {
        Debug.Log("***Start");

        audioClip = GetComponent<AudioSource>().clip = AudioClip.Create("emulator", 4096, 2, 48000, true);

        _emul = CheckResident();

        EmuOpenFloppyDisk(0, ".\\DriveA.D88", 0);
        EmuOpenFloppyDisk(1, ".\\DriveB.D88", 0);

        var tex = new Texture2D(
            GetEmulWidth(_emul),
            GetEmulHeight(_emul),
            TextureFormat.ARGB32,
            false);
        GetComponent<Renderer>().material.mainTexture = tex;

        SetEmulTexturePtr(_emul, tex.GetNativeTexturePtr());

        SetEmulPause(_emul, false);
        Exec();
        StartCoroutine(OnRender());

    }

    void OnDisable() {
        Debug.Log("***OnDisable");
        if (_emul == IntPtr.Zero) return;

        Debug.Log("**Disable:Pause On");       // エディタでの停止
        SetEmulPause(_emul, true);
    }

    void OnEnable() {
        Debug.Log($"{System.IO.Directory.GetCurrentDirectory()}");
        Debug.Log($"{Application.dataPath}");
        Debug.Log("***OnEnable");
        if (_emul == IntPtr.Zero) return;

        Debug.Log("**Enable:Pause Off");       // エディタでの再開
        SetEmulPause(_emul, false);
    }

    private void OnAudioFilterRead(float[] data, int channels) {
        Debug.Log($"{data.Length} {channels}");
        EmuSendAudio(data, data.Length, channels);
        audioClip.SetData(data, 0);
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

    private void Update() {
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
            if (Input.GetKeyDown(keyCode)) {
                Debug.Log($"{keyCode} down");
                EmulKeyDown(GetKeyCode(keyCode));
            }

            if (Input.GetKeyUp(keyCode)) {
                Debug.Log($"{keyCode} up");
                EmulKeyUp(GetKeyCode(keyCode));
            }
        }
    }

    private void OnDestroy() {
        Debug.Log("***OnDestroy");
        StopEmulation();
    }

    IEnumerator OnRender() {
        for (; ; ) {
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(GetRenderEventFunc(), 0);
        }
    }
}
