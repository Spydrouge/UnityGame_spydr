
using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;


namespace CogBlock
{
	/// <summary>
	/// Placeholder
	/// </summary>
	[ExecuteInEditMode]
	public class CogBlockVolumeRenderer: VolumeRenderer
	{
		void Awake()
		{
			if(material == null)
			{
				// This shader should be appropriate in most scenarios, and makes a good default.
				material = Instantiate(Resources.Load("Materials/ColoredCubes", typeof(Material))) as Material;
			}
		}
	}
}