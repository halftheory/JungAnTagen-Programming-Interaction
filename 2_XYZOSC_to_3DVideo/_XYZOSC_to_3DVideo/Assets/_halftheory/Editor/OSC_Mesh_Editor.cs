using UnityEngine;
using UnityEditor;

namespace _halftheory {
    [CustomEditor(typeof(OSC_Mesh))]
    [CanEditMultipleObjects]
    public class OSC_Mesh_Editor : Editor {

		SerializedProperty active;
		SerializedProperty peaks;
		SerializedProperty level;
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
			active = serializedObject.FindProperty("active");
			peaks = serializedObject.FindProperty("peaks");
			level = serializedObject.FindProperty("level");
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

	 		EditorGUILayout.PropertyField(active, new GUIContent("Active",""));
			peaks.intValue = EditorGUILayout.IntSlider(new GUIContent("Peaks",""), peaks.intValue, 0, MainSettingsVars.pointsLength);
			level.floatValue = EditorGUILayout.Slider(new GUIContent("Level",""), level.floatValue, 0, 1f);
	 		EditorGUILayout.PropertyField(meshTopologySelect, new GUIContent("Topology",""));
	 		EditorGUILayout.PropertyField(meshColorSelect, new GUIContent("Color",""));
	 		EditorGUILayout.PropertyField(meshShaderSelect, new GUIContent("Material",""));
            if (meshShaderSelect.enumValueIndex == 2) {
                EditorGUI.indentLevel++;
				alpha.floatValue = EditorGUILayout.Slider(new GUIContent("Opacity",""), alpha.floatValue, 0, 1f);
                EditorGUI.indentLevel--;
            }
	 		EditorGUILayout.PropertyField(randomX, new GUIContent("Random end point X",""));
	 		EditorGUILayout.PropertyField(randomY, new GUIContent("Random end point Y",""));
			rotateSpeed.floatValue = EditorGUILayout.Slider(new GUIContent("Rotate forward/back",""), rotateSpeed.floatValue, -1f, 1f);
			smoothTime.floatValue = EditorGUILayout.Slider(new GUIContent("Smooth time (sec)",""), smoothTime.floatValue, 0, 0.5f);
			clearTime.floatValue = EditorGUILayout.Slider(new GUIContent("Clear time (sec)",""), clearTime.floatValue, 0, 30f);
	 		EditorGUILayout.PropertyField(noClearTime, new GUIContent("No clear time",""));
			traceTime.floatValue = EditorGUILayout.Slider(new GUIContent("Trace time (sec)",""), traceTime.floatValue, 0, 2f);

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
				// set data
				OSC_Mesh monoTarget = (OSC_Mesh)target;
				monoTarget.data.active = active.boolValue;
				monoTarget.data.peaks = peaks.intValue;
				monoTarget.data.level = level.floatValue;
				monoTarget.data.meshTopologySelect = (MeshTopology)System.Enum.Parse(typeof(MeshTopology), meshTopologySelect.enumValueIndex.ToString());
				monoTarget.data.meshColorSelect = (meshColor)System.Enum.Parse(typeof(meshColor), meshColorSelect.enumValueIndex.ToString());
				monoTarget.data.meshShaderSelect = (meshShader)System.Enum.Parse(typeof(meshShader), meshShaderSelect.enumValueIndex.ToString());
				monoTarget.data.alpha = alpha.floatValue;
				monoTarget.data.randomX = randomX.boolValue;
				monoTarget.data.randomY = randomY.boolValue;
				monoTarget.data.rotateSpeed = rotateSpeed.floatValue;
				monoTarget.data.smoothTime = smoothTime.floatValue;
				monoTarget.data.clearTime = clearTime.floatValue;
				monoTarget.data.noClearTime = noClearTime.boolValue;
				monoTarget.data.traceTime = traceTime.floatValue;
			}
		}
	}
}