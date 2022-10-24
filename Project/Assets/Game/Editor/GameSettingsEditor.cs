using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
	[MenuItem("Ecs/Show Settings")]
	public static void ShowSettings()
	{
		var settings = Resources.Load("GameSettings");
		Selection.activeObject = settings;
		EditorGUIUtility.PingObject(settings);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUI.EndDisabledGroup();

		EditorGUILayout.PropertyField(serializedObject.FindProperty("MultiPlayer"));
		var settings = target as GameSettings;
		if (!settings.MultiPlayer) {
			EditorGUI.BeginDisabledGroup(true);
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("_containerHosts"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_ipHosts"));

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.LabelField("Selected host:", GUILayout.MaxWidth(80));
			var savedHost = serializedObject.FindProperty("_savedHostIndex").intValue;
			var hosts = settings.GetHostsNames();
			var newIndex = EditorGUILayout.Popup(savedHost, hosts.ToArray());
			if (newIndex != savedHost) {
				settings.SaveHost(newIndex);
			}
		}
		EditorGUILayout.EndHorizontal();

		if (!settings.MultiPlayer) {
			EditorGUI.EndDisabledGroup();
		}

		if (serializedObject.hasModifiedProperties)
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(settings);
		}
	}
}
