using UnityEngine;
using System.Collections;
using UnityEditor;

using System.IO;

using CubSub;
using Cubiquity;
using Cubiquity.Impl;
using System;

namespace CogBlock
{
	public class MainMenuEntries : MonoBehaviour
	{		
		[MenuItem ("GameObject/Create Other/Cog Block Volume")]
		static void CreateCogBlockVolume()
		{
			int width = 256;
			int height = 32;
			int depth = 256;
			
			CogBlockVolumeData data = Cubiquity.VolumeDataAsset.CreateEmptyVolumeData<CogBlockVolumeData>(new Region(0, 0, 0, width-1, height-1, depth-1));
			
			GameObject obj = CogBlockVolume.CreateGameObject(data);
			
			// And select it, so the user can get straight on with editing.
			Selection.activeGameObject = obj;
			
			int floorThickness = 8;
			QuantizedColor floorColor = CubSub.HexColor.QuantHex("FF5555FF");
			
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
		
		[MenuItem ("Assets/Create/CogBlock Volume Data/Empty Volume Data")]
		static void CreateEmptyCogBlockVolumeDataAsset()
		{			
			ScriptableWizard.DisplayWizard<CreateEmptyCogBlockVolumeDataAssetWizard>("Create CogBlock Volume Data", "Create");
		}
		
		[MenuItem ("Assets/Create/CogBlock Volume Data/From Voxel Database")]
		static void CreateCogBlockVolumeDataAssetFromVoxelDatabase()
		{	
			// Resulting path already contains UNIX-style seperators (even on Wondows).
			String toVDB = EditorUtility.OpenFilePanel("Choose a Voxel Database (.vdb) file to load", CubSub.Paths.VDBFiles, "vdb");
			String toAsset = CubSub.Paths.VoxelDatabases + "/New CogBlock Volume Data From File.asset";
			
			if(toVDB.Length != 0)
			{
					// Pass through to the other version of the method.
				VolumeDataAsset.CreateFromVoxelDatabase<CogBlockVolumeData>(CubSub.Paths.RootToDirectory(Application.streamingAssetsPath, toAsset));
			}
		}
		
		
	}
}

