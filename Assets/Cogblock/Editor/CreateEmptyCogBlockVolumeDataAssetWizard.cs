using UnityEngine;
using UnityEditor;

using System.Collections;

using Cubiquity;
using Cubiquity.Impl;

namespace CogBlock
{
	public class CreateEmptyCogBlockVolumeDataAssetWizard : ScriptableWizard
	{
		public int width = 256;
		public int height = 256;
		public int depth = 256;
		
		public bool generateFloor = true;
		
		void OnWizardCreate()
		{
			CogBlockVolumeData data = Cubiquity.VolumeDataAsset.CreateEmptyVolumeData<CogBlockVolumeData>(new Region(0, 0, 0, width-1, height-1, depth-1));
			
			if(generateFloor)
			{
				// Create a floor so the volume data is actually visible in the editor.
				int floorThickness = 8;
				QuantizedColor floorColor = new QuantizedColor(255, 192, 192, 255);
				
				for(int z = 0; z <= depth-1; z++)
				{
					for(int y = 0; y < floorThickness; y++)
					{
						for(int x = 0; x <= width-1; x++)
						{
							data.SetVoxel(x, y, z, floorColor);
						}
					}
				}
			}
		}  
	}
}