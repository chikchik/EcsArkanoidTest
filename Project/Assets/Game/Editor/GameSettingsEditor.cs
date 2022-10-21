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
				settings.SelectedUdpHostIndex = i;
			}

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("address", GUILayout.Width(50));
			settings.UdpHosts[i].Address = GUILayout.TextField(settings.UdpHosts[i].Address);
			EditorGUILayout.EndHorizontal();

			if (IPAddress.TryParse(settings.UdpHosts[i].Address, out _))
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("port", GUILayout.Width(50));
				settings.UdpHosts[i].Port = EditorGUILayout.IntField(settings.UdpHosts[i].Port);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

			if (GUILayout.Button("Del", GUILayout.Width(30)))
			{
				List<GameSettingsScriptable.UdpHost> tmp = new List<GameSettingsScriptable.UdpHost>(settings.UdpHosts);
				tmp.RemoveAt(i);
				settings.UdpHosts = tmp.ToArray();
			}

			EditorGUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Add"))
		{
			Array.Resize(ref settings.UdpHosts, settings.UdpHosts.Length + 1);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
