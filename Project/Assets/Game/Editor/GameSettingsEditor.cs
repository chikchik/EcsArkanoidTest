using System;
using System.Collections.Generic;
using System.Net;
using Game;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameSettings))]

public class GameSettingsEditor : Editor
{
	[MenuItem("Ecs/Show settings")]
	public static void ShowSettings()
	{
		var settings = Resources.Load("GameSettings");
		Selection.activeObject = settings;
		EditorGUIUtility.PingObject(settings);
	}
	
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
		EditorGUI.EndDisabledGroup();
		
		serializedObject.Update();
		GameSettings settings = target as GameSettings;

		var isMultiPlayer = EditorGUILayout.Toggle("Multi Player", settings.MultiPlayer);
		if (isMultiPlayer != settings.MultiPlayer)
		{
			settings.MultiPlayer = isMultiPlayer;
			EditorUtility.SetDirty(settings);
		}
		
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
				GUILayout.Label("tcpPort", GUILayout.Width(50));
				var ipPort = EditorGUILayout.IntField(settings.UdpHosts[i].tcpPort);
				if (settings.UdpHosts[i].tcpPort != ipPort)
				{
					settings.UdpHosts[i].tcpPort = ipPort;
					EditorUtility.SetDirty(settings);

				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("udpPort", GUILayout.Width(50));
				var udpPort = EditorGUILayout.IntField(settings.UdpHosts[i].udpPort);
				if (settings.UdpHosts[i].udpPort != udpPort)
				{
					settings.UdpHosts[i].udpPort = udpPort;
					EditorUtility.SetDirty(settings);
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

			if (GUILayout.Button("Del", GUILayout.Width(30)))
			{
				List<GameSettings.ConnectionHost> tmp = new List<GameSettings.ConnectionHost>(settings.UdpHosts);
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
