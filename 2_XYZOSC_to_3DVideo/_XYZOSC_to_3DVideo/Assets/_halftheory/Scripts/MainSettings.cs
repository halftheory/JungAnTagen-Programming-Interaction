using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace _halftheory {

    public class MainSettingsData : ScriptableObject {

        // stereo3d
        public bool stereo3d_enabled = false;
        public float stereo3d_interaxial = 65f;
        public float stereo3d_zeroPrlxDist = 2f;
        public float stereo3d_H_I_T = 0;
        // osc
        public int osc_inPort = 8888;
        public string osc_outIP = "127.0.0.1";
        public int osc_outPort = 6161;
        //mouselook
        public bool mouselook_enabled = false;

        // animations
        public string currentAnimationName;
    }

    public static class MainSettingsVars {

        public static bool initialized = false;

        /* DATA */
        public static MainSettingsData data;
        public static string dataPathSuffix = "_halftheory/Data";
        public static string dataPath;
        public static string dataFilePathSuffix = "MainSettingsData.json";
        public static string dataFilePath;

        /* OBJECTS + COMPONENTS */
        // camera
        public static GameObject mainCamObject;
        public static Camera mainCamComponent;
        // light
        public static GameObject lightObject;
        public static Light lightComponent;
        // mouselook
        public static MouseLook mouselookComponent;
        // stereo3d
        public static stereo3dCameraSBS stereo3dComponent;
        // osc
        public static GameObject oscObject;
        public static OSC oscComponent;
        // get these after data is loaded
        public static GameObject currentAnimationObject;
        public static OSC_Animation currentAnimationComponent;

        public static bool loadData() {
            data = ScriptableObject.CreateInstance<MainSettingsData>();
            bool test = hasDataPath();
            if (!test) {
                return (false);
            }
            // try saved file
            if (File.Exists(dataFilePath)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataFilePath, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), data);
                file.Close();
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
                }
                if (mouselookComponent != null) {
                    mouselookComponent.enabled = data.mouselook_enabled;
                }
                return (true);
            }
            // new
            else {
                saveData();
            }
            if (data != null) {
                return (true);
            }
            return (false);
        }
        public static void saveData() {
            if (data == null) {
                return;
            }
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
            }
            if (mouselookComponent != null) {
                data.mouselook_enabled = mouselookComponent.enabled;
            }
            bool test = hasDataPath();
            if (!test) {
                return;
            }
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataFilePath);
            var json = JsonUtility.ToJson(data);
            bf.Serialize(file, json);
            file.Close();
        }
        public static bool hasDataPath() {
            if (dataPath != null) {
                return (true);
            }
            if (Directory.Exists(Application.dataPath+"/"+dataPathSuffix)) {
                dataPath = Application.dataPath+"/"+dataPathSuffix;
                dataFilePath = dataPath+"/"+dataFilePathSuffix;
                return (true);
            }
            else if (Directory.Exists(Application.persistentDataPath+"/"+dataPathSuffix)) {
                dataPath = Application.persistentDataPath+"/"+dataPathSuffix;
                dataFilePath = dataPath+"/"+dataFilePathSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.dataPath+"/"+dataPathSuffix);
            if (Directory.Exists(Application.dataPath+"/"+dataPathSuffix)) {
                dataPath = Application.dataPath+"/"+dataPathSuffix;
                dataFilePath = dataPath+"/"+dataFilePathSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.persistentDataPath+"/"+dataPathSuffix);
            if (Directory.Exists(Application.persistentDataPath+"/"+dataPathSuffix)) {
                dataPath = Application.persistentDataPath+"/"+dataPathSuffix;
                dataFilePath = dataPath+"/"+dataFilePathSuffix;
                return (true);
            }
            return (false);
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

        public static bool loadAnimationObject() {
            if (oscObject == null) {
                return (false);
            }
            Transform testTransform = oscObject.transform;
            // try saved name
            if (!string.IsNullOrEmpty(data.currentAnimationName)) {
                testTransform = oscObject.transform.Find(data.currentAnimationName);
            }
            // try first child
            else if (oscObject.transform.childCount > 0) {
                testTransform = oscObject.transform.GetChild(0);
            }
            if (testTransform && testTransform.GetComponent<OSC_Animation>()) {
                currentAnimationObject = testTransform.gameObject;
                currentAnimationComponent = currentAnimationObject.GetComponent<OSC_Animation>();
                data.currentAnimationName = currentAnimationObject.name;
                return (true);
            }
            // new
            else {
                makeAnimationObject();
            }
            if (currentAnimationObject != null) {
                return (true);
            }
            return (false);
        }
        public static void makeAnimationObject() {
            string objectName = DateTime.Now.ToString("yyyyMMddHHmmss");
            currentAnimationObject = new GameObject(objectName, typeof(OSC_Animation));
            currentAnimationObject.transform.parent = oscObject.transform;
            currentAnimationComponent = currentAnimationObject.GetComponent<OSC_Animation>();
            data.currentAnimationName = objectName;
        }
        public static bool hasAnimationObject() {
            if (currentAnimationObject != null) {
                return (true);
            }
            bool test = loadAnimationObject();
            if (test) {
                return (true);
            }
            return (false);
        }
    }

    [RequireComponent(typeof(Camera)), RequireComponent(typeof(stereo3dCameraSBS))]
    public class MainSettings : MonoBehaviour {

        void Initialize() {
            if (MainSettingsVars.initialized) {
                return;
            }

            // camera
            if (GetComponent<Camera>()) {
                MainSettingsVars.mainCamObject = this.gameObject;
                MainSettingsVars.mainCamComponent = GetComponent<Camera>();
                MainSettingsVars.mainCamComponent.clearFlags = CameraClearFlags.SolidColor;
                MainSettingsVars.mainCamComponent.backgroundColor = Color.black;
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": Camera not found");
                return;
            }

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

            // stereo3d
            if (GetComponent<stereo3dCameraSBS>()) {
                MainSettingsVars.stereo3dComponent = GetComponent<stereo3dCameraSBS>();
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": stereo3dCameraSBS not found");
                return;
            }

            // osc
            OSC oscComponent = (OSC)FindObjectOfType(typeof(OSC));
            if (oscComponent) {
                MainSettingsVars.oscObject = oscComponent.gameObject;
                MainSettingsVars.oscComponent = oscComponent;
            }
            else {
                Debug.Log("HALFTHEORY: "+this.GetType()+": OSC not found");
                return;
            }

            // data
            bool test = MainSettingsVars.loadData();
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadData failed");
                return;
            }

            // get these after data is loaded
            test = MainSettingsVars.loadAnimationObject();
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": loadAnimationObject failed");
                return;
            }
            if (!MainSettingsVars.oscObject.GetComponent<OSC_Distributor>()) {
                MainSettingsVars.oscObject.AddComponent<OSC_Distributor>();
            }

            MainSettingsVars.initialized = true;
        }

        void Awake() {
            Initialize();
        }

        void OnDisable() {
            MainSettingsVars.saveData();
        }
    }

}