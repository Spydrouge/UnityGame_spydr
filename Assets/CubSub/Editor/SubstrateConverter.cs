
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
	/// <summary>
	/// Creates an editor window with which we can convert Substrate Map directories (which contain a region folder and an unknown number of regions) into Cubiquity Colored Cubes Volumes.
	/// </summary>
	public class SubstrateConverter : EditorWindow
	{

		/// <summary>
		///  The location of a directory which represents a Substrate Map. This variable reads from the Editor window and can change at any time.
		/// </summary>
		protected String _toSubstrateMap = "";

		/// <summary>
		/// The name under which to save new .vdb and .asset files for Cubiquity. This variable reads from the Editor window and can change at any time.
		/// </summary>
		protected string _saveName = "New Voxel Database";
		protected String toVDB = Paths.VDBFiles +"/New Voxel Database";
		protected String toAsset = Paths.VoxelDatabases +"/New Voxel Database";

		/// <summary>
		/// Opens a drawer of additional parameters for our import/export use.
		/// </summary>
		protected bool _options = false;

		/// <summary>
		/// Will remove this number of blocks from the bottom of a chunk. This variable reads from the Editor window and can change at any time.
		/// </summary>
		protected int _clipFromBelow = 60;
		protected const int defaultClipFromBelow = 60;

		/// <summary>
		/// Will remove this number of blocks from the top of a chunk. This variable reads from the Editor window and can change at any time.
		/// </summary>
		protected int _clipFromAbove = 150;
		protected const int defaultClipFromAbove = 150;

		//hard code in some stuff we know about substrate, to keep track of it. 
		protected int subChunkSize = 16;
		protected int subMapDepth = 256;

		/// <summary>
		/// Mapping substrate values to quantized colors for Cubiquity, currently using the neat HexColor.QuantHex() function;
		/// </summary>
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

	
		/// <summary>
		/// [MenuItem ("Window/OpenCog/Substrate Converter")] puts this EditorWindow into Unity's Menus under Window/OpenCog/Substrate Converter.'Show Window' is called when the menu item is clicked.
		/// </summary>
		[MenuItem ("Window/OpenCog/Substrate Converter")]
		public static void ShowWindow()
		{
			//LOOK AT ME COPY-PASTING UNITY CODE, DOH!
			//(this function is built into Unity, and makes sure if we hit the same menu option twice, we
			// don't bring up two different windows, but rather create the 1st one or re-select the 1st one if
			// it already exists.)
			EditorWindow.GetWindow(typeof(SubstrateConverter));

			//debugging
			Paths.PingDirectories();
		}
		

		/// <summary>
		/// Dis iz vhere de actual 'look' of the panel goes. OnGUI() is called like an Update() of sorts.
		/// </summary>
		public void OnGUI()
		{
			//change the little folder topper that displays the title of the Editor Window
			title = "Substrate Converter";

			//useful helpers later on, will temporary hold norms while I shrink/expand things
			float labelWidth;
			float fieldWidth;


			//------------------------
			// SUBSTRATE MAP
			//------------------------
			//Grab the map folder file (a folder containing level.dat and region data, usually) we're going to convert from
			GUILayout.Label("Substrate", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal(); //Watch me use {}s with absolutely no code-related reason and no functionality whatsoever!
			{
				_toSubstrateMap = EditorGUILayout.TextField("Input Map (directory)", _toSubstrateMap);

				if(GUILayout.Button("Browse", GUILayout.Width(100)))
				{
					_toSubstrateMap = EditorUtility.OpenFolderPanel("Choose a Substrate map directory to load", Paths.MAPFolders, "mapFolder");
			
					//we can probably do some errorchecking here to help users navigate to the correct directory
				}
			}
			GUILayout.EndHorizontal();

			//------------------------
			// VDB/ CUBIQUITY MAP / and asset
			//------------------------
			//Grab the location for the .vdb file we'll want to save at
			GUILayout.Label("Voxel Database", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			{
				_saveName = EditorGUILayout.TextField("Name to Save As", _saveName);
				toVDB = Paths.VDBFiles + "/" +_saveName;
				toAsset = Paths.VoxelDatabases + "/" + _saveName;

				//if(GUILayout.Button("Browse", GUILayout.Width(100)))
				//{
				//	_toVDB = EditorUtility.SaveFilePanel("Choose a place to save the Voxel Database file", Paths.VDBFiles, "NewVDB", "");
				//}
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			//------------------------
			// ADDITIONAL OPTIONS!
			//------------------------

			GUILayout.Label("Additional Parameters", EditorStyles.boldLabel);
			_options = EditorGUILayout.Foldout(_options, "Show", EditorStyles.foldout);

			//I want to make a note here and mention that we are basically sending a bunch of render commands with each
			//EditorGUILayout.IntField. We're not just setting values. Therefore, setting indent here to 1, and later to 0,
			//or what have you really does have an immediate effect on what comes after it. 
			EditorGUI.indentLevel = 1;

			if(_options)
			{
				//It is possible to clip voxels off the top and bottom (and possibly in the future other dimensions)
				//We are going to list those controls now
				EditorGUILayout.LabelField("Voxels to clip", EditorStyles.boldLabel);
				{ //welcome to the land of organizational brackets that do nothing.


					//We want these properties to have smaller fields than others :3
					labelWidth = EditorGUIUtility.labelWidth;
					fieldWidth = EditorGUIUtility.fieldWidth;
					
					EditorGUIUtility.labelWidth = 75f;
					EditorGUIUtility.fieldWidth = 25f;

					//Top and Bottom
					GUILayout.BeginHorizontal();
					{
						//The actual GUI Fields
						_clipFromBelow = EditorGUILayout.IntField("Bottom", _clipFromBelow);
						_clipFromAbove = EditorGUILayout.IntField("Top", _clipFromAbove);

						//create an option to quickly reset to default (recommended, and OpenCog/Substrate specific) dimensions. 
						if(GUILayout.Button("Defaults", GUILayout.Width(100))) 
						{
							_clipFromBelow = defaultClipFromBelow;
							_clipFromAbove = defaultClipFromAbove;
						}
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						//Watch as this empty label imposes its indentation on the following button,
						//which otherwise would completely ignore my indentation attempts!
						//MUAHAHAHAHAHHAHAHAH!!!!
						EditorGUILayout.LabelField("", EditorStyles.label, GUILayout.Width(10));

						//create an option to quickly zero out all the clip dimensions
						if(GUILayout.Button("Zero Out (No Voxels Clipped)"))
						{
							_clipFromBelow = 0;
							_clipFromAbove = 0;
						}
					}
					GUILayout.EndHorizontal();

					//return these attributes to normal
					EditorGUIUtility.labelWidth = labelWidth;
					EditorGUIUtility.fieldWidth = fieldWidth;
				}
				
			}
			EditorGUILayout.Separator();
			EditorGUI.indentLevel = 0;

			//------------------------
			// CONVERT!
			//------------------------

			//This is the final conversion button
			GUILayout.Label("Load Substrate as Voxel Database", EditorStyles.boldLabel);
			GUILayout.Label("When satisfied with the above settings, click to Convert the Substrate Map folder to Cubiquity Voxel Database. The .asset file(s) this creates will be saved at: " + Paths.VoxelDatabases, EditorStyles.wordWrappedMiniLabel);
			if(GUILayout.Button("Convert", GUILayout.Width (100)))
			{
				ConvertMap();
			}
		}

		/// <summary>
		/// Simple function for checking out our dictionary for Substrate-to-Cubiquity Block ID conversions. Will attempt to add a new dictionary key if a block of unknown type is found.
		/// </summary>
		/// <returns>
		/// A Cubiquity Block ID (While using colored cubes, the 'Block ID' is actually just the Quantized Color we'll be rendering).
		/// </returns>
		/// <param name="subBlockType">The Substrate Block ID we started with.</param>
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
				subToCubDict.Add (subBlockType, new QuantizedColor((byte)255, (byte)255, (byte)subBlockType, (byte)99));

				Debug.Log("Unable to find block " + subBlockType.ToString () + ". Attempting to create dummy block in yellow.");

				return subToCubDict[subBlockType];
			}
	
		
		}



		//notice this function will only return ONE representative. This code is not singularly robust for situations in which two 
		//ColoredCubesVolumeDatas are attempting to access the same vdb in read-only mode. 
		/// <summary>
		/// This interesting little function should, in the event that we try to overwrite a vdb, allow us to find out if there
		/// is a ColoredCubesVolumeData .asset which might have the vdb currently checked out.
		/// We can then attempt to close/destroy the data in order to release the vdb for deletion. 
		/// As an IEnumerable which uses yield, it can and should be used with a foreach loop in ConvertMap.
		/// </summary>
		/// <returns> An .asset which is suspected to have the .vdb open.</returns>
		/// <param name="vdbPath">An absolute path to the .vdb file.</param>
		public IEnumerable<ColoredCubesVolumeData> FindVolumeMatchingPath(String vdbPath)
		{
			//This Resources function allows us to nab all .assets, .prefabs, .etc not just stuff that's currently on the loaded scene.
			//We want to get an array of all ColoredCubesVolumeData objects there are; any of them might have our vdb open

			//Debug.Log ("Beginning search for ColoredCubesVolumeData which matches vdbPath.");
			ColoredCubesVolumeData[] datas = Resources.FindObjectsOfTypeAll(typeof(ColoredCubesVolumeData)) as ColoredCubesVolumeData[];
			foreach(ColoredCubesVolumeData data in datas)
			{

				//We have the path to the vdb we want to destroy. The datas each keep paths to their vdbs. Everything is in absolute path
				//form. Therefore we can use a straightforward string compare to see if any data has our vdb open

				//Debug.Log ("Comparing the strings from Data: "  + data.fullPathToVoxelDatabase + " && VDBPath: " + vdbPath);
				if(String.Compare (data.fullPathToVoxelDatabase, vdbPath) == 0)
				{
					//Debug.Log ("Match found");
					yield return data;
				}
			}
			//Debug.Log ("Match not found");
			yield break;
		}

		public IEnumerable<ColoredCubesVolume> FindObjectMatchingData(ColoredCubesVolumeData data)
		{
			//I am not yet convinced I should be using Resources.FindObjectsOfTypeAll; I seem to remember there used
			//to be an in-scene-only variant. But for now we'll try this.
			ColoredCubesVolume[] vols = Resources.FindObjectsOfTypeAll(typeof(ColoredCubesVolume)) as ColoredCubesVolume[];
			foreach (ColoredCubesVolume vol in vols)
			{
				//hey, do these have the same data? XD
				if(vol.data == data)
					yield return vol;
				
			}
			yield break;
			
		}

		//------------------------------------------------------------
		//          GIGANTIC MAP CONVERSION FUNCTION!
		//------------------------------------------------------------
		///<summary>
		///		<para>This is the function that will be called when the big 'convert' button is pressed on the panel. 
		///     	Conversion is BIG. I have divided it into the following stages:</para>
        /// 		<para></para>
		/// 		<para>1) Open the Substrate directory, read it, and initialize cubiquity stuff </para>
		/// 
		/// 		<para>2) Handle file names. Handle closing existing files, deleting them, replacing them, relinking them, etc. Make sure everything lines up smoothly,  and
		/// 			minimize crashing as a result of overwriting things.</para>
		/// 		  
		/// 		<para>3) Translate the 3D Array of Voxels from Substrate to Cubiquity, minding things like Regions and dimensions.</para>
		/// 
		/// 		<para>4) Save all the materials we created and wish to keep. Again, ensure no naming conflict problems. </para>
		/// 
		/// 		<para>5) Refresh everything dependent on the newly saved materials</para>
		///			<para></para>
		/// 		<para> Although this function takes no parameters, it is reliant heavily on class members; such as toAsset, toVdb, _options, _saveName,  _toSubstrateMap, and in general
		/// 			pretty much every other part of SubstrateConverter. It also relies on Application., AssetDatabase., and File. calls.</para>
		/// </summary>
		public void ConvertMap()
		{
			//------------------------------------------------------------
			//------- STAGE 1: GET THE MAPS OPEN AND CREATE SOME HANDLERS!
			//------------------------------------------------------------

			//Load in the Substrate Map folder! (The three versions of map are 'Alpha, Beta, and Anvil.' Our 
			//maps are served in Anvil format. We don't have to errorcheck; substrate will pop an error if we failed.
			AnvilWorld leWorld = AnvilWorld.Create(_toSubstrateMap);

			//The region actually contains data about the chunks.
			AnvilRegionManager leRegions = leWorld.GetRegionManager();

			//I have no idea what a more clever way of getting the number of regions in leRegions might be, so let's do this for now
			int regionTotal = 0;
			foreach(AnvilRegion region in leRegions)
			{
				if(region != null) //I hate that warning in the editor that region is declared but never used...
					regionTotal++;
			}

			//debugging to make sure loops work and understand if any assets went mysteriously missing.
			Debug.Log ("Attempting to load " + regionTotal.ToString() + " regions");

			//this exists ENTIRELY as a set of helpers so that I can concatenate strings together and perform sexy operations on them :|
			String pathVDB = "";
			String pathAsset = "";

			//this has helped tremendously during the debug phase
			int failDetector = 0;

			//this needs to be rooted at the Assets directory, or there shall be chaos! CHAOS! 
			//(If improperly done it will result in 'unable to create asset' and DEATH!
			//Debug.Log ("Rooting _toAsset: " + _toAsset);
			String toAssetHelp = Paths.RootToDirectory(Application.dataPath, toAsset);
	
			//------------------------------------------------------------
			//------- PROCEED REGION BY REGION!
			//------------------------------------------------------------
			// I have added a wiki page on the evils of conversion. Right now, I am creating a VoxelData object per region
			//btw, we can use foreach on leRegions because leRegion is 'Enumerable'. Check out its inheritance. 
			int regionCount = 0;
			foreach(AnvilRegion region in leRegions)
			{
				//well, since I put it in the above foreach, I suddenly feel obligated to do it here, also... Don't judge me!
				if(region == null) continue;

				//Chunk Size * chunks in region.... map is automatically 256 deep
				int xSize = region.XDim * this.subChunkSize;
				int zSize = region.ZDim * this.subChunkSize;
				int ySize = this.subMapDepth - _clipFromAbove;


				//it appears that while minecraft prefers xzy, cubiquity prefers xyz (height compes second)
				//meh, I nabbed this from a little deeper in Cubiquity than its top-level menu selectors, cause I wanted to be able to specify my own vdb name
				//it creates both the asset (which we are going to have to save) and the .vdb

				//anyway, make sure we create the new data with the proper file name!!!
				ColoredCubesVolumeData data = null;

				//------------------------------------------------------------
				//------- STAGE 2: HANDLE NAMING CONFLICTS/REIMPORTING/ETC
				//------------------------------------------------------------

				//This handy-dandy notebook is going to record for us information we need to regenerate copies of the VolumeData .asset that used to link to the
				//vdb previous to import AND it holds onto the original data structures so we can ask ColoredCubesVolume if their data == data we replaced. 
				Dictionary<String, ColoredCubesVolumeData> relinker = new Dictionary<String, ColoredCubesVolumeData>();

				//use that nice helper variable with this nice helper function to ensure our path is prepped for either single or multiple saves...
				//all without cluttering our code with loads of if/thens
				pathVDB = Paths.HelpMakePath(toVDB, regionTotal, regionCount, ".vdb");
				pathAsset = Paths.HelpMakePath(toAssetHelp, regionTotal, regionCount, ".asset");

				//Debug.Log ("Created pathAsset: " + pathAsset);

				//Alrighty then. What we want to do is check and see if this VDB already exists.
				//if it exists, we want to try and delete it.
				//to delete it, we have to figure out if it's somehow locked and then attempt to unlock it.
				//then we have to try and delete it and we might get some errors (for which we should prep a try/catch)
				if(File.Exists (pathVDB))
				{

					Debug.Log ("A .vdb by this name already exists. Searching through .asset files to see if it is currently opened.");

					//Alright, so we're going to do some hacking and see if we can figure out how to delete the vdbs live.
					//this is gonna look for the .asset file that the vdb is attached to...

					//if we managed to find the .asset that links to this vdb, we must BURN IT MUAHAHAHAHHAHAA
					//I've changed if(oldData) to while(oldData) to account for the possibility that multiple Data .assets
					//might have the vdb open in read only mode 

					failDetector = 0;
					foreach(ColoredCubesVolumeData oldData in FindVolumeMatchingPath(pathVDB))
					{

						Debug.Log ("Successfully found an .asset reading from the .vdb. Attempting to shut it down to release the .vdb.");

						//I'm going out on a limb here to see if this works... If it doesn't, we can fudge around a little
						//more or just try to fail gracefully.
						oldData.ShutdownCubiquityVolume();

						//referencing this function (GetAssetPath) takes a million bajillion years.
						String oldDataPath = AssetDatabase.GetAssetPath (oldData);

						//write down in our handy-dandy notebook that this oldData once existed at some location
						if(!relinker.ContainsKey (oldDataPath))
						{
							relinker.Add (oldDataPath, oldData);
						}

						//now let's try and delete the asset itself so we get no linking errors...
						AssetDatabase.DeleteAsset(oldDataPath);

						failDetector++;
						if(failDetector >= 1000) break;
					}

					//this should no longer ever fire because olddata does a comparison now; but I'll leave it in place. 
					if(failDetector >= 1000)
					{
						throw new System.ArgumentException("I need to write better while loops", failDetector.ToString());
				
					}

					Debug.Log ("Attempting to delete the old .vdb under the assumption it is no longer open. If the deletion fails, the conversion will not proceed. The .vdb can be deleted manually with the Unity Editor closed, or you can specify a different save name for the .vdb.");

					//When this error is thrown, the entire conversion attempt stops; and we don't corrupt our existing data.
					File.Delete (pathVDB);
				
				} //checking for if VDB exists and deleting it/its .assets

				Debug.Log ("Creating new VDB");

				//CREATE LE NEW DATA with the path we just got :)
				data = VolumeData.CreateEmptyVolumeData<ColoredCubesVolumeData>(new Region(0, 0, 0, xSize-1, ySize-1-_clipFromBelow, zSize-1), pathVDB);

				//Mayday!
				if(data == null)
				{

					Debug.Log("Unable to initialize ColoredCubesVolumeData. Attempting to fail gracefully by abandoning conversion attempt.");
					return;
				}


				//------------------------------------------------------------
				//------- STAGE 3: TRANSFER THE 3D ARRAYS, BLOCK BY BLOCK
				//------------------------------------------------------------

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
						for(int k = _clipFromBelow; k < ySize; k++)
						{
							//NAB THE ID! Using the Substrate block collections 'get id'
							int blockId = blocks.GetID (iBlock, k, jBlock);

							///Translate the ID using our personal 'ConvertBlock' and throw that quantizedColor variable into Cubiquity's voxeldata collection!
							data.SetVoxel(iBlock, k-_clipFromBelow, jBlock, ConvertBlock (blockId));
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


				//------------------------------------------------------------
				//------- STAGE 4: SAVE EVERYTHING WHERE IT NEEDS TO GO, AGAIN PREVENT NAMING CONFLICTS
				//------------------------------------------------------------

				//Now, data should be filled with all of the cubes we extracted from the chunks. We need to save the .asset files! We want to add on
				//the region number if we loaded more than one region, and leave the name the way it is if we didn't.
				//we just have to make the new asset(s) permenant

				//there is a possibility a nincompoop with good intentions somehow ended up with an .asset file that
				//has the name we're replacing, but a .vdb that's named something else entirely.
				//in this case we don't want to destroy that potentially valuable .vdb- but we've got to get rid of the 
				//asset. And since the .vdb might be locked, we have to shut it down to prevent strange deletion difficulties.

				if(File.Exists (pathAsset))
				{
					//check out if this is a ColoredCubesVolumeData or not
					ColoredCubesVolumeData oldData = AssetDatabase.LoadAssetAtPath(pathAsset, typeof(ColoredCubesVolumeData)) as ColoredCubesVolumeData;
					
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
						Debug.Log("An .asset of a different type (non ColoredCubesVolumeData) has been found at the save location. Attempting to overwrite it.");
					}
					
					
					//now let's try and delete the asset itself so we get no linking errors...
					AssetDatabase.DeleteAsset(pathAsset);
				}


				//Debug.Log ("The hell is pathAsset? " + pathAsset);

				//Create the asset
				AssetDatabase.CreateAsset(data, pathAsset);
				AssetDatabase.SaveAssets();

				//Do some selection/saving/cleanup
				EditorUtility.FocusProjectWindow ();
				Selection.activeObject = data;

				//------------------------------------------------------------
				//------- STAGE 5: REFRESH DEPENDENT COMPONENTS
				//------------------------------------------------------------

				//This nifty little loop is going to handle refreshing our ColoredCubesVolumes!
				//right off the bat I'm not going to have it create .asset files; especially cause I haven't shared
				
				//so, let's iterate through all of the oldDatas we destroyed
				foreach(KeyValuePair<String, ColoredCubesVolumeData> toDo in relinker)
				{

					foreach(ColoredCubesVolume toLink in FindObjectMatchingData(toDo.Value))
					{
						//update it
						toLink.data = data;

					}
				}

				AssetDatabase.SaveAssets();
			
				//iterate :3
				regionCount++;

			}//for each region

			AssetDatabase.Refresh ();

			Debug.Log ("Conversion attempt was successful");
		}
		
	
	}
}
 