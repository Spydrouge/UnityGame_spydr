using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;


namespace CogBlock
{
	/// <summary>
	/// The CogBlockVolume is an extension of ColoredCubesVolume  which I am using to give Colored Cubes
	/// additional functionality. It may eventually be subsumed by a similar class written by Lake.
	/// </summary>
	[ExecuteInEditMode]
	public class CogBlockVolume: ColoredCubesVolume
	{
		//we do not currently HAVE CogBlockVolumeData

		/// <summary>
		/// Creates a propery with getters/setters called data (Which will contain our volume data), which is understood to overwrite 
		/// pre-existing properties called 'data' in both ColoredCubesVolume and Volume. 
		/// </summary>
		/// <value>Returns itself</value>
		 
		//Here, 'new' is used to override existing 'data' in parent classes, and does not entail instantiation.
		// 'base' is used to reference the parent class. So this lets us cast/save/load an object of CogBlockVolumeData
		//inside a Volume mData; object inside Cubiquity Volume.cs
		public new CogBlockVolumeData data 
	    {
	        get { return (CogBlockVolumeData)base.data; }
			set { base.data = value; }
	    }



		/// <summary>
		/// When using code to create a new CogBlockVolume, we have to set up a game object with a few different scripts. This little function handles that for us, producing
		/// a game object that has all the correct renderers/colliders/etc.
		/// </summary>
		/// <returns>A game object with the correct renderer/collider/and</returns>
		/// <param name="data">This is the VolumeData object which you want to render, (.asset) attached to a corresponding .vdb. Pass in 'null' to start off rendering nothing.</param>
		/// 

		public static GameObject CreateGameObject(CogBlockVolumeData data)
		{
			//Le Game object!
			GameObject newObject = new GameObject("New CogBlock Volume");
			
			//add on the volume component :3 and then use evil powers to immediately append the data to the returned volume object, muahahah!!
			newObject.AddComponent<CogBlockVolume>().data = data;

			//Add the other important components
			newObject.AddComponent<CogBlockVolumeRenderer>(); 
			newObject.AddComponent<CogBlockVolumeCollider>(); 

			return newObject;
		}
	}

}

