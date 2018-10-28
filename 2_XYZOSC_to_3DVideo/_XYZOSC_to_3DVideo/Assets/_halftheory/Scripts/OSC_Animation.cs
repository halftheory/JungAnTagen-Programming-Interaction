using UnityEngine;
using Unity.Collections;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace _halftheory {

	public class OSC_Animation_Data : ScriptableObject {

		public bool initialized = false;

		public string audio_file_path;
		public int _fps = 60;
		public int fps {
            get { return _fps; }
            set { _fps = value;
            	Initialize();
            	if (initialized) {
            		MainSettingsVars.currentAnimationComponent.SettingsFPS();
            	}
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

	    public float x_center;
	    public float y_center;
	    public float z_center;
		public Vector3 worldCenter;

		public List<string> meshData = new List<string>();

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
				worldCenter = MainSettingsVars.currentAnimationObject.transform.TransformPoint(new Vector3(x_center, y_center, z_center));
			}
			else {
	            Debug.Log("HALFTHEORY: "+this.GetType()+": data.Initialize failed for "+MainSettingsVars.data.currentAnimation);
			}
			initialized = res;
		}
	}

    [RequireComponent(typeof(Animation))]
	public class OSC_Animation : MonoBehaviour {

		public bool current = false;
		public bool initialized = false;

        /* DATA */
		public OSC_Animation_Data data;
        private string dataFileSuffix = "_Data.json";
        public string dataFile;
        private string animationFileSuffix = "_Animation.json";
        public string animationFile;
        private string pointsFileSuffix = "_Points.json";
        public string pointsFile;

		[HideInInspector] public float activeTime;
		[HideInInspector] public float frameTime;
		private bool _active = false;
		public bool active {
            get { return _active; }
            set {
                if (_active != value) {
                	if (value) {
						activeTime = Time.unscaledTime;
                	}
                	// live
                	if (MainSettingsVars.data.currentGameMode == gameMode.live) {
                		if (value) {
							points.Clear();
							SettingsFPS();
							SettingsCamera();
						}
					}
                }
				_active = value;
            }
        }

        public bool loadData() {
            data = ScriptableObject.CreateInstance<OSC_Animation_Data>();
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
				data.Initialize();
                loadMeshData();
                return (true);
            }
            return (false);
        }
        public void saveData() {
            if (data == null) {
                return;
            }
            saveMeshData();
            bool test = hasDataFolder();
            if (test) {
            	BinaryFormatter bf = new BinaryFormatter();
            	FileStream file = File.Create(dataFile);
            	var json = JsonUtility.ToJson(data);
            	bf.Serialize(file, json);
            	file.Close();
            }
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
		public bool hasDataFolder() {
			if (!string.IsNullOrEmpty(dataFile)) {
				return (true);
			}
            bool test = MainSettingsVars.hasDataFolder();
            if (!test) {
                return (false);
            }
            dataFile = MainSettingsVars.dataFolder+"/"+this.gameObject.name+dataFileSuffix;
            animationFile = MainSettingsVars.dataFolder+"/"+this.gameObject.name+animationFileSuffix;
            pointsFile = MainSettingsVars.dataFolder+"/"+this.gameObject.name+pointsFileSuffix;
            return (true);
		}
        public bool hasDataFile() {
            bool test = hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(dataFile)) {
	            return (true);
            }
            return (false);
        }
        public bool hasAnimationFile() {
            bool test = hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(animationFile)) {
	            return (true);
            }
            return (false);
        }
        public bool hasPointsFile() {
            bool test = hasDataFolder();
            if (!test) {
                return (false);
            }
            if (File.Exists(pointsFile)) {
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
        	test = hasAnimationFile();
        	if (test) {
        		File.Delete(animationFile);
        		#if UNITY_EDITOR
        		File.Delete(animationFile+".meta");
        		#endif
        	}
        	test = hasPointsFile();
        	if (test) {
        		File.Delete(pointsFile);
        		#if UNITY_EDITOR
        		File.Delete(pointsFile+".meta");
        		#endif
        	}
        }

        // Mesh Data

        [HideInInspector] public OSC_Mesh[] meshComponents = new OSC_Mesh[MainSettingsVars.groupsLength];
		private Dictionary<int, int> meshPeaks = new Dictionary<int, int>();

		public void meshPeaksAdd(int group, int peaks) {
			if (meshPeaks.ContainsKey(group)) {
				meshPeaks[group] = peaks;
			}
			else {
				meshPeaks.Add(group, peaks);
			}
		}
        void loadMeshData() {
        	if (transform.childCount == MainSettingsVars.groupsLength) {
        		return;
        	}
        	GameObject newObj;
			for (int i=0; i < MainSettingsVars.groupsLength; i++) {
				string objName = "group"+i;
                Transform testTransform = transform.Find(objName);
                if (testTransform) {
                    DestroyImmediate(testTransform.gameObject);
                }
                newObj = new GameObject(objName, typeof(OSC_Mesh));
                meshComponents[i] = newObj.GetComponent<OSC_Mesh>();
        		if (data.meshData.Count > i) {
					JsonUtility.FromJsonOverwrite((string)data.meshData[i], meshComponents[i].data);
					meshComponents[i].loadData();
				}
                newObj.transform.parent = transform;
                meshPeaksAdd(i, meshComponents[i].data.peaks);
        	}
        }
        void saveMeshData() {
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

        // Animation Data

        // Points Data

        // Operation

        void stopAndSave() {
        	// stop
        	active = false;
        	// save
        	if (initialized) {
        		saveData();
        	}
        }

		IEnumerator Initialize() {
			if (initialized) {
				yield break;
			}
			while(!MainSettingsVars.initialized) {
			     yield return null;
			}
			if (!this.enabled) {
				yield break;
			}
			if (!current) {
				yield break;
			}
			bool test = hasData();
			yield return null;
			if (!test) {
				yield break;
			}
			initialized = true;
			yield break;
		}

		void OnEnable() {
            StartCoroutine(Initialize());
			SettingsFPS();
			SettingsCamera();
		}
		IEnumerator Start() {
            yield return StartCoroutine(Initialize());
			SettingsFPS();
			SettingsCamera();
		}
		void LateUpdate() {
			if (!current) {
				return;
			}
			// more initializations
			// gameMode {live, record, play, render}
		}
        void OnDisable() {
			 stopAndSave();
        }

		public void SettingsFPS() {
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
		public void SettingsCamera() {
			if (!initialized || !data.initialized || !current) {
				return;
			}
			// this object is 0
			transform.position = Vector3.zero;
			// root object is 0
			transform.parent.position = Vector3.zero;
			// main cam is center x,y centered and behind highest(frontmost) z
			float z_diff = Mathf.Abs(data.z_high - data.z_center);
			float mainCam_z = data.z_high - z_diff;
			if (data.z_high > data.z_center) {
				mainCam_z = data.z_high + z_diff;
			}
			MainSettingsVars.mainCamObject.transform.position = transform.TransformPoint(new Vector3(data.x_center, data.y_center, mainCam_z));
			// light above cam
			if (MainSettingsVars.lightObject != null) {
				float y_diff = Mathf.Abs(data.y_high - data.y_center);
				float light_y = data.y_high + y_diff;
				if (data.y_high < data.y_center) {
					light_y = data.y_high - y_diff;
				}
				MainSettingsVars.lightObject.transform.position = transform.TransformPoint(new Vector3(data.x_center, light_y, mainCam_z));
			}
		}

		[HideInInspector] public Dictionary<int, Vector4> points = new Dictionary<int, Vector4>();
		private Vector4 xyz;
		private float pointTime;

		public void collectPoints(int point, float x, float y, float z) {
			if (!initialized || !data.initialized || !current) {
				return;
			}
	        pointTime = Time.unscaledTime - activeTime;
			xyz = new Vector4(x,y,z,pointTime);
			if (points.ContainsKey(point)) {
				points[point] = xyz;
			}
			else {
				points.Add(point, xyz);
			}
			// record
            //Debug.Log("HALFTHEORY: "+this.GetType()+": collectPoints: "+point+" "+x+" "+y+" "+z);
		}

	}
}