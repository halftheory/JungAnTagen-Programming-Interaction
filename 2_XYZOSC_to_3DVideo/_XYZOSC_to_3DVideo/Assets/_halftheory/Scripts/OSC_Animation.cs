using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _halftheory {

    // Data
	public class OSC_Animation_Data : ScriptableObject {

		[System.NonSerialized] public bool initialized = false;
        [System.NonSerialized] public OSC_Animation parentComponent;

		public string audio_file_path = "";
		public int _fps = MainSettingsVars.defaults["fps"];
		public int fps {
            get { return _fps; }
            set { _fps = value;
            	Initialize();
            }
		}
		public float _x_low;
		public float x_low {
            get { return _x_low; }
            set { _x_low = value;
            	Initialize();
            }
		}
		public float _x_high;
		public float x_high {
            get { return _x_high; }
            set { _x_high = value;
            	Initialize();
            }
		}
		public float _y_low;
		public float y_low {
            get { return _y_low; }
            set { _y_low = value;
            	Initialize();
            }
		}
		public float _y_high;
		public float y_high {
            get { return _y_high; }
            set { _y_high = value;
            	Initialize();
            }
		}
		public float _z_low;
		public float z_low {
            get { return _z_low; }
            set { _z_low = value;
            	Initialize();
            }
		}
		public float _z_high;
		public float z_high {
            get { return _z_high; }
            set { _z_high = value;
            	Initialize();
            }
		}

	    [System.NonSerialized] public float x_center;
	    [System.NonSerialized] public float y_center;
	    [System.NonSerialized] public float z_center;
		[System.NonSerialized] public Vector3 worldCenter;

        public Vector3 camera_position = Vector3.zero;
        public Vector3 camera_eulerAngles = Vector3.zero;

		public List<string> meshData = new List<string>(); // top int is group

		public void Initialize() {
			bool res = true;
			if (fps == 0) {
				res = false;
			}
			else if (x_low == 0 && x_high == 0) {
				res = false;
			}
			else if (y_low == 0 && y_high == 0) {
				res = false;
			}
			else if (z_low == 0 && z_high == 0) {
				res = false;
			}
			if (res) {
			    x_center = (x_low + x_high) / 2f;
			    y_center = (y_low + y_high) / 2f;
			    z_center = (z_low + z_high) / 2f;
                if (parentComponent != null) {
                    worldCenter = parentComponent.transform.TransformPoint(new Vector3(x_center, y_center, z_center));
                    parentComponent.settingsFPS();
                    parentComponent.settingsCamera();
                }
			}
			else if (parentComponent != null) {
	            UnityEngine.Debug.Log("HALFTHEORY: "+this.GetType()+": data.Initialize failed for "+parentComponent.gameObject.name);
			}
			initialized = res;
		}
	}

    // Points
    public class OSC_Animation_Data_Points : ScriptableObject {
        // top int is index
        public List<float> times = new List<float>();
        public List<Data_Points_Points> points = new List<Data_Points_Points>();
    }
    [System.Serializable]
    public class Data_Points_Points {
        // top int is point
        public List<Vector4> xyz = new List<Vector4>();
    }

    // Animation
    public class OSC_Animation_Data_Animation : ScriptableObject {
        // top int is group
        public List<Data_Animation_Groups> groups = new List<Data_Animation_Groups>();
    }
    [System.Serializable]
    public class Data_Animation_Groups {
        // top int is index
        public List<string> fields = new List<string>();
        public List<AnimationCurve> curves = new List<AnimationCurve>();
    }

    // Mono
    [RequireComponent(typeof(Animation))]
	public class OSC_Animation : MonoBehaviour {

		public bool current = false;
		public bool initialized = false;

        private string _thisName;
        private string thisName {
            get {
                if (string.IsNullOrEmpty(_thisName)) {
                    _thisName = this.gameObject.name;
                }
                return _thisName;
            }
            set { _thisName = value; }
        }

        private Animation animationComponent;
        [HideInInspector] public OSC_Mesh[] meshComponents = new OSC_Mesh[MainSettingsVars.groupsLength];

		public OSC_Animation_Data data;
        private string dataFileSuffix = "_Data.json";
        public string dataFile;
        private string pointsFileSuffix = "_Points.json";
        public string pointsFile;
        private string animationFileSuffix = "_Animation.json";
        public string animationFile;

        // start / stop
        [HideInInspector] public float activeTime;
		private bool _active = false;
		public bool active {
            get { return _active; }
            set {
                if (!initialized || !data.initialized || !current) {
                    value = false;
                }
                if (_active != value) {
                    _active = value;
                    // start for all
                	if (value) {
                        settingsFPS();
                        settingsCamera();
                        meshCalculatePointsStart();
                	}
                    // end for all
                    else {
                        if (current) {
                            // update camera
                            data.camera_position = MainSettingsVars.maincameraObject.transform.position;
                            data.camera_eulerAngles = MainSettingsVars.maincameraObject.transform.eulerAngles;
                        }
                    }
                	// live
                	if (MainSettingsVars.data.currentGameMode == gameMode.live) {
                		if (value) {
                            activeTime = MainSettingsVars.time;
						}
					}
                    // record_points
                    else if (MainSettingsVars.data.currentGameMode == gameMode.record_points) {
                        if (value) {
                            dataPoints.Clear();
                            activeTime = MainSettingsVars.time;
                        }
                        else {
                            pointTime = MainSettingsVars.time - activeTime;
                            dataPointsAdd(pointTime);
                        }
                    }
                    // record_animation
                    else if (MainSettingsVars.data.currentGameMode == gameMode.record_animation) {
                        if (value) {
                            animationClip2dataAnimation();
                            StartCoroutine(playStart());
                        }
                        else {
                            playStop();
                            dataAnimation2animationClip();
                        }
                    }
                	// play - start/stop
                	else if (MainSettingsVars.data.currentGameMode == gameMode.play) {
                		if (value) {
                            StartCoroutine(playStart());
						}
						else {
                            playStop();
						}
                	}
                	// render
                	else if (MainSettingsVars.data.currentGameMode == gameMode.render) {
                        if (value) {
                            renderStart();
                        }
                        else {
                             StartCoroutine(renderStop());
                        }
                	}
                }
            }
        }

        private float saveTime = 0.0f;

        public bool loadData() {
            data = ScriptableObject.CreateInstance<OSC_Animation_Data>();
            // try saved files
            bool test = hasDataFile();
            if (test) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataFile, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), data);
                file.Close();
            }
            if (data != null) {
                data.parentComponent = this;
				data.Initialize();
                if (!data.initialized) {
                    UnityEngine.Debug.Log("HALFTHEORY: "+this.GetType()+": data.Initialize failed for "+thisName);
                }
                StartCoroutine(meshLoadData());
                // more files
                StartCoroutine(loadPointsData());
                StartCoroutine(loadAnimationData());
                saveTime = Time.realtimeSinceStartup;
                return (true);
            }
            return (false);
        }
        void saveData() {
            // ondisable
            if (Time.frameCount == 0) {
                if (MainSettingsVars.mainsettingsComponent.quitStarted) {
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
            meshSaveData();
            bool test = MainSettingsVars.hasDataFolder();
            if (test) {
            	BinaryFormatter bf = new BinaryFormatter();
            	FileStream file = File.Create(MainSettingsVars.dataFolder+"/"+thisName+dataFileSuffix);
            	var json = JsonUtility.ToJson(data);
            	bf.Serialize(file, json);
            	file.Close();
                // more files
                savePointsData();
                saveAnimationData();
            }
            saveTime = Time.realtimeSinceStartup;
        }
        public bool hasData() {
            if (data != null) {
                return (true);
            }
            bool test = loadData();
            if (test) {
                return (true);
            }
            return (false);
        }
        public bool hasDataFile() {
            if (!string.IsNullOrEmpty(dataFile)) {
                return (true);
            }
            bool test = MainSettingsVars.hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(MainSettingsVars.dataFolder+"/"+thisName+dataFileSuffix)) {
                dataFile = MainSettingsVars.dataFolder+"/"+thisName+dataFileSuffix;
	            return (true);
            }
            return (false);
        }
        public bool hasPointsFile() {
            if (!string.IsNullOrEmpty(pointsFile)) {
                return (true);
            }
            bool test = MainSettingsVars.hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(MainSettingsVars.dataFolder+"/"+thisName+pointsFileSuffix)) {
                pointsFile = MainSettingsVars.dataFolder+"/"+thisName+pointsFileSuffix;
                return (true);
            }
            return (false);
        }
        public bool hasAnimationFile() {
            if (!string.IsNullOrEmpty(animationFile)) {
                return (true);
            }
            bool test = MainSettingsVars.hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(MainSettingsVars.dataFolder+"/"+thisName+animationFileSuffix)) {
                animationFile = MainSettingsVars.dataFolder+"/"+thisName+animationFileSuffix;
	            return (true);
            }
            return (false);
        }
        public void deleteFiles() {
        	bool test = hasDataFile();
        	if (test) {
        		File.Delete(dataFile);
        		#if UNITY_EDITOR
        		File.Delete(dataFile+".meta");
        		#endif
        	}
            test = hasPointsFile();
            if (test) {
                File.Delete(pointsFile);
                #if UNITY_EDITOR
                File.Delete(pointsFile+".meta");
                #endif
            }
        	test = hasAnimationFile();
        	if (test) {
        		File.Delete(animationFile);
        		#if UNITY_EDITOR
        		File.Delete(animationFile+".meta");
        		#endif
        	}
        }

        // Mesh Data

		private Dictionary<int, int> meshPeaks = new Dictionary<int, int>();

		public void meshPeaksAdd(int group, int peaks) {
			if (meshPeaks.ContainsKey(group)) {
				meshPeaks[group] = peaks;
			}
			else {
				meshPeaks.Add(group, peaks);
			}
            // remove old points when peaks are reduced
            while (points.Count > meshPeaks.Values.Max()) {
                int key = points.Count - 1;
                if (points.ContainsKey(key)) {
                    points.Remove(key);
                }
                else {
                    break;
                }
            }
		}
        IEnumerator meshLoadData() {
        	if (transform.childCount == MainSettingsVars.groupsLength) {
        		yield break;
        	}
        	GameObject newObj;
			for (int i=0; i < MainSettingsVars.groupsLength; i++) {
				string objName = "group"+i;
                Transform testTransform = transform.Find(objName);
                if (testTransform != null) {
                    DestroyImmediate(testTransform.gameObject);
                }
                newObj = new GameObject(objName, typeof(OSC_Mesh));
                newObj.transform.parent = transform;
                meshComponents[i] = newObj.GetComponent<OSC_Mesh>();
        		if (data.meshData.Count > i) {
					JsonUtility.FromJsonOverwrite((string)data.meshData[i], meshComponents[i].data);
                    meshComponents[i].loadData();
				}
                meshPeaksAdd(i, meshComponents[i].data.peaks);
        	}
            yield return null;
            yield break;
        }
        void meshSaveData() {
			data.meshData.Clear();
			if (transform.childCount == 0) {
        		return;
        	}
        	for (int i=0; i < meshComponents.Length; i++) {
        		meshComponents[i].saveData();
				var json = JsonUtility.ToJson(meshComponents[i].data);
				data.meshData.Add(json);
        	}
        }
        void meshCalculatePointsStart() {
            if (transform.childCount == 0) {
                return;
            }
            points.Clear();
            for (int i=0; i < meshComponents.Length; i++) {
                meshComponents[i].transform.position = Vector3.zero;
                meshComponents[i].transform.rotation = Quaternion.identity;
                meshComponents[i].calculatePointsStart();
            }
        }
        void meshCalculatePointsStop() {
            if (transform.childCount == 0) {
                return;
            }
            for (int i=0; i < meshComponents.Length; i++) {
                meshComponents[i].calculatePointsStop();
            }
            points.Clear();
        }

        // Points Data

        private SortedDictionary<float, Dictionary<int, Vector4>> dataPoints = new SortedDictionary<float, Dictionary<int, Vector4>>();
        private Dictionary<int, Vector4> pointsNew = new Dictionary<int, Vector4>();

        void dataPointsAdd(float time) {
            if (points.Count == 0) {
                return;
            }
            pointsNew = new Dictionary<int, Vector4>(points);
            // if already a set of points at the frame, move to the previous/next frame
            if (dataPoints.ContainsKey(time)) {
                if (dataPoints.Count >= MainSettingsVars.data.maxSizeCollections) {
                    return;
                }
                if (!dataPoints.ContainsKey(time-frameTime) && active) {
                    dataPoints.Add(time-frameTime, pointsNew);
                }
                else if (!dataPoints.ContainsKey(time+frameTime)) {
                    dataPoints.Add(time+frameTime, pointsNew);
                }
                else {
                    dataPoints[time+frameTime] = pointsNew;
                }
            }
            else {
                dataPoints.Add(time, pointsNew);
            }
        }
        public IEnumerator loadPointsData() {
            dataPoints.Clear();
            bool test = hasPointsFile();
            if (test) {
                OSC_Animation_Data_Points dataPointsList = ScriptableObject.CreateInstance<OSC_Animation_Data_Points>();

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(pointsFile, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), dataPointsList);
                file.Close();

                for (int i=0; i < dataPointsList.times.Count; i++) {
                    float time = dataPointsList.times[i];
                    pointsNew = new Dictionary<int, Vector4>();
                    int j=0;
                    foreach (Vector4 xyz in dataPointsList.points[i].xyz) {
                        pointsNew.Add(j, xyz);
                        j++;
                    }
                    dataPoints.Add(time, pointsNew);
                }
            }
            yield return null;
            yield break;
        }
        void savePointsData() {
            if (dataPoints.Count == 0) {
                bool test = hasPointsFile();
                if (test) {
                    File.Delete(pointsFile);
                    #if UNITY_EDITOR
                    File.Delete(pointsFile+".meta");
                    #endif
                }
                return;
            }

            OSC_Animation_Data_Points dataPointsList = ScriptableObject.CreateInstance<OSC_Animation_Data_Points>();
            Data_Points_Points pointsArr = new Data_Points_Points();

            dataPointsList.times = dataPoints.Keys.ToList();
            for (int i=0; i < dataPointsList.times.Count; i++) {
                pointsNew = new Dictionary<int, Vector4>(dataPoints[dataPointsList.times[i]]);
                pointsArr = new Data_Points_Points();
                pointsArr.xyz = pointsNew.Values.ToList();
                dataPointsList.points.Add(pointsArr);
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(MainSettingsVars.dataFolder+"/"+thisName+pointsFileSuffix);
            var json = JsonUtility.ToJson(dataPointsList);
            bf.Serialize(file, json);
            file.Close();

            dataPointsList.times.Clear();
            dataPointsList.points.Clear();
        }

        // Animation Data

        private Dictionary<int, Dictionary<string, AnimationCurve>> dataAnimation = new Dictionary<int, Dictionary<string, AnimationCurve>>();
        private Dictionary<int, Dictionary<string, List<float>>> dataAnimationKeyframeTimes = new Dictionary<int, Dictionary<string, List<float>>>();
		private AnimationClip animationClip;

        void dataAnimationAdd(int group, string field, AnimationCurve curve) {
            if (dataAnimation.ContainsKey(group)) {
                if (dataAnimation[group].ContainsKey(field)) {
                    dataAnimation[group][field] = curve;
                }
                else {
                    dataAnimation[group].Add(field, curve);
                }
            }
            else {
                Dictionary<string, AnimationCurve> entry = new Dictionary<string, AnimationCurve>(){
                    {field, curve}
                };
                dataAnimation.Add(group, entry);
            }
        }
        public void dataAnimationAddKey(int group, string field, dynamic time = null, dynamic value = null, dynamic valueOld = null) {
            // resolve value
            float val = 0.0f;
            if (value != null) {
                if (value.GetType() == typeof(float)) {
                    val = (float)value;
                }
                else if (value.GetType() == typeof(int)) {
                    val = (float)((int)value);
                }
                else if (value.GetType() == typeof(bool)) {
                    val = System.Convert.ToSingle(value);
                }
                else if (value.GetType().ToString().IndexOf("_halftheory") == 0) {
                    if (value.GetType().ToString().IndexOf("meshTopology") > 0) {
                        meshTopology myEnum = (meshTopology)System.Enum.Parse(typeof(meshTopology), value.ToString());
                        val = (float)myEnum;
                    }
                    else if (value.GetType().ToString().IndexOf("meshColor") > 0) {
                        meshColor myEnum = (meshColor)System.Enum.Parse(typeof(meshColor), value.ToString());
                        val = (float)myEnum;
                    }
                    else if (value.GetType().ToString().IndexOf("meshShader") > 0) {
                        meshShader myEnum = (meshShader)System.Enum.Parse(typeof(meshShader), value.ToString());
                        val = (float)myEnum;
                    }
                    else {
                        return;
                    }
                }
                else {
                    return;
                }
            }

            bool useValueOld = false;

            // resolve time
            float t = MainSettingsVars.time - activeTime;
            if (time != null && time.GetType() == typeof(float)) {
                t = (float)time;
            }
            else {
                // if already a keyframe at the frame, move to the previous/next frame
                if (!dataAnimationKeyframeTimes.ContainsKey(group)) {
                    dataAnimationKeyframeTimes.Add(group, new Dictionary<string, List<float>>());
                }
                if (!dataAnimationKeyframeTimes[group].ContainsKey(field)) {
                    dataAnimationKeyframeTimes[group].Add(field, new List<float>());
                    useValueOld = true;
                }
                if (dataAnimationKeyframeTimes[group][field].IndexOf(t) != -1) {
                    if (dataAnimationKeyframeTimes[group][field].IndexOf(t-frameTime) == -1 && t-frameTime >= 0.0f) {
                        t = t-frameTime;
                    }
                    else {
                        t = t+frameTime;
                    }
                }
                dataAnimationKeyframeTimes[group][field].Add(t);
            }

            // maybe use valueOld
            float valOld = 0.0f;
            if (useValueOld && t != 0.0f && valueOld != null) {
                if (valueOld.GetType() == typeof(float)) {
                    valOld = (float)valueOld;
                }
                else if (valueOld.GetType() == typeof(int)) {
                    valOld = (float)((int)valueOld);
                }
                else if (valueOld.GetType() == typeof(bool)) {
                    valOld = System.Convert.ToSingle(valueOld);
                }
                else if (valueOld.GetType().ToString().IndexOf("_halftheory") == 0) {
                    if (valueOld.GetType().ToString().IndexOf("meshTopology") > 0) {
                        meshTopology myEnum = (meshTopology)System.Enum.Parse(typeof(meshTopology), valueOld.ToString());
                        valOld = (float)myEnum;
                    }
                    else if (valueOld.GetType().ToString().IndexOf("meshColor") > 0) {
                        meshColor myEnum = (meshColor)System.Enum.Parse(typeof(meshColor), valueOld.ToString());
                        valOld = (float)myEnum;
                    }
                    else if (valueOld.GetType().ToString().IndexOf("meshShader") > 0) {
                        meshShader myEnum = (meshShader)System.Enum.Parse(typeof(meshShader), valueOld.ToString());
                        valOld = (float)myEnum;
                    }
                }
            }
            else {
                useValueOld = false;
            }

            // add keyframe to the curve
            if (dataAnimation.ContainsKey(group)) {
                if (dataAnimation[group].ContainsKey(field)) {
                    // with starting keyframe
                    if (dataAnimation[group][field].keys.Length == 0 && useValueOld) {
                        dataAnimation[group][field].AddKey(0.0f, valOld);
                    }
                    dataAnimation[group][field].AddKey(t, val);
                }
                else {
                    // with starting keyframe
                    if (useValueOld) {
                        dataAnimation[group].Add(field, new AnimationCurve(new Keyframe(0.0f, valOld), new Keyframe(t, val)));
                    }
                    else {
                        dataAnimation[group].Add(field, new AnimationCurve(new Keyframe(t, val)));
                    }
                }
            }
            else {
                Dictionary<string, AnimationCurve> entry;
                // with starting keyframe
                if (useValueOld) {
                    entry = new Dictionary<string, AnimationCurve>(){
                        {field, new AnimationCurve(new Keyframe(0.0f, valOld), new Keyframe(t, val))}
                    };
                }
                else {
                    entry = new Dictionary<string, AnimationCurve>(){
                        {field, new AnimationCurve(new Keyframe(t, val))}
                    };
                }
                dataAnimation.Add(group, entry);
            }
        }
        void dataAnimation2animationClip() {
            if (dataAnimation.Count == 0) {
                return;
            }
            Dictionary<string, AnimationCurve> groupCurves;
            string field;
            AnimationCurve curve;
            for (int i=0; i < meshComponents.Length; i++) {
                if (dataAnimation.TryGetValue(i, out groupCurves)) {
                    foreach (var pair in groupCurves) {
                        field = pair.Key;
                        curve = pair.Value;
                        // 1. set keyframe tangents
                        if (curve.keys.Length >= 2) {
                            // float = Linear (Flat) / Clamped Auto
                            if (meshComponents[i].GetType().GetField(field).FieldType == typeof(float)) {
                                for (int j=0; j < curve.keys.Length; ++j) {
                                    Keyframe key = curve[j];
                                    float inTangent = 0.0f;
                                    float outTangent = 0.0f;
                                    bool inTangent_set = false;
                                    bool outTangent_set = false;
                                    Vector2 point1, point2, deltapoint;
                                    if (j == 0) {
                                        inTangent_set = true;
                                    }
                                    if (j == curve.keys.Length - 1) {
                                        outTangent_set = true;
                                    }
                                    if (!inTangent_set) {
                                        point1.x = curve.keys[j - 1].time;
                                        point1.y = curve.keys[j - 1].value;
                                        point2.x = curve.keys[j].time;
                                        point2.y = curve.keys[j].value;
                                        deltapoint = point2 - point1;
                                        inTangent = deltapoint.y / deltapoint.x;
                                    }
                                    if (!outTangent_set) {
                                        point1.x = curve.keys[j].time;
                                        point1.y = curve.keys[j].value;
                                        point2.x = curve.keys[j + 1].time;
                                        point2.y = curve.keys[j + 1].value;
                                        deltapoint = point2 - point1;
                                        outTangent = deltapoint.y / deltapoint.x;
                                    }
                                    key.inTangent = inTangent;
                                    key.outTangent = outTangent;
                                    curve.MoveKey(j, key);
                                }
                            }
                            // int/bool/enum = Constant / broken
                            else {
                                for (int j=0; j < curve.keys.Length; ++j) {
                                    Keyframe key = curve[j];
                                    key.outTangent = Mathf.Infinity;
                                    curve.MoveKey(j, key);
                                }
                            }
                        }
                        // 2. add curves to the clip
                        animationClip.SetCurve(meshComponents[i].transform.name, meshComponents[i].GetType(), field, curve);
                    }
                }
            }
        }
        void animationClip2dataAnimation() {
            // editor: send curves to dataAnimation in case we changed them in editor
            #if UNITY_EDITOR
            dataAnimation.Clear();
            dataAnimationKeyframeTimes.Clear();
            int i; // group
            string field;
            AnimationCurve curve;
            foreach (var binding in AnimationUtility.GetCurveBindings(animationClip)) {
                if (binding.path.IndexOf("group") == 0) {
                    string[] parts = Regex.Split(binding.path, "group");
                    i = int.Parse(parts[1]);
                    field = binding.propertyName;
                    curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                    dataAnimationAdd(i, field, curve);
                    if (curve.keys.Length > 0) {
                        for (int j=0; j < curve.keys.Length; ++j) {
                            dataAnimationAddKey(i, field, curve.keys[j].time, curve.keys[j].value);
                        }
                    }
                }
            }
            #endif
        }
        public IEnumerator loadAnimationData() {
            dataAnimation.Clear();
            dataAnimationKeyframeTimes.Clear();

            // 1. make a clip
            animationClip = new AnimationClip();
            animationClip.name = thisName+"_"+MainSettingsVars.defaultAnimationClip;
            animationClip.wrapMode = WrapMode.Once;

            // 2. load curves from file
            bool hasFile = hasAnimationFile();
            if (hasFile) {
                OSC_Animation_Data_Animation dataAnimationList = ScriptableObject.CreateInstance<OSC_Animation_Data_Animation>();

                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(animationFile, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), dataAnimationList);
                file.Close();

                if (dataAnimationList.groups.Count > 0 && meshComponents.Length > 0) {
                    for (int i=0; i < dataAnimationList.groups.Count; i++) {
                        if (i > meshComponents.Length - 1) {
                            break;
                        }
                        for (int j=0; j < dataAnimationList.groups[i].fields.Count; j++) {
                            dataAnimationAdd(i, dataAnimationList.groups[i].fields[j], dataAnimationList.groups[i].curves[j]);
                        }
                    }
                }
            }

            // 3. add all fields from all children
            //object val;
            for (int i=0; i < meshComponents.Length; i++) {
                foreach (var field in meshComponents[i].GetType().GetFields()) {
                    // already exists?
                    if (dataAnimation.ContainsKey(i)) {
                        if (dataAnimation[i].ContainsKey(field.Name)) {
                            continue;
                        }
                    }
                    if (!field.IsPublic) {
                        continue;
                    }
                    if (field.Name.IndexOf("_") != 0) {
                        continue;
                    }
                    //val = field.GetValue(meshComponents[i]);
                    //dataAnimationAddKey(i, field.Name, 0.0f, val);
                    dataAnimationAdd(i, field.Name, new AnimationCurve());
                }
            }

            // 4. place in the scene
            animationClip.legacy = true;
            animationComponent.clip = animationClip;
            animationComponent.AddClip(animationComponent.clip, animationClip.name);
            dataAnimation2animationClip();

        	yield return null;
			yield break;
        }
        void saveAnimationData() {
            animationClip2dataAnimation();

            OSC_Animation_Data_Animation dataAnimationList = ScriptableObject.CreateInstance<OSC_Animation_Data_Animation>();

            if (dataAnimation.Count > 0) {
                Dictionary<string, AnimationCurve> groupCurves;
                string field;
                AnimationCurve curve;
                for (int i=0; i < meshComponents.Length; i++) {
                    if (dataAnimation.TryGetValue(i, out groupCurves)) {
                        foreach (var pair in groupCurves) {
                            field = pair.Key;
                            curve = pair.Value;
                            if (curve.keys.Length == 0) {
                                continue;
                            }
                            else if (curve.keys.Length == 1) {
                                if (curve[0].time == 0.0f) {
                                    continue;
                                }
                            }
                            if (i > dataAnimationList.groups.Count - 1) {
                                dataAnimationList.groups.Add(new Data_Animation_Groups());
                            }
                            dataAnimationList.groups[i].fields.Add(field);
                            dataAnimationList.groups[i].curves.Add(curve);
                        }
                    }
                }
            }

            if (dataAnimationList.groups.Count == 0) {
                bool test = hasAnimationFile();
                if (test) {
                    File.Delete(animationFile);
                    #if UNITY_EDITOR
                    File.Delete(animationFile+".meta");
                    #endif
                }
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(MainSettingsVars.dataFolder+"/"+thisName+animationFileSuffix);
            var json = JsonUtility.ToJson(dataAnimationList);
            bf.Serialize(file, json);
            file.Close();

            dataAnimationList.groups.Clear();
        }

        // Operation

		IEnumerator Initialize() {
			if (initialized) {
				yield break;
			}
			while(!MainSettingsVars.initialized) {
			     yield return null;
			}
            animationComponent = GetComponent<Animation>();
            if (animationComponent == null) {
                yield break;
            }
            bool test = hasData();
            if (!test) {
                yield break;
            }
			initialized = true;
            yield return null;
			yield break;
		}
        IEnumerator Start() {
            yield return StartCoroutine(Initialize());
            settingsFPS();
            settingsCamera();
            setCamera();
            meshCalculatePointsStart();
        }
        void OnEnable() {
            StartCoroutine(Initialize());
            settingsFPS();
            settingsCamera();
            setCamera();
            meshCalculatePointsStart();
        }
        void OnDisable() {
            meshCalculatePointsStop();
            stopAndSave();
        }
        void OnDestroy() {
            meshCalculatePointsStop();
            active = false;
        }
        public void stopAndSave() {
            // stop
            active = false;
            // save
            if (initialized) {
                saveData();
            }
        }

        // settings from Max values

        private float frameTime;

		public void settingsFPS() {
			if (!initialized || !data.initialized || !current) {
				return;
			}
			frameTime = 1f / (float)data.fps;
			if (MainSettingsVars.forceFPS) {
				if (Application.targetFrameRate != data.fps) {
					Application.targetFrameRate = data.fps;
				}
			}
		}

        private Vector3 maincameraPosition, lightPosition;

		public void settingsCamera() {
			if (!initialized || !data.initialized || !current) {
				return;
			}
			// main cam is center x,y centered and behind highest(frontmost) z
			float z_diff = Mathf.Abs(data.z_high - data.z_center);
			float mainCam_z = data.z_high - z_diff;
			if (data.z_high > data.z_center) {
				mainCam_z = data.z_high + z_diff;
			}
			maincameraPosition = new Vector3(data.x_center, data.y_center, mainCam_z);
			// light above cam
			if (MainSettingsVars.lightObject != null) {
				float y_diff = Mathf.Abs(data.y_high - data.y_center);
				float light_y = data.y_high + y_diff;
				if (data.y_high < data.y_center) {
					light_y = data.y_high - y_diff;
				}
				lightPosition = new Vector3(data.x_center, light_y, mainCam_z);
			}
		}
        public void setCamera() {
            if (!initialized || !data.initialized || !current) {
                return;
            }
            if (data.camera_position == Vector3.zero && data.camera_eulerAngles == Vector3.zero) {
                resetCamera();
                return;
            }
            // this object is 0
            transform.position = Vector3.zero;
            // root object is 0
            transform.parent.position = Vector3.zero;
            // main cam from data
            MainSettingsVars.maincameraObject.transform.position = data.camera_position;
            MainSettingsVars.maincameraObject.transform.eulerAngles = data.camera_eulerAngles;
            // light above cam
            if (MainSettingsVars.lightObject != null) {
                MainSettingsVars.lightObject.transform.position = transform.TransformPoint(lightPosition);
            }
        }
        public void resetCamera() {
            if (!initialized || !data.initialized || !current) {
                return;
            }
            // this object is 0
            transform.position = Vector3.zero;
            // root object is 0
            transform.parent.position = Vector3.zero;
            // main cam is center x,y centered and behind highest(frontmost) z
            MainSettingsVars.maincameraObject.transform.position = transform.TransformPoint(maincameraPosition);
            MainSettingsVars.maincameraObject.transform.rotation = Quaternion.identity;
            // light above cam
            if (MainSettingsVars.lightObject != null) {
                MainSettingsVars.lightObject.transform.position = transform.TransformPoint(lightPosition);
            }
        }

        // live

		[HideInInspector] public Dictionary<int, Vector4> points = new Dictionary<int, Vector4>();
        private float pointTime;
		private Vector4 xyz;

		public void collectPoints(int point, float x, float y, float z) { // this function waits for OSC data
			if (!initialized || !data.initialized || !current) {
				return;
			}
	        pointTime = MainSettingsVars.time - activeTime;
			xyz = new Vector4(x,y,z,pointTime);
			if (points.ContainsKey(point)) {
				points[point] = xyz;
			}
			else {
				points.Add(point, xyz);
			}

			// record
			if (MainSettingsVars.data.currentGameMode == gameMode.record_points && active) {
				// dataPoints - add when received full set of points
				if (point == (meshPeaks.Values.Max() - 1)) {
                    dataPointsAdd(pointTime);
				}
			}
		}

        // play

        private SortedDictionary<float, Dictionary<int, Vector4>> dataPointsNew = new SortedDictionary<float, Dictionary<int, Vector4>>();

        IEnumerator playStart() {
            if (dataPoints.Count == 0) {
                active = false;
                yield break;
            }
            // audio
            if (MainSettingsVars.audiosourceComponent != null && !string.IsNullOrEmpty(data.audio_file_path)) {
                if (MainSettingsVars.currentAudioFile != data.audio_file_path && File.Exists(data.audio_file_path)) {
                    AudioType audioType;
                    if (data.audio_file_path.IndexOf(".wav") > 0) {
                        audioType = AudioType.WAV;
                    }
                    else {
                        audioType = AudioType.AIFF;
                    }
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://"+data.audio_file_path, audioType)) {       
                        yield return www.Send();
                        if (www.isNetworkError) {
                            UnityEngine.Debug.Log("HALFTHEORY: "+this.GetType()+": playStart: www.isNetworkError: "+www.error);
                        }
                        else {
                            MainSettingsVars.audiosourceComponent.clip = DownloadHandlerAudioClip.GetContent(www);
                            MainSettingsVars.audiosourceComponent.clip.name = data.audio_file_path;
                            MainSettingsVars.currentAudioFile = data.audio_file_path;
                        }
                    }
                    yield return null;
                }
                MainSettingsVars.audiosourceComponent.Play();
            }
            // animation
            if (MainSettingsVars.data.currentGameMode != gameMode.record_animation) {
                animationComponent.Play();
            }
            // points
            dataPointsNew = new SortedDictionary<float, Dictionary<int, Vector4>>(dataPoints);
            activeTime = MainSettingsVars.time;
            InvokeRepeating("playPoints", MainSettingsVars.repeatInterval, MainSettingsVars.repeatInterval);
            yield break;
        }
        void playPoints() { // also used by record_animation, render
            // can we stop?
            if (dataPointsNew.Count == 0) {
                // still audio
                if (MainSettingsVars.audiosourceComponent != null) {
                    if (MainSettingsVars.audiosourceComponent.isPlaying) {
                        return;
                    }
                }
                // still echoes
                for (int i=0; i < meshComponents.Length; i++) {
                    if (meshComponents[i].transform.childCount > 0) {
                        return;
                    }
                }
                active = false;
                return;
            }
            // check for points before now
            pointTime = MainSettingsVars.time - activeTime;
            if (dataPointsNew.Keys.First() <= pointTime) {
                points = new Dictionary<int, Vector4>(dataPointsNew.Values.First());
                dataPointsNew.Remove(dataPointsNew.Keys.First());
            }
        }
        void playStop() {
            if (dataPoints.Count == 0) {
                return;
            }
            // audio
            if (MainSettingsVars.audiosourceComponent != null) {
                if (MainSettingsVars.audiosourceComponent.isPlaying) {
                    MainSettingsVars.audiosourceComponent.Stop();
                }
            }
            // animation
            if (animationComponent.isPlaying) {
                animationComponent.Stop();
                animationComponent.Rewind();
                animationClip.SampleAnimation(this.gameObject, 0.0f);
            }
            // points
            CancelInvoke("playPoints");
            dataPointsNew.Clear();
        }

        // render

        [HideInInspector] public int frameNumber = -1;
        [HideInInspector] public float totalFrames; // estimated
        private Dictionary<int,RenderCamera> renderCameras = new Dictionary<int,RenderCamera>();
        private RenderCamera renderCamera;
        private float maximumDeltaTime_backup;
        private string _ffmpegPath;
        private string ffmpegPath {
            get {
                if (string.IsNullOrEmpty(_ffmpegPath)) {
                    string pathEnd = "";
                    var platform = UnityEngine.Application.platform;
                    if (platform == UnityEngine.RuntimePlatform.OSXPlayer || platform == UnityEngine.RuntimePlatform.OSXEditor) {
                        pathEnd = "ffmpeg/osx/ffmpeg";
                    }
                    else if (platform == UnityEngine.RuntimePlatform.LinuxPlayer || platform == UnityEngine.RuntimePlatform.LinuxEditor) {
                        pathEnd = "ffmpeg/linux/ffmpeg";
                    }
                    else if (platform == UnityEngine.RuntimePlatform.WindowsPlayer || platform == UnityEngine.RuntimePlatform.WindowsEditor) {
                        pathEnd = "ffmpeg/win/ffmpeg.exe";
                    }
                    else {
                        return _ffmpegPath;
                    }
                    _ffmpegPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, pathEnd);
                }
                return _ffmpegPath;
            }
            set { _ffmpegPath = value; }
        }
        Thread ffmpegThread;
        Process ffmpegProcess;
        private bool ffmpegThreadDone = false;

        void renderStart() {
            if (dataPoints.Count == 0) {
                active = false;
                return;
            }
            bool test = MainSettingsVars.hasRecordFolder();
            if (!test) {
                active = false;
                return;
            }
            if (!File.Exists(ffmpegPath)) {
                active = false;
                return;
            }

            // setup render vars
            renderCameras.Clear();
            for (int i=0; i < MainSettingsVars.renderCamerasNames.Count; i++) {
                renderCamera = new RenderCamera(i, thisName);
                if (renderCamera.initialized) {
                    renderCameras.Add(i, renderCamera);
                }
            }
            if (renderCameras.Count == 0) {
                active = false;
                return;
            }
            
            // set cameras
            foreach (var pair in renderCameras) {
                renderCamera = pair.Value;
                renderCamera.Setup();
            }

            // kill echoes
            for (int i=0; i < meshComponents.Length; i++) {
                if (meshComponents[i].transform.childCount > 0) {
                    foreach (Transform child in meshComponents[i].transform) {
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    }
                }
            }

            // set fps
            maximumDeltaTime_backup = Time.maximumDeltaTime;
            Time.maximumDeltaTime = (1.0f / (float)data.fps);
            Time.captureFramerate = data.fps;

            // animation
            animationComponent.Play();
            // points
            dataPointsNew = new SortedDictionary<float, Dictionary<int, Vector4>>(dataPoints);
            totalFrames = dataPointsNew.Keys.Last() / frameTime;
            activeTime = MainSettingsVars.time;
            InvokeRepeating("playPoints", MainSettingsVars.repeatInterval, MainSettingsVars.repeatInterval);

            frameNumber = 0;
        }
        void LateUpdate() {
            if (!initialized || !data.initialized || !current) {
                return;
            }
            if (MainSettingsVars.data.currentGameMode == gameMode.render && active) {
                StartCoroutine(renderFrame());
            }
        }
        IEnumerator renderFrame() {
            if (frameNumber == -1) {
                yield break;
            }
            yield return new WaitForEndOfFrame();

            foreach (var pair in renderCameras) {
                renderCamera = pair.Value;
                Texture2D tex = renderCamera.GetTexture();
                // write file
                byte[] bytes;
                bytes = tex.EncodeToPNG();
                Destroy(tex);
                tex = null;
                var frameFile = Path.Combine(MainSettingsVars.recordFolder, renderCamera.pngFile+"_"+frameNumber.ToString("D5")+".png");
                File.WriteAllBytes(frameFile, bytes);
            }

            frameNumber++;
            yield break;
        }
        IEnumerator renderStop() {
            if (dataPoints.Count == 0) {
                frameNumber = -1;
                yield break;
            }
            yield return new WaitForEndOfFrame();

            // reset fps
            Time.maximumDeltaTime = maximumDeltaTime_backup;
            Time.captureFramerate = 0;

            // reset cameras
            foreach (var pair in renderCameras) {
                renderCamera = pair.Value;
                renderCamera.Reset();
            }

            // animation
            if (animationComponent.isPlaying) {
                animationComponent.Stop();
                animationComponent.Rewind();
                animationClip.SampleAnimation(this.gameObject, 0.0f);
            }
            // points
            CancelInvoke("playPoints");
            dataPointsNew.Clear();

            if (frameNumber <= 0) {
                frameNumber = -1;
                yield break;
            }

            //yield break;

            // save
            if (MainSettingsVars.guiComponent != null) {
                MainSettingsVars.guiComponent.guiSaving = true;
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }
            // new thread
            ffmpegThreadDone = false;
            ffmpegThread = new Thread(ffmpegThreadAction);
            ffmpegThread.IsBackground = true;
            ffmpegThread.Priority = System.Threading.ThreadPriority.BelowNormal;
            ffmpegThread.Start();
            while (!ffmpegThreadDone) {
                yield return null;
            }
            ffmpegThread.Join();
            ffmpegThread.Abort();
            ffmpegThread = null;
            // stop save
            renderCameras.Clear();
            if (MainSettingsVars.guiComponent != null) {
                MainSettingsVars.guiComponent.guiSaving = false;
            }

            frameNumber = -1;
            yield break;
        }
        void ffmpegThreadAction() {
            foreach (var pair in renderCameras) {
                renderCamera = pair.Value;
                // start ffmpeg subprocess
                string arguments;
                if (!string.IsNullOrEmpty(data.audio_file_path) && File.Exists(data.audio_file_path)) {
                    arguments = "-y -framerate "+data.fps+" -i "+renderCamera.pngFile+"_%05d.png -i \""+data.audio_file_path+"\" -c:v "+MainSettingsVars.renderCodecs[MainSettingsVars.data.renderCodec]+" -vf fps="+data.fps+" -c:a copy "+renderCamera.movFile;
                }
                else {
                    arguments = "-y -framerate "+data.fps+" -i "+renderCamera.pngFile+"_%05d.png -c:v "+MainSettingsVars.renderCodecs[MainSettingsVars.data.renderCodec]+" -vf fps="+data.fps+" "+renderCamera.movFile;
                }
                ffmpegProcess = Process.Start(new ProcessStartInfo {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    WorkingDirectory = MainSettingsVars.recordFolder,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
                ffmpegProcess.PriorityClass = ProcessPriorityClass.Idle;
                while (!File.Exists(renderCamera.movFile)) {
                    Thread.Sleep(100);
                }
                // stop ffmpeg subprocess
                ffmpegProcess.StandardInput.Close();
                ffmpegProcess.WaitForExit();
                ffmpegProcess.Close();
                ffmpegProcess.Dispose();
                ffmpegProcess = null;
                // cleanup frames
                for (int i=0; i < (frameNumber + 4); i++) {
                    var frameFile = Path.Combine(MainSettingsVars.recordFolder, renderCamera.pngFile+"_"+i.ToString("D5")+".png");
                    if (!File.Exists(frameFile)) {
                        continue;
                    }
                    File.Delete(frameFile);
                }
            }
            ffmpegThreadDone = true;
        }

	}
}