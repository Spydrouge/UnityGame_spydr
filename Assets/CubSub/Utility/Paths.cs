using UnityEngine;

using System;
using System.Collections;
using System.IO;

namespace CubSub
{
	/// <summary>
	/// <para>Paths to File-Places of Awesome! With Sauce! Rawr, the Grammars!</para>
	/// Translation: Editing this file allows an Editor-User to quickly alter where CubSub saves things.
	/// It also contains helper functions for building paths.
	/// </summary>
	public class Paths
	{

		/// <summary>
		/// Concatenates using Application.streamingAssetsPath + /CubSub where the CubSub Streaming Assets directory will be. This is typically used to create the other paths, such as VoxelDatabases.
		/// </summary>
		/// <value>An absolute path to the CubSub folder under Streaming Assets.</value>
		public static String CubSub
		{
			get { return Application.streamingAssetsPath + "/CubSub"; }
		}

		/// <summary>
		/// Determines where .asset files will be created during Substrate-to-Cubiquity conversion.
		/// </summary>
		/// <value>An absolute path to where the .asset files will initially be created. These .asset files can be moved to any place the Editor-User desires within the project hierarchy.</value>
		public static String VoxelDatabases
		{
			get { return CubSub + "/VoxelDatabases"; }
		}
		/// <summary>
		/// Determines where .vdb files will be created during Substrate-to-Cubiquity conversion.
		/// </summary>
		/// <value>An absolute path to where the .vdb files will be stored. These database files typically should not be moved after creation.</value>
		public static String VDBFiles
		{
			get { return VoxelDatabases + "/vdb"; }
		}

		/// <summary>
		/// This path supposes the project might have a dedicated parent directory for Substrate Map directores to be loaded from. It is the default location the editor will look in to find maps, but otherwise serves no purpose.
		/// </summary>
		/// <value>A string representing the absolute path to where we expect Substrate Map directories to be placed for import.</value>
		public static String MAPFolders
		{
			get{return CubSub + "/SubstrateData";}
		}

		/// <summary>
		/// This debug function pings the directories which Path is responsible for concatenating/supervising, and ensures that all of them exist.
		/// Save and load errors might abound if some of these directories were to go missing.
		/// </summary>
		public static void PingDirectories()
		{
			Debug.Log ("Checking the existance of Paths,\n Click to see they all register as 'True'" +
			"\nCubSub: " + CubSub + " : " + Directory.Exists (CubSub) +
			"\nVoxelDatabases: " + VoxelDatabases + " : " + Directory.Exists (VoxelDatabases) +
			"\nVDBFiles: " + VDBFiles + " : " + Directory.Exists (VDBFiles) +
			"\nMAPFolders: " + MAPFolders + " : " + Directory.Exists (MAPFolders));
		}

		///<summary>
		///<para>This function was more useful back when users were inputting their own save directories for .vdbs and VoxelDatabase .assets (and should be used
		///in the event a User-Editor would like to re-script such capabilities!) At present it is still used for
		///homogenizing path input AND dealing with situations where only one file name was given but we actually need to save multiple files by:</para>
		///<para></para>
		///<para>1. if there already is an extension, remove it.</para>
		///<para>2. determine if we have to concatenate any numerals and an _ based on whether we are saving an array of things</para>
		///<para>3. re-add the extension (or add it to begin with if there never was one)</para>
		///<para>4. return the result</para>
		///</summary>
		/// <returns>The homogenized and kosher path.</returns>
		/// <param name="path">The path we've been given for saving at, which might have uncertain extensions or need numeric additions depending on how much of what we're saving.</param>
		/// <param name="numToSave">The number of materials we'll be saving in a given set.</param>
		/// <param name="numOn">The index of the material we're saving in a given set.</param>
		/// <param name="extension">The extension the path needs to conclude with.</param>
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

		/// <summary>
		/// <para>Attempts to make an absolute file path relative by rooting it at the chosen parent directory. Handles situations in which the path is already rooted. Will throw an exception if the chosen root does not show up in the file path's parent directories. </para>
		/// <para></para>
		/// <para>Note: As of yet will *not* be able to handle an improperly rooted relative path. Ie: if the file path is 'Assets/Bob/file.txt' and the root is 'D:/.../Assets/' it will work. But if the root is 'd:/.../Assets/Bob/, though the root and file path obviously overlap, it will nevertheless throw an exception. </para>
		/// </summary>
		/// <returns>A relativ</returns>
		/// <param name="root">The 'root' at which we want to anchor the path. Ie: Application.DataPath</param>
		/// <param name="path">An absolute path to a file or directory which we would like to root. This path *may* already be rooted, but it cannot be relative *and* unrooted.</param>
		public static String RootToDirectory(String root, String path)
		{
			if (path.StartsWith(root)) 
			{
				//so this may be crazy, but if path starts with root, then the last folder of root (ie:Assets) can
				//be found by searching for the last index of / and adding 1. and that position will correspond to the
				//same place in path, allowing us to get the rooted path.
				return path.Substring(root.LastIndexOf("/") + 1);
			}
			//checks to see if the path is *already* rooted
			else if(path.StartsWith (root.Substring(root.LastIndexOf("/") + 1)))
			{
				return path;
			}
			else
			{
				throw new System.ArgumentException("A path ought to have been rooted at " + root + " but was not.", path);
				//return "";
			}
		}

	}
}
