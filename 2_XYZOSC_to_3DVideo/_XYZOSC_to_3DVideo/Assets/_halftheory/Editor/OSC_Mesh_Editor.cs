using UnityEngine;
using UnityEditor;

namespace _halftheory {
    [CustomEditor(typeof(OSC_Mesh))]
    [CanEditMultipleObjects]
    public class OSC_Mesh_Editor : Editor {

        SerializedProperty meshTopologySelect;
        SerializedProperty meshColorSelect;
        SerializedProperty meshShaderSelect;
        SerializedProperty alpha;
        SerializedProperty randomX;
        SerializedProperty randomY;
        SerializedProperty rotateSpeed;
        SerializedProperty smoothTime;
        SerializedProperty clearTime;
        SerializedProperty noClearTime;
        SerializedProperty traceTime;

        void OnEnable() {
			meshTopologySelect = serializedObject.FindProperty("meshTopologySelect");
			meshColorSelect = serializedObject.FindProperty("meshColorSelect");
			meshShaderSelect = serializedObject.FindProperty("meshShaderSelect");
			alpha = serializedObject.FindProperty("alpha");
			randomX = serializedObject.FindProperty("randomX");
			randomY = serializedObject.FindProperty("randomY");
			rotateSpeed = serializedObject.FindProperty("rotateSpeed");
			smoothTime = serializedObject.FindProperty("smoothTime");
			clearTime = serializedObject.FindProperty("clearTime");
			noClearTime = serializedObject.FindProperty("noClearTime");
			traceTime = serializedObject.FindProperty("traceTime");
		}

		public override void OnInspectorGUI() {
			// Show the editor controls.
			serializedObject.Update();

	 		EditorGUILayout.PropertyField(meshTopologySelect, new GUIContent("Topology",""));
	 		EditorGUILayout.PropertyField(meshColorSelect, new GUIContent("Color",""));
	 		EditorGUILayout.PropertyField(meshShaderSelect, new GUIContent("Material",""));
            if (meshShaderSelect.enumValueIndex == 2) {
                EditorGUI.indentLevel++;
				alpha.floatValue = EditorGUILayout.Slider(new GUIContent("Opacity",""), alpha.floatValue, 0, 1f);
                EditorGUI.indentLevel--;
            }
	 		EditorGUILayout.PropertyField(randomX, new GUIContent("Randomize end point X",""));
	 		EditorGUILayout.PropertyField(randomY, new GUIContent("Randomize end point Y",""));
			rotateSpeed.floatValue = EditorGUILayout.Slider(new GUIContent("Rotate forward/back",""), rotateSpeed.floatValue, -1f, 1f);
			smoothTime.floatValue = EditorGUILayout.Slider(new GUIContent("Smooth time (sec)",""), smoothTime.floatValue, 0, 0.5f);
			clearTime.floatValue = EditorGUILayout.Slider(new GUIContent("Clear time (sec)",""), clearTime.floatValue, 0, 30f);
	 		EditorGUILayout.PropertyField(noClearTime, new GUIContent("No clear time",""));
			traceTime.floatValue = EditorGUILayout.Slider(new GUIContent("Trace time (sec)",""), traceTime.floatValue, 0, 2f);

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}
		}
	}
}