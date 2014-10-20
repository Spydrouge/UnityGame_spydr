﻿using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	[ExecuteInEditMode]
	/// Causes the colored cubes volume to have a collision mesh and allows it to participate in collisions.
	/**
	 * See the base VolumeCollider class for further details.
	 */
	public class ColoredCubesVolumeCollider : VolumeCollider
	{
		public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
		{
			// At some point I should read this: http://forum.unity3d.com/threads/5687-C-plugin-pass-arrays-from-C
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
		
			return collisionMesh;
		}
	}
}
