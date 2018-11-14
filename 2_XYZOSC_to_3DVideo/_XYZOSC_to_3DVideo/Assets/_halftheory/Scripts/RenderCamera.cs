using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace _halftheory {
    public class RenderCamera {
        public bool initialized = false;

        public int index;
        public int width;
        public int height;
        public int rtWidth;
        public string pngFile;
        public string movFile;
        public Camera[] cameras;
        private float[] aspect_backup;
        private RenderTexture renderTexture;
        private RenderTexture renderTexture_backup;
        private Texture2D textureFull;

        public RenderCamera(int i, string name) {
            if (!MainSettingsVars.renderCamerasNames.ContainsKey(i)) {
                return;
            }
            if (!MainSettingsVars.renderCamerasComponents.ContainsKey(i)) {
                return;
            }
            if (!MainSettingsVars.data.renderCamerasActive[i]) {
                return;
            }
            if (MainSettingsVars.data.renderCamerasResolution[i].x == 0.0f && MainSettingsVars.data.renderCamerasResolution[i].y == 0.0f) {
                return;
            }
            index = i;
            width = (int)((float)MainSettingsVars.data.renderCamerasResolution[i].x);
            height = (int)((float)MainSettingsVars.data.renderCamerasResolution[i].y);
            name = Regex.Replace(name, "[^a-z0-9_-]+", "_", RegexOptions.IgnoreCase);
            string fileSuffix = Regex.Replace(MainSettingsVars.renderCamerasNames[i], "[^a-z0-9_-]+", "_", RegexOptions.IgnoreCase);
            pngFile = name+"_"+fileSuffix;
            movFile = Path.Combine(MainSettingsVars.recordFolder, name+"_"+fileSuffix+".mov");
            cameras = MainSettingsVars.renderCamerasComponents[i];

            initialized = true;
        }
        public void Setup() {
            // cameras
            aspect_backup = new float[cameras.Length];
            for (int i=0; i < cameras.Length; i++) {
                aspect_backup[i] = cameras[i].aspect;
                cameras[i].aspect = (float)width / (float)height;
                cameras[i].backgroundColor = Color.clear;
            }
            // RenderTexture
            rtWidth = (int)((float)width / (float)cameras.Length);
            renderTexture = new RenderTexture(rtWidth, height, 32, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
            renderTexture.antiAliasing = MainSettingsVars.data.renderAntiAliasing;
            renderTexture.wrapMode = TextureWrapMode.Clamp;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.Create();
        }
        public Texture2D GetTexture() {
            ResetTextures();
            textureFull = new Texture2D((rtWidth * cameras.Length), height, TextureFormat.ARGB32, false);
            for (int i=0; i < cameras.Length; i++) {
                // backup component + object states
                bool activeSelf_backup = cameras[i].gameObject.activeSelf;
                cameras[i].gameObject.SetActive(true);
                bool enabled_backup = cameras[i].enabled;
                cameras[i].enabled = true;
                Rect rect_backup = cameras[i].rect;
                cameras[i].rect = new Rect(0, 0, 1.0f, 1.0f);
                // set camera to use RenderTexture
                renderTexture_backup = RenderTexture.active;
                cameras[i].targetTexture = renderTexture;
                cameras[i].Render();
                RenderTexture.active = renderTexture;
                // read the texture
                textureFull.ReadPixels(new Rect(0, 0, rtWidth, height), (rtWidth * i), 0);
                // reset camera
                cameras[i].targetTexture = null;
                RenderTexture.active = renderTexture_backup;
                // reset component + object states
                cameras[i].gameObject.SetActive(activeSelf_backup);
                cameras[i].enabled = enabled_backup;
                cameras[i].rect = rect_backup;
            }
            return textureFull;
        }
        void ResetTextures() {
            if (textureFull != null) {
                UnityEngine.Object.Destroy(textureFull);
                textureFull = null;
            }
        }
        public void Reset() {
            ResetTextures();
            // cameras
            for (int i=0; i < cameras.Length; i++) {
                cameras[i].aspect = aspect_backup[i];
                cameras[i].backgroundColor = Color.black;
            }
            // RenderTexture
            UnityEngine.Object.Destroy(renderTexture);
            renderTexture = null;
        }

    }
}