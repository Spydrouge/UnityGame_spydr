using UnityEngine;
using UnityEditor;

using System.Collections;
using Cubiquity;
using Cubiquity.Impl;

namespace CogBlock
{	
	/// <summary>
	/// Placeholder
	/// </summary>
	[CustomEditor (typeof(CogBlockVolumeRenderer))]
	public class CogBlockVolumeRendererInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			CogBlockVolumeRenderer renderer = target as CogBlockVolumeRenderer;
			
			float labelWidth = 120.0f;
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Receive Shadows:", GUILayout.Width(labelWidth));
			renderer.receiveShadows = EditorGUILayout.Toggle(renderer.receiveShadows);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Cast Shadows:", GUILayout.Width(labelWidth));
			renderer.castShadows = EditorGUILayout.Toggle(renderer.castShadows);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			renderer.material = EditorGUILayout.ObjectField("Material: ", renderer.material, typeof(Material), true) as Material;
			EditorGUILayout.EndHorizontal();
		}
	}
}
