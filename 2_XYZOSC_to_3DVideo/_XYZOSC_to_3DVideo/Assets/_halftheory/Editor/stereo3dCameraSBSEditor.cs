using UnityEngine;
using UnityEditor;

namespace _halftheory {
    [CustomEditor(typeof(stereo3dCameraSBS))]
    [CanEditMultipleObjects]
    public class stereo3dCameraSBSEditor : Editor {

        SerializedProperty e_interaxial;
        SerializedProperty e_zeroPrlxDist;
        SerializedProperty e_H_I_T;

        void OnEnable() {
			e_interaxial = serializedObject.FindProperty("interaxial");
			e_zeroPrlxDist = serializedObject.FindProperty("zeroPrlxDist");
			e_H_I_T = serializedObject.FindProperty("H_I_T");
		}

		public override void OnInspectorGUI() {
			// Show the editor controls.
			serializedObject.Update();

			e_interaxial.floatValue = EditorGUILayout.Slider(new GUIContent("Interaxial (mm)","Distance (in millimeters) between cameras."), e_interaxial.floatValue, 0, 1000f);
			e_zeroPrlxDist.floatValue = EditorGUILayout.Slider(new GUIContent("Zero Prlx Dist (M)","Distance (in meters) at which left and right images converge."), e_zeroPrlxDist.floatValue, 0.1f, 100f);
			e_H_I_T.floatValue = EditorGUILayout.Slider(new GUIContent("H I T","Horizontal Image Transform (default 0)"), e_H_I_T.floatValue, -25f, 25f);
			//DrawDefaultInspector();

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}
		}
	}
}
