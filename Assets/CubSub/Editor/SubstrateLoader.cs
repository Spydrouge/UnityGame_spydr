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
		protected int _skyOffset = 80;
		protected int _defaultSkyOffset = 80;

		protected int subChunkSize = 16;
		protected int subMapDepth = 256;

		protected Dictionary<int, int> blockTranslator = new Dictionary<int, QuantizedColor>()
		{ {0, new QuantizedColor(0,0,0,0)}
			, {1, 4}
			, {2, 1}
			, {3, 0}
			, {4, 5}
			, {5, 12}
			, {6, 15}
			, {7, 6}
			, {8, 8}
			, {9, 8}
			, {10, 25}
			, {11, 25}
			, {12, 3}
			, {16, 7}
			, {17, 11}
			, {18, 13}
			, {26, 26}		
			, {35, 31}
			, {45, 23}
			, {46, 22}
			, {62, 24}
			, {90, 29}
		};

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
				ConvertMap();
			}
		}

		public QuantizedColor ConvertBlock(int subBlockType)
		{

		}

		public void ConvertMap()
		{

			//Load in the Substrate Map folder! (The three versions of map are 'Alpha, Beta, and Anvil.' Our 
			//maps are served in Anvil format. We don't have to errorcheck; substrate will pop an error if we failed.
			AnvilWorld leWorld = AnvilWorld.Create(_toSubstrateMap);

			//The region actually contains data about the chunks.
			AnvilRegionManager leRegions = leWorld.GetRegionManager();

			// CONVERSION OF REGIONS
			// I have added a wiki page on the evils of conversion. Right now, I am creating a VoxelData object per region
			foreach(AnvilRegion region in leRegions)
			{
				//Chunk Size * chunks in region.... map is automatically 256 deep
				int xSize = region.XDim * this.subChunkSize;
				int zSize = region.ZDim * this.subChunkSize;
				int ySize = this.subMapDepth;

				//it appears that while minecraft prefers xzy, cubiquity prefers xyz (height compes second)
				//meh, I nabbed this from a little deeper in Cubiquity than its top-level menu selectors, cause I wanted to be able to specify my own vdb name
				//it creates both the asset (which we are going to have to save) and the .vdb
				ColoredCubesVolumeData data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, xSize-1, ySize-1, zSize-1), _toVDB);

				if(data == null)
				{
					Debug("Unable to initialize ColoredCubesVolumeData. Attempting to fail gracefully by abandoning conversion attempt.");
					return;
				}

				//declare the chunk reference we'll be using!
				ChunkRef chunk = null;
				AlphaBlockCollection blocks = null;
				
				//iterate through the chunks and blocks on x axis 
				for(int iChunk = 0, iBlock = 0; iChunk < region.XDim;)
				{

					//iterate through the chunks and blocks on z axis (the odd initialization parameters will fire the blocks called 'ITERATION CODE' on the first run.)
					//(and the odd termination parameters should fire when jChunk = the last chunk and jBlock = the last block
					for(int jChunk = -1, jBlock = this.subChunkSize; jChunk < region.ZDim - 1 || jBlock < this.subChunkSize - 1; jBlock++)
					{	
						//ITERATION CODE FOR J/Z
						if(jBlock >= this.subChunkSize)
						{
							jBlock = 0;
							jChunk ++;

							//nab the new chunk
							chunk = region.GetChunkRef(iChunk, jChunk);
						
							//determine if it's valid
							if(chunk == null)
							{
								//hehe, I'm cheating, I'm cheating. this allows the ITERATION CODE block to handle loading the next chunk ;) 
								jBlock = this.subChunkSize;
								continue;
							}
							if(!chunk.IsTerrainPopulated)
							{
								jBlock = this.subChunkSize;
								continue;
							}

							//handily access its blocks
							blocks = chunk.Blocks;
						}
								
						//there is only 1 chunk on the y axis, so go straight through the blocks without worrking about kChunks or kBlocks
						for(int k = 0; k < ySize; k++)
						{
							int blockId = blocks.GetID (iBlock, k, jBlock);
							data.SetVoxel(iBlock, k, jBlock, ConvertBlock (blockId));
						}
						
					}
					
					//ITERATION CODE FOR I/X
					iBlock++;
					if(iBlock >= this.subChunkSize)
					{
						iBlock = 0;
						iChunk ++;
					}


					
				}
			}
				
				
				
			/*AssetDatabase.CreateAsset (instance, assetPathAndName);
			
			AssetDatabase.SaveAssets ();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = instance;	
			*/

			Debug.Log ("Yay.");
		}
		
	
	}
}
