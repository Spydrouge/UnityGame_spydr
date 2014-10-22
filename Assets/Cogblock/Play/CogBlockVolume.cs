using UnityEngine;
using UnityEditor;

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
	public class CogBlockVolume: Volume
	{

		[System.NonSerialized]
		public CogBlockOctreeNode rootOctreeNode = null;

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
			/*CogBlockVolumeRenderer volRend = */newObject.AddComponent<CogBlockVolumeRenderer>(); 
			newObject.AddComponent<CogBlockVolumeCollider>(); 

			/*//initialize the volume renderer so that it's normals face in the correct direction
			if(volRend != null) //I miss actionscript with lazy evaluation so I could string these all together in one big if..
			{
				if(volRend.material != null)
				{		
					// We compute surface normals using derivative operations in the fragment shader, but for some reason
					// these are backwards on Linux. We can correct for this in the shader by setting the multiplier below.
					#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
					float normalMultiplier = -1.0f;
					#else
					float normalMultiplier = 1.0f;
					#endif					
					volRend.material.SetFloat("normalMultiplier", normalMultiplier);
				}
			}*/

			return newObject;
		}

		/// <summary>
		/// Cubiquity itself lamented a lack of the ability to select objects based on raycasting. In light of that, I'm going to attempt getting this function to work.
		/// </summary>
		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				RaycastHit hitInfo = new RaycastHit();
				if(hitInfo.transform == null) return;
				if (this != hitInfo.transform.gameObject) return;
			
				Selection.activeObject = this;
				Debug.Log ("RaycastingSelect");

			}

			/*if(otherdebug >= 1 && data != null)
			{
				Debug.Log("Data: " + data.printData());
				otherdebug = 0;
				debugvar = 1;
			}*/
		}


				      

		/// <summary>
		/// Synchronize this instance.
		/// </summary>
		protected override void Synchronize()
		{			
			//super!
			base.Synchronize();


			//Not sure if this needs to be done every synchronization but okay
			CogBlockVolumeRenderer volRend = gameObject.GetComponent<CogBlockVolumeRenderer>();

			//initialize the volume renderer so that it's normals face in the correct direction
			if(volRend != null) //I miss actionscript with lazy evaluation so I could string these all together in one big if..
			{
				if(volRend.material != null)
				{		
					// We compute surface normals using derivative operations in the fragment shader, but for some reason
					// these are backwards on Linux. We can correct for this in the shader by setting the multiplier below.
					#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
					float normalMultiplier = -1.0f;
					#else
					float normalMultiplier = 1.0f;
					#endif					
					volRend.material.SetFloat("normalMultiplier", normalMultiplier);
				}
			}
						
			// Check to make sure we have anything to Synchronize
			if(data == null)
			{
				return;
			}
		
			//still checking
			if(!data.volumeHandle.HasValue)
			{
				return;
			}

			//update the volume
			CubiquityDLL.UpdateVolume(data.volumeHandle.Value);

			//try to synchronize the octrees
			if(CubiquityDLL.HasRootOctreeNode(data.volumeHandle.Value) == 1)
			{		
				if(rootOctreeNode == null || rootOctreeNodeGameObject == null)
				{
					Debug.Log("Creating RootOctreeNode from null");
					uint rootNodeHandle = CubiquityDLL.GetRootOctreeNode(data.volumeHandle.Value);
					rootOctreeNode = OctreeNodeAlt.CreateOctreeNode(typeof(CogBlockOctreeNode), rootNodeHandle, gameObject) as CogBlockOctreeNode;	
					rootOctreeNodeGameObject = rootOctreeNode.gameObject;
				}

				//Sync the Node and remember it will return syncs remaining. If some are remaining, the mesh is syncronized.
				isMeshSyncronized = (rootOctreeNode.SyncNode(maxNodesPerSync) != 0);

			}
		}
	}

}

