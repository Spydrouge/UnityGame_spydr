using UnityEngine;
using Cubiquity.Impl;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


using Cubiquity;

namespace CogBlock
{
	public class OctreeNodeAlt : MonoBehaviour
	{

		[System.NonSerialized]
		public uint meshLastSyncronised;
		[System.NonSerialized]
		public Vector3 lowerCorner;
		[System.NonSerialized]
		public OctreeNodeAlt[] children;

		[System.NonSerialized]
		/// <summary> We store this to reference the location CubiquityDLL is storing our Octree information. </summary>
		public uint nodeHandle;

		//at a later date, there shall be a better way of handling both of these
		//probably the octreenodes don't even need to hold onto them forever; or
		//decisions could be percolated down at key points. 
		[System.NonSerialized]
		public VolumeRenderer volRend = null;
		[System.NonSerialized]
		public VolumeCollider volColl = null;

		/// <summary>
		/// Because Octrees are MonoBehaviors, they must be attached to game objects. This static function will deliver an octreeNode, properly attached
		/// to a game object, of a given derivative type.
		/// </summary>
		/// <returns>A game object with an octree node attached.</returns>
		/// <param name="octreeType">A type derived from the class OctreeNodeAlt, specific to the volume for which this OctreeNodeAlt is to be created.</param>
		/// <param name="nodeHandle">The node handle to give the OctreeNodeAlt</param>
		/// <param name="parent">The parent game object of the OctreeNodeAlt</param>
		public static OctreeNodeAlt CreateOctreeNode(Type octreeType, uint nodeHandle, GameObject parent)
		{
			if(octreeType == null) 
			{
				Debug.LogWarning("OctreeNodeAlt->CreateOctreeNode was passed a null type. Attempting to fail gracefully by creating a non-functional OctreeNodeAlt.");
				octreeType = typeof(OctreeNodeAlt);
			}
			if(!octreeType.IsSubclassOf(typeof(OctreeNodeAlt)))
			{
				Debug.LogWarning("OctreeNodeAlt->CreateOctreeNode was passed a type ("+octreeType+") that does not inherit from OctreeNodeAlt. Attempting to fail gracefully by creating a non-functional OctreeNodeAlt.");
				octreeType = typeof(OctreeNodeAlt);
			}
						
			//new object
			GameObject obj = new GameObject();
		
			//Add the component of octreeType, DYNAMICALLY MUAHAHAHAHHA
			OctreeNodeAlt node = obj.AddComponent(octreeType) as OctreeNodeAlt;

			//follow the addComponent immediately (I like breaking things into chunks)
			node.InitializeNode(nodeHandle);
			node.InitializeObj(null);
			node.AttachParent(parent);

			return node;
		}

		/// <summary>
		/// Initializes the GameObject to which a new node will belong, and does not have to be called by the average Editor-User. Already called in CreateOctreeNode, immediately after InitializeNode.
		/// It may be overwritten with care (nodeHandle is particularly important).
		/// </summary>
		/// <param name="nodeHandle">The CubiquityDLL handle of the node we're creating. </param>
		protected virtual void InitializeNode(uint nodeHandle)
		{
			//DIS IS VEDDY, VEDDY IMPORTANT. Without this, we can do nothing. NOTHING! Call super to let this happpen!
			this.nodeHandle = nodeHandle;

			//All Octree nodes have positions. Let's get ours in 3 easy steps.
			int nx, ny, nz;
			CubiquityDLL.GetNodePosition(nodeHandle, out nx, out ny, out nz);
			lowerCorner = new Vector3(nx, ny, nz);
		}

		/// <summary>
		/// Initializes the GameObject to which a new node will belong, and does not have to be called by the average Editor-User. Typically called in CreateOctreeNode, immediately after InitializeNode.
		/// It may be overwritten with care (particularly in building the localPosition)
		/// </summary>
		/// <param name="arg">Unused argument parameter, in the event a future Editor-User wishes to overwrite this function and send in parameters.</param>
		protected virtual void InitializeObj(GameObject arg = null)
		{
			//Allows us not to view octree objects while studying the gamestate.
			//i can turn this off for debugging purposes. 
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			
			//zeroing out stuff!
			//(0,0,0), (0,0,0,0), (1,1,1)
			gameObject.transform.localPosition = new Vector3();  //Yes this will be immediately overwritten, but we're trying to make sure people dont' break things!
			gameObject.transform.localRotation = new Quaternion();
			gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

			//just in case
			if(!lowerCorner) return;

			//and set the name for debug purposes ;)
			gameObject.name = new StringBuilder("(" + lowerCorner.x + ", " + lowerCorner.y + ", " + lowerCorner.z + ")").ToString();
		}

		/// <summary>
		/// Performs operations on a new OctreeNodeAlt GameObject, and does not have to be called by the average Editor-User. Typically called in CreateOctreeNode, immediately after InitializeNode.
		/// It may be overwritten with care (particularly in building the localPosition)
		/// </summary>
		/// <param name="parent">The parent game object, typically expected to either be something derived from Cubiquity.Volume OR another Octree Node of an identical type.</param>
		public virtual void AttachParent(GameObject parent)
		{
			//in an attempt to prevent -= on a null when silly people get involved ;)
			if(lowerCorner)
				gameObject.transform.localPosition = lowerCorner;

			//we Should be the child of something, but just in case...
			if(!parent)	return;
			
			//yay unity-relevant initialization stuff
			gameObject.layer = parent.layer;
			gameObject.transform.parent = parent.transform; //this is actually what physically hooks them together. Compare to addChild really.
			
			//see if we are the child of a volume, or of another node...
			OctreeNodeAlt parentNode = parent.GetComponent<OctreeNodeAlt>();

			//if there is no parent node...
			if(parentNode)
			{
				//Transform the localposition by the parentnode
				gameObject.transform.localPosition -= parentNode.lowerCorner;

				//grab the parent node's volume renderer & collider
				volRend = parentNode.volRend;
				volColl = parentNode.volColl;
			}
			else
			{
				//otherwise I really hope this is the child of a Volume!
				volRend = parent.GetComponent<VolumeRenderer>();
				volColl = parent.GetComponent<VolumeCollider>();
			}

		
		}

		/// <summary>
		/// This function is a placeholder for derived OctreeNode classes to build their meshes from Cubiquity Data and attach these meshes to themselves.  
		/// It is left 'virtual' instead of 'abstract' so that OctreeNodeAlt can be instantiated as a placeholder in the event that all hell breaks loose somewhere
		/// else in the code.
		/// </summary>
		public virtual Mesh BuildMesh()
		{
			return null;
		}

		/// <summary>
		/// Protected function called by SynchNode() for code clarity. Most likely this should never be overwritten.
		/// </summary>
		protected virtual void NodeMeshDown()
		{
			MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>() as MeshCollider;
			if(meshCollider) DestroyImmediate(meshCollider);
			
			MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>() as MeshRenderer;
			if(meshRenderer) DestroyImmediate(meshRenderer);
			
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>() as MeshFilter;
			if(meshFilter) DestroyImmediate(meshFilter);
		}

		/// <summary>
		/// Protected function called by SynchNode() for code clarity. Most likely this should never be overwritten.
		/// </summary>
		protected virtual void NodeMeshUp()
		{
			//BUILD LE MESH! (This is the function that needs to be implemented by derived classes) 
			Mesh mesh = BuildMesh();

			//throw in the mesh render schtuff if and only if there's even a volRend component to begin with.
			if(volRend)
			{
				//Make sure these components exist, and nab hold of them
				MeshFilter meshFilter = gameObject.GetOrAddComponent<MeshFilter>() as MeshFilter;
				MeshRenderer meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>() as MeshRenderer;

				//make sure we release any previously existing dataz.
				if(meshFilter.sharedMesh != null) DestroyImmediate(meshFilter.sharedMesh);
							
				//FIXME: Can I figure out a good explanation for wtf this is using .sharedMesh instead of .mesh for?
				//In any event, send in the mesh 
				meshFilter.sharedMesh = mesh;

				//and then nab the choices made in volRend
				meshRenderer.sharedMaterial = volRend.material;
				meshRenderer.enabled = volRend.enabled;
				meshRenderer.castShadows = volRend.castShadows;
				meshRenderer.receiveShadows = volRend.receiveShadows;

				
				#if UNITY_EDITOR
					EditorUtility.SetSelectedWireframeHidden(meshRenderer, true);
				#endif
			}

			//throw in the same mesh  to collider and find out later if that was a bad idea XD
			if(volColl && Application.isPlaying)
			{
				MeshCollider meshCollider = gameObject.GetOrAddComponent<MeshCollider>() as MeshCollider;

				//send in the mesh
				meshCollider.sharedMesh = mesh;

				//and then nab the choices made in volColl
			}
		}

		public void RelayRendererChanges(VolumeRenderer volRend)
		{

		}
		
		//The purpose of availableNodeSyncs is because: This function is recursive. availableNodeSyncs tells it how deep to plumb
		/// <summary>
		/// A recursive function responsible for synchronizing Unity data with the CubiquityDLL data. 
		/// </summary>
		/// <returns>The amount of synchronization steps left unexpended.</returns>
		/// <param name="availableNodeSyncs">a parameter to track how much synchronization can and will be done from recrusive layer to recursive layer.</param>
		public int SyncNode(int availableNodeSyncs)
		{
			//terminator of the all-powerful recursion
			if(availableNodeSyncs <= 0) return 0;

			//-------------------------------------------
			//				PHASE ONE: Building Meshes!
			//-------------------------------------------

			//is a synchronization even called for?
			uint meshLastUpdated = CubiquityDLL.GetMeshLastUpdated(nodeHandle);		

			//we don't have to synchronize this node if it wasn't updated.
			if(meshLastSyncronised >= meshLastUpdated)
			{
				//I like major control branches that are concise to read!
				if(CubiquityDLL.NodeHasMesh(nodeHandle) == 1)
					NodeMeshDown ();
				else NodeMeshUp ();

				//absolutely do not decrement availableNodesyncs if no meshSyncing occured, or we'll never 
				//explore the whole tree @.@
				meshLastSyncronised = CubiquityDLL.GetCurrentTime();
				availableNodeSyncs--;
			}

			//-------------------------------------------
			//			PHASE THREE: RECURSE!
			//-------------------------------------------
			uint unusedChildren = 0;

			//normally we would have to iterate across the two x children, the two y children, and the two z children using 3 nested
			//for-loops. Typically speaking, nesting for loops in a recursive function causes nausea, dry mouth, dizzinss and vomiting. 
			//Let's flatten the array!
			for(uint x =0, y = 0, z = 0, i = 0;; x++, i++)
			{
				//iteration code!
				if(x > 1) {y++; x=0;}
				if(y > 1) {z++; y=0;}
				if(z > 1) break;

				//Steps:
				// 1) Ascertain whether we SHOULD have a node
				// 1.5) Ascertain whether we have a children array.
				// 2) Ascertain whether we DO have a node
				// 3) Match/Sync Them

				bool needChildNode = (CubiquityDLL.HasChildNode(nodeHandle, x, y, z) == 1);

				//if we need a child Node and have none, we need to init the array. If we need no children but have them, we need to unmake them. 
				if(needChildNode && !children) 
				{
					children = new OctreeNodeAlt[8](null, null, null, null, null, null, null, null);
				}
				else if(!needChildNode && children) 
				{
					if(children[i])
						DisposeChild(i); 
					continue;
				}

				//Useage of the "?" operator!!!!!
				//  	condition ? first_expression : second_expression;
				//		if condition == true, whole block evaluates as first_expression. If condition == false, whole block evaluates as  second_expression
				OctreeNodeAlt childNode = GetChild(x,y,z);

				//check if there's a child on the dll side
				if(CubiquityDLL.HasChildNode(nodeHandle, x, y, z) != 1)
				{	
					//Dispose of this child and try the next one immediately
					DisposeChild(x, y, z);
					continue;
				}

				//nab the child Node Handle
				uint childNodeHandle = CubiquityDLL.GetChildNode(nodeHandle, x, y, z);					

				//make sure our helper array knows about this child.
				SetChild(x, y, z, childNodeHandle);

				//and synchronize the child
				availableNodeSyncs = childNode.SyncNode(availableNodeSyncs);
			}

			
			return availableNodeSyncs;
		}


		protected OctreeNodeAlt GetChild(uint x, uint y, uint z)
		{
			//I love bit shifting for flatteninig things
			uint i = x + y<<1 + z<<2;

			//make sure we're not out of bounds, and that we have an array to search in
			if(i > 8 || !children) return null;

			return children[i];

		}
		
		protected void SetChild(uint x, uint y, uint z, uint childNodeHandle)
		{
			//i love bit shifting
			uint i = x + y<<1 + z<<2;

			//make sure no errors occur
			if(i>8) return;

			//nodes are not initialized with this array. Keeps down costs for leaves
			if(children == null) children = new OctreeNodeAlt[8](null, null, null, null, null, null, null, null);

			if(children[i] != null)
			{
				if(children[i].nodeHandle == childNodeHandle) return;
				else DisposeChild(x, y, z)
			}

			//create a child node of the same type!
			children[i] = OctreeNodeAlt.CreateOctreeNode(this.GetType(), childNodeHandle, gameObject);
		}
		
	}
}
