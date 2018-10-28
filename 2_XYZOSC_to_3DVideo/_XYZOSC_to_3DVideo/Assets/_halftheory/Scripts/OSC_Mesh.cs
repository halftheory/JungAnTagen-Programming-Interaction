using UnityEngine;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _halftheory {

	public enum meshColor {white, red, black, blue, cyan, green, grey, magenta, yellow}
	public enum meshShader {Color, Standard, Transparent}

	public class OSC_Mesh_Data : ScriptableObject {
		public bool active = false;
		public int peaks = 0;
		public float level = 0.0f;
		public MeshTopology meshTopologySelect = MeshTopology.LineStrip;
		public meshColor meshColorSelect = meshColor.white;
		public meshShader meshShaderSelect = meshShader.Color;
		public float alpha = 1f;
		public bool randomX = false;
		public bool randomY = false;
		public float rotateSpeed = 0.0f;
		public float smoothTime = 0.0f;
		public float clearTime = 0.0f;
		public bool noClearTime = false;
		public float traceTime = 0.0f;
	}

	[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
	public class OSC_Mesh : MonoBehaviour {

		private OSC_Animation parentAnimation;
		private int meshNumber;
	    public Mesh mesh;
	    public Material material;
		public OSC_Mesh_Data data;

		// use these in operation, only load/save to data
		public bool active;
		public int peaks;
		public float level;
		public MeshTopology meshTopologySelect;
		public meshColor meshColorSelect;
		public meshShader meshShaderSelect;
		public float alpha;
		public bool randomX;
		public bool randomY;
		public float rotateSpeed;
		public float smoothTime;
		public float clearTime;
		public bool noClearTime;
		public float traceTime;

		// data
        public void loadData() {
			if (data == null) {
            	data = ScriptableObject.CreateInstance<OSC_Mesh_Data>();
            }
			if (data != null) {
	        	active = data.active;
	        	peaks = data.peaks;
	        	level = data.level;
	        	meshTopologySelect = data.meshTopologySelect;
	        	meshColorSelect = data.meshColorSelect;
	        	meshShaderSelect = data.meshShaderSelect;
	        	alpha = data.alpha;
	        	randomX = data.randomX;
	        	randomY = data.randomY;
	        	rotateSpeed = data.rotateSpeed;
	        	smoothTime = data.smoothTime;
	        	clearTime = data.clearTime;
	        	noClearTime = data.noClearTime;
	        	traceTime = data.traceTime;
	        }
        }
        public void saveData() {
			if (data != null) {
	        	data.active = active;
	        	data.peaks = peaks;
	        	data.level = level;
	        	data.meshTopologySelect = meshTopologySelect;
	        	data.meshColorSelect = meshColorSelect;
	        	data.meshShaderSelect = meshShaderSelect;
	        	data.alpha = alpha;
	        	data.randomX = randomX;
	        	data.randomY = randomY;
	        	data.rotateSpeed = rotateSpeed;
	        	data.smoothTime = smoothTime;
	        	data.clearTime = clearTime;
	        	data.noClearTime = noClearTime;
	        	data.traceTime = traceTime;
	        }
        }

        // operation
		void Start() {
			meshNumber = transform.GetSiblingIndex();
			parentAnimation = (OSC_Animation)this.GetComponentInParent(typeof(OSC_Animation));
			if (mesh == null && GetComponent<MeshFilter>() && GetComponent<MeshRenderer>()) {
			    mesh = new Mesh();
			    //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			    mesh.MarkDynamic();
			    GetComponent<MeshFilter>().mesh = mesh;
				setShader(true);
				setColor(true);
			}
			if (parentAnimation != null && mesh != null) {
				InvokeRepeating("calculatePoints", MainSettingsVars.repeatInterval, MainSettingsVars.repeatInterval);
				// maybe have to slow this to once per frame for render??
			}
        }
		void OnEnable() {
			loadData();
		}
        void OnDisable() {
        	saveData();
        }

        // shader
		private Dictionary<meshShader, string> meshShaderArr = new Dictionary<meshShader, string>(){
			{meshShader.Color, "Unlit/Color"},
			{meshShader.Standard, "Standard"},
			{meshShader.Transparent, "Transparent/Diffuse"}
		};
		private string currentShader;
        void setShader(bool force = false) {
        	if (currentShader != meshShaderArr[meshShaderSelect] || force) {
        		currentShader = meshShaderArr[meshShaderSelect];
        		material = new Material(Shader.Find(currentShader));
        		setColor(true);
        	}
        }

        // color
		private Dictionary<meshColor, Color> meshColorArr = new Dictionary<meshColor, Color>(){
			{meshColor.white, Color.white},
			{meshColor.red, Color.red},
			{meshColor.black, Color.black},
			{meshColor.blue, Color.blue},
			{meshColor.cyan, Color.cyan},
			{meshColor.green, Color.green},
			{meshColor.grey, Color.grey},
			{meshColor.magenta, Color.magenta},
			{meshColor.yellow, Color.yellow}
		};
		private Color currentColor;
        void setColor(bool force = false) {
        	if (currentColor != meshColorArr[meshColorSelect] || meshShaderSelect == meshShader.Transparent || force) {
	        	currentColor = meshColorArr[meshColorSelect];
	        	if (meshShaderSelect == meshShader.Transparent) {
		        	currentColor.a = alpha;
		        }
	        	material.color = currentColor;
				GetComponent<MeshRenderer>().material = material;
        	}
        }

		// points
		private SortedDictionary<int, Dictionary<Vector4, Vector3>> meshPoints = new SortedDictionary<int, Dictionary<Vector4, Vector3>>();
		private Dictionary<Vector4, Vector3> meshPoint = new Dictionary<Vector4,Vector3>();
		private List<Vector3> meshVertices = new List<Vector3>();
		private List<Vector3> meshVerticesCopy = new List<Vector3>();
		private int meshVerticesMaxLength = 65000;
		// helpers
		private float smoothX = 0.0f;
		private float smoothY = 0.0f;
		private float smoothZ = 0.0f;
		private float smoothEndX = 0.0f;
		private float smoothEndY = 0.0f;
		private float smoothEndZ = 0.0f;
		private int currentPeaks = 0;
		private float currentTime;
		private float pointTime;

        void meshPointsAdd(int key, Dictionary<Vector4, Vector3> val) {
            if (meshPoints.Count >= MainSettingsVars.data.maxSizeCollections) {
        		meshPoints.Remove(meshPoints.Keys.Last()); // works for SortedDictionary
            }
            if (meshPoints.ContainsKey(key)) {
                meshPoints[key] = val;
            }
            else {
                meshPoints.Add(key, val);
            }
        }

        void calculatePoints() {
        	if (currentPeaks != peaks) {
        		currentPeaks = peaks;
				parentAnimation.meshPeaksAdd(meshNumber, peaks);
			}

			meshVertices.Clear();

			if (!active) {
				return;
			}

	        currentTime = Time.unscaledTime - parentAnimation.activeTime;
			Vector4 start;
			Vector3 end;

			// make meshPoints - get start + end points - slice to peaks
			if (parentAnimation.points.Count > 0 && peaks > 0 && level > 0f) {
				float x,y,z,endX,endY,endZ;
				for (int point=0; point < peaks; point++) {
					if (!parentAnimation.points.ContainsKey(point)) {
						break;
					}
					x = parentAnimation.points[point][0];
					y = parentAnimation.points[point][1];
					z = parentAnimation.points[point][2];
					pointTime = parentAnimation.points[point][3];
					// scale
					if (level < 1f) {
			            x = parentAnimation.data.x_center + ((x - parentAnimation.data.x_center) * level);
			            y = parentAnimation.data.y_center + ((y - parentAnimation.data.y_center) * level);
			            z = parentAnimation.data.z_center + ((z - parentAnimation.data.z_center) * level);
					}
					endX = x;
					endY = y;
			        endZ = parentAnimation.data.z_center;
			        if (z > parentAnimation.data.z_center) {
			            endZ = parentAnimation.data.z_center - (z - parentAnimation.data.z_center);
			        }
			        else if (z < parentAnimation.data.z_center) {
			            endZ = parentAnimation.data.z_center + (parentAnimation.data.z_center - z);
			        }
			        // randomization (controlled)
			        if (randomX || randomY) {
				        int endPoint = UnityEngine.Random.Range(0,4);
				        // zero will make no change
				        if (randomY && (endPoint == 1 || endPoint == 3)) {
				            if (y > parentAnimation.data.y_center) {
				                endY = parentAnimation.data.y_center - (y - parentAnimation.data.y_center);
				            }
				            else if (y < parentAnimation.data.y_center) {
				                endY = parentAnimation.data.y_center + (parentAnimation.data.y_center - y);
				            }
				        }
				        if (randomX && (endPoint == 2 || endPoint == 3)) {
				            if (x > parentAnimation.data.x_center) {
				                endX = parentAnimation.data.x_center - (x - parentAnimation.data.x_center);
				            }
				            else if (x < parentAnimation.data.x_center) {
				                endX = parentAnimation.data.x_center + (parentAnimation.data.x_center - x);
				            }
				        }
			        }
					// smoothing
					if (smoothTime > 0.0f) {
						if (meshPoints.TryGetValue(point, out meshPoint)) {
							start = meshPoint.Keys.First();
							end = meshPoint.Values.First();
							x = Mathf.SmoothDamp(start[0], x, ref smoothX, smoothTime);
							y = Mathf.SmoothDamp(start[1], y, ref smoothY, smoothTime);
							z = Mathf.SmoothDamp(start[2], z, ref smoothZ, smoothTime);
							endX = Mathf.SmoothDamp(end[0], endX, ref smoothEndX, smoothTime);
							endY = Mathf.SmoothDamp(end[1], endY, ref smoothEndY, smoothTime);
							endZ = Mathf.SmoothDamp(end[2], endZ, ref smoothEndZ, smoothTime);
						}
						else {
							x = Mathf.SmoothDamp(parentAnimation.data.x_center, x, ref smoothX, smoothTime);
							y = Mathf.SmoothDamp(parentAnimation.data.y_center, y, ref smoothY, smoothTime);
							z = Mathf.SmoothDamp(parentAnimation.data.z_center, z, ref smoothZ, smoothTime);
							endX = Mathf.SmoothDamp(parentAnimation.data.x_center, endX, ref smoothEndX, smoothTime);
							endY = Mathf.SmoothDamp(parentAnimation.data.y_center, endY, ref smoothEndY, smoothTime);
							endZ = Mathf.SmoothDamp(parentAnimation.data.z_center, endZ, ref smoothEndZ, smoothTime);							
						}
					}
					// add the item
					meshPoint = new Dictionary<Vector4,Vector3>(){
						{new Vector4(x,y,z,pointTime), new Vector3(endX,endY,endZ)}
					};
					meshPointsAdd(point, meshPoint);
				}
			}

			// make meshVertices
			if (meshPoints.Count > 0) {
				int index = 0;
				foreach (var i in meshPoints.Keys.ToArray()) {
					meshPoint = meshPoints[i];
					start = meshPoint.Keys.First();
					end = meshPoint.Values.First();
					pointTime = start[3];

					// skip + remove old frames
					if (pointTime < (currentTime - MainSettingsVars.pointsTime)) {
						meshPoints.Remove(i);
						continue;
					}

					// add vertices
					meshVertices.Add(new Vector3(start[0],start[1],start[2]));
					index++;
					meshVertices.Add(end);
					index++;
					if (index >= meshVerticesMaxLength) {
						break;
					}
				}
			}
        }

        private GameObject echoObj;
        private Dictionary<meshEchoType, SortedDictionary<int, string>> echoObjArr = new Dictionary<meshEchoType, SortedDictionary<int, string>>();
        private SortedDictionary<int, string> echoObjArrType = new SortedDictionary<int, string>();

        void echoObjArrAdd(meshEchoType echoType, GameObject val) {
			if (transform.childCount == 0 && echoObjArr.Count > 0) {
				echoObjArr.Clear();
			}
        	int key = 0;
            if (echoObjArr.ContainsKey(echoType)) {
            	echoObjArrType = echoObjArr[echoType];
            	if (echoObjArrType.Count > 0) {
            		if (echoObjArrType.Count >= MainSettingsVars.data.maxSizeChildren) {
		                Transform testTransform = transform.Find(echoObjArrType[echoObjArrType.Keys.First()]);
		                if (testTransform) {
		                    Destroy(testTransform.gameObject);
		                }
            			echoObjArrType.Remove(echoObjArrType.Keys.First());
            		}
            		key = echoObjArrType.Keys.Max() + 1;
            	}
                val.name = val.name+key;
            	echoObjArrType.Add(key, val.name);
                echoObjArr[echoType] = echoObjArrType;
            }
            else {
                val.name = val.name+key;
            	echoObjArrType = new SortedDictionary<int, string>(){
            		{key, val.name}
            	};
                echoObjArr.Add(echoType, echoObjArrType);
            }
        }

		void Update() {
			if (mesh == null) {
				return;
			}
			// make a copy
			meshVerticesCopy = meshVertices;
			// clear mesh
			if (!active || meshVerticesCopy.Count == 0 || (meshVerticesCopy.Count != mesh.vertices.Length && mesh.vertices.Length > 0)) {
				mesh.Clear();
				if (transform.childCount == 0) {
	        		return;
	        	}
			}
			// draw mesh
			if (meshVerticesCopy.Count > 0) {
				// set vertices
		        mesh.SetVertices(meshVerticesCopy);
		        meshVerticesCopy.Clear();
		        // get indices
		        int meshIndicesLength = mesh.vertices.Length;
				if (meshTopologySelect == MeshTopology.Triangles) {
					// The number of supplied triangle indices must be a multiple of 3.
					meshIndicesLength = Mathf.FloorToInt((float)meshIndicesLength / 3f) * 3;
				}
		        int[] meshIndices = Enumerable.Range(0, meshIndicesLength).ToArray();
		        // set indices + topology
	        	mesh.SetIndices(meshIndices, meshTopologySelect, 0, false);
		        // shader + color
				setShader();
				setColor();
				// echo objects
				// clear
	        	if (noClearTime) {
					echoObj = new GameObject("noClearTime", typeof(OSC_Mesh_Echo));
					echoObjArrAdd(meshEchoType.noClearTime, echoObj);
					echoObj.transform.parent = transform;
					echoObj.GetComponent<OSC_Mesh_Echo>().Initialize(meshEchoType.noClearTime);
	        	}
	        	else if (clearTime > 0.0f) {
					echoObj = new GameObject("clearTime", typeof(OSC_Mesh_Echo));
					echoObjArrAdd(meshEchoType.clearTime, echoObj);
					echoObj.transform.parent = transform;
					echoObj.GetComponent<OSC_Mesh_Echo>().Initialize(meshEchoType.clearTime);
	        	}
				// trace
				if (traceTime > 0.0f) {
					echoObj = new GameObject("traceTime", typeof(OSC_Mesh_Echo));
					echoObjArrAdd(meshEchoType.traceTime, echoObj);
					echoObj.transform.parent = transform;
					echoObj.GetComponent<OSC_Mesh_Echo>().Initialize(meshEchoType.traceTime);
				}
			}
			// rotate - leave for children
			if (rotateSpeed != 0.0f) {
				transform.RotateAround(parentAnimation.data.worldCenter, transform.up, Time.smoothDeltaTime * (rotateSpeed * -1f * 90f));
			}
		}
	}
}