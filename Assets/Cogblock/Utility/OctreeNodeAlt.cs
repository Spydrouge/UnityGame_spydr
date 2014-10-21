using UnityEngine;
using Cubiquity.Impl;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Text;

using Cubiquity;

namespace CogBlock
{
	public class OctreeNodeAlt : MonoBehaviour
	{

		[System.NonSerialized]
		public uint meshLastSyncronised;
		[System.NonSerialized]
		public uint lastSyncronisedWithVolumeRenderer;
		[System.NonSerialized]
		public uint lastSyncronisedWithVolumeCollider;
		[System.NonSerialized]
		public Vector3 lowerCorner;
		[System.NonSerialized]
		public GameObject[,,] children;
		
		[System.NonSerialized]
		public uint nodeHandle;

		/// <summary>
		/// Because Octrees are MonoBehaviors, they must be attached to game objects. This static function will deliver an octreeNode, properly attached
		/// to a game object, of a given derivative type.
		/// </summary>
		/// <returns>A game object with an octree node attached.</returns>
		/// <param name="octreeType">A type derived from the class OctreeNodeAlt, specific to the volume for which this OctreeNodeAlt is to be created.</param>
		/// <param name="nodeHandle">The node handle to give the OctreeNodeAlt</param>
		/// <param name="mother">The parent game object of the OctreeNodeAlt</param>
		public static GameObject CreateOctree(Type octreeType, uint nodeHandle, GameObject mother)
		{	
			if(octreeType == null) 
			{
				Debug.LogWarning("OctreeNodeAlt->CreateOctreeNode was passed a null type. Attempting to fial gracefully by creating a non-functional OctreeNodeAlt.");
				octreeType = typeof(OctreeNodeAlt);
			}
			if(!octreeType.IsSubclassOf(typeof(OctreeNodeAlt)))
			{
				Debug.LogWarning("OctreeNodeAlt->CreateOctreeNode was passed a type ("+volumeType+") that does not inherit from OctreeNodeAlt. Attempting to fail gracefully by creating a non-functional OctreeNodeAlt.");
				octreeType = typeof(OctreeNodeAlt);
			}

			//All Octree nodes have positions. Let's get ours. 
			int nx, ny, nz;
			CubiquityDLL.GetNodePosition(nodeHandle, out nx, out ny, out nz);

			//Since we're currently saving everything in a tree of game objects, we're going to name the game objects uniquely by using their positions.
			StringBuilder name = new StringBuilder("(" + nx + ", " + ny + ", " + nz + ")");
			GameObject obj = new GameObject(name.ToString());


			//
			OctreeNodeAlt node = obj.AddComponent<octreeType.GetConstructors(System.Reflection.BindingFlags.Public)>();

			node.lowerCorner = new Vector3(nx, ny, nz);
			node.nodeHandle = nodeHandle;

			//Allows us not to view octree objects while studying the gamestate.
			//i can turn this off for debugging purposes. 
			obj.hideFlags = HideFlags.HideInHierarchy;

			//zeroing out stuff!
			//(0,0,0), (0,0,0,0), (1,1,1)
			obj.transform.localPosition = new Vector3();
			obj.transform.localRotation = new Quaternion();
			obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

			//we should be the child of something, but just in case...
			if(!mother) 
			{
				obj.transform.localPosition = node.lowerCorner;
				return obj;
			}

			//yay unity initialization stuff
			obj.layer = mother.layer;
			obj.transform.parent = mother.transform;

			//see if we are the child of a volume, or of another node...
			OctreeNodeAlt parent = mother.GetComponent<OctreeNodeAlt>();
			
			if(!parent)
			{
				obj.transform.localPosition = node.lowerCorner;
				return obj;
			}

			Vector3 parentLowerCorner = parent.lowerCorner;
			obj.transform.localPosition = node.lowerCorner - parent.lowerCorner;

			return obj;
		}

		//The purpose of availableNodeSyncs is because: This function is recursive. availableNodeSyncs tells it how deep to plumb
		public int syncNode(int availableNodeSyncs, GameObject voxelTerrainGameObject)
		{

			//-------------------------------------------
			//				PHASE ONE: Building Meshes!
			//-------------------------------------------


			//terminator of the all-powerful recursion
			if(availableNodeSyncs <= 0) return 0;
			'
			int nodeSyncsPerformed = 0;
			
			uint meshLastUpdated = CubiquityDLL.GetMeshLastUpdated(nodeHandle);		
			
			if(meshLastSyncronised < meshLastUpdated)
			{			
				if(CubiquityDLL.NodeHasMesh(nodeHandle) == 1)
				{					
					// Set up the rendering mesh											
					VolumeRenderer volumeRenderer = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
					if(volumeRenderer != null)
					{						
						//Mesh renderingMesh = volumeRenderer.BuildMeshFromNodeHandle(nodeHandle);
						
						Mesh renderingMesh = null;

						//-------------------------------------------
						//				THE CODE OF EVIL!
						//-------------------------------------------

						Type volType = voxelTerrainGameObject.GetComponent<Volume>().GetType();

						if(volType == typeof(TerrainVolume))
						{
							renderingMesh = BuildMeshFromNodeHandleForTerrainVolume(nodeHandle);
						}
						else if(volType == typeof(ColoredCubesVolume))
						{
							renderingMesh = BuildMeshFromNodeHandleForColoredCubesVolume(nodeHandle);
						}
						else if(volType == typeof(CogBlockVolume))
						{
							renderingMesh = BuildMeshFromNodeHandleForCogBlockVolume(nodeHandle);
						}
						
						MeshFilter meshFilter = gameObject.GetOrAddComponent<MeshFilter>() as MeshFilter;
						MeshRenderer meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>() as MeshRenderer;
						
						if(meshFilter.sharedMesh != null)
						{
							DestroyImmediate(meshFilter.sharedMesh);
						}
						
						meshFilter.sharedMesh = renderingMesh;
						
						meshRenderer.sharedMaterial = volumeRenderer.material;
						
						#if UNITY_EDITOR
						EditorUtility.SetSelectedWireframeHidden(meshRenderer, true);
						#endif
					}
					
					// Set up the collision mesh
					VolumeCollider volumeCollider = voxelTerrainGameObject.GetComponent<VolumeCollider>();					
					if((volumeCollider != null) && (Application.isPlaying))
					{
						Mesh collisionMesh = volumeCollider.BuildMeshFromNodeHandle(nodeHandle);
						MeshCollider meshCollider = gameObject.GetOrAddComponent<MeshCollider>() as MeshCollider;
						meshCollider.sharedMesh = collisionMesh;
					}
				}
				// If there is no mesh in Cubiquity then we make sure there isn't on in Unity.
				else
				{
					MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>() as MeshCollider;
					if(meshCollider)
					{
						DestroyImmediate(meshCollider);
					}
					
					MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
					if(meshRenderer)
					{
						DestroyImmediate(meshRenderer);
					}
					
					MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>() as MeshFilter;
					if(meshFilter)
					{
						DestroyImmediate(meshFilter);
					}
				}
				
				meshLastSyncronised = CubiquityDLL.GetCurrentTime();
				availableNodeSyncs--;
				nodeSyncsPerformed++;
				
			}

			//-------------------------------------------
			//			PHASE TWO: Synchronize Renderer and Collider
			//-------------------------------------------
			
			VolumeRenderer vr = voxelTerrainGameObject.GetComponent<VolumeRenderer>();
			MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
			if(vr != null && mr != null)
			{
				if(mr.enabled != vr.enabled) // Not sure we really need this check?
				{
					mr.enabled = vr.enabled;
				}
				
				if(lastSyncronisedWithVolumeRenderer < vr.lastModified)
				{
					mr.receiveShadows = vr.receiveShadows;
					mr.castShadows = vr.castShadows;
					lastSyncronisedWithVolumeRenderer = Clock.timestamp;
				}
			}
			
			VolumeCollider vc = voxelTerrainGameObject.GetComponent<VolumeCollider>();
			MeshCollider mc = gameObject.GetComponent<MeshCollider>();
			if(vc != null && mc != null)
			{
				if(mc.enabled != vc.enabled) // Not sure we really need this check?
				{
					mc.enabled = vc.enabled;
				}
				
				if(lastSyncronisedWithVolumeCollider < vc.lastModified)
				{
					// Actual syncronization to be filled in in the future when we have something to syncronize.
					lastSyncronisedWithVolumeCollider = Clock.timestamp;
				}
			}


			//-------------------------------------------
			//			PHASE THREE: RECURSE!
			//-------------------------------------------
			
			//Now syncronise any children
			for(uint z = 0; z < 2; z++)
			{
				for(uint y = 0; y < 2; y++)
				{
					for(uint x = 0; x < 2; x++)
					{
						if(CubiquityDLL.HasChildNode(nodeHandle, x, y, z) == 1)
						{					
							
							uint childNodeHandle = CubiquityDLL.GetChildNode(nodeHandle, x, y, z);					
							
							GameObject childGameObject = GetChild(x,y,z);
							
							if(childGameObject == null)
							{							
								childGameObject = OctreeNodeAlt.CreateOctreeNode(childNodeHandle, gameObject);
								
								SetChild(x, y, z, childGameObject);
							}
							
							//syncNode(childNodeHandle, childGameObject);
							
							OctreeNodeAlt childOctreeNode = childGameObject.GetComponent<OctreeNodeAlt>();
							int syncs = childOctreeNode.syncNode(availableNodeSyncs, voxelTerrainGameObject);
							availableNodeSyncs -= syncs;
							nodeSyncsPerformed += syncs;
						}
					}
				}
			}
			
			return nodeSyncsPerformed;
		}
		
		public GameObject GetChild(uint x, uint y, uint z)
		{
			if(children != null)
			{
				return children[x, y, z];
			}
			else
			{
				return null;
			}
		}
		
		public void SetChild(uint x, uint y, uint z, GameObject gameObject)
		{
			if(children == null)
			{
				children = new GameObject[2, 2, 2];
			}
			
			children[x, y, z] = gameObject;
		}
		
	}
}
