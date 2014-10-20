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
			
			if(toVDB.Length == 0) return;

			// Pass through to the other (deeper) version of the method.
			VolumeData data = VolumeData.CreateFromVoxelDatabase<CogBlockVolumeData>(toVDB, VolumeData.WritePermissions.ReadWrite);

			//to stop future errors and data corruption in the event a path cannot be opened. 
			if(data == null) return;

			if(File.Exists (toAsset))
			{
				//check out if this is a ColoredCubesVolumeData or not
				CogBlockVolumeData oldData = AssetDatabase.LoadAssetAtPath(toAsset, typeof(CogBlockVolumeData)) as CogBlockVolumeData;
				
				if(oldData != null)
				{
					Debug.Log ("A stray .asset file has been found with an identical name but with a .vdb at " + oldData.fullPathToVoxelDatabase + " Will attempt to shutdown and overwrite the .asset without harming the .vdb");
					
					//again, this little bugger is going to help me refresh all the ColoredCubesVolumes at the end
					//replacer.Add (pathVDB, oldData);
					
					//I'm going out on a limb here to see if this works... If it doesn't, we can fudge around a little
					//more or just try to fail gracefully.
					oldData.ShutdownCubiquityVolume();
					
					//I am on the fence about whether I want to relink this data. And I don't think I do. After all, our previous foreach iterator 
					//would have found this current oldData if there wasn't a mixmatch with vdbs. 
				}
				else
				{
					Debug.Log("An .asset of a different type (non CogBlockVolumeData) has been found at the save location. Attempting to overwrite it.");
				}
				
				
				//now let's try and delete the asset itself so we get no linking errors...
				AssetDatabase.DeleteAsset(toAsset);
			}

			String pathAsset = CubSub.Paths.RootToDirectory(Application.dataPath, toAsset);
			Debug.Log ("Trying to save .asset at: " + pathAsset);
			
			//Create the asset
			AssetDatabase.CreateAsset(data, pathAsset);
			AssetDatabase.SaveAssets();
			
			//Do some selection/saving/cleanup
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = data;
		}
		
		
	}
}

