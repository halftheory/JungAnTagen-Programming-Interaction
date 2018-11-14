using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UnityEngine;

namespace _halftheory {

    public enum gameMode {live, record_points, record_animation, play, render}

    public class MainSettingsData : ScriptableObject {

        // global
        public bool _gui_enabled = MainSettingsVars.defaults["gui_enabled"]; 
        public bool gui_enabled {
            get { return _gui_enabled; }
            set { _gui_enabled = value;
                if (value && MainSettingsVars.guiComponent != null) {
                    MainSettingsVars.guiComponent.setRenderCamerasResolution();
                }
            }
        }
        public int maxSizeCollections = MainSettingsVars.defaults["maxSizeCollections"];
        public int maxSizeChildren = MainSettingsVars.defaults["maxSizeChildren"];

        // stereo3d
        public bool _stereo3d_enabled = MainSettingsVars.defaults["stereo3d_enabled"];
        public bool stereo3d_enabled {
            get { return _stereo3d_enabled; }
            set { _stereo3d_enabled = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.enabled = value;
                }
            }
        }
        public float _stereo3d_interaxial = MainSettingsVars.defaults["stereo3d_interaxial"];
        public float stereo3d_interaxial {
            get { return _stereo3d_interaxial; }
            set { _stereo3d_interaxial = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.interaxial = value;
                }
            }
        }
        public float _stereo3d_zeroPrlxDist = MainSettingsVars.defaults["stereo3d_zeroPrlxDist"];
        public float stereo3d_zeroPrlxDist {
            get { return _stereo3d_zeroPrlxDist; }
            set { _stereo3d_zeroPrlxDist = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.zeroPrlxDist = value;
                }
            }
        }
        public float _stereo3d_H_I_T = MainSettingsVars.defaults["stereo3d_H_I_T"];
        public float stereo3d_H_I_T {
            get { return _stereo3d_H_I_T; }
            set { _stereo3d_H_I_T = value;
                if (MainSettingsVars.stereo3dComponent != null) {
                    MainSettingsVars.stereo3dComponent.H_I_T = value;
                }
            }
        }
        // osc
        public int _osc_inPort = MainSettingsVars.defaults["osc_inPort"];
        public int osc_inPort {
            get { return _osc_inPort; }
            set { _osc_inPort = value;
                if (MainSettingsVars.oscComponent != null) {
                    MainSettingsVars.oscComponent.inPort = value;
                }
            }
        }
        public string osc_outIP = MainSettingsVars.defaults["osc_outIP"];
        public int osc_outPort = MainSettingsVars.defaults["osc_outPort"];
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
        // gameMode
        public gameMode _currentGameMode = MainSettingsVars.defaults["currentGameMode"];
        public gameMode currentGameMode {
            get { return _currentGameMode; }
            set {
                if (_currentGameMode != value && MainSettingsVars.currentAnimationComponent != null) {
                    MainSettingsVars.currentAnimationComponent.active = false;
                }
                _currentGameMode = value;
            }
        }
        // animations
        public string[] animations;
        public string _currentAnimation;
        public string currentAnimation {
            get { return _currentAnimation; }
            set {
                bool test = MainSettingsVars.loadCurrentAnimation(value);
                if (test) {
                    _currentAnimation = value;
                }
            }
        }
        // render
        public int renderAntiAliasing = 1;
        public string renderCodec = MainSettingsVars.defaults["renderCodec"];
        public bool[] renderCamerasActive = new bool[MainSettingsVars.renderCamerasNames.Count];
        public Vector2[] renderCamerasResolution = new Vector2[MainSettingsVars.renderCamerasNames.Count];
    }

    public static class MainSettingsVars {

        public static bool initialized = false;

        /* DEFAULTS */
        public static Dictionary<string, dynamic> defaults = new Dictionary<string, dynamic>(){
            {"gui_enabled",(bool)true},
            {"maxSizeCollections",(int)10000}, // range 2000-20000
            {"maxSizeChildren",(int)1000}, // range 100-5000
            {"stereo3d_enabled",(bool)false},
            {"stereo3d_interaxial",(float)65f},
            {"stereo3d_zeroPrlxDist",(float)2f},
            {"stereo3d_H_I_T",(float)0.0f},
            {"osc_inPort",(int)8888},
            {"osc_outIP",(string)"127.0.0.1"},
            {"osc_outPort",(int)9999},
            {"currentGameMode",(gameMode)gameMode.live},
            {"renderCodec",(string)"Animation"},
            {"fps",(int)60},
            {"meshActive",(bool)false},
            {"meshPeaks",(int)0},
            {"meshLevel",(float)1.0f},
            {"meshTopology",(meshTopology)meshTopology.LineStrip},
            {"meshColor",(meshColor)meshColor.white},
            {"meshShader",(meshShader)meshShader.Color},
            {"meshAlpha",(float)1.0f},
            {"meshRandomX",(bool)false},
            {"meshRandomY",(bool)false},
            {"meshRotateSpeed",(float)0.0f},
            {"meshSmoothTime",(float)0.0f},
            {"meshClearTime",(float)0.0f},
            {"meshNoClearTime",(bool)false},
            {"meshTraceTime",(float)0.0f}
        };

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
        public static string rootFolder; // contains trailing slash
        public static string recordFolderSuffix = "record";
        public static string recordFolder;

        /* OBJECTS + COMPONENTS */
        public static MainSettings mainsettingsComponent;
        // light
        public static GameObject lightObject;
        public static Light lightComponent;
        // mouselook
        public static MouseLook mouselookComponent;
        // audiosource
        public static AudioSource audiosourceComponent;
        public static string currentAudioFile;
        // camera
        public static GameObject maincameraObject;
        public static Camera maincameraComponent;
        // stereo3d
        public static stereo3dCameraSBS stereo3dComponent;
        // osc
        public static GameObject oscObject;
        public static OSC oscComponent;
        // gui
        public static GUISettings guiComponent;
        // animations
        // get these after main data is loaded
        public static GameObject[] animationObjects;
        public static OSC_Animation[] animationComponents;
        public static GameObject currentAnimationObject;
        public static OSC_Animation currentAnimationComponent;

        /* ANIMATION */
        public static bool forceFPS = false;
        public static int groupsLength = 4;
        public static int pointsLength = 200;
        public static float pointsTime = 0.1f; // time in seconds each point stays on screen - about 3 frames @ 24fps
        public static float repeatInterval = 1f / (float)defaults["fps"];
        public static string defaultAnimatorController = "AnimatorController";
        public static string defaultAnimationClip = "AnimationClip";
        public static float time {
            get {
                if (currentAnimationRender) {
                    return Time.time;
                }
                else {
                    return Time.unscaledTime;
                }
            }
        }
        // render
        public static bool currentAnimationRender {
            get {
                if (currentAnimationComponent == null) {
                    return (false);
                }
                if (data.currentGameMode == gameMode.render && currentAnimationComponent.active) {
                    return (true);
                }
                return (false);
            }
        }
        public static Dictionary<int,string> renderAntiAliasing = new Dictionary<int,string>(){
            {1, "x 1"},
            {2, "x 2"},
            {4, "x 4"}
        };
        public static Dictionary<string,string> renderCodecs = new Dictionary<string,string>(){
            {"Animation", "qtrle"},
            {"ProRes4444", "prores_ks -pix_fmt yuva444p10le"}
        };
        public static Dictionary<string,Vector2> renderResolutions = new Dictionary<string,Vector2>(){
            {"HD", new Vector2(1920.0f,1080.0f)},
            {"DCI 2K", new Vector2(2048.0f,1080.0f)},
            {"4K UHD", new Vector2(3840.0f,2160.0f)},
            {"DCI 4K", new Vector2(4096.0f,2160.0f)}
        };
        public static Dictionary<int,string> renderCamerasNames = new Dictionary<int,string>(){
            {0, "Single"},
            {1, "Side-By-Side Half"},
            {2, "Side-By-Side Full"}
        };
        public static Dictionary<int,Camera[]> renderCamerasComponents = new Dictionary<int,Camera[]>();

        private static float saveTime = 0.0f;

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
                    loadAnimations();
                }
                if (mouselookComponent != null) {
                    mouselookComponent.enabled = data.mouselook_enabled;
                }
                saveTime = Time.realtimeSinceStartup;
                return (true);
            }
            return (false);
        }
        public static void saveData() {
            // ondisable
            if (Time.frameCount == 0) {
                if (mainsettingsComponent.quitStarted) {
                    return;
                }
            }            
            // normal
            else {
                if (saveTime > Time.realtimeSinceStartup - 3.0f) {
                    return;
                }
            }
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
                loadAnimations();
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
            saveTime = Time.realtimeSinceStartup;
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
        public static bool hasRootFolder() {
            if (!string.IsNullOrEmpty(rootFolder)) {
                return (true);
            }
            string splitter = "/2_XYZOSC_to_3DVideo/";
            if (Application.dataPath.IndexOf(splitter) == -1) {
                return (false);
            }
            string[] parts = Regex.Split(Application.dataPath, splitter);
            rootFolder = parts[0]+splitter;
            return (true);
        }
        public static bool hasRecordFolder() {
            if (!string.IsNullOrEmpty(recordFolder)) {
                return (true);
            }
            bool test = hasRootFolder();
            if (!test) {
                return (false);
            }
            if (Directory.Exists(rootFolder+recordFolderSuffix)) {
                recordFolder = rootFolder+recordFolderSuffix;
                return (true);
            }
            Directory.CreateDirectory(rootFolder+recordFolderSuffix);
            if (Directory.Exists(rootFolder+recordFolderSuffix)) {
                recordFolder = rootFolder+recordFolderSuffix;
                return (true);
            }
            return (false);
        }
        public static void dataActions(int action) {
            bool test = hasDataFolder();
            if (!test) {
                return;
            }
            test = hasRootFolder();
            if (!test) {
                return;
            }
            if (currentAnimationComponent != null) {
                currentAnimationComponent.stopAndSave();
            }
            #if UNITY_EDITOR
                string editorDir = dataFolder;
                string runtimeDir = rootFolder+"build/_XYZOSC_to_3DVideo.app/Contents/"+dataFolderSuffix;
            #else
                string runtimeDir = dataFolder;
                string editorDir = rootFolder+"_XYZOSC_to_3DVideo/Assets/"+dataFolderSuffix;
            #endif
            switch (action) {
                // "Copy Runtime Data to Editor",
                case 0:
                    DirectoryCopy(runtimeDir, editorDir);
                    break;
                // "Copy Editor Data to Runtime"
                case 1:
                    DirectoryCopy(editorDir, runtimeDir);
                    break;
            }
        }
        private static void DirectoryCopy(string sourceDir, string destDir) {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) {
                return;
            }
            if (!Directory.Exists(destDir)) {
                Directory.CreateDirectory(destDir);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files) {
                if (file.Name.IndexOf(".meta") > 0) {
                    continue;
                }
                string tempPath = Path.Combine(destDir, file.Name);
                file.CopyTo(tempPath, true);
            }
        }

        public static void loadAnimations() {
            if (oscObject == null) {
                return;
            }
            // use array
            if (data.animations != null && data.animations.Length > 0) {
                // destroy children not in the array
                if (oscObject.transform.childCount > 0) {
                    int test = -1;
                    foreach (Transform child in oscObject.transform) {
                        test = Array.IndexOf(data.animations, child.name);
                        if (test == -1) {
                            child.GetComponent<OSC_Animation>().deleteFiles();
                            UnityEngine.Object.Destroy(child.gameObject);
                        }
                    }
                }
                Array.Sort(data.animations);
                animationObjects = new GameObject[data.animations.Length];
                animationComponents = new OSC_Animation[data.animations.Length];
                GameObject newObj;
                for (int i=0; i < data.animations.Length; i++) {
                    Transform testTransform = oscObject.transform.Find(data.animations[i]);
                    // exists
                    if (testTransform != null) {
                        animationObjects[i] = testTransform.gameObject;
                        animationComponents[i] = testTransform.GetComponent<OSC_Animation>();
                        continue;
                    }
                    // new
                    newObj = new GameObject(data.animations[i], typeof(OSC_Animation));
                    newObj.GetComponent<Animation>().playAutomatically = false;
                    newObj.transform.parent = oscObject.transform;
                    newObj.SetActive(false);
                    animationObjects[i] = newObj;
                    animationComponents[i] = newObj.GetComponent<OSC_Animation>();
                }
            }
            else {
                // destroy unwanted children
                if (oscObject.transform.childCount > 0) {
                    foreach (Transform child in oscObject.transform) {
                        child.GetComponent<OSC_Animation>().deleteFiles();
                        UnityEngine.Object.Destroy(child.gameObject);
                    }
                }
                data.animations = new string[0];
                data.currentAnimation = "";
                animationObjects = null;
                animationComponents = null;
            }
        }
        public static bool loadCurrentAnimation(string objectName) {
            if (oscObject == null) {
                return (false);
            }
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
                if (testTransform != null) {
                    foreach (Transform child in oscObject.transform) {
                        if (child.name == objectName) {
                            child.GetComponent<OSC_Animation>().current = true;
                            child.gameObject.SetActive(true);
                            if (child.GetComponent<OSC_Animation>().initialized) {
                                currentAnimationObject = child.gameObject;
                                currentAnimationComponent = child.GetComponent<OSC_Animation>();
                            }
                            else {
                                loadCurrentAnimation("");
                                return (false);
                            }
                            continue;
                        }
                        child.GetComponent<OSC_Animation>().current = false;
                        child.gameObject.SetActive(false);
                    }
                    return (true);
                }
            }
            return (false);
        }
        public static bool hasAnimationObject() {
            if (currentAnimationObject != null) {
                return (true);
            }
            if (string.IsNullOrEmpty(data.currentAnimation)) {
                return (false);
            }
            bool test = loadCurrentAnimation(data.currentAnimation);
            if (test) {
                return (true);
            }
            return (false);
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

            MainSettingsVars.mainsettingsComponent = this;

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
                MainSettingsVars.maincameraObject = this.gameObject;
                MainSettingsVars.maincameraComponent = GetComponent<Camera>();
                MainSettingsVars.maincameraComponent.clearFlags = CameraClearFlags.SolidColor;
                MainSettingsVars.maincameraComponent.backgroundColor = Color.black;

                MainSettingsVars.renderCamerasComponents.Add(0, new Camera[]{ MainSettingsVars.maincameraComponent });
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": Camera not found");
                yield break;
            }

            // stereo3d
            if (GetComponent<stereo3dCameraSBS>()) {
                MainSettingsVars.stereo3dComponent = GetComponent<stereo3dCameraSBS>();

                if (MainSettingsVars.stereo3dComponent.leftCam != null && MainSettingsVars.stereo3dComponent.rightCam != null) {
                    MainSettingsVars.renderCamerasComponents.Add(1, new Camera[]{ MainSettingsVars.stereo3dComponent.leftCam.GetComponent<Camera>(), MainSettingsVars.stereo3dComponent.rightCam.GetComponent<Camera>() });
                }
                if (MainSettingsVars.stereo3dComponent.leftCamRecord != null && MainSettingsVars.stereo3dComponent.rightCamRecord != null) {
                    MainSettingsVars.renderCamerasComponents.Add(2, new Camera[]{ MainSettingsVars.stereo3dComponent.leftCamRecord.GetComponent<Camera>(), MainSettingsVars.stereo3dComponent.rightCamRecord.GetComponent<Camera>() });
                }
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

            // gui
            GUISettings guiComponent = (GUISettings)FindObjectOfType(typeof(GUISettings));
            if (guiComponent) {
                MainSettingsVars.guiComponent = guiComponent;
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": GUISettings not found");
            }

            MainSettingsVars.initialized = true;

            // data
            bool test = MainSettingsVars.loadData();
            yield return null;
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadData failed");
                yield break;
            }

            // get these after data is loaded
            test = MainSettingsVars.loadCurrentAnimation(MainSettingsVars.data.currentAnimation);
            yield return null;
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadCurrentAnimation failed");
            }

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
            if (!MainSettingsVars.initialized) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) {
                MainSettingsVars.data.gui_enabled = !MainSettingsVars.data.gui_enabled;
                MainSettingsVars.data.mouselook_enabled = !MainSettingsVars.data.gui_enabled;
            }
            if (Input.GetKeyDown(KeyCode.Space) && MainSettingsVars.currentAnimationComponent != null) {
                MainSettingsVars.currentAnimationComponent.active = !MainSettingsVars.currentAnimationComponent.active;
            }
        }

        public bool quitStarted = false;

        public IEnumerator Quit() {
            quitStarted = true;
            yield return new WaitForEndOfFrame();
            if (MainSettingsVars.initialized && MainSettingsVars.animationComponents != null) {
                if (MainSettingsVars.animationComponents.Length > 0) {
                    for (int i=0; i < MainSettingsVars.animationComponents.Length; i++) {
                        MainSettingsVars.animationComponents[i].stopAndSave();
                        yield return null;
                    }
                }
            }
            MainSettingsVars.saveData();
            yield return null;
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
            yield break;
        }
    }
}