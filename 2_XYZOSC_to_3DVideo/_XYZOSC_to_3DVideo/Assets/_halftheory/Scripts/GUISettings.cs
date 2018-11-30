using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace _halftheory {
	public class GUISettings : MonoBehaviour {

        private bool initialized = false;

        public static int guiWindowWidth = 840;
        public static int guiWindowHeight = 200;
        public static int guiWindowMargin = 20;
        public static Rect guiWindowRect = new Rect(guiWindowMargin, guiWindowMargin, guiWindowWidth, guiWindowHeight);
        private bool hasScroll = false;
        public static Vector2 scrollPosition;
        private GUIStyle guiStyleLabelCenter, guiStyleLabelRight, guiStyleButtonLeft, guiStyleButtonCenter;
        public bool guiSaving = false;

        // textfields
        public static string[] renderCamerasResolution_x = new string[MainSettingsVars.renderCamerasNames.Count];
        public static string[] renderCamerasResolution_y = new string[MainSettingsVars.renderCamerasNames.Count];
        public static string guiStringAnimationNew;
        // toggle
        private bool guiToggleMain, guiToggleAnimation, guiToggleMesh, guiToggleQuit;
        private bool[] guiToggleMeshTopology = new bool[MainSettingsVars.groupsLength];
        private bool[] guiToggleMeshColor = new bool[MainSettingsVars.groupsLength];
        private bool[] guiToggleMeshShader = new bool[MainSettingsVars.groupsLength];
        // grid        
        public string[] guiGridGameMode, guiGridAntiAliasing, guiGridCodec, guiGridAnimations, guiGridMeshTopology, guiGridMeshColor, guiGridMeshShader;
        public string[] guiGridDataArr = {
            "Copy Runtime Data to Editor",
            "Copy Editor Data to Runtime"
        };
        // grid ints
        public static int guiGridAntiAliasingInt, guiGridAntiAliasingIntNew, guiGridCodecInt, guiGridCodecIntNew, guiGridDataInt, guiGridGameModeInt, guiGridGameModeIntNew, guiGridAnimationSelectInt, guiGridAnimationDeleteInt;
        public int[] guiGridMeshTopologyInt = new int[MainSettingsVars.groupsLength];
        public int[] guiGridMeshTopologyIntNew = new int[MainSettingsVars.groupsLength];
        public int[] guiGridMeshColorInt = new int[MainSettingsVars.groupsLength];
        public int[] guiGridMeshColorIntNew = new int[MainSettingsVars.groupsLength];
        public int[] guiGridMeshShaderInt = new int[MainSettingsVars.groupsLength];
        public int[] guiGridMeshShaderIntNew = new int[MainSettingsVars.groupsLength];

        private bool guiLabel(bool myBool, string label, string current = "", bool meshStyle = false) {
            if (myBool) {
                label = label+" -";
            }
            else {
                label = label+" +";
            }
            if (string.IsNullOrEmpty(current)) {
                myBool = GUILayout.Toggle(myBool, label, guiStyleButtonLeft);
            }
            else {
                GUILayout.BeginHorizontal();
                myBool = GUILayout.Toggle(myBool, label, guiStyleButtonLeft);
                int labelWidth = 100;
                if (meshStyle) {
                    labelWidth = 80;
                }
                GUILayout.Label(current, GUILayout.Width(labelWidth));
                GUILayout.EndHorizontal();
            }
            return myBool;
        }

        private int guiTextFieldId = 0;
        public static Dictionary<string,string> guiTextFields = new Dictionary<string,string>(); // key = id+label
        private Dictionary<string,dynamic> guiTextFieldsValues = new Dictionary<string,dynamic>(); // key = id+label

        private dynamic guiField(dynamic value, string label, dynamic valueDefault = null, dynamic min = null, dynamic max = null, bool meshStyle = false) {
            GUILayout.BeginVertical("box");
            if (value.GetType() == typeof(bool)) {
                if (!meshStyle) {
                    value = GUILayout.Toggle(value, label, guiStyleButtonCenter);
                }
                else {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label);
                    string labelToggle = "off";
                    if (value) {
                        labelToggle = "on";
                    }
                    value = GUILayout.Toggle(value, labelToggle, guiStyleButtonCenter, GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                }
            }
            else if (value.GetType() == typeof(int)) {
                if (valueDefault == null) {
                    valueDefault = 0;
                }
                else {
                    valueDefault = (int)valueDefault;
                }
                if (min == null) {
                    min = 0;
                }
                else {
                    min = (int)min;
                }
                if (max == null) {
                    max = 100;
                }
                else {
                    max = (int)max;
                }
                GUILayout.BeginHorizontal();
                string labelNew = label+" (default "+valueDefault.ToString()+")";
                if (meshStyle) {
                    labelNew = label;
                }
                GUILayout.Label(labelNew);
                int valueWidth = 50;
                if (meshStyle) {
                    valueWidth = 30;
                }
                // TextField
                if (!guiTextFields.ContainsKey(guiTextFieldId+label)) {
                    guiTextFields.Add(guiTextFieldId+label, value.ToString());
                    guiTextFieldsValues.Add(guiTextFieldId+label, value);
                }
                guiTextFields[guiTextFieldId+label] = Regex.Replace(guiTextFields[guiTextFieldId+label], "[^0-9-+]+", "", RegexOptions.IgnoreCase);
                string valueString = GUILayout.TextField(guiTextFields[guiTextFieldId+label], 10, GUILayout.Width(valueWidth));
                if (guiTextFields[guiTextFieldId+label] != valueString) {
                    guiTextFields[guiTextFieldId+label] = valueString;
                    try {
                        int valueNew = int.Parse(valueString);
                        if (valueNew <= max && valueNew >= min) {
                            value = valueNew;
                            guiTextFieldsValues[guiTextFieldId+label] = value;
                        }
                    }
                    catch (Exception) { }
                }
                else if (guiTextFieldsValues[guiTextFieldId+label] != value) {
                    guiTextFieldsValues[guiTextFieldId+label] = value;
                    guiTextFields[guiTextFieldId+label] = value.ToString();
                }
                guiTextFieldId++;
                if (GUILayout.Button("reset", GUILayout.Width(50))) {
                    value = valueDefault;
                }
                GUILayout.EndHorizontal();
                value = GUILayout.HorizontalSlider((float)value, (float)min, (float)max);
                value = (int)value;
            }
            else if (value.GetType() == typeof(float)) {
                if (valueDefault == null) {
                    valueDefault = 0.0f;
                }
                else {
                    valueDefault = (float)valueDefault;
                }
                if (min == null) {
                    min = 0.0f;
                }
                else {
                    min = (float)min;
                }
                if (max == null) {
                    max = 1.0f;
                }
                else {
                    max = (float)max;
                }
                GUILayout.BeginHorizontal();
                string labelNew = label+" (default "+valueDefault.ToString()+")";
                if (meshStyle) {
                    labelNew = label;
                }
                GUILayout.Label(labelNew);
                int valueWidth = 50;
                if (meshStyle) {
                    valueWidth = 30;
                }
                // TextField
                if (!guiTextFields.ContainsKey(guiTextFieldId+label)) {
                    guiTextFields.Add(guiTextFieldId+label, value.ToString());
                    guiTextFieldsValues.Add(guiTextFieldId+label, value);
                }
                guiTextFields[guiTextFieldId+label] = Regex.Replace(guiTextFields[guiTextFieldId+label], "[^0-9-+\\.]+", "", RegexOptions.IgnoreCase);
                string valueString = GUILayout.TextField(guiTextFields[guiTextFieldId+label], 10, GUILayout.Width(valueWidth));
                if (guiTextFields[guiTextFieldId+label] != valueString) {
                    guiTextFields[guiTextFieldId+label] = valueString;
                    try {
                        float valueNew = float.Parse(valueString);
                        if (valueNew <= max && valueNew >= min) {
                            value = valueNew;
                            guiTextFieldsValues[guiTextFieldId+label] = value;
                        }
                    }
                    catch (Exception) { }
                }
                else if (guiTextFieldsValues[guiTextFieldId+label] != value) {
                    guiTextFieldsValues[guiTextFieldId+label] = value;
                    guiTextFields[guiTextFieldId+label] = value.ToString();
                }
                guiTextFieldId++;
                if (GUILayout.Button("reset", GUILayout.Width(50))) {
                    value = valueDefault;
                }
                GUILayout.EndHorizontal();
                value = GUILayout.HorizontalSlider(value, min, max);
                value = (float)value;
            }
            GUILayout.EndVertical();
            return value;
        }

        void OnGUI() {
            if (!MainSettingsVars.initialized) {
                return;
            }
            if (!initialized) {
                // styles
                GUI.backgroundColor = Color.white;
                if (guiStyleLabelCenter == null) {
                    guiStyleLabelCenter = new GUIStyle(GUI.skin.GetStyle("label"));
                    guiStyleLabelCenter.alignment = TextAnchor.MiddleCenter;
                }
                if (guiStyleLabelRight == null) {
                    guiStyleLabelRight = new GUIStyle(GUI.skin.GetStyle("label"));
                    guiStyleLabelRight.alignment = TextAnchor.MiddleRight;
                }
                if (guiStyleButtonLeft == null) {
                    guiStyleButtonLeft = new GUIStyle(GUI.skin.GetStyle("button"));
                    guiStyleButtonLeft.alignment = TextAnchor.MiddleLeft;
                }
                if (guiStyleButtonCenter == null) {
                    guiStyleButtonCenter = new GUIStyle(GUI.skin.GetStyle("button"));
                    guiStyleButtonCenter.alignment = TextAnchor.MiddleCenter;
                }
                // grids
                if (guiGridGameMode.Length == 0) {
                    guiGridGameMode = System.Enum.GetNames(typeof(gameMode));
                    for (int i=0; i < guiGridGameMode.Length; i++) {
                        guiGridGameMode[i] = guiGridGameMode[i].First().ToString().ToUpper() + guiGridGameMode[i].Substring(1).Replace("_", " ");
                    }
                }
                if (guiGridAntiAliasing.Length == 0) {
                    guiGridAntiAliasing = MainSettingsVars.renderAntiAliasing.Values.ToArray();
                }
                if (guiGridCodec.Length == 0) {
                    guiGridCodec = MainSettingsVars.renderCodecs.Keys.ToArray();
                }
                if (guiGridMeshTopology.Length == 0) {
                    guiGridMeshTopology = System.Enum.GetNames(typeof(meshTopology));
                }
                if (guiGridMeshColor.Length == 0) {
                    guiGridMeshColor = System.Enum.GetNames(typeof(meshColor));
                }
                if (guiGridMeshShader.Length == 0) {
                    guiGridMeshShader = System.Enum.GetNames(typeof(meshShader));
                }

                guiGridDataInt = -1;
                guiGridGameModeInt = (int)MainSettingsVars.data.currentGameMode;
                guiGridGameModeIntNew = guiGridGameModeInt;

                int antiAliasing = 1;
                if (MainSettingsVars.renderAntiAliasing.ContainsKey(MainSettingsVars.data.renderAntiAliasing)) {
                    antiAliasing = MainSettingsVars.data.renderAntiAliasing;
                }
                else if (MainSettingsVars.renderAntiAliasing.ContainsKey(QualitySettings.antiAliasing)) {
                     antiAliasing = QualitySettings.antiAliasing;
                     MainSettingsVars.data.renderAntiAliasing = antiAliasing;
                }
                else {
                    MainSettingsVars.data.renderAntiAliasing = antiAliasing;
                }
                guiGridAntiAliasingInt = Array.IndexOf(guiGridAntiAliasing, MainSettingsVars.renderAntiAliasing[antiAliasing]);
                guiGridAntiAliasingIntNew = guiGridAntiAliasingInt;

                guiGridCodecInt = Array.IndexOf(guiGridCodec, MainSettingsVars.data.renderCodec);
                guiGridCodecIntNew = guiGridCodecInt;

                setRenderCamerasResolution();
                setAnimationGui();
                setMeshGui();

                initialized = true;
            }

            guiGameMode();

            if (!MainSettingsVars.data.gui_enabled) {
                return;
            }

            if (!guiToggleMain && !guiToggleAnimation && !guiToggleMesh) {
                guiWindowRect.width = guiWindowWidth;
                guiWindowRect.height = guiWindowHeight;
                hasScroll = false;
            }
            if (guiWindowRect.height > Screen.height - (guiWindowMargin * 2)) {
                hasScroll = true;
            }

            guiWindowRect = GUILayout.Window(0, guiWindowRect, guiWindowFunc, "MENU", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MaxWidth(Screen.width - (guiWindowMargin * 2)), GUILayout.MaxHeight(Screen.height - (guiWindowMargin * 2)));
        }

        void guiGameMode() {
            int width = 160;
            GUILayout.BeginArea(new Rect(Screen.width - width - (guiWindowMargin * 2) - 5, guiWindowMargin, width, guiWindowMargin * 5));
            GUILayout.BeginHorizontal();
            if (guiSaving) {
                GUILayout.Label("Saving", guiStyleLabelRight);
            }
            else if (MainSettingsVars.currentAnimationComponent != null) {
                string currentGameMode = MainSettingsVars.data.currentGameMode.ToString();
                currentGameMode = currentGameMode.First().ToString().ToUpper() + currentGameMode.Substring(1).Replace("_", " ");
                GUILayout.Label(currentGameMode, guiStyleLabelRight);
                string labelToggle = "off";
                if (MainSettingsVars.currentAnimationComponent.active) {
                    labelToggle = "on";
                }
                MainSettingsVars.currentAnimationComponent.active = GUILayout.Toggle(MainSettingsVars.currentAnimationComponent.active, labelToggle, guiStyleButtonCenter, GUILayout.Width(50));
            }
            GUILayout.EndHorizontal();
            // render bar
            if (MainSettingsVars.currentAnimationRender) {
                GUILayout.BeginHorizontal("box");
                float frameNumber = (float)MainSettingsVars.currentAnimationComponent.frameNumber;
                GUILayout.HorizontalSlider(frameNumber, 0.0f, MainSettingsVars.currentAnimationComponent.totalFrames);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(Screen.width - (guiWindowMargin * 2), guiWindowMargin, guiWindowMargin, guiWindowMargin * 5));
            guiToggleQuit = GUILayout.Toggle(guiToggleQuit, "x", guiStyleButtonCenter, GUILayout.Width(20));
            if (guiToggleQuit && !MainSettingsVars.mainsettingsComponent.quitStarted) {
                guiSaving = true;
                StartCoroutine(MainSettingsVars.mainsettingsComponent.Quit());
            }
            GUILayout.EndArea();
        }

        void guiWindowFunc(int id = 0) {
            guiTextFieldId = 0; // must be here - stops working inside OnGUI?
            if (hasScroll) {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            }

            // Select Mode
            GUILayout.BeginVertical("box");
            guiGridGameModeIntNew = GUILayout.SelectionGrid(guiGridGameModeInt, guiGridGameMode, guiGridGameMode.Length);
            if (guiGridGameModeInt != guiGridGameModeIntNew) {
                guiGridGameModeInt = guiGridGameModeIntNew;
                MainSettingsVars.data.currentGameMode = (gameMode)System.Enum.Parse(typeof(gameMode), guiGridGameModeInt.ToString());
            }
            GUILayout.EndVertical();

            // Main Settings
            guiToggleMain = guiLabel(guiToggleMain, "Main Settings");
            if (guiToggleMain) {
                GUILayout.Label("Main Settings");
                MainSettingsVars.data.maxSizeCollections = guiField(MainSettingsVars.data.maxSizeCollections, "Maximum Size of Collections.", MainSettingsVars.defaults["maxSizeCollections"], 2000, 20000);
                MainSettingsVars.data.maxSizeChildren = guiField(MainSettingsVars.data.maxSizeChildren, "Maximum Number of Child Objects.", MainSettingsVars.defaults["maxSizeChildren"], 100, 5000);
                // 3D Settings
                GUILayout.Label("3D Settings");
                MainSettingsVars.data.stereo3d_enabled = guiField(MainSettingsVars.data.stereo3d_enabled, "Enable Side-By-Side");
                MainSettingsVars.data.stereo3d_interaxial = guiField(MainSettingsVars.data.stereo3d_interaxial, "Interaxial (mm) - Distance (in millimeters) between cameras.", MainSettingsVars.defaults["stereo3d_interaxial"], 0.0f, 1000f);
                MainSettingsVars.data.stereo3d_zeroPrlxDist = guiField(MainSettingsVars.data.stereo3d_zeroPrlxDist, "Zero Prlx Dist (M) - Distance (in meters) at which left and right images converge.", MainSettingsVars.defaults["stereo3d_zeroPrlxDist"], 0.1f, 100f);
                MainSettingsVars.data.stereo3d_H_I_T = guiField(MainSettingsVars.data.stereo3d_H_I_T, "H I T - Horizontal Image Transform.", MainSettingsVars.defaults["stereo3d_H_I_T"], -25f, 25f);
                // OSC Settings
                GUILayout.Label("OSC Settings");
                MainSettingsVars.data.osc_inPort = guiField(MainSettingsVars.data.osc_inPort, "Incoming Port.", MainSettingsVars.defaults["osc_inPort"], 1000, 10000);
                // Render Settings
                GUILayout.Label("Render Settings");
                GUILayout.BeginHorizontal();
                    // antiAliasing
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Anti-aliasing", guiStyleLabelCenter);
                    guiGridAntiAliasingIntNew = GUILayout.SelectionGrid(guiGridAntiAliasingInt, guiGridAntiAliasing, 1);
                    if (guiGridAntiAliasingInt != guiGridAntiAliasingIntNew) {
                        guiGridAntiAliasingInt = guiGridAntiAliasingIntNew;
                        foreach (var pair in MainSettingsVars.renderAntiAliasing) {
                            if (pair.Value == guiGridAntiAliasing[guiGridAntiAliasingInt]) {
                                MainSettingsVars.data.renderAntiAliasing = pair.Key;
                                break;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    // codec
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Codec", guiStyleLabelCenter);
                    guiGridCodecIntNew = GUILayout.SelectionGrid(guiGridCodecInt, guiGridCodec, 1);
                    if (guiGridCodecInt != guiGridCodecIntNew) {
                        guiGridCodecInt = guiGridCodecIntNew;
                        MainSettingsVars.data.renderCodec = guiGridCodec[guiGridCodecInt];
                    }
                    GUILayout.EndVertical();
                    // resolution helpers
                    GUILayout.BeginVertical();
                    foreach (var pair in MainSettingsVars.renderResolutions) {
                        if (GUILayout.Button(pair.Key)) {
                            string x = pair.Value.x.ToString();
                            string x2 = (pair.Value.x * 2.0f).ToString();
                            string y = pair.Value.y.ToString();
                            for (int i=0; i < MainSettingsVars.renderCamerasNames.Count; i++) {
                                if (i == 2) {
                                    renderCamerasResolution_x[i] = x2;
                                }
                                else {
                                    renderCamerasResolution_x[i] = x;
                                }
                                renderCamerasResolution_y[i] = y;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                    // cameras
                    foreach (var pair in MainSettingsVars.renderCamerasNames) {
                        GUILayout.BeginVertical("box");
                        GUILayout.Label(pair.Value, guiStyleLabelCenter);
                        MainSettingsVars.data.renderCamerasActive[pair.Key] = guiField(MainSettingsVars.data.renderCamerasActive[pair.Key], "Active");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("");
                        renderCamerasResolution_x[pair.Key] = GUILayout.TextField(renderCamerasResolution_x[pair.Key], 4, GUILayout.Width(50));
                        GUILayout.Label("x", guiStyleLabelCenter, GUILayout.Width(10));
                        renderCamerasResolution_y[pair.Key] = GUILayout.TextField(renderCamerasResolution_y[pair.Key], 4, GUILayout.Width(50));
                        GUILayout.Label("");
                        GUILayout.EndHorizontal();
                        if (GUILayout.Button("confirm")) {
                            MainSettingsVars.data.renderCamerasResolution[pair.Key] = new Vector2(float.Parse(renderCamerasResolution_x[pair.Key]), float.Parse(renderCamerasResolution_y[pair.Key]));
                        }
                        GUILayout.EndVertical();
                    }
                GUILayout.EndHorizontal();
                // Data Settings
                GUILayout.Label("Data Settings");
                GUILayout.BeginVertical("box");
                guiGridDataInt = GUILayout.SelectionGrid(guiGridDataInt, guiGridDataArr, guiGridDataArr.Length);
                if (guiGridDataInt != -1) {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("confirm")) {
                        MainSettingsVars.dataActions(guiGridDataInt);
                        guiGridDataInt = -1;
                    }
                    if (GUILayout.Button("cancel")) {
                        guiGridDataInt = -1;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            // Animation Settings
            guiToggleAnimation = guiLabel(guiToggleAnimation, "Animation Settings");
            if (guiToggleAnimation) {
                // Select Animation
                if (guiGridAnimations.Length > 0) {
                    GUILayout.Label("Select Animation");
                    GUILayout.BeginVertical("box");
                    guiGridAnimationSelectInt = GUILayout.SelectionGrid(guiGridAnimationSelectInt, guiGridAnimations, MainSettingsVars.groupsLength);
                    if (guiGridAnimationSelectInt != -1) {
                        GUILayout.Label("Audio File: "+MainSettingsVars.animationComponents[guiGridAnimationSelectInt].data.audio_file_path);
                        GUILayout.Label("FPS: "+MainSettingsVars.animationComponents[guiGridAnimationSelectInt].data.fps);
                        // Data
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label("Data:", GUILayout.Width(80));
                        bool test = MainSettingsVars.animationComponents[guiGridAnimationSelectInt].hasDataFile();
                        if (test) {
                            GUILayout.Label("File:", GUILayout.Width(30));
                            GUILayout.Label(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].dataFile);
                        }
                        if (GUILayout.Button("delete", GUILayout.Width(50))) {
                            if (test) {
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].dataFile);
                                #if UNITY_EDITOR
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].dataFile+".meta");
                                #endif
                            }
                            MainSettingsVars.animationComponents[guiGridAnimationSelectInt].dataFile = null;
                            MainSettingsVars.animationComponents[guiGridAnimationSelectInt].loadData();
                        }
                        GUILayout.EndHorizontal();
                        // Points
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label("Points:", GUILayout.Width(80));
                        test = MainSettingsVars.animationComponents[guiGridAnimationSelectInt].hasPointsFile();
                        if (test) {
                            GUILayout.Label("File:", GUILayout.Width(30));
                            GUILayout.Label(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].pointsFile);
                        }
                        if (GUILayout.Button("delete", GUILayout.Width(50))) {
                            if (test) {
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].pointsFile);
                                #if UNITY_EDITOR
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].pointsFile+".meta");
                                #endif
                            }
                            MainSettingsVars.animationComponents[guiGridAnimationSelectInt].pointsFile = null;
                            StartCoroutine(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].loadPointsData());
                        }
                        GUILayout.EndHorizontal();

                        // Animation
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label("Animation:", GUILayout.Width(80));
                        test = MainSettingsVars.animationComponents[guiGridAnimationSelectInt].hasAnimationFile();
                        if (test) {
                            GUILayout.Label("File:", GUILayout.Width(30));
                            GUILayout.Label(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].animationFile);
                        }
                        if (GUILayout.Button("delete", GUILayout.Width(50))) {
                            if (test) {
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].animationFile);
                                #if UNITY_EDITOR
                                File.Delete(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].animationFile+".meta");
                                #endif
                            }
                            MainSettingsVars.animationComponents[guiGridAnimationSelectInt].animationFile = null;
                            StartCoroutine(MainSettingsVars.animationComponents[guiGridAnimationSelectInt].loadAnimationData());
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("confirm")) {
                            MainSettingsVars.data.currentAnimation = MainSettingsVars.data.animations[guiGridAnimationSelectInt];
                            setAnimationGui();
                            setMeshGui();
                        }
                        if (GUILayout.Button("cancel")) {
                            guiGridAnimationSelectInt = -1;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
                // New Animation
                GUILayout.Label("New Animation");
                GUILayout.BeginHorizontal("box");
                GUILayout.Label("Name", GUILayout.Width(50));
                if (string.IsNullOrEmpty(guiStringAnimationNew)) {
                    guiStringAnimationNew = MainSettingsVars.getAnimationName();
                }
                guiStringAnimationNew = Regex.Replace(guiStringAnimationNew, "[^a-z0-9_-]+", "_", RegexOptions.IgnoreCase);
                guiStringAnimationNew = GUILayout.TextField(guiStringAnimationNew, 50);
                if (GUILayout.Button("ok", GUILayout.Width(50))) {
                    List<string> animationsList = new List<string>(MainSettingsVars.data.animations);
                    animationsList.Add(guiStringAnimationNew);
                    MainSettingsVars.data.animations = animationsList.ToArray();
                    MainSettingsVars.loadAnimations();
                    MainSettingsVars.data.currentAnimation = guiStringAnimationNew;
                    guiStringAnimationNew = "";
                    setAnimationGui();
                    setMeshGui();
                }
                GUILayout.EndHorizontal();
                // Delete Animation
                if (guiGridAnimations.Length > 0) {
                    GUILayout.Label("Delete Animation");
                    GUILayout.BeginVertical("box");
                    guiGridAnimationDeleteInt = GUILayout.SelectionGrid(guiGridAnimationDeleteInt, guiGridAnimations, MainSettingsVars.groupsLength);
                    if (guiGridAnimationDeleteInt != -1) {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("confirm")) {
                            if (MainSettingsVars.data.currentAnimation == MainSettingsVars.data.animations[guiGridAnimationDeleteInt]) {
                                MainSettingsVars.data.currentAnimation = "";
                            }
                            List<string> animationsList = new List<string>(MainSettingsVars.data.animations);
                            animationsList.RemoveAt(guiGridAnimationDeleteInt);
                            MainSettingsVars.data.animations = animationsList.ToArray();
                            MainSettingsVars.loadAnimations();
                            setAnimationGui();
                            setMeshGui();
                        }
                        if (GUILayout.Button("cancel")) {
                            guiGridAnimationDeleteInt = -1;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }

            // Mesh Settings
            if (MainSettingsVars.currentAnimationComponent != null) {
                guiToggleMesh = guiLabel(guiToggleMesh, "Mesh Settings : "+MainSettingsVars.data.currentAnimation);
                if (guiToggleMesh) {
                    GUILayout.BeginHorizontal();
                    for (int i=0; i < MainSettingsVars.groupsLength; i++) {
                        GUILayout.BeginVertical("box");
                        GUILayout.Label("Group "+(i+1), guiStyleLabelCenter);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].active = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].active, "Active");
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].peaks = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].peaks, "Peaks", MainSettingsVars.defaults["meshPeaks"], 0, MainSettingsVars.pointsLength, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].level = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].level, "Level", MainSettingsVars.defaults["meshLevel"], 0.0f, 1.0f, true);
                        guiToggleMeshTopology[i] = guiLabel(guiToggleMeshTopology[i], "Topology", MainSettingsVars.currentAnimationComponent.meshComponents[i].meshTopologySelect.ToString(), true);
                        if (guiToggleMeshTopology[i]) {
                            guiGridMeshTopologyIntNew[i] = GUILayout.SelectionGrid(guiGridMeshTopologyInt[i], guiGridMeshTopology, 2);
                            if (guiGridMeshTopologyInt[i] != guiGridMeshTopologyIntNew[i]) {
                                guiGridMeshTopologyInt[i] = guiGridMeshTopologyIntNew[i];
                                MainSettingsVars.currentAnimationComponent.meshComponents[i].meshTopologySelect = (meshTopology)System.Enum.Parse(typeof(meshTopology), guiGridMeshTopologyInt[i].ToString());
                                guiToggleMeshTopology[i] = false;
                            }
                        }
                        guiToggleMeshColor[i] = guiLabel(guiToggleMeshColor[i], "Color", MainSettingsVars.currentAnimationComponent.meshComponents[i].meshColorSelect.ToString(), true);
                        if (guiToggleMeshColor[i]) {
                            guiGridMeshColorIntNew[i] = GUILayout.SelectionGrid(guiGridMeshColorInt[i], guiGridMeshColor, 2);
                            if (guiGridMeshColorInt[i] != guiGridMeshColorIntNew[i]) {
                                guiGridMeshColorInt[i] = guiGridMeshColorIntNew[i];
                                MainSettingsVars.currentAnimationComponent.meshComponents[i].meshColorSelect = (meshColor)System.Enum.Parse(typeof(meshColor), guiGridMeshColorInt[i].ToString());
                                guiToggleMeshColor[i] = false;
                            }
                        }
                        guiToggleMeshShader[i] = guiLabel(guiToggleMeshShader[i], "Material", MainSettingsVars.currentAnimationComponent.meshComponents[i].meshShaderSelect.ToString(), true);
                        if (guiToggleMeshShader[i]) {
                            guiGridMeshShaderIntNew[i] = GUILayout.SelectionGrid(guiGridMeshShaderInt[i], guiGridMeshShader, 2);
                            if (guiGridMeshShaderInt[i] != guiGridMeshShaderIntNew[i]) {
                                guiGridMeshShaderInt[i] = guiGridMeshShaderIntNew[i];
                                MainSettingsVars.currentAnimationComponent.meshComponents[i].meshShaderSelect = (meshShader)System.Enum.Parse(typeof(meshShader), guiGridMeshShaderInt[i].ToString());
                                guiToggleMeshShader[i] = false;
                            }
                        }
                        if (guiGridMeshShaderInt[i] == 2) {
                            MainSettingsVars.currentAnimationComponent.meshComponents[i].alpha = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].alpha, "Opacity", MainSettingsVars.defaults["meshAlpha"], 0.0f, 1.0f, true);
                        }
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].randomX = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].randomX, "Random end X", null, null, null, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].randomY = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].randomY, "Random end Y", null, null, null, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].rotateSpeed = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].rotateSpeed, "Rotate", MainSettingsVars.defaults["meshRotateSpeed"], -1.0f, 1.0f, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].smoothTime = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].smoothTime, "Smooth time", MainSettingsVars.defaults["meshSmoothTime"], 0.0f, 0.5f, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].clearTime = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].clearTime, "Clear time", MainSettingsVars.defaults["meshClearTime"], 0.0f, 30.0f, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].noClearTime = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].noClearTime, "No clear time", null, null, null, true);
                        MainSettingsVars.currentAnimationComponent.meshComponents[i].traceTime = guiField(MainSettingsVars.currentAnimationComponent.meshComponents[i].traceTime, "Trace time", MainSettingsVars.defaults["meshTraceTime"], 0.0f, 2.0f, true);
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (hasScroll) {
                GUILayout.EndScrollView();
            }
        }

        public void setRenderCamerasResolution() {
            for (int i=0; i < MainSettingsVars.data.renderCamerasResolution.Length; i++) {
                renderCamerasResolution_x[i] = MainSettingsVars.data.renderCamerasResolution[i].x.ToString();
                renderCamerasResolution_y[i] = MainSettingsVars.data.renderCamerasResolution[i].y.ToString();
            }
        }

        public void setAnimationGui() {
            guiGridAnimations = new string[MainSettingsVars.data.animations.Length];
            guiGridAnimationSelectInt = -1;
            guiGridAnimationDeleteInt = -1;
            for (int i=0; i < MainSettingsVars.data.animations.Length; i++) {
                if (MainSettingsVars.data.animations[i] == MainSettingsVars.data.currentAnimation) {
                    guiGridAnimations[i] = MainSettingsVars.data.animations[i]+" (current)";
                    continue;
                }
                guiGridAnimations[i] = MainSettingsVars.data.animations[i];
            }
        }

        public void setMeshGui() {
            for (int i=0; i < MainSettingsVars.groupsLength; i++) {
                // toggles
                guiToggleMeshTopology[i] = false;
                guiToggleMeshColor[i] = false;
                guiToggleMeshShader[i] = false;
                // grid ints
                if (MainSettingsVars.currentAnimationComponent != null) {
                    guiGridMeshTopologyInt[i] = (int)MainSettingsVars.currentAnimationComponent.meshComponents[i].meshTopologySelect;
                    guiGridMeshColorInt[i] = (int)MainSettingsVars.currentAnimationComponent.meshComponents[i].meshColorSelect;
                    guiGridMeshShaderInt[i] = (int)MainSettingsVars.currentAnimationComponent.meshComponents[i].meshShaderSelect;
                }
                else {
                    guiGridMeshTopologyInt[i] = 0;
                    guiGridMeshColorInt[i] = 0;
                    guiGridMeshShaderInt[i] = 0;
                }
                guiGridMeshTopologyIntNew[i] = guiGridMeshTopologyInt[i];
                guiGridMeshColorIntNew[i] = guiGridMeshColorInt[i];
                guiGridMeshShaderIntNew[i] = guiGridMeshShaderInt[i];
            }
        }

	}
}
