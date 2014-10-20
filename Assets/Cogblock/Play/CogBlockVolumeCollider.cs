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
	public class CogBlockVolumeCollider: VolumeCollider
	{

		//what the devil is a nodeHandle?
		public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
		{
			Debug.Log("Trying to build ColoredCubes mesh from node");		
			
			// Create rendering and possible collision meshes.
			Mesh collisionMesh = new Mesh();		
			collisionMesh.hideFlags = HideFlags.DontSave;
			
			// Get the data from Cubiquity.
			int[] indices = CubiquityDLL.GetIndices(nodeHandle);		
			ColoredCubesVertex[] cubiquityVertices = CubiquityDLL.GetVertices(nodeHandle);			
			
			// Create the arrays which we'll copy the data to.
			Vector3[] vertices = new Vector3[cubiquityVertices.Length];
			
			for(int ct = 0; ct < cubiquityVertices.Length; ct++)
			{
				// Get the vertex data from Cubiquity.
				vertices[ct] = new Vector3(cubiquityVertices[ct].x, cubiquityVertices[ct].y, cubiquityVertices[ct].z);
			}
			
			//FIXME - set collision mesh bounds as we do with rendering mesh?
			collisionMesh.vertices = vertices;
			collisionMesh.triangles = indices;

			Debug.Log ("Collision Mesh Built from verticies of length: " + cubiquityVertices.Length.ToString());

			return collisionMesh;
		}
	}
}
