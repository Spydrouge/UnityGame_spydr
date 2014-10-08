﻿using UnityEngine;
using System.Collections;

using Cubiquity.Impl;

namespace Cubiquity
{
	[ExecuteInEditMode]
	/// Causes the terrain volume to have a collision mesh and allows it to participate in collisions.
	/**
	 * See the base VolumeCollider class for further details.
	 */
	public class TerrainVolumeCollider : VolumeCollider
	{
		public override Mesh BuildMeshFromNodeHandle(uint nodeHandle)
		{
			Mesh collisionMesh = new Mesh();
			collisionMesh.hideFlags = HideFlags.DontSave;

			// Get the data from Cubiquity.
			int[] indices = CubiquityDLL.GetIndicesMC(nodeHandle);		
			TerrainVertex[] cubiquityVertices = CubiquityDLL.GetVerticesMC(nodeHandle);			
			
			// Create the arrays which we'll copy the data to.	
			Vector3[] vertices = new Vector3[cubiquityVertices.Length];
			
			for(int ct = 0; ct < cubiquityVertices.Length; ct++)
			{
				// Get the vertex data from Cubiquity.
				vertices[ct] = new Vector3(cubiquityVertices[ct].x, cubiquityVertices[ct].y, cubiquityVertices[ct].z);
				vertices[ct] *= (1.0f / 256.0f);
			}
			
			// Assign vertex data to the meshes.
			collisionMesh.vertices = vertices;
			collisionMesh.triangles = indices;
			
			return collisionMesh;
		}
	}
}
