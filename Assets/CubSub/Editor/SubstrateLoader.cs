using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
//---------------------------


//---------------------------
using Substrate;
using Cubiquity;
//---------------------------


//Purpose: It is useful to know how to create my own EditorWindow, even if I decided to go with menu options for the code I'm currently working on
//Author: Melissa Kronenberger (Spydrouge)
using Cubiquity.Impl;



namespace CubSub
{	
	//By Deriving from EditorWindow, I have created an independent piece of software accessible at edit time that does not have to be attached to a gameobject, like ColoredCubeVolume
	//It will be its own panel, and hopefully more useful as a result. 
	public class SubstrateLoader : EditorWindow
	{

		//Let's store this info!
		protected String _toSubstrateMap = "/folder/";
		protected String _toVDB = "xxx.vdb";
		protected String _toAsset = "xxx.asset";

		//any additional optioins we want to add
		protected bool _options = false;
		protected int _skyOffset = 80;
		protected int _defaultSkyOffset = 80;

		//hard code in some stuff we know about substrate, to keep track of it. 
		protected int subChunkSize = 16;
		protected int subMapDepth = 256;

		//dictonary mapping substrate values to quantized colors for Cubiquity
		protected Dictionary<int, QuantizedColor> subToCubDict = new Dictionary<int, QuantizedColor>()
		{ 	{0, HexColor.QuantHex("00000000")},  //This should be an empty block. I watched as it was used in the 'delete cube' function of Cubiquity (Quantized color(0,0,0,0))
			{1, HexColor.QuantHex("636573FF")},  	//Stone
			{2, HexColor.QuantHex("19E619FF")},		//Grass
			{3, HexColor.QuantHex("8F4700FF")},  	//Dirt
			{4, HexColor.QuantHex("8A8C96FF")},		//Cobble
			{5, HexColor.QuantHex("CCAA00FF")},		//Wood
			{6, HexColor.QuantHex("CBFF96FF")},		//Sapling
			{7, HexColor.QuantHex("000A24FF")},		//Bedrock
			{8, HexColor.QuantHex("4064C7FF")},		//Flowing Water
			{9, HexColor.QuantHex("1B24A6FF")},		//Water
			{10, HexColor.QuantHex("ED590EFF")},	//Flowing Lava
			{11, HexColor.QuantHex("D94800FF")},	//Lava
			{12, HexColor.QuantHex("F7F2C8FF")},	//Sand
			{16, HexColor.QuantHex("1A1616FF")},	//Coal 
			{17, HexColor.QuantHex("FFCC00FF")},	//Log
			{18, HexColor.QuantHex("39A377FF")},	//Leaves
			{26, HexColor.QuantHex("FFB8E0FF")},	//Bed
			{35, HexColor.QuantHex("E7E8D8FF")},	//Wool
			{43, HexColor.QuantHex("CCCCCCFF")},	//Stone slab
			{45, HexColor.QuantHex("753437FF")},	//Brick
			{46, HexColor.QuantHex("FF1C27FF")},	//TNT
			{62, HexColor.QuantHex("2E2D2DFF")},	//Furnace
			{81, HexColor.QuantHex("67ED00FF")},	//Cactus
			{90, HexColor.QuantHex("C795F0FF")},	//Portal
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
			GUILayout.BeginHorizontal(); //Watch me use {}s with absolutely no code-related reason and no functionality whatsoever!
			{
				_toSubstrateMap = EditorGUILayout.TextField("Input Map (folder)", _toSubstrateMap);

				if(GUILayout.Button("Browse", GUILayout.Width(100)))
				{
					_toSubstrateMap = EditorUtility.OpenFolderPanel("Choose a Substrate map folder to load", Paths.MAPFolders, "mapFolder");
			
				}
			}
			GUILayout.EndHorizontal();

			//------------------------
			// VDB/ CUBIQUITY MAP
			//------------------------
			//Grab the location for the .vdb file we'll want to save at
			GUILayout.Label("Voxel Database", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				_toVDB = EditorGUILayout.TextField("Save location and name", _toVDB);

				if(GUILayout.Button("Browse", GUILayout.Width(100)))
				{
					_toVDB = EditorUtility.SaveFilePanel("Choose a place to save the Voxel Database file", Paths.VDBFiles, "NewVDB", "");
				}
			}
			GUILayout.EndHorizontal();

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

				}
				GUILayout.EndHorizontal();

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

		//Simple function for checking out our subtoCub dictionary for block type swapping. 
		//contains a little logic for handling the situation if a match isn't found.
		public QuantizedColor ConvertBlock(int subBlockType)
		{
			//check and see if there's a match
			if(subToCubDict.ContainsKey(subBlockType))
			{
				return subToCubDict[subBlockType];
			}

			//we're going to try and create a substitution if there is no match
			else
			{
				//To all future coders! This is a silly way of trying to account for unexpected blockIDs without data loss.
				//recall that cubiquity is going to save all blocks as QuantizedColors, and not with BlockIDs.
				//Therefore, every imported block *must* have a different QuantizedColor associated with it, even if it differs by just 1 unit.

				//Now bright yellow is an uncommon color, but  its always possible the subBlockType will be 255 or otherwise end up creating a color identical to
				//a preexisting block. But then we have one component which tends to not be anywhere near as variable or random as rgb, and that's a. Alpha.
				//Odds are you're never going to end up with 99 alpha haphazardly. Most blocks are gonna be 256. A rare few will be like 128 or 64. But 99? Unlikely.
				//therefore preserving data of unexpected blocks with an alpha of 99 seemed clever ;)

				//eh, fix/replaced/tune it later if that's what one wishes. 
				subToCubDict.Add (subBlockType, new QuantizedColor((byte)255, (byte)255, (byte)subBlockType, (byte)255));

				Debug.Log("Unable to find block " + subBlockType.ToString () + ". Attempting to create dummy block in yellow.");

				return subToCubDict[subBlockType];
			}
		
		}

		//This is the function that will be called when the big 'convert' button is pressed on the panel. 
		public void ConvertMap()
		{

			//Load in the Substrate Map folder! (The three versions of map are 'Alpha, Beta, and Anvil.' Our 
			//maps are served in Anvil format. We don't have to errorcheck; substrate will pop an error if we failed.
			AnvilWorld leWorld = AnvilWorld.Create(_toSubstrateMap);

			//The region actually contains data about the chunks.
			AnvilRegionManager leRegions = leWorld.GetRegionManager();

			//I have no idea what a more clever way of getting the number of regions in leRegions might be, so let's do this for now
			int regionCount = 0;
			foreach(AnvilRegion region in leRegions)
			{
				if(region != null) //I hate that warning in the editor that region is declared but never used...
					regionCount++;
			}

			//we'll pop em in an array for now so that it's easier to save them
			ColoredCubesVolumeData[] newAssets = new ColoredCubesVolumeData[regionCount];

			Debug.Log ("newAssets.Length = " + newAssets.Length.ToString());

			//this exists ENTIRELY so that I can concatenate strings together and perform sexy operations on them :|
			String pathHelp = "";
	
			// ----CONVERSION OF REGIONS-----!//
			// I have added a wiki page on the evils of conversion. Right now, I am creating a VoxelData object per region
			//btw, we can use foreach on leRegions because leRegion is 'Enumerable'. Check out its inheritance. 
			regionCount = 0;
			foreach(AnvilRegion region in leRegions)
			{
				//Chunk Size * chunks in region.... map is automatically 256 deep
				int xSize = region.XDim * this.subChunkSize;
				int zSize = region.ZDim * this.subChunkSize;
				int ySize = this.subMapDepth;


				//it appears that while minecraft prefers xzy, cubiquity prefers xyz (height compes second)
				//meh, I nabbed this from a little deeper in Cubiquity than its top-level menu selectors, cause I wanted to be able to specify my own vdb name
				//it creates both the asset (which we are going to have to save) and the .vdb

				//anyway, make sure we create the new data with the proper file name!!!
				ColoredCubesVolumeData data = null;

				//use that nice helper variable with this nice helper function to ensure our path is prepped for either single or multiple saves...
				//all without cluttering our code with loads of if/thens
				pathHelp = Paths.HelpMakePath(_toVDB, newAssets.Length, regionCount, ".vdb");

				//temporary test condition
				if(File.Exists (pathHelp))
				{

					File.Delete (pathHelp);
					Debug.Log ("Attempted to save a VDB that already exists. Attempting to fail gracefully by aborting conversion attempt.");
					return;

				}

				//CREATE LE NEW DATA with the path we just got :)
				data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, xSize-1, ySize-1, zSize-1), pathHelp);

				//Mayday!
				if(data == null)
				{

					Debug.Log("Unable to initialize ColoredCubesVolumeData. Attempting to fail gracefully by abandoning conversion attempt.");
					return;
				}

				//declare the chunk-reference-thingy we'll be using to access Substrate blocks!
				ChunkRef chunk = null;
				AlphaBlockCollection blocks = null; //I get the impression Substrate once thought it needed Alpha, Beta, and Anvil Block Collections... and then ended up only needing 1 kind...


				//----- The array CONVERSION! -----//
				//And here is where we will actually go through the loop's 3 dimensions and attempt to change the blocks
				//Scroll down to the super-interior 'k' loop to find the block replacement code

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
							//NAB THE ID! Using the Substrate block collections 'get id'
							int blockId = blocks.GetID (iBlock, k, jBlock);

							///Translate the ID using our personal 'ConvertBlock' and throw that quantizedColor variable into Cubiquity's voxeldata collection!
							data.SetVoxel(iBlock, k, jBlock, ConvertBlock (blockId));
						}//K/Y loop
						
					}//J/Z loop
					
					//ITERATION CODE FOR I/X
					iBlock++;
					if(iBlock >= this.subChunkSize)
					{
						iBlock = 0;
						iChunk ++;
					}
				} //I/X loop

				//and now pop em into the array and iterate
				newAssets[regionCount] = data;
				regionCount++;
				

			}//for each region

			//Now, data should be filled with all of the cubes we extracted from the chunks. We need to save the .asset files! We want to add on
			//the region number if we loaded more than one region, and leave the name the way it is if we didn't.
			//we just have to make the new asset(s) permenant

			//so grab where we want to save the asset(s)
			this._toAsset = EditorUtility.SaveFilePanel("Choose a place to save the Voxel Database", Paths.VoxelDatabases, "NewVoxelDatabase", "");

			if (_toAsset.StartsWith(Application.dataPath)) {
				_toAsset = "Assets" + _toAsset.Substring(Application.dataPath.Length);
			}
			else {
				throw new System.ArgumentException("SubstrateLoader was asked to save a VoxelDatabase outside the Asset Folder", "absolutePath");
			}

			//if we have more than one asset, we want to append an enumerator after the name (_1, _2, _3, etc)
			regionCount = 0;
			for(regionCount = 0; regionCount < newAssets.Length; regionCount++)
			{
				//use the pathHelper to streamline things and unclutter our code again!
				pathHelp = Paths.HelpMakePath(_toAsset, newAssets.Length, regionCount, ".asset");

				//Creat ethe asset
				AssetDatabase.CreateAsset(newAssets[regionCount], pathHelp);

				//Iteration code
				regionCount++;
			}

			//Sellect the VoxelDatabase.asset we just created so we can drag/drop it to where we wish
			//AssetDatabase.SaveAssets ();
			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = newAssets[0];

			Debug.Log ("SubstrateLoader: Regions Loaded.");
		}
		
	
	}
}
