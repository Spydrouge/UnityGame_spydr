using UnityEngine;
using UnityEditor;
using System.Collections;

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Cubiquity.Impl;

//Purpose: It is useful to know how to create my own EditorWindow, even if I decided to go with menu options for the code I'm currently working on
//Author: Melissa Kronenberger (Spydrouge)

namespace CubSub
{	
	//By Deriving from EditorWindow, I have created an independent piece of software accessible at edit time that does not have to be attached to a gameobject, like ColoredCubeVolume
	//It will be its own panel, and hopefully more useful as a result. 
	public class PlaceHolder : EditorWindow
	{
		//This sexy code puts this EditorWindow into Unity's Menus (ya know, at the top of the program)
		[MenuItem ("Window/OpenCog/Placeholder")]
		
		//LOOK AT ME COPY-PASTING UNITY CODE, DOH!
		//(this function is built into Unity, and makes sure if we hit the same menu option twice, we
		// don't bring up two different windows, but rather create the 1st one or re-select the 1st one if
		// it already exists.)
		public static void ShowWindow()
		{
			EditorWindow.GetWindow(typeof(PlaceHolder));
		}
		
		//Dis is vhere de actual 'look' of the panel goes.
		void OnGUI()
		{
			
			GUILayout.Label("This Window Has No Function!", EditorStyles.boldLabel);
		}
		
	
	}
}
