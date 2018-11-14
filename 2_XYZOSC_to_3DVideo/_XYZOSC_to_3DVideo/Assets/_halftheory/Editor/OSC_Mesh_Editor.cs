using UnityEditor;
using UnityEngine;

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
			active = serializedObject.FindProperty("_active");
			peaks = serializedObject.FindProperty("_peaks");
			level = serializedObject.FindProperty("_level");
			meshTopologySelect = serializedObject.FindProperty("_meshTopologySelect");
			meshColorSelect = serializedObject.FindProperty("_meshColorSelect");
			meshShaderSelect = serializedObject.FindProperty("_meshShaderSelect");
			alpha = serializedObject.FindProperty("_alpha");
			randomX = serializedObject.FindProperty("_randomX");
			randomY = serializedObject.FindProperty("_randomY");
			rotateSpeed = serializedObject.FindProperty("_rotateSpeed");
			smoothTime = serializedObject.FindProperty("_smoothTime");
			clearTime = serializedObject.FindProperty("_clearTime");
			noClearTime = serializedObject.FindProperty("_noClearTime");
			traceTime = serializedObject.FindProperty("_traceTime");
		}

		public override void OnInspectorGUI() {
			OSC_Mesh monoTarget = (OSC_Mesh)target;

			// Show the editor controls.
			serializedObject.Update();

	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(active, new GUIContent("Active",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.active = active.boolValue;
			}

			monoTarget.peaks = EditorGUILayout.IntSlider(new GUIContent("Peaks",""), peaks.intValue, 0, MainSettingsVars.pointsLength);
			monoTarget.level = EditorGUILayout.Slider(new GUIContent("Level",""), level.floatValue, 0, 1f);

	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(meshTopologySelect, new GUIContent("Topology",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.meshTopologySelect = (meshTopology)System.Enum.Parse(typeof(meshTopology), meshTopologySelect.enumValueIndex.ToString());
				if (MainSettingsVars.data.gui_enabled && MainSettingsVars.guiComponent != null) {
					MainSettingsVars.guiComponent.guiGridMeshTopologyInt[monoTarget.meshNumber] = meshTopologySelect.enumValueIndex;
				}
			}
	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(meshColorSelect, new GUIContent("Color",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.meshColorSelect = (meshColor)System.Enum.Parse(typeof(meshColor), meshColorSelect.enumValueIndex.ToString());
				if (MainSettingsVars.data.gui_enabled && MainSettingsVars.guiComponent != null) {
					MainSettingsVars.guiComponent.guiGridMeshColorInt[monoTarget.meshNumber] = meshColorSelect.enumValueIndex;
				}
			}
	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(meshShaderSelect, new GUIContent("Material",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.meshShaderSelect = (meshShader)System.Enum.Parse(typeof(meshShader), meshShaderSelect.enumValueIndex.ToString());
				if (MainSettingsVars.data.gui_enabled && MainSettingsVars.guiComponent != null) {
					MainSettingsVars.guiComponent.guiGridMeshShaderInt[monoTarget.meshNumber] = meshShaderSelect.enumValueIndex;
				}
			}

            if (meshShaderSelect.enumValueIndex == 2) {
                EditorGUI.indentLevel++;
				monoTarget.alpha = EditorGUILayout.Slider(new GUIContent("Opacity",""), alpha.floatValue, 0, 1f);
                EditorGUI.indentLevel--;
            }

	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(randomX, new GUIContent("Random end point X",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.randomX = randomX.boolValue;
			}
	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(randomY, new GUIContent("Random end point Y",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.randomY = randomY.boolValue;
			}

			monoTarget.rotateSpeed = EditorGUILayout.Slider(new GUIContent("Rotate forward/back",""), rotateSpeed.floatValue, -1f, 1f);
			monoTarget.smoothTime = EditorGUILayout.Slider(new GUIContent("Smooth time (sec)",""), smoothTime.floatValue, 0, 0.5f);
			monoTarget.clearTime = EditorGUILayout.Slider(new GUIContent("Clear time (sec)",""), clearTime.floatValue, 0, 30f);

	 		EditorGUI.BeginChangeCheck();
	 		EditorGUILayout.PropertyField(noClearTime, new GUIContent("No clear time",""));
			if (EditorGUI.EndChangeCheck()) {
				monoTarget.noClearTime = noClearTime.boolValue;
			}

			monoTarget.traceTime = EditorGUILayout.Slider(new GUIContent("Trace time (sec)",""), traceTime.floatValue, 0, 2f);

			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				EditorUtility.SetDirty(target);
				// set data
				/*
				if (monoTarget.data != null) {
					monoTarget.data.active = active.boolValue;
					monoTarget.data.peaks = peaks.intValue;
					monoTarget.data.level = level.floatValue;
					monoTarget.data.meshTopologySelect = (meshTopology)System.Enum.Parse(typeof(meshTopology), meshTopologySelect.enumValueIndex.ToString());
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
				*/
			}
		}
	}
}