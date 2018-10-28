/* adapted from s3dCameraSBS.js in http://projects.ict.usc.edu/mxr/diy/mxr-unity-package/ */
using UnityEngine;
using UnityEditor;

namespace _halftheory {
    [CustomEditor(typeof(stereo3dCameraSBS))]
    [CanEditMultipleObjects]
    public class stereo3dCameraSBSEditor : Editor {

        SerializedProperty interaxial;
        SerializedProperty zeroPrlxDist;
        SerializedProperty H_I_T;

        void OnEnable() {
			interaxial = serializedObject.FindProperty("interaxial");
			zeroPrlxDist = serializedObject.FindProperty("zeroPrlxDist");
			H_I_T = serializedObject.FindProperty("H_I_T");
		}

		public override void OnInspectorGUI() {
			// Show the editor controls.
			serializedObject.Update();

			interaxial.floatValue = EditorGUILayout.Slider(new GUIContent("Interaxial (mm)","Distance (in millimeters) between cameras."), interaxial.floatValue, 0, 1000f);
			zeroPrlxDist.floatValue = EditorGUILayout.Slider(new GUIContent("Zero Prlx Dist (M)","Distance (in meters) at which left and right images converge."), zeroPrlxDist.floatValue, 0.1f, 100f);
			H_I_T.floatValue = EditorGUILayout.Slider(new GUIContent("H I T","Horizontal Image Transform (default 0)."), H_I_T.floatValue, -25f, 25f);
			//DrawDefaultInspector();

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}
		}
	}
}