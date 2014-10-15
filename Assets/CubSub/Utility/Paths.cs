using UnityEngine;

using System;
using System.Collections;
using System.IO;

namespace CubSub
{
	//Paths to File-Places of Awesome! Rawr, the Grammars!
	public class Paths
	{
		//Location of CubSub StreamingAssets
		public static String CubSub
		{
			get { return Application.streamingAssetsPath + "/CubSub"; }
		}

		public static String VoxelDatabases
		{
			get { return CubSub + "/VoxelDatabases"; }
		}

		public static String VDBFiles
		{
			get { return VoxelDatabases + "/vdb"; }
		}

		public static String MAPFolders
		{
			get{return CubSub + "/SubstrateData";}
		}

		public static void PingDirectories()
		{
			Debug.Log ("Checking the existance of Paths,\n Click to see they all register as 'True'" +
			"\nCubSub: " + CubSub + " : " + Directory.Exists (CubSub) +
			"\nVoxelDatabases: " + VoxelDatabases + " : " + Directory.Exists (VoxelDatabases) +
			"\nVDBFiles: " + VDBFiles + " : " + Directory.Exists (VDBFiles) +
			"\nMAPFolders: " + MAPFolders + " : " + Directory.Exists (MAPFolders));
		}

		//Okay here are our steps to homogenizing path input AND dealing with a situation where one file name
		//was given but we actually need to save multiple files:

		//1. if there already is an extension, remove it.
		//2. determine if we have to concatenate any numerals and an _
		//3. re-add the extension (or add it to begin with if there never was one)
		//4. return the result

		public static String HelpMakePath(String path, int numToSave, int numOn, string extension)
		{
			//first of all, we have to be wary of unexpected dots showing up earlier. It's unlikey, yeah, but 
			//don't underestimate the power of stupid! begin by finding where the filename is, and ignoring the
			//folder path... and index decutter at that point
			int decutter = path.LastIndexOf("/");

			//create a string to automate this extension-cutting process with a loop (we want to cut off not only our
			//own extension, but any other common culprits)
			String[] toCuts = {extension, ".asset", ".dat", ".vdb"};

			//loop through the extensions to chop off!
			foreach(String toCut in toCuts)
			{
				//now use the index we saved in decutter, and from there afterwards look for any instance of the extension
				int cutter = path.IndexOf(toCut, decutter);

				//if we found that extension, clip it off
				if(cutter != -1)
					path = path.Substring(0, cutter);
			}
		
			//if we only have one item to save, or sent in something strange like zero, just return the path plus extension.
			if(numToSave <= 1) return path + extension;

			//otherwise, throw in an understore and a toString
			return path + "_" + numOn.ToString () + extension;

		}

	}
}
