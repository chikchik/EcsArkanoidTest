using System;
using System.Collections.Generic;
using System.Net;
using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSettingsScriptable))]

public class GameSettingsEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		GameSettingsScriptable settings = target as GameSettingsScriptable;
		
		EditorGUILayout.PropertyField(serializedObject.FindProperty("MultiPlayer"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("IsLocalServer"));

		for (int i = 0; i < settings.UdpHosts.Length; i++)
		{
			EditorGUILayout.BeginHorizontal();
			if (EditorGUILayout.Toggle(settings.SelectedUdpHostIndex == i, "radio", GUILayout.Width(20)))
			{
				if (settings.SelectedUdpHostIndex != i)
				{
					settings.SelectedUdpHostIndex = i;
					EditorUtility.SetDirty(settings);
				}
			}

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("address", GUILayout.Width(50));
			var address = GUILayout.TextField(settings.UdpHosts[i].Address);
			if (settings.UdpHosts[i].Address != address)
			{
				settings.UdpHosts[i].Address = address;
				EditorUtility.SetDirty(settings);
			}

			EditorGUILayout.EndHorizontal();

			if (IPAddress.TryParse(settings.UdpHosts[i].Address, out _))
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("port", GUILayout.Width(50));
				var port = EditorGUILayout.IntField(settings.UdpHosts[i].Port);
				if (settings.UdpHosts[i].Port != port)
				{
					settings.UdpHosts[i].Port = port;
					EditorUtility.SetDirty(settings);

				}
				 
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

			if (GUILayout.Button("Del", GUILayout.Width(30)))
			{
				List<GameSettingsScriptable.UdpHost> tmp = new List<GameSettingsScriptable.UdpHost>(settings.UdpHosts);
				tmp.RemoveAt(i);
				settings.UdpHosts = tmp.ToArray();
				EditorUtility.SetDirty(settings);
			}

			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Add"))
		{
			Array.Resize(ref settings.UdpHosts, settings.UdpHosts.Length + 1);
			EditorUtility.SetDirty(settings);
		}
		
		AssetDatabase.SaveAssetIfDirty(settings);
	}
}
