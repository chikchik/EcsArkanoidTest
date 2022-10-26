using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CommonEcsGameSettings))]
public class CommonEcsGameSettingsEditor : Editor
{
	[MenuItem("XFlowEcs/Show Common Ecs Game Settings")]
	public static void ShowSettings()
	{
		var settings = Resources.Load<CommonEcsGameSettings>("CommonEcsGameSettings");
		if (settings == null)
		{
			settings = CreateInstance<CommonEcsGameSettings>();
			settings.MultiPlayer = false;
			AssetDatabase.CreateAsset(settings, "Assets/Resources/CommonEcsGameSettings.asset");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		Selection.activeObject = settings;
		EditorGUIUtility.PingObject(settings);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawDefaultInspector();
		var settings = target as CommonEcsGameSettings;
		
		EditorGUILayout.LabelField("Selected Server:");
		var hosts = settings.GetServerNames();
		var index = settings.GetSelectedServer();

		var newIndex = EditorGUILayout.Popup(index, hosts.ToArray());
		if (newIndex != index) {
			settings.SelectServerByIndex(newIndex);
		}
		
		if (serializedObject.hasModifiedProperties)
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(settings);
		}
	}
}
