using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;


namespace CogBlock
{
	/// <summary>
	/// The reason the BuildFromNodeHandle code was subsumed into OctreeNode was because it the Mesh is built bit by bit, by traversing a tree. 
	/// It is obviously not specific to the whole volume and should not be handled by CogBlockVolumeCollider. Therefore, we are creating a new
	/// Octree derivation which will implement the mesh itself
	/// </summary>
	[ExecuteInEditMode]
	public class CogBlockOctreeNode: OctreeNodeAlt
	{
		/// <summary>
		/// Builds the Mesh represented by the Node. Uses the Node's nodeHandle (inherited from OctreeNodeAlt)
		/// </summary>
		/// <returns>The mesh.</returns>
		public override Mesh BuildMesh()
		{
			// At some point I should read this: http://forum.unity3d.com/threads/5687-C-plugin-pass-arrays-from-C

			Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f); // Required for the CubicVertex decoding process.
			
			// Meshes for rendering and collition.
			Mesh mesh = new Mesh();		
			
			//prevents memory leaks >:)
			mesh.hideFlags = HideFlags.DontSave;

			// Get the data from where it is stored in Cubiquity.
			int[] indices = CubiquityDLL.GetIndices(nodeHandle);		
			ColoredCubesVertex[] cubiquityVertices = CubiquityDLL.GetVertices(nodeHandle);			

			// Create the arrays which we'll copy the data to.
			Vector3[] rendererVertices = new Vector3[cubiquityVertices.Length];	
			Color32[] rendererColors = new Color32[cubiquityVertices.Length];

			//translate the data from Cubiquity's forms to Unity's
			for(int ct = 0; ct < cubiquityVertices.Length; ct++)
			{
				// Get the vertex data from Cubiquity.
				Vector3 position = new Vector3(cubiquityVertices[ct].x, cubiquityVertices[ct].y, cubiquityVertices[ct].z);

				// Part of the CubicVertex decoding process.
				position -= offset; 

				//nab the color
				QuantizedColor color = cubiquityVertices[ct].color;
				
				// Copy it to the arrays.
				rendererVertices[ct] = position;
				rendererColors[ct] = (Color32)color;
			}

			// Assign vertex data to the meshes.
			mesh.vertices = rendererVertices; 
			mesh.colors32 = rendererColors;
			mesh.triangles = indices;

			// FIXME - Get proper bounds
			mesh.bounds.SetMinMax(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(32.0f, 32.0f, 32.0f));
			mesh.name = "nodeHandle: " + nodeHandle.ToString ();
		
			return mesh;
			//return null;
		
		}
	}
}