/* adapted from s3dCameraSBS.js in http://projects.ict.usc.edu/mxr/diy/mxr-unity-package/ */
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace _halftheory {
	[ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class stereo3dCameraSBS : MonoBehaviour {

		private bool initialized = false;

		// Cameras
		public Camera mainCamComponent;
		public GameObject leftCam; // left view camera
		public GameObject rightCam; // right view camera
		public GameObject leftCamRecord;
		public GameObject rightCamRecord;

		// Stereo Parameters
		[SerializeField] public float interaxial = 65f; // Distance (in millimeters) between cameras
		[SerializeField] public float zeroPrlxDist = 2f; // Distance (in meters) at which left and right images overlap exactly
		[SerializeField] public float H_I_T = 0; // Horizontal Image Transform - shift left and right image horizontally
		private float offAxisFrustum = 0; // Esoteric parameter

		// Side by Side Parameters
		private enum cams_3D {LeftRight, LeftOnly, RightOnly, RightLeft}
		private cams_3D cameraSelect = cams_3D.LeftRight; // View order - swap cameras for cross-eyed free-viewing
		private bool sideBySideSqueezed = true; // 50% horizontal scale for 3DTVs

		private bool isInit() {
			if (initialized) {
				return (true);
			}
			// main cam
			if (GetComponent<Camera>()) {
				mainCamComponent = GetComponent<Camera>();
				mainCamComponent.clearFlags = CameraClearFlags.SolidColor;
				mainCamComponent.backgroundColor = Color.black;
			}
			else {
				return (false);
			}

			// stereo cams
			Transform testTransform = transform.Find("leftCam");
			if (testTransform) {
				leftCam = testTransform.gameObject;
			}
			else {
				leftCam = new GameObject("leftCam", typeof(Camera));
				leftCam.transform.parent = transform; // child object
			}
			testTransform = transform.Find("rightCam");
			if (testTransform) {
				rightCam = testTransform.gameObject;
			}
			else {
				rightCam = new GameObject("rightCam", typeof(Camera));
				rightCam.transform.parent = transform;
			}
			testTransform = transform.Find("leftCamRecord");
			if (testTransform) {
				leftCamRecord = testTransform.gameObject;
			}
			else {
				leftCamRecord = new GameObject("leftCamRecord", typeof(Camera));
				leftCamRecord.transform.parent = transform;
			}
			testTransform = transform.Find("rightCamRecord");
			if (testTransform) {
				rightCamRecord = testTransform.gameObject;
			}
			else {
				rightCamRecord = new GameObject("rightCamRecord", typeof(Camera));
				rightCamRecord.transform.parent = transform;
			}

			initialized = true;
			return (true);
		}

		void OnEnable() {
			bool test = isInit();
			if (test) {
				SetCameraSettings();
			}
		}
		void Start() {
			bool test = isInit();
			if (test) {
				SetCameraSettings();
				// set tags
				// must define tags first in Project Settings
				leftCamRecord.tag = "leftCamRecord";
				rightCamRecord.tag = "rightCamRecord";
				#if UNITY_EDITOR
				/*
					if (EditorApplication.isPlaying) {
						mainCamComponent.enabled = false;
					}
					else {
						mainCamComponent.enabled = true; // need camera enabled when in edit mode
					}
				*/
				#endif
			}
		}
		void OnDisable() {
			bool test = isInit();
			if (test) {
				mainCamComponent.enabled = true;
				leftCam.SetActive(false);
				rightCam.SetActive(false);
				leftCamRecord.SetActive(false);
				rightCamRecord.SetActive(false);
			}
		}
		void OnDestroy() {
			bool test = isInit();
			if (test) {
				mainCamComponent.enabled = true;
				DestroyImmediate(leftCam);
				DestroyImmediate(rightCam);
				DestroyImmediate(leftCamRecord);
				DestroyImmediate(rightCamRecord);
			}
		}
		void Update() {
			UpdateView();
		}

		void SetCameraSettings() {
			if (!initialized) {
				return;
			}
			leftCam.SetActive(true);
			rightCam.SetActive(true);
			leftCamRecord.SetActive(false);
			rightCamRecord.SetActive(false);

			leftCam.GetComponent<Camera>().CopyFrom(mainCamComponent);
			rightCam.GetComponent<Camera>().CopyFrom(mainCamComponent);
			leftCamRecord.GetComponent<Camera>().CopyFrom(mainCamComponent);
			rightCamRecord.GetComponent<Camera>().CopyFrom(mainCamComponent);

			// rendering order (back to front): leftCamRecord/rightCamRecord/Main Camera/leftCam/rightCam
			leftCamRecord.GetComponent<Camera>().depth = mainCamComponent.depth - 2;
			rightCamRecord.GetComponent<Camera>().depth = mainCamComponent.depth - 1;
			leftCam.GetComponent<Camera>().depth = mainCamComponent.depth + 1;
			rightCam.GetComponent<Camera>().depth = mainCamComponent.depth + 2;

			// rect
			leftCam.GetComponent<Camera>().rect = new Rect(0,0,0.5f,1f);
			rightCam.GetComponent<Camera>().rect = new Rect(0.5f,0,0.5f,1f);
			leftCamRecord.GetComponent<Camera>().rect = new Rect(0,0,1f,1f);
			rightCamRecord.GetComponent<Camera>().rect = new Rect(0,0,1f,1f);

			// aspect
			mainCamComponent.ResetAspect();
			mainCamComponent.aspect *= leftCam.GetComponent<Camera>().rect.width*2/leftCam.GetComponent<Camera>().rect.height;
			leftCam.GetComponent<Camera>().aspect = mainCamComponent.aspect;
			rightCam.GetComponent<Camera>().aspect = mainCamComponent.aspect;
			leftCamRecord.GetComponent<Camera>().aspect = mainCamComponent.aspect;
			rightCamRecord.GetComponent<Camera>().aspect = mainCamComponent.aspect;

			mainCamComponent.enabled = false;
		}

		void UpdateView() {
			if (!initialized) {
				return;
			}
			// position
			switch (cameraSelect) {
				case cams_3D.LeftRight:{
					leftCam.transform.position = transform.position + transform.TransformDirection(new Vector3(-interaxial/2000.0f, 0, 0));
					rightCam.transform.position = transform.position + transform.TransformDirection(new Vector3(interaxial/2000.0f, 0, 0));
					break;
				}
				case cams_3D.LeftOnly:{
					leftCam.transform.position = transform.position + transform.TransformDirection(new Vector3(-interaxial/2000.0f, 0, 0));
					rightCam.transform.position = transform.position + transform.TransformDirection(new Vector3(-interaxial/2000.0f, 0, 0));
					break;
				}
				case cams_3D.RightOnly:{
					leftCam.transform.position = transform.position + transform.TransformDirection(new Vector3(interaxial/2000.0f, 0, 0));
					rightCam.transform.position = transform.position + transform.TransformDirection(new Vector3(interaxial/2000.0f, 0, 0));
					break;
				}
				case cams_3D.RightLeft:{
					leftCam.transform.position = transform.position + transform.TransformDirection(new Vector3(interaxial/2000.0f, 0, 0));
					rightCam.transform.position = transform.position + transform.TransformDirection(new Vector3(-interaxial/2000.0f, 0, 0));
					break;
				}
			}
			leftCamRecord.transform.position = leftCam.transform.position;
			rightCamRecord.transform.position = rightCam.transform.position;
			// rotation
			leftCam.transform.rotation = transform.rotation; 
			rightCam.transform.rotation = transform.rotation;
			leftCamRecord.transform.rotation = transform.rotation;
			rightCamRecord.transform.rotation = transform.rotation;
			// projectionMatrix
			switch (cameraSelect) {
				case cams_3D.LeftRight:{
					leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
					rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
					break;
				}
				case cams_3D.LeftOnly:{
					leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
					rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
					break;
				}
				case cams_3D.RightOnly:{
					leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
					rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
					break;
				}
				case cams_3D.RightLeft:{
					leftCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(false);
					rightCam.GetComponent<Camera>().projectionMatrix = setProjectionMatrix(true);
					break;
				}
			}
		}

		private Matrix4x4 setProjectionMatrix(bool isLeftCam) {
			float left;
			float right;
			float a;
			float b;
			float FOVrad;
			float tempAspect = mainCamComponent.aspect;
			FOVrad = mainCamComponent.fieldOfView / 180.0f * Mathf.PI;
			if (!sideBySideSqueezed) {
				tempAspect /= 2f;	// if side by side unsqueezed, double width
			}
			a = mainCamComponent.nearClipPlane * Mathf.Tan(FOVrad * 0.5f);
			b = mainCamComponent.nearClipPlane / zeroPrlxDist;
			if (isLeftCam) {
				left  = (-tempAspect * a) + (interaxial/2000.0f * b) + (H_I_T/100f) + (offAxisFrustum/100f);
				right =	(tempAspect * a) + (interaxial/2000.0f * b) + (H_I_T/100f) + (offAxisFrustum/100f);
			}
			else {
				left  = (-tempAspect * a) - (interaxial/2000.0f * b) - (H_I_T/100f) + (offAxisFrustum/100f);
				right =	(tempAspect * a) - (interaxial/2000.0f * b) - (H_I_T/100f) + (offAxisFrustum/100f);
			}
			return PerspectiveOffCenter(left, right, -a, a, mainCamComponent.nearClipPlane, mainCamComponent.farClipPlane);
		}

		private Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
			float x = (2.0f * near) / (right - left);
			float y = (2.0f * near) / (top - bottom);
			float a = (right + left) / (right - left);
			float b = (top + bottom) / (top - bottom);
			float c = -(far + near) / (far - near);
			float d = -(2.0f * far * near) / (far - near);
			float e = -1.0f;
			Matrix4x4 m = new Matrix4x4();
			m[0,0] = x;  m[0,1] = 0;  m[0,2] = a;  m[0,3] = 0;
			m[1,0] = 0;  m[1,1] = y;  m[1,2] = b;  m[1,3] = 0;
			m[2,0] = 0;  m[2,1] = 0;  m[2,2] = c;  m[2,3] = d;
			m[3,0] = 0;  m[3,1] = 0;  m[3,2] = e;  m[3,3] = 0;
			return m;
		}

		void OnDrawGizmos() {
			Vector3 gizmoLeft = transform.position + transform.TransformDirection(-interaxial/2000.0f, 0, 0);
			Vector3 gizmoRight = transform.position + transform.TransformDirection(interaxial/2000.0f, 0, 0);
			Vector3 gizmoTarget = transform.position + transform.TransformDirection (Vector3.forward) * zeroPrlxDist;
			Gizmos.color = Color.white;
			Gizmos.DrawLine(gizmoLeft, gizmoTarget);
			Gizmos.DrawLine(gizmoRight, gizmoTarget);
			Gizmos.DrawLine(gizmoLeft, gizmoRight);
			Gizmos.DrawSphere(gizmoLeft, 0.02f);
			Gizmos.DrawSphere(gizmoRight, 0.02f);
			Gizmos.DrawSphere(gizmoTarget, 0.02f);
		}
	}
}