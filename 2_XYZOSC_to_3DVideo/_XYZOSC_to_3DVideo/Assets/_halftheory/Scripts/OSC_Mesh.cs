using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _halftheory {

	public enum meshTopology {Lines = 0, LineStrip = 1, Points = 2}
	public enum meshColor {white, red, black, blue, cyan, green, grey, magenta, yellow}
	public enum meshShader {Color, Standard, Transparent}

	public class OSC_Mesh_Data : ScriptableObject {

		public string objectName;

		public meshTopology meshTopologySelect = meshTopology.LineStrip;
		public meshColor meshColorSelect = meshColor.white;
		public meshShader meshShaderSelect = meshShader.Color;
		public float alpha = 1f;
		public bool randomX = false;
		public bool randomY = false;
		public float rotateSpeed = 0;
		public float smoothTime = 0;
		public float clearTime = 0;
		public bool noClearTime = false;
	}

	[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
	public class OSC_Mesh : MonoBehaviour {

		public OSC_Mesh_Data data;

		public meshTopology meshTopologySelect;
		public meshColor meshColorSelect;
		public meshShader meshShaderSelect;
		public float alpha;
		public bool randomX;
		public bool randomY;
		public float rotateSpeed;
		public float smoothTime;
		public float clearTime;
		public bool noClearTime;

		private List<Color> colorList = new List<Color>(new Color[]{
			Color.white, Color.red, Color.black, Color.blue, Color.cyan, Color.green, Color.grey, Color.magenta, Color.yellow
		});
		private Color currentColor;
		private List<string> shaderList = new List<string>(new string[]{
        	"Unlit/Color", "Standard", "Transparent/Diffuse"
		});
		private string currentShader;

		// points
	    private Mesh mesh;
		private List<Vector4> pointsStart = new List<Vector4>();
		private List<Vector3> pointsEnd = new List<Vector3>();

		// helpers
		private Vector3 worldCenter;
		private float smoothX = 0.0f;
		private float smoothY = 0.0f;
		private float smoothZ = 0.0f;
		private float smoothEndX = 0.0f;
		private float smoothEndY = 0.0f;
		private float smoothEndZ = 0.0f;
		private int clearTimeFrames = 0;

		void OnEnable() {
			if (data == null) {
            	data = ScriptableObject.CreateInstance<OSC_Mesh_Data>();
            }
		}
		void Start() {
            //loadData();
			if (mesh == null && GetComponent<MeshFilter>() && GetComponent<MeshRenderer>()) {
			    mesh = new Mesh();
			    GetComponent<MeshFilter>().mesh = mesh;
			    mesh.MarkDynamic();
				setMaterialColor();
				worldCenter = transform.TransformPoint(new Vector3(MainSettingsVars.currentAnimationComponent.data.x_center, MainSettingsVars.currentAnimationComponent.data.y_center, MainSettingsVars.currentAnimationComponent.data.z_center));
			}
        }
        void OnDisable() {
        	//saveData();
        }
        public void loadData() {
			if (data != null) {
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
	        }
        }
        public void saveData() {
			if (data != null) {
	        	data.objectName = this.gameObject.name;
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
	        }
        }

        void setMaterialColor() {
        	if (currentShader != shaderList[(int)meshShaderSelect]) {
        		currentShader = shaderList[(int)meshShaderSelect];
        		GetComponent<MeshRenderer>().material = new Material(Shader.Find(currentShader));
	            //Debug.Log("HALFTHEORY: "+this.GetType()+": setMaterialColor : "+currentShader);
        		setColor();
        		return;
        	}
        	if (currentColor != colorList[(int)meshColorSelect] || meshShaderSelect == meshShader.Transparent) {
        		setColor();
        	}
        }
        void setColor() {
        	currentColor = colorList[(int)meshColorSelect];
        	if (meshShaderSelect == meshShader.Transparent) {
	        	currentColor.a = alpha;
	        }
        	GetComponent<MeshRenderer>().material.color = currentColor;
            //Debug.Log("HALFTHEORY: "+this.GetType()+": setColor : "+currentColor);
        }

        void timeToFrames() {
			clearTimeFrames = (int)Mathf.Round(clearTime * (float)MainSettingsVars.currentAnimationComponent.data.fps);
        }

		public void collectPoints(int point, float x, float y, float z) {
			if (mesh == null) {
				return;
			}

			timeToFrames();

			// start: x y z
			// get end point		
	        float endX = x;
	        float endY = y;
	        float endZ = MainSettingsVars.currentAnimationComponent.data.z_center;
	        if (z > MainSettingsVars.currentAnimationComponent.data.z_center) {
	            endZ = MainSettingsVars.currentAnimationComponent.data.z_center - (z - MainSettingsVars.currentAnimationComponent.data.z_center);
	        }
	        else if (z < MainSettingsVars.currentAnimationComponent.data.z_center) {
	            endZ = MainSettingsVars.currentAnimationComponent.data.z_center + (MainSettingsVars.currentAnimationComponent.data.z_center - z);
	        }

	        // randomization (controlled)
	        if (randomX || randomY) {
		        int endPoint = Random.Range(0,4);
		        // zero will make no change
		        if (randomY && (endPoint == 1 || endPoint == 3)) {
		            if (y > MainSettingsVars.currentAnimationComponent.data.y_center) {
		                endY = MainSettingsVars.currentAnimationComponent.data.y_center - (y - MainSettingsVars.currentAnimationComponent.data.y_center);
		            }
		            else if (y < MainSettingsVars.currentAnimationComponent.data.y_center) {
		                endY = MainSettingsVars.currentAnimationComponent.data.y_center + (MainSettingsVars.currentAnimationComponent.data.y_center - y);
		            }
		        }
		        if (randomX && (endPoint == 2 || endPoint == 3)) {
		            if (x > MainSettingsVars.currentAnimationComponent.data.x_center) {
		                endX = MainSettingsVars.currentAnimationComponent.data.x_center - (x - MainSettingsVars.currentAnimationComponent.data.x_center);
		            }
		            else if (x < MainSettingsVars.currentAnimationComponent.data.x_center) {
		                endX = MainSettingsVars.currentAnimationComponent.data.x_center + (MainSettingsVars.currentAnimationComponent.data.x_center - x);
		            }
		        }
	        }

	        float frameCount = (float)Time.frameCount;
	        // already in the list
			if (pointsStart.Count > point && !noClearTime) {
				// smoothing
				if (smoothTime > 0.0f) {
					x = Mathf.SmoothDamp(pointsStart[point][0], x, ref smoothX, smoothTime);
					y = Mathf.SmoothDamp(pointsStart[point][1], y, ref smoothY, smoothTime);
					z = Mathf.SmoothDamp(pointsStart[point][2], z, ref smoothZ, smoothTime);
					endX = Mathf.SmoothDamp(pointsEnd[point][0], endX, ref smoothEndX, smoothTime);
					endY = Mathf.SmoothDamp(pointsEnd[point][1], endY, ref smoothEndY, smoothTime);
					endZ = Mathf.SmoothDamp(pointsEnd[point][2], endZ, ref smoothEndZ, smoothTime);
				}
				if (clearTimeFrames == 0) {
					pointsStart[point] = new Vector4(x,y,z,frameCount);
					pointsEnd[point] = new Vector3(endX,endY,endZ);
				}
				else {
					// compare existing vector, maybe keep it
					int oldFrameCount = (int)pointsStart[point][3];
					if (oldFrameCount >= ((int)frameCount - clearTimeFrames)) {
						pointsStart.Add(new Vector4(x,y,z,frameCount));
						pointsEnd.Add(new Vector3(endX,endY,endZ));
					}
					else {
						pointsStart[point] = new Vector4(x,y,z,frameCount);
						pointsEnd[point] = new Vector3(endX,endY,endZ);
					}
				}
			}
			else {
	        	pointsStart.Add(new Vector4(x,y,z,frameCount));
		        pointsEnd.Add(new Vector3(endX,endY,endZ));
			}
		}

		void Update() {
			if (mesh == null) {
				return;
			}

			// make verticesList
			List<Vector3> verticesList = new List<Vector3>();
	        //List<Vector3> verticesList = mesh.vertices.ToList();
			for (int point = 0; point < pointsStart.Count; point++) {
				// check if old frame
				if ((int)pointsStart[point][3] < (Time.frameCount - clearTimeFrames - 3) && !noClearTime) { // 3 = correction number
					pointsStart.RemoveAt(point);
					pointsEnd.RemoveAt(point);
					continue;
				}
				verticesList.Add(new Vector3(pointsStart[point][0],pointsStart[point][1],pointsStart[point][2]));
				verticesList.Add(pointsEnd[point]);
			}

			// set verticesList
			if (verticesList.Count == 0) {
				mesh.Clear();
				return;
			}
			else if (verticesList.Count != mesh.vertices.Length) {
				mesh.Clear();
			}
	        mesh.SetVertices(verticesList);

	        // topology
	        int[] indices = Enumerable.Range(0, mesh.vertices.Length).ToArray();
	        if (meshTopologySelect == meshTopology.Lines) {
	        	mesh.SetIndices(indices, MeshTopology.Lines, 0);
	        }
	        else if (meshTopologySelect == meshTopology.LineStrip) {
	        	mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
	        }
	        else {
				mesh.SetIndices(indices, MeshTopology.Points, 0);
	        }

	        // color
			setMaterialColor();

			// rotate
			if (rotateSpeed != 0.0f) {
				transform.RotateAround(worldCenter, transform.up, Time.smoothDeltaTime * (rotateSpeed * 90f));
			}
		}
	}
}