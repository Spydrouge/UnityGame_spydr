using UnityEngine;
using UnityEditor;
using System;
//---------------------------


//---------------------------
using Substrate;
using Cubiquity;
//---------------------------


//Purpose: It is useful to know how to create my own EditorWindow, even if I decided to go with menu options for the code I'm currently working on
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{	
	//By Deriving from EditorWindow, I have created an independent piece of software accessible at edit time that does not have to be attached to a gameobject, like ColoredCubeVolume
	//It will be its own panel, and hopefully more useful as a result. 
	public class SubstrateLoader : EditorWindow
	{

		//Let's store this info!
		protected String _toSubstrateMap = "/folder/";
		protected String _toVDB = "xxx.vdb";

		protected bool _options = false;
		protected int _skyOffset = 85;
		protected int _defaultSkyOffset = 85;

		//This sexy code puts this EditorWindow into Unity's Menus (ya know, at the top of the program)
		//And attached this 'Show Window' function to clicking the menu item
		[MenuItem ("Window/OpenCog/SubstrateLoader")]
		public static void ShowWindow()
		{
			//LOOK AT ME COPY-PASTING UNITY CODE, DOH!
			//(this function is built into Unity, and makes sure if we hit the same menu option twice, we
			// don't bring up two different windows, but rather create the 1st one or re-select the 1st one if
			// it already exists.)
			EditorWindow.GetWindow(typeof(SubstrateLoader));
		}
		
		//Dis is vhere de actual 'look' of the panel goes. OnGUI() is called like an Update() 
		public void OnGUI()
		{
			//change the little folder topper that displays the title of the Editor Window
			title = "Import Substrate";


			//------------------------
			// SUBSTRATE MAP
			//------------------------
			//Grab the map folder file (a folder containing level.dat and region data, usually) we're going to convert from
			GUILayout.Label("Substrate", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				_toSubstrateMap = EditorGUILayout.TextField("Input Map (folder)", _toSubstrateMap);

				if(GUILayout.Button("Browse", GUILayout.Width(100)))
				{
					_toSubstrateMap = EditorUtility.OpenFolderPanel("Choose a Substrate map folder to load", Paths.MAPFolders, "mapFolder");
			
				}
			}GUILayout.EndHorizontal();

			//------------------------
			// VDB
			//------------------------
			//Grab the location for the .vdb file we'll want to save at
			GUILayout.Label("Voxel Database", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				_toVDB = EditorGUILayout.TextField("Save location (.vdb)", _toVDB);

				if(GUILayout.Button("Browse", GUILayout.Width(100)))
				{
					_toVDB = EditorUtility.SaveFilePanel("Choose a place to save the Voxel Database file (.vdb)", Paths.VDBFiles, "NewVDB.vdb", "vdb");
				}
			}GUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			//------------------------
			// ADDITIONAL OPTIONS!
			//------------------------

			GUILayout.Label("Additional Parameters", EditorStyles.boldLabel);
			_options = EditorGUILayout.Foldout(_options, "Show", EditorStyles.foldout);

			EditorGUI.indentLevel = 1;

			if(_options)
			{
				GUILayout.BeginHorizontal();
				{
					_skyOffset = EditorGUILayout.IntField("Vertical Offset:", _skyOffset, EditorStyles.numberField);
					if(GUILayout.Button("Default", GUILayout.Width(100))) _skyOffset = _defaultSkyOffset;

				}GUILayout.EndHorizontal();

			}
			EditorGUILayout.Separator();
			EditorGUI.indentLevel = 0;

			//------------------------
			// CONVERT!
			//------------------------

			//This is the final conversion button
			GUILayout.Label("Load Substrate as Voxel Database", EditorStyles.boldLabel);
			GUILayout.Label("When satisfied with the above settings, click to Convert the Substrate Map folder to Cubiquity Voxel Database (.vdb) format. You will be prompted to save an additional Cubiquity Volume Data asset on a successful conversion.", EditorStyles.wordWrappedMiniLabel);
			if(GUILayout.Button("Convert", GUILayout.Width (100)))
			{
				ConvertFunction();
			}
		}

		public void ConvertFunction()
		{
			Debug.Log ("I can successfully access substrate and Cubiquity types.");

			int width = 256;
			int height = 32;
			int depth = 256;
			
			ColoredCubesVolumeData ccvd = VolumeDataAsset.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, width-1, height-1, depth-1));

			AnvilWorld datWorld = AnvilWorld.Create(_toSubstrateMap);

			if(datWorld == null)
			{
				Debug.Log ("Fail to find world");
			}
			else
			{
				Debug.Log ("Found world");
			}

			Debug.Log ("Yay.");
		}
		
	
	}
}
