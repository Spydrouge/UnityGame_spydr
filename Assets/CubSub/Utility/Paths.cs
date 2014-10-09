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
		public static string CubSub
		{
			get { return Application.streamingAssetsPath + "/CubSub"; }
		}

		public static string VoxelDatabases
		{
			get { return CubSub + "/VoxelDatabases"; }
		}

		public static string VDBFiles
		{
			get { return VoxelDatabases + "/vdbs"; }
		}
	}
}
