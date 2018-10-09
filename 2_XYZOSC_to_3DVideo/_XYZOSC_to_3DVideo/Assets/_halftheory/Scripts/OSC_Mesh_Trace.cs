using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace _halftheory {
	[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
	public class OSC_Mesh_Trace : MonoBehaviour {

		private bool initialized = false;

		public float traceTime = 0.0f;
		public float initialAlpha = 1.0f;
		public float currentAlpha = 1.0f;
		private float startTime;
	    private Mesh mesh;
		private Color currentColor;

        public bool isInit() {
            if (initialized) {
                return (true);
            }
			if (!GetComponent<MeshFilter>()) {
                return (false);
			}
			if (!GetComponent<MeshRenderer>()) {
                return (false);
			}
			if (GetComponent<MeshFilter>().mesh == null) {
                return (false);
			}
			if (GetComponent<MeshFilter>().mesh.vertices.Length == 0) {
                return (false);
			}
			mesh = GetComponent<MeshFilter>().mesh;
		    mesh.MarkDynamic();
			currentColor = GetComponent<MeshRenderer>().material.color;
    		GetComponent<MeshRenderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
    		GetComponent<MeshRenderer>().material.color = currentColor;
    		startTime = Time.time;
            initialized = true;
            return (true);
		}

		void Start() {
			isInit();
        }

		void Update() {
			if (currentAlpha == 0.0f) {
				Destroy(gameObject);
			}
			bool test = isInit();
			if (test) {
				float t = (Time.time - startTime) / traceTime;
				currentAlpha = Mathf.SmoothStep(initialAlpha, 0.0f, t);
		        currentColor.a = currentAlpha;
	        	GetComponent<MeshRenderer>().material.color = currentColor;
			}
		}
	}
}