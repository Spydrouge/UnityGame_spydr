using UnityEngine;
using UnityEditor;

using System.Collections;
using Cubiquity;
using Cubiquity.Impl;

namespace CogBlock
{	
	/// <summary>
	/// This is where I can put all sorts of fun drawing tools, and also nab tools from the texture editor
	/// </summary>
	[CustomEditor (typeof(CogBlockVolume))]
	public class CogBlockVolumeInspector: Editor
	{
		CogBlockVolume volume;
		
		public static Tool lastTool = Tool.None;

		private static bool mSettingsMode = true;
		private static bool mAddMode = false;
		private static bool mDeleteMode = false;
		private static bool mPaintMode = false;

		Color paintColor = Color.white;
		
		GUIContent warningLabelContent;


		private static bool settingsMode
		{
			get { return mSettingsMode; }
			set { if(mSettingsMode != value) { mSettingsMode = value; OnEditorToolChanged(); } }
		}
		private static bool addMode
		{
			get { return mAddMode; }
			set { if(mAddMode != value) { mAddMode = value; OnEditorToolChanged(); } }
		}
		
		private static bool deleteMode
		{
			get { return mDeleteMode; }
			set { if(mDeleteMode != value) { mDeleteMode = value; OnEditorToolChanged(); } }
		}
		
		private static bool paintMode
		{
			get { return mPaintMode; }
			set { if(mPaintMode != value) { mPaintMode = value; OnEditorToolChanged(); } }
		}
		

		public void OnEnable()
		{
			volume = target as CogBlockVolume;
		}
		
		public override void OnInspectorGUI()
		{		
			// Check whether the selected Unity transform tool has changed.
			if(CogBlockVolumeInspector.lastTool != Tools.current)
			{
				OnTransformToolChanged();				
				CogBlockVolumeInspector.lastTool = Tools.current;
			}
			
			EditorGUILayout.LabelField("To modify the volume, please choose");
			EditorGUILayout.LabelField("a tool from the options below");
			
			EditorGUILayout.BeginHorizontal();
			{
				if(GUILayout.Toggle(settingsMode, "Settings", EditorStyles.miniButtonRight, GUILayout.Height(24)))
				{
					addMode = false;
					deleteMode = false;
					paintMode = false;
					settingsMode = true;
				}
				if(GUILayout.Toggle(addMode, "Add", EditorStyles.miniButtonLeft, GUILayout.Height(24)))
				{
					addMode = true;
					deleteMode = false;
					paintMode = false;
					settingsMode = false;
				}
				
				if(GUILayout.Toggle(deleteMode, "Delete", EditorStyles.miniButtonMid, GUILayout.Height(24)))
				{
					addMode = false;
					deleteMode = true;
					paintMode = false;
					settingsMode = false;
				}
				
				if(GUILayout.Toggle(paintMode, "Paint", EditorStyles.miniButtonMid, GUILayout.Height(24)))
				{
					addMode = false;
					deleteMode = false;
					paintMode = true;
					settingsMode = false;
				}
			}
			EditorGUILayout.EndHorizontal();
			
			if(addMode || paintMode)
			{
				paintColor = EditorGUILayout.ColorField("Voxel Color:", paintColor);
			}
			
			if(settingsMode)
			{
				DrawInstructions("Create new volume data through 'Main Menu -> Assets -> Create -> Cog Block Volume Data' and then assign it below.");
				volume.data = EditorGUILayout.ObjectField("Volume Data: ", volume.data, typeof(CogBlockVolumeData), true) as CogBlockVolumeData;
			}

		}
		
		private void DrawInstructions( string message)
		{
			EditorGUILayout.LabelField("Instructions", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(message, MessageType.None);
			EditorGUILayout.Space();
		}
		
		public void OnSceneGUI()
		{
			if(addMode || deleteMode || paintMode)
			{
				Event e = Event.current;
				
				Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
				
				if(((e.type == EventType.MouseDown) || (e.type == EventType.MouseDrag)) && (e.button == 0))
				{
					// Perform the raycasting. If there's a hit the position will be stored in these ints.
					PickVoxelResult pickResult;
					if(addMode)
					{
						bool hit = Picking.PickLastEmptyVoxel(volume, ray, 1000.0f, out pickResult);
						if(hit)
						{
							volume.data.SetVoxel(pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, (QuantizedColor)paintColor);
						}
					}
					else if(deleteMode)
					{					
						bool hit = Picking.PickFirstSolidVoxel(volume, ray, 1000.0f, out pickResult);
						if(hit)
						{
							volume.data.SetVoxel(pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, new QuantizedColor(0,0,0,0));
						}
					}
					else if(paintMode)
					{
						bool hit = Picking.PickFirstSolidVoxel(volume, ray, 1000.0f, out pickResult);
						if(hit)
						{
							volume.data.SetVoxel(pickResult.volumeSpacePos.x, pickResult.volumeSpacePos.y, pickResult.volumeSpacePos.z, (QuantizedColor)paintColor);
						}
					}
					
					Selection.activeGameObject = volume.gameObject;
				}
				else if ( e.type == EventType.Layout )
				{
					// See: http://answers.unity3d.com/questions/303248/how-to-paint-objects-in-the-editor.html
					HandleUtility.AddDefaultControl( GUIUtility.GetControlID( GetHashCode(), FocusType.Passive ) );
				}
			}
		}
		
		private static void OnEditorToolChanged()
		{
			// Whenever the user selects a terrain editing tool we need to make sure that Unity's transform widgets
			// are disabled. Otherwise the user can end up moving the terrain around while they are editing it.
			if(addMode || deleteMode || paintMode)
			{
				Tools.current = Tool.None;
			}
		}
		
		private static void OnTransformToolChanged()
		{
			// Deselect our editor tools if the user has selected a transform tool
			if(Tools.current != Tool.None)
			{
				mAddMode = false;
				mDeleteMode = false;
				mPaintMode = false;
				mSettingsMode = false;
			}
		}
	}
}

