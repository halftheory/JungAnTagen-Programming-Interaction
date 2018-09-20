using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace _halftheory {

	public class OSC_Animation_Data : ScriptableObject {

		public bool initialized = false;

		public int fps = 30;
		public float x_low;
		public float x_high;
		public float y_low;
		public float y_high;
		public float z_low;
		public float z_high;

	    public float x_center;
	    public float y_center;
	    public float z_center;

		public List<string> meshData = new List<string>();

		public bool isInit() {
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
				getCenterX();
				getCenterY();
				getCenterZ();
	            Debug.Log("HALFTHEORY: "+this.GetType()+": data.isInit OK");
			}
			initialized = res;
			return res;
		}

		void getCenterX() {
		    x_center = (x_low + x_high) / 2f;
		}
		void getCenterY() {
		    y_center = (y_low + y_high) / 2f;
		}
		void getCenterZ() {
		    z_center = (z_low + z_high) / 2f;
		}
	}

	public class OSC_Animation : MonoBehaviour {

		public bool initialized = false;
		public bool current = false;

		public OSC_Animation_Data data;
        private string dataFilePathSuffix = "_Data.json";
        public string dataFilePath;

        // child
        private GameObject currentMeshObject;
        private OSC_Mesh currentMeshComponent;

		void Awake() {
			QualitySettings.vSyncCount = 0;
		}

		void Start() {
			bool test = isCurrent();
			if (test) {
				test = data.isInit();
				if (test) {
					SettingsFPS();
					SettingsCamera();
				}
			}
		}

		void OnEnable() {
			bool test = isCurrent();
			if (test) {
				test = data.isInit();
				if (test) {
					SettingsFPS();
					SettingsCamera();
				}
			}
		}

		void LateUpdate() {
			bool test = isCurrent();
			if (test) {
	            if (data.initialized) {
	            	return;
	            }
				else if (Application.isPlaying) {
					test = data.isInit();
					if (test) {
						SettingsFPS();
						SettingsCamera();
					}
				}
			}
		}

		private bool isInit() {
			if (initialized) {
				return (true);
			}
			if (!MainSettingsVars.initialized) {
				return (false);
			}
			if (!this.enabled) {
				return (false);
			}
			bool test = hasData();
			if (!test) {
				return (false);
			}
			initialized = true;
			return (true);
		}

		private bool isCurrent() {
			bool res = true;
			bool test = isInit();
			if (!test) {
				res = false;
			}
			if (MainSettingsVars.currentAnimationComponent != this) {
				res = false;
			}
			current = res;
			return res;
		}

        private bool hasData() {
            if (data != null) {
                return (true);
            }
            data = ScriptableObject.CreateInstance<OSC_Animation_Data>();
            dataFilePath = MainSettingsVars.dataPath+"/"+this.gameObject.name+dataFilePathSuffix; 
            if (File.Exists(dataFilePath)) {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataFilePath, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), data);
                file.Close();
	        	data.initialized = false;
                loadMeshData();
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

        void saveData() {
            if (data == null) {
                return;
            }
            saveMeshData();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataFilePath);
            var json = JsonUtility.ToJson(data);
            bf.Serialize(file, json);
            file.Close();
        }

        void OnDisable() {
            if (initialized) {
                saveData();
            }
        }

        void loadMeshData() {
        	if (data.meshData.Count == 0) {
        		return;
        	}
			foreach (var d in data.meshData) {
				GameObject newMesh = new GameObject("newMesh", typeof(OSC_Mesh));
				newMesh.transform.parent = transform;
				JsonUtility.FromJsonOverwrite((string)d, newMesh.GetComponent<OSC_Mesh>().data);
				if (string.IsNullOrEmpty(newMesh.GetComponent<OSC_Mesh>().data.objectName)) {
					Destroy(newMesh);
				}
				else {
					newMesh.name = newMesh.GetComponent<OSC_Mesh>().data.objectName;
				}
			}
        }
        void saveMeshData() {
			data.meshData = new List<string>();
			if (transform.childCount == 0) {
        		return;
        	}
			foreach (Transform child in transform) {
				child.gameObject.GetComponent<OSC_Mesh>().data.objectName = child.name;
				var json = JsonUtility.ToJson(child.gameObject.GetComponent<OSC_Mesh>().data);
				data.meshData.Add(json);
			}
        }
        public void deleteMeshData() {
			data.meshData = new List<string>();
			if (transform.childCount == 0) {
        		return;
        	}
			foreach (Transform child in transform) {
				Destroy(child.gameObject);
			}
        }

		void SettingsFPS() {
			if (Application.targetFrameRate != data.fps) {
				Application.targetFrameRate = data.fps;
			}
		}

		void SettingsCamera() {
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

		public void collectPoints(string address, float x, float y, float z) {
			if (!initialized || !data.initialized || !current) {
				return;
			}
			// start recording?

			string[] parts = address.Split('/');

			string meshName = parts[1];
            if (string.IsNullOrEmpty(meshName)) {
	            Debug.Log("HALFTHEORY: "+this.GetType()+": collectPoints failed on meshName: "+meshName);
				return;
            }

			int meshPoint = int.Parse(parts[parts.Length - 1]);
			if (meshPoint < 0) {
	            Debug.Log("HALFTHEORY: "+this.GetType()+": collectPoints failed on meshPoint: "+meshPoint);
				return;
			}

			// try currentMeshObject
			if (currentMeshObject != null) {
				if (currentMeshObject.name == meshName) {
					currentMeshComponent.collectPoints(meshPoint, x, y, z);
					return;
				}
			}
			// try existing child mesh
			if (transform.childCount > 0) {
				Transform testTransform = transform.Find(meshName);
				if (testTransform) {
					currentMeshObject = testTransform.gameObject;
					currentMeshComponent = currentMeshObject.GetComponent<OSC_Mesh>();
					currentMeshComponent.collectPoints(meshPoint, x, y, z);
					return;
				}
			}
			// add new child
			GameObject newMesh = new GameObject(meshName, typeof(OSC_Mesh));
			newMesh.transform.parent = transform;
			if (currentMeshObject != null) {
				newMesh.GetComponent<OSC_Mesh>().data = currentMeshComponent.data;
			}
			currentMeshObject = newMesh;
			currentMeshComponent = currentMeshObject.GetComponent<OSC_Mesh>();
			currentMeshComponent.collectPoints(meshPoint, x, y, z);
            Debug.Log("HALFTHEORY: "+this.GetType()+": collectPoints newMesh : "+meshName);
		}
	}
}