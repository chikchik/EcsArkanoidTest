using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
	[MenuItem("Ecs/Show Settings")]
	public static void ShowSettings()
	{
		var settings = Resources.Load<GameSettings>("GameSettings");
		if (settings == null)
		{
			settings = CreateInstance<GameSettings>();
			settings.MultiPlayer = false;
			AssetDatabase.CreateAsset(settings, "Assets/Resources/GameSettings.asset");
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
		var settings = target as GameSettings;
		
		EditorGUILayout.LabelField("Selected host:", GUILayout.MaxWidth(80));
		var hosts = settings.GetHostsNames();
		var index = settings.GetAddressIndex();

		var newIndex = EditorGUILayout.Popup(index, hosts.ToArray());
		if (newIndex != index) {
			settings.SaveHost(newIndex);
		}
		
		if (serializedObject.hasModifiedProperties)
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(settings);
		}
	}
}
