using UnityEngine;
using UnityEditor;
using System.Collections;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Cubiquity.Impl;

//Purpose: MenuOptions for bringing in Substrate maps to Cubiquity
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{	
	public class SubstrateLoader : MonoBehaviour
	{
		//Use C# notation to attach all this code to an item in the menu
		[MenuItem ("Assets/Create/Colored Cubes Volume Data/From Substrate Data")]
		static void CreateVDBFromDat()
		{	
			//GET DA PATH (Namespaces For The Win)
			string toDat = EditorUtility.OpenFilePanel("Choose a Substrate (.dat) file to load", Paths.VoxelDatabases, "dat");

			//Something went horribly, horribly wrong. Or we hit cancel. Give up and abandon ship!
			if(toDat.Length == 0) return;


		}

		[MenuItem ("Assets/Create/Colored Cubes Volume Data/Empty Volume Data...")]
		static void CreateEmptyColoredCubesVolumeDataAsset()
		{			
			ScriptableWizard.DisplayWizard<CreateEmptyColoredCubesVolumeDataAssetWizard>("Create Colored Cubes Volume Data", "Create");
		}



		[MenuItem ("GameObject/Create Other/Colored Cubes Volume")]
		static void CreateColoredCubesVolume()
		{
			int width = 256;
			int height = 32;
			int depth = 256;
			
			ColoredCubesVolumeData data = VolumeDataAsset.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, width-1, height-1, depth-1));
			
			GameObject coloredCubesGameObject = ColoredCubesVolume.CreateGameObject(data, true, true);
			
			// And select it, so the user can get straight on with editing.
			Selection.activeGameObject = coloredCubesGameObject;
			
			int floorThickness = 8;
			QuantizedColor floorColor = new QuantizedColor(192, 192, 192, 255);
			
			for(int z = 0; z <= depth-1; z++)
			{
				for(int y = 0; y < floorThickness; y++)
				{
					for(int x = 0; x <= width-1; x++)
					{
						data.SetVoxel(x, y, z, floorColor);
					}
				}
			}
		}


		//public static VolumeDataType CreateEmptyVolumeData<VolumeDataType>(Region region, string pathToVoxelDatabase = null) where VolumeDataType : VolumeData

		//CAN USE FOR OUTPUT OF VBS
		/* USEFUL INFO (shows how to call Createemptyvolumedata more specifically, and not use randomly generated names. 
		 * public static VolumeDataType CreateEmptyVolumeData<VolumeDataType>(Region region) where VolumeDataType : VolumeData
		{			
			VolumeDataType data = VolumeData.CreateEmptyVolumeData<VolumeDataType>(region, Impl.Utility.GenerateRandomVoxelDatabaseName());
			CreateAssetFromInstance<VolumeDataType>(data);			
			return data;
		}
		*
		*/
			
			//CAN USE FOR INPUT OF DATS
			/* 
		 * public static VolumeDataType CreateFromVoxelDatabase<VolumeDataType>(string relativePathToVoxelDatabase) where VolumeDataType : VolumeData
		{			
			VolumeDataType data = VolumeData.CreateFromVoxelDatabase<VolumeDataType>(relativePathToVoxelDatabase);
			string assetName = Path.GetFileNameWithoutExtension(relativePathToVoxelDatabase);
			CreateAssetFromInstance<VolumeDataType>(data, assetName);
			return data;
		}
		*
		*/
	}
}
