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

        public string currentAnimationName;

        public void loadToObjects() {
            if (MainSettingsVars.stereo3dComponent != null) {
                MainSettingsVars.stereo3dComponent.enabled = stereo3d_enabled;
                MainSettingsVars.stereo3dComponent.interaxial = stereo3d_interaxial;
                MainSettingsVars.stereo3dComponent.zeroPrlxDist = stereo3d_zeroPrlxDist;
                MainSettingsVars.stereo3dComponent.H_I_T = stereo3d_H_I_T;
            }
            if (MainSettingsVars.oscComponent != null) {
                MainSettingsVars.oscComponent.inPort = osc_inPort;
                MainSettingsVars.oscComponent.outIP = osc_outIP;
                MainSettingsVars.oscComponent.outPort = osc_outPort;
            }
        }
        public void saveFromObjects() {
            if (MainSettingsVars.stereo3dComponent != null) {
                stereo3d_enabled = MainSettingsVars.stereo3dComponent.enabled;
                stereo3d_interaxial = MainSettingsVars.stereo3dComponent.interaxial;
                stereo3d_zeroPrlxDist = MainSettingsVars.stereo3dComponent.zeroPrlxDist;
                stereo3d_H_I_T = MainSettingsVars.stereo3dComponent.H_I_T;
            }
            if (MainSettingsVars.oscComponent != null) {
                osc_inPort = MainSettingsVars.oscComponent.inPort;
                osc_outIP = MainSettingsVars.oscComponent.outIP;
                osc_outPort = MainSettingsVars.oscComponent.outPort;
            }
        }
    }

    public static class MainSettingsVars {

        public static bool initialized = false;

        /* DATA */
        // dataPath
        public static string dataPathSuffix = "_halftheory/Data";
        public static string dataPath;
        public static string dataFilePathSuffix = "MainSettingsData.json";
        public static string dataFilePath;
        // data
        public static MainSettingsData data;

        /* OBJECTS + COMPONENTS */
        // camera
        public static GameObject mainCamObject;
        public static Camera mainCamComponent;
        // light
        public static GameObject lightObject;
        public static Light lightComponent;
        // stereo3d
        public static stereo3dCameraSBS stereo3dComponent;
        // osc
        public static GameObject oscObject;
        public static OSC oscComponent;
        // after data loaded
        public static GameObject currentAnimationObject;
        public static OSC_Animation currentAnimationComponent;

        public static bool hasDataPath() {
            if (dataPath != null) {
                return (true);
            }
            if (Directory.Exists(Application.dataPath+"/"+dataPathSuffix)) {
                dataPath = Application.dataPath+"/"+dataPathSuffix;
                return (true);
            }
            else if (Directory.Exists(Application.persistentDataPath+"/"+dataPathSuffix)) {
                dataPath = Application.persistentDataPath+"/"+dataPathSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.dataPath+"/"+dataPathSuffix);
            if (Directory.Exists(Application.dataPath+"/"+dataPathSuffix)) {
                dataPath = Application.dataPath+"/"+dataPathSuffix;
                return (true);
            }
            Directory.CreateDirectory(Application.persistentDataPath+"/"+dataPathSuffix);
            if (Directory.Exists(Application.persistentDataPath+"/"+dataPathSuffix)) {
                dataPath = Application.persistentDataPath+"/"+dataPathSuffix;
                return (true);
            }
            return (false);
        }

        public static bool hasData() {
            if (data != null) {
                return (true);
            }
            data = ScriptableObject.CreateInstance<MainSettingsData>();
            dataFilePath = dataPath+"/"+dataFilePathSuffix;
            if (File.Exists(dataFilePath)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataFilePath, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), data);
                file.Close();
                data.loadToObjects();
                return (true);
            }
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
            data.saveFromObjects();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataFilePath);
            var json = JsonUtility.ToJson(data);
            bf.Serialize(file, json);
            file.Close();
        }

        public static bool hasAnimationObject() {
            if (currentAnimationObject != null) {
                return (true);
            }
            if (oscObject == null) {
                return (false);
            }
            Transform testTransform = oscObject.transform;
            if (!string.IsNullOrEmpty(data.currentAnimationName)) {
                testTransform = oscObject.transform.Find(data.currentAnimationName);
            }
            else if (oscObject.transform.childCount > 0) {
                // try first child
                testTransform = oscObject.transform.GetChild(0);
            }
            if (testTransform && testTransform.GetComponent<OSC_Animation>()) {
                currentAnimationObject = testTransform.gameObject;
                currentAnimationComponent = currentAnimationObject.GetComponent<OSC_Animation>();
                data.currentAnimationName = currentAnimationObject.name;
                return (true);
            }
            makeAnimationObject();
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

            // disable MouseLook on build
            /*
            #if UNITY_STANDALONE
            if (GetComponent<MouseLook>()) {
                GetComponent<MouseLook>().enabled = false;
            }
            #endif
            */

            // dataPath
            bool test = MainSettingsVars.hasDataPath();
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": hasDataPath failed");
                return;
            }
            // data
            test = MainSettingsVars.hasData();
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": hasData failed");
                return;
            }

            // after data loaded
            test = MainSettingsVars.hasAnimationObject();
            if (!test) {
                Debug.Log("HALFTHEORY: "+this.GetType()+": hasAnimationObject failed");
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
            if (MainSettingsVars.initialized) {
                MainSettingsVars.saveData();
            }
        }
    }

}