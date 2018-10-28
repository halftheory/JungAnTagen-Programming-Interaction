using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _halftheory {

	public enum meshEchoType {noClearTime, clearTime, traceTime}

	[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
	public class OSC_Mesh_Echo : MonoBehaviour {

		private bool initialized = false;
		private meshEchoType meshEcho = meshEchoType.clearTime;
		private OSC_Mesh parentMesh;
	    private Mesh mesh;
		private Material material;

		// noClearTime

		// clearTime
		private float clearTime = 0.0f;

		// traceTime
		private float traceTime = 0.0f;
		private float startTime = 0.0f;
		private float initialAlpha = 1.0f;
		private float currentAlpha = 1.0f;
		private Color currentColor;

        public void Initialize(meshEchoType echoType) {
        	// checks
			if (!GetComponent<MeshFilter>()) {
                Destroy(gameObject);
                return;
			}
			if (!GetComponent<MeshRenderer>()) {
                Destroy(gameObject);
                return;
			}
			parentMesh = (OSC_Mesh)this.GetComponentInParent(typeof(OSC_Mesh));
			if (parentMesh == null) {
                Destroy(gameObject);
                return;
			}
			if (!parentMesh.noClearTime && parentMesh.clearTime == 0.0f && parentMesh.traceTime == 0.0f) {
                Destroy(gameObject);
                return;
			}
			if (parentMesh.mesh == null) {
                Destroy(gameObject);
                return;
			}
			if (parentMesh.mesh.vertices.Length == 0) {
                Destroy(gameObject);
                return;
			}
			if (parentMesh.material == null) {
                Destroy(gameObject);
                return;
			}
			// setup
			mesh = new Mesh();
			mesh.vertices = parentMesh.mesh.vertices;
	        mesh.SetIndices(parentMesh.mesh.GetIndices(0), parentMesh.mesh.GetTopology(0), 0, false);
		    GetComponent<MeshFilter>().mesh = mesh;

		    material = new Material(parentMesh.material.shader);
		    material.color = parentMesh.material.color;
		    GetComponent<MeshRenderer>().material = material;

			transform.position = parentMesh.transform.position;
			transform.rotation = parentMesh.transform.rotation;

    		// setup specific types
    		meshEcho = echoType;
    		if (meshEcho == meshEchoType.noClearTime) {
				mesh.UploadMeshData(true);
    		}
    		else if (meshEcho == meshEchoType.clearTime) {
				mesh.UploadMeshData(true);
				clearTime = parentMesh.clearTime;
    		}
    		else if (meshEcho == meshEchoType.traceTime) {
			    mesh.MarkDynamic();
			    traceTime = parentMesh.traceTime;
			    initialAlpha = parentMesh.alpha;
				currentColor = material.color;
	    		material = new Material(Shader.Find("Transparent/Diffuse"));
	    		material.color = currentColor;
	    		GetComponent<MeshRenderer>().material = material;
	    		startTime = Time.unscaledTime;
    		}
    		else {
                Destroy(gameObject);
                return;
    		}
            initialized = true;
		}

        float t;

		void Update() {
			if (!initialized) {
				return;
			}
    		if (meshEcho == meshEchoType.noClearTime) {
				if (!parentMesh.noClearTime) {
					if (parentMesh.clearTime == 0.0f) {
		                Destroy(gameObject);
		                return;
	                }
	                else {
	                	// switch to clearTime
						meshEcho = meshEchoType.clearTime;
						clearTime = parentMesh.clearTime;
	                }
				}
    		}
    		else if (meshEcho == meshEchoType.clearTime) {
				Destroy(gameObject, clearTime);
    		}
    		else if (meshEcho == meshEchoType.traceTime) {
				if (currentAlpha == 0.0f) {
					Destroy(gameObject);
	                return;
				}
				t = (Time.unscaledTime - startTime) / traceTime;
				currentAlpha = Mathf.SmoothStep(initialAlpha, 0.0f, t);
		        currentColor.a = currentAlpha;
	        	material.color = currentColor;
	        	GetComponent<MeshRenderer>().material = material;
    		}
    		else {
                Destroy(gameObject);
                return;
    		}
		}
	}
}