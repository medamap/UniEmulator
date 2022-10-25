﻿using System.Collections;
using UnityEngine;

namespace jp.megamin.UniEmulator.Runtime.Emulator.Presentation
{
    public class X1Turbo : EmulatorBehaviour
    {
        protected override void OnPreInitialize()
        {
            EmulatorName = "x1turbo";
        }

        protected override void OnPostInitialize()
        {
            GetComponent<Renderer>().material.mainTexture = Emulator.Texture2D;
            StartCoroutine(OnRender());
        }
        
        IEnumerator OnRender() {
            for (; ; ) {
                yield return new WaitForEndOfFrame();
                GL.IssuePluginEvent(Emulator.GetRenderEventFunc(), 0);
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (Emulator == null) return;
            Emulator.EmuSendAudio(data, data.Length, channels);
            audioClip.SetData(data, 0);
        }

    }
}
