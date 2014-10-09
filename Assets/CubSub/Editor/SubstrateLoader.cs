using UnityEngine;
using UnityEditor;
using System.Collections;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Cubiquity.Impl;

//Purpose: Editor Window for bringing in Substrate maps to Cubiquity
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{	
	//By Deriving from EditorWindow, I have created an independent piece of software accessible at edit time that does not have to be attached to a gameobject, like ColoredCubeVolume
	//It will be its own panel, and hopefully more useful as a result. 
	public class SubstrateLoader : EditorWindow
	{
		//This sexy code puts this EditorWindow into Unity's Menus (ya know, at the top of the program)
		[MenuItem ("Window/OpenCog/Substrate Loader")]

		//LOOK AT ME COPY-PASTING UNITY CODE, DOH!
		//(this function is built into Unity, and makes sure if we hit the same menu option twice, we
		// don't bring up two different windows, but rather create the 1st one or re-select the 1st one if
		// it already exists.)
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(SubstrateLoader));
		}

		//Dis is vhere de actual 'look' of the panel goes.
		void OnGUI()
		{

			GUILayout.Label("Substrate File", EditorStyles.boldLabel);

			DrawInstructions("Create new volume data through 'Main Menu -> Assets -> Create -> Colored Cubes Volume Data' and then assign it below.");
			//coloredCubesVolume.data = EditorGUILayout.ObjectField("Volume Data: ", coloredCubesVolume.data, typeof(ColoredCubesVolumeData), true) as ColoredCubesVolumeData;
		}


		[MenuItem ("Assets/Create/Colored Cubes Volume Data/Empty Volume Data...")]
		static void CreateEmptyColoredCubesVolumeDataAsset()
		{			
			ScriptableWizard.DisplayWizard<CreateEmptyColoredCubesVolumeDataAssetWizard>("Create Colored Cubes Volume Data", "Create");
		}
		
		[MenuItem ("Assets/Create/Colored Cubes Volume Data/From Substrate Data...")]
		static void CreateVDBFromDat()
		{	
			// Resulting path already contains UNIX-style seperators (even on Wondows).
			string pathToVoxelDatabase = EditorUtility.OpenFilePanel("Choose a Voxel Database (.vdb) file to load", Paths.voxelDatabases, "vdb");
			
			if(pathToVoxelDatabase.Length != 0)
			{
				string relativePathToVoxelDatabase = PathUtils.MakeRelativePath(Paths.voxelDatabases + '/', pathToVoxelDatabase);
				
				// Pass through to the other version of the method.
				VolumeDataAsset.CreateFromVoxelDatabase<ColoredCubesVolumeData>(relativePathToVoxelDatabase);
			}
		}
	}
}
