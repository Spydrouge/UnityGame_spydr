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
	[CustomEditor (typeof(CogBlockVolumeData))]
	public class CogBlockVolumeDataInspector : Cubiquity.VolumeDataInspector
	{
		public override void OnInspectorGUI()
		{
			OnInspectorGUIImpl();
		}
	}
}
