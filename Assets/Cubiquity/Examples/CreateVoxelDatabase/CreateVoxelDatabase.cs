using UnityEngine;
using System;
using System.Collections;

namespace Cubiquity
{
	public class CreateVoxelDatabase : MonoBehaviour
	{
		void Start ()
		{
			int planetRadius = 60;

			// Randomize the filename incase the file already exists
			System.Random randomIntGenerator = new System.Random();
			int randomInt = randomIntGenerator.Next();
			string saveLocation = Paths.voxelDatabases + "/planet-" + randomInt + ".vdb";

			Region volumeBounds = new Region(-planetRadius, -planetRadius, -planetRadius, planetRadius, planetRadius, planetRadius);		
			TerrainVolumeData data = VolumeData.CreateEmptyVolumeData<TerrainVolumeData>(volumeBounds, saveLocation);
			
			// The numbers below control the thickness of the various layers.
			TerrainVolumeGenerator.GeneratePlanet(data, planetRadius, planetRadius - 1, planetRadius - 10, planetRadius - 35);

			// We need to commit this so that the changes made by the previous,line are actually written
			// to the voxel database. Otherwise they are just kept in temporary storage and will be lost.
			data.CommitChanges();
		}
	}
}