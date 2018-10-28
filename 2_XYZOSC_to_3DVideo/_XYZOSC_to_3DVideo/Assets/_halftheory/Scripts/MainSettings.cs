using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace _halftheory {

    public enum gameMode {live, record, play, render}

    public class MainSettingsData : ScriptableObject {

        // global
        public bool guiActive = true;
        public int maxSizeCollections = 10000; // range 2000-?
        public int maxSizeChildren = 1000;  // range 100-?

        // stereo3d
        public bool _stereo3d_enabled = false;
        public bool stereo3d_enabled {
            get { return _stereo3d_enabled; }
            set { _stereo3d_enabled = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.enabled = value;
                }
            }
        }
        public float _stereo3d_interaxial = 65f;
        public float stereo3d_interaxial {
            get { return _stereo3d_interaxial; }
            set { _stereo3d_interaxial = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.interaxial = value;
                }
            }
        }
        public float _stereo3d_zeroPrlxDist = 2f;
        public float stereo3d_zeroPrlxDist {
            get { return _stereo3d_zeroPrlxDist; }
            set { _stereo3d_zeroPrlxDist = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.zeroPrlxDist = value;
                }
            }
        }
        public float _stereo3d_H_I_T = 0;
        public float stereo3d_H_I_T {
            get { return _stereo3d_H_I_T; }
            set { _stereo3d_H_I_T = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.H_I_T = value;
                }
            }
        }
        // osc
        public int _osc_inPort = 8888;
        public int osc_inPort {
            get { return _osc_inPort; }
            set { _osc_inPort = value;
                if (MainSettingsVars.oscComponent != null) {
                    MainSettingsVars.oscComponent.inPort = value;
                }
            }
        }
        public string osc_outIP = "127.0.0.1";
        public int osc_outPort = 9999;
        //mouselook
        public bool _mouselook_enabled = false;
        public bool mouselook_enabled {
            get { return _mouselook_enabled; }
            set { _mouselook_enabled = value;
                if (MainSettingsVars.mouselookComponent != null) {
                    MainSettingsVars.mouselookComponent.enabled = value;
                }
            }
        }
        // animations
        public gameMode currentGameMode = gameMode.live;
        public string[] animations;
        public string _currentAnimation;
        public string currentAnimation {
            get { return _currentAnimation; }
            set {
                bool test = MainSettingsVars.loadAnimationObject(value);
                if (test) {
                    _currentAnimation = value;
                }
            }
        }
    }

    public static class MainSettingsVars {

        public static bool initialized = false;

        /* DATA */
        public static MainSettingsData data;
        public static string dataFolderSuffix = "_halftheory/Data";
        private static string _dataFolder;
        public static string dataFolder {
            get { return _dataFolder; }
            set { _dataFolder = value;
                dataFile = dataFolder+"/"+dataFileSuffix;
            }
        }
        public static string dataFileSuffix = "MainSettingsData.json";
        public static string dataFile;
        public static string recordFolderSuffix = "record";
        public static string recordFolder;

        /* OBJECTS + COMPONENTS */
        // light
        public static GameObject lightObject;
        public static Light lightComponent;
        // mouselook
        public static MouseLook mouselookComponent;
        // audiosource
        public static AudioSource audiosourceComponent;
        // camera
        public static GameObject mainCamObject;
        public static Camera mainCamComponent;
        // stereo3d
        public static stereo3dCameraSBS stereo3dComponent;
        // osc
        public static GameObject oscObject;
        public static OSC oscComponent;
        // get these after main data is loaded
        public static GameObject currentAnimationObject;
        public static OSC_Animation currentAnimationComponent;

        /* ANIMATION */
        public static bool forceFPS = false;
        public static int groupsLength = 4;
        public static int pointsLength = 200;
        public static float pointsTime = 0.05f; // time in seconds each point stays on screen
        public static float repeatInterval = 1f / 60f;
        public static string defaultAnimatorController = "AnimatorController";
        public static string defaultAnimationClip = "AnimationClip";

        public static bool loadData() {
            data = ScriptableObject.CreateInstance<MainSettingsData>();
            // try saved file
            bool test = hasDataFile();
            if (test) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataFile, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), data);
                file.Close();
            }
            // new
            else {
                saveData();
            }
            if (data != null) {
                if (stereo3dComponent != null) {
                    stereo3dComponent.enabled = data.stereo3d_enabled;
                    stereo3dComponent.interaxial = data.stereo3d_interaxial;
                    stereo3dComponent.zeroPrlxDist = data.stereo3d_zeroPrlxDist;
                    stereo3dComponent.H_I_T = data.stereo3d_H_I_T;
                }
                if (oscComponent != null) {
                    oscComponent.inPort = data.osc_inPort;
                    oscComponent.outIP = data.osc_outIP;
                    oscComponent.outPort = data.osc_outPort;
                    // children
                    if (data.animations != null && data.animations.Length > 0) {
                        Array.Sort(data.animations);
                        GameObject newObj;
                        for (int i = 0; i < data.animations.Length; i++) {
                            Transform testTransform = oscObject.transform.Find(data.animations[i]);
                            if (testTransform && testTransform.GetComponent<OSC_Animation>()) {
                                continue;
                            }
                            newObj = new GameObject(data.animations[i], typeof(OSC_Animation));
                            newObj.GetComponent<OSC_Animation>().current = false;
                            newObj.GetComponent<Animation>().playAutomatically = false;
                            newObj.SetActive(false);
                            newObj.transform.parent = oscObject.transform;
                        }
                    }
                }
                if (mouselookComponent != null) {
                    mouselookComponent.enabled = data.mouselook_enabled;
                }
                return (true);
            }
            return (false);
        }
        public static void saveData() {
            if (data == null) {
                return;
            }
            // in case component settings were changed in editor
            if (stereo3dComponent != null) {
                data.stereo3d_enabled = stereo3dComponent.enabled;
                data.stereo3d_interaxial = stereo3dComponent.interaxial;
                data.stereo3d_zeroPrlxDist = stereo3dComponent.zeroPrlxDist;
                data.stereo3d_H_I_T = stereo3dComponent.H_I_T;
            }
            if (oscComponent != null) {
                data.osc_inPort = oscComponent.inPort;
                data.osc_outIP = oscComponent.outIP;
                data.osc_outPort = oscComponent.outPort;
                // children
                reloadAnimations();
            }
            if (mouselookComponent != null) {
                data.mouselook_enabled = mouselookComponent.enabled;
            }
            bool test = hasDataFolder();
            if (test) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(dataFile);
                var json = JsonUtility.ToJson(data);
                bf.Serialize(file, json);
                file.Close();
            }
        }
        public static bool hasData() {
            if (data != null) {
                return (true);
            }
            bool test = loadData();
            if (test) {
                return (true);
            }
            return (false);
        }
        public static bool hasDataFolder() {
            if (!string.IsNullOrEmpty(dataFolder)) {
                return (true);
            }
            if (Directory.Exists(Application.dataPath+"/"+dataFolderSuffix)) {
                dataFolder = Application.dataPath+"/"+dataFolderSuffix;
                return (true);
            }
            else if (Directory.Exists(Application.persistentDataPath+"/"+dataFolderSuffix)) {
                dataFolder = Application.persistentDataPath+"/"+dataFolderSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.dataPath+"/"+dataFolderSuffix);
            if (Directory.Exists(Application.dataPath+"/"+dataFolderSuffix)) {
                dataFolder = Application.dataPath+"/"+dataFolderSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.persistentDataPath+"/"+dataFolderSuffix);
            if (Directory.Exists(Application.persistentDataPath+"/"+dataFolderSuffix)) {
                dataFolder = Application.persistentDataPath+"/"+dataFolderSuffix;
                return (true);
            }
            return (false);
        }
        public static bool hasDataFile() {
            bool test = hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(dataFile)) {
                return (true);
            }
            return (false);
        }
        public static bool hasRecordFolder() {
            if (!string.IsNullOrEmpty(recordFolder)) {
                return (true);
            }
            string rootFolder = "/2_XYZOSC_to_3DVideo/";
            if (Application.dataPath.IndexOf(rootFolder) == -1) {
                return (false);
            }
            string[] parts = Regex.Split(Application.dataPath, rootFolder);
            if (Directory.Exists(parts[0]+rootFolder+recordFolderSuffix)) {
                recordFolder = parts[0]+rootFolder+recordFolderSuffix;
                return (true);
            }
            Directory.CreateDirectory(parts[0]+rootFolder+recordFolderSuffix);
            if (Directory.Exists(parts[0]+rootFolder+recordFolderSuffix)) {
                recordFolder = parts[0]+rootFolder+recordFolderSuffix;
                return (true);
            }
            return (false);
        }

        public static bool loadAnimationObject(string objectName) {
            if (oscObject == null) {
                return (false);
            }
            // same
            if (currentAnimationObject != null && currentAnimationObject.name == objectName && currentAnimationObject.activeSelf && currentAnimationComponent.current) {
                 return (true);
            }
            // better stop everything
            /*
            if (currentAnimationComponent != null) {
                Debug.Log("stopAndSave loadAnimationObject");
                currentAnimationComponent.stopAndSave();
            }
            */
            // empty
            if (string.IsNullOrEmpty(objectName)) {
                foreach (Transform child in oscObject.transform) {
                    child.GetComponent<OSC_Animation>().current = false;
                    child.gameObject.SetActive(false);
                }
                currentAnimationObject = null;
                currentAnimationComponent = null;
                return (true);
            }
            // try saved name
            if (oscObject.transform.childCount > 0) {
                Transform testTransform = oscObject.transform.Find(objectName);
                if (testTransform && testTransform.GetComponent<OSC_Animation>()) {
                    foreach (Transform child in oscObject.transform) {
                        if (child.name == objectName) {
                            child.GetComponent<OSC_Animation>().current = true;
                            child.gameObject.SetActive(true);
                            continue;
                        }
                        child.GetComponent<OSC_Animation>().current = false;
                        child.gameObject.SetActive(false);
                    }
                    currentAnimationObject = testTransform.gameObject;
                    currentAnimationComponent = currentAnimationObject.GetComponent<OSC_Animation>();
                    return (true);
                }
            }
            // new
            currentAnimationObject = new GameObject(objectName, typeof(OSC_Animation));
            currentAnimationObject.transform.parent = oscObject.transform;
            currentAnimationComponent = currentAnimationObject.GetComponent<OSC_Animation>();
            currentAnimationObject.GetComponent<Animation>().playAutomatically = false;
            foreach (Transform child in oscObject.transform) {
                if (child.name == objectName) {
                    child.GetComponent<OSC_Animation>().current = true;
                    child.gameObject.SetActive(true);
                    continue;
                }
                child.GetComponent<OSC_Animation>().current = false;
                child.gameObject.SetActive(false);
            }
            reloadAnimations();
            return (true);
        }
        public static IEnumerator deleteAnimationObject(string objectName) {
            // delete current
            if (currentAnimationObject != null && currentAnimationObject.name == objectName) {
                data.currentAnimation = "";
            }
            foreach (Transform child in oscObject.transform) {
                if (child.name == objectName) {
                    child.GetComponent<OSC_Animation>().deleteFiles();
                    UnityEngine.Object.Destroy(child.gameObject);
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
            reloadAnimations();
            yield break;
        }
        public static bool hasAnimationObject() {
            if (currentAnimationObject != null) {
                return (true);
            }
            if (string.IsNullOrEmpty(data.currentAnimation)) {
                return (false);
            }
            bool test = loadAnimationObject(data.currentAnimation);
            if (test) {
                return (true);
            }
            return (false);
        }
        public static void reloadAnimations() {
            if (oscObject == null) {
                return;
            }
            data.animations = new string[oscObject.transform.childCount];
            if (oscObject.transform.childCount > 0) {
                int i = 0;
                foreach (Transform child in oscObject.transform) {
                    data.animations[i] = child.name;
                    i++;
                }
                Array.Sort(data.animations);
            }
        }
        public static string getAnimationName() {
            string objectName = DateTime.Now.ToString("yyyyMMddHHmmss");
            return objectName;
        }
    }

    [RequireComponent(typeof(Camera)), RequireComponent(typeof(stereo3dCameraSBS))]
    public class MainSettings : MonoBehaviour {

        IEnumerator Initialize() {
            if (MainSettingsVars.initialized) {
                yield break;
            }

            // optional
            // light
            Light lightComponent = (Light)FindObjectOfType(typeof(Light));
            if (lightComponent) {
                MainSettingsVars.lightObject = lightComponent.gameObject;
                MainSettingsVars.lightComponent = lightComponent;
                MainSettingsVars.lightComponent.color = Color.white;
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": Light not found");
            }
            // mouselook
            if (GetComponent<MouseLook>()) {
                MainSettingsVars.mouselookComponent = GetComponent<MouseLook>();
            }
            // audiosource
            if (GetComponent<AudioSource>()) {
                MainSettingsVars.audiosourceComponent = GetComponent<AudioSource>();
            }

            // required
            // camera
            if (GetComponent<Camera>()) {
                MainSettingsVars.mainCamObject = this.gameObject;
                MainSettingsVars.mainCamComponent = GetComponent<Camera>();
                MainSettingsVars.mainCamComponent.clearFlags = CameraClearFlags.SolidColor;
                MainSettingsVars.mainCamComponent.backgroundColor = Color.black;
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": Camera not found");
                yield break;
            }

            // stereo3d
            if (GetComponent<stereo3dCameraSBS>()) {
                MainSettingsVars.stereo3dComponent = GetComponent<stereo3dCameraSBS>();
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": stereo3dCameraSBS not found");
                yield break;
            }

            // osc
            OSC oscComponent = (OSC)FindObjectOfType(typeof(OSC));
            if (oscComponent) {
                MainSettingsVars.oscObject = oscComponent.gameObject;
                MainSettingsVars.oscComponent = oscComponent;
                if (!MainSettingsVars.oscObject.GetComponent<OSC_Distributor>()) {
                    MainSettingsVars.oscObject.AddComponent<OSC_Distributor>();
                }
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": OSC not found");
                yield break;
            }

            // data
            bool test = MainSettingsVars.loadData();
            yield return null;
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadData failed");
                yield break;
            }

            // get these after data is loaded
            test = MainSettingsVars.loadAnimationObject(MainSettingsVars.data.currentAnimation);
            yield return null;
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadAnimationObject failed");
            }

            MainSettingsVars.initialized = true;
            yield break;
        }

        void Awake() {
            if (MainSettingsVars.forceFPS) {
                QualitySettings.vSyncCount = 0;
            }
        }
        IEnumerator Start() {
            yield return StartCoroutine(Initialize());
        }
        void OnDisable() {
            MainSettingsVars.saveData();
        }
        void LateUpdate() {
            if (MainSettingsVars.initialized && Input.GetKeyDown(KeyCode.Escape)) {
                MainSettingsVars.data.guiActive = !MainSettingsVars.data.guiActive;
                MainSettingsVars.data.mouselook_enabled = !MainSettingsVars.data.guiActive;
            }
        }

        // GUI
        #if UNITY_EDITOR
        private Rect guiWindowRect = new Rect(100, 100, Screen.width - 200, 200);
        private GUIStyle guiStyleButton;
        private bool guiToggle3d, guiToggleOsc = false;

        private bool guiLabel(bool myBool, string label, string current = "") {
            if (myBool) {
                label = label+" -";
            }
            else {
                label = label+" +";
            }
            if (string.IsNullOrEmpty(current)) {
                myBool = GUILayout.Toggle(myBool, label, guiStyleButton);
            }
            else {
                GUILayout.BeginHorizontal();
                myBool = GUILayout.Toggle(myBool, label, guiStyleButton);
                GUILayout.Label(current, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            return myBool;
        }
        private dynamic guiField(dynamic value, string label, dynamic valueDefault = null, dynamic min = null, dynamic max = null) {
            GUILayout.BeginVertical("box");
            if (value.GetType() == typeof(bool)) {
                value = GUILayout.Toggle(value, label, "button");
            }
            else if (value.GetType() == typeof(int)) {
                if (valueDefault == null) {
                    valueDefault = 0;
                }
                if (min == null) {
                    min = 0;
                }
                if (max == null) {
                    max = 100;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(label+" (default "+valueDefault.ToString()+")");
                string valueString = GUILayout.TextField(value.ToString(), 10, GUILayout.Width(50));
                value = int.Parse(valueString);
                if (GUILayout.Button("reset", GUILayout.Width(50))) {
                    value = valueDefault;
                }
                GUILayout.EndHorizontal();
                value = GUILayout.HorizontalSlider((float)value, (float)min, (float)max);
                value = (int)value;
            }
            else if (value.GetType() == typeof(float)) {
                if (valueDefault == null) {
                    valueDefault = 0f;
                }
                if (min == null) {
                    min = 0f;
                }
                if (max == null) {
                    max = 1f;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(label+" (default "+valueDefault.ToString()+")");
                string valueString = GUILayout.TextField(value.ToString(), 10, GUILayout.Width(50));
                value = float.Parse(valueString);
                if (GUILayout.Button("reset", GUILayout.Width(50))) {
                    value = valueDefault;
                }
                GUILayout.EndHorizontal();
                value = GUILayout.HorizontalSlider(value, min, max);
            }
            GUILayout.EndVertical();
            return value;
        }
        void OnGUI() {
            if (!MainSettingsVars.initialized) {
                return;
            }
            if (!MainSettingsVars.data.guiActive) {
                return;
            }
            GUI.backgroundColor = Color.white;
            if (guiStyleButton == null) {
                guiStyleButton = GUI.skin.GetStyle("button");
                guiStyleButton.alignment = TextAnchor.MiddleLeft;
            }
            guiWindowRect = GUILayout.Window(0, guiWindowRect, guiWindowFunc, "MENU", GUILayout.MaxWidth(1080));
        }
        void guiWindowFunc(int id = 0) {
            // 3D Settings
            guiToggle3d = guiLabel(guiToggle3d, "3D Settings");
            if (guiToggle3d) {
                MainSettingsVars.data.stereo3d_enabled = guiField(MainSettingsVars.data.stereo3d_enabled, "Enable Side-By-Side");
                MainSettingsVars.data.stereo3d_interaxial = guiField(MainSettingsVars.data.stereo3d_interaxial, "Interaxial (mm) - Distance (in millimeters) between cameras.", 65f, 0, 1000f);
                MainSettingsVars.data.stereo3d_zeroPrlxDist = guiField(MainSettingsVars.data.stereo3d_zeroPrlxDist, "Zero Prlx Dist (M) - Distance (in meters) at which left and right images converge.", 2f, 0.1f, 100f);
                MainSettingsVars.data.stereo3d_H_I_T = guiField(MainSettingsVars.data.stereo3d_H_I_T, "H I T - Horizontal Image Transform.", 0f, -25f, 25f);
            }
            // OSC Settings
            guiToggleOsc = guiLabel(guiToggleOsc, "OSC Settings");
            if (guiToggleOsc) {
                MainSettingsVars.data.osc_inPort = guiField(MainSettingsVars.data.osc_inPort, "Incoming Port.", 8888, 1000, 10000);
            }
        }
        #endif
    }

}