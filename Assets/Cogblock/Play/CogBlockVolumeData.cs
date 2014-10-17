using UnityEngine;

using System;
using System.IO;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;


namespace CogBlock
{
	/// <summary>
	/// I am nervous that this cannot inherit from ColoredCubes, and wary of errors it may cause. 
	/// </summary>
	[System.Serializable]
	public sealed class CogBlockVolumeData : VolumeData
	{	
		/// Gets the color of the specified position.
		/**
		 * \param x The 'x' position of the voxel to get.
		 * \param y The 'y' position of the voxel to get.
		 * \param z The 'z' position of the voxel to get.
		 * \return The color of the voxel.
		 */
		public QuantizedColor GetVoxel(int x, int y, int z)
		{
			// The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
			QuantizedColor result;
			if(volumeHandle.HasValue)
			{
				CubiquityDLL.GetVoxel(volumeHandle.Value, x, y, z, out result);
			}
			else
			{
				result = new QuantizedColor();
			}
			return result;
		}
		
		/// Sets the color of the specified position.
		/**
		 * \param x The 'x' position of the voxel to set.
		 * \param y The 'y' position of the voxel to set.
		 * \param z The 'z' position of the voxel to set.
		 * \param quantizedColor The color the voxel should be set to.
		 */
		public void SetVoxel(int x, int y, int z, QuantizedColor quantizedColor)
		{
			// The initialization can fail (bad filename, database locked, etc), so the volume handle could still be null.
			if(volumeHandle.HasValue)
			{
				if(x >= enclosingRegion.lowerCorner.x && y >= enclosingRegion.lowerCorner.y && z >= enclosingRegion.lowerCorner.z
				   && x <= enclosingRegion.upperCorner.x && y <= enclosingRegion.upperCorner.y && z <= enclosingRegion.upperCorner.z)
				{						
					CubiquityDLL.SetVoxel(volumeHandle.Value, x, y, z, quantizedColor);
				}
			}
		}
		
		public override void CommitChanges()
		{
			if(!IsVolumeHandleNull())
			{
				if(writePermissions == WritePermissions.ReadOnly)
				{
					throw new InvalidOperationException("Cannot commit changes to read-only voxel database (" + fullPathToVoxelDatabase +")");
				}
				
				CubiquityDLL.AcceptOverrideBlocks(volumeHandle.Value);
				//We can discard the blocks now that they have been accepted.
				CubiquityDLL.DiscardOverrideBlocks(volumeHandle.Value);
			}
		}
		
		public override void DiscardChanges()
		{
			if(!IsVolumeHandleNull())
			{
				CubiquityDLL.DiscardOverrideBlocks(volumeHandle.Value);
			}
		}
		
		/// \cond
		protected override void InitializeEmptyCubiquityVolume(Region region)
		{		
			// We check 'mVolumeHandle' instead of 'volumeHandle' as the getter for the latter will in turn call this method.
			DebugUtils.Assert(mVolumeHandle == null, "Volume handle should be null prior to initializing volume");
			
			if(!initializeAlreadyFailed) // If it failed before it will fail again - avoid spamming error messages.
			{
				try
				{
					// Create an empty region of the desired size.
					volumeHandle = CubiquityDLL.NewEmptyColoredCubesVolume(region.lowerCorner.x, region.lowerCorner.y, region.lowerCorner.z,
					                                                       region.upperCorner.x, region.upperCorner.y, region.upperCorner.z, fullPathToVoxelDatabase, DefaultBaseNodeSize);
				}
				catch(CubiquityException exception)
				{
					volumeHandle = null;
					initializeAlreadyFailed = true;
					Debug.LogException(exception);
					Debug.LogError("Failed to open voxel database '" + fullPathToVoxelDatabase + "'");
				}
			}
		}
		/// \endcond
		
		/// \cond
		protected override void InitializeExistingCubiquityVolume()
		{				
			// We check 'mVolumeHandle' instead of 'volumeHandle' as the getter for the latter will in turn call this method.
			DebugUtils.Assert(mVolumeHandle == null, "Volume handle should be null prior to initializing volume");
			
			if(!initializeAlreadyFailed) // If it failed before it will fail again - avoid spamming error messages.
			{
				try
				{
					// Create an empty region of the desired size.
					volumeHandle = CubiquityDLL.NewColoredCubesVolumeFromVDB(fullPathToVoxelDatabase, writePermissions, DefaultBaseNodeSize);
				}
				catch(CubiquityException exception)
				{
					volumeHandle = null;
					initializeAlreadyFailed = true;
					Debug.LogException(exception);
					Debug.LogError("Failed to open voxel database '" + fullPathToVoxelDatabase + "'");
				}
			}
		}
		/// \endcond
		
		/// \cond
		public override void ShutdownCubiquityVolume()
		{
			if(!IsVolumeHandleNull())
			{
				// We only save if we are in editor mode, not if we are playing.
				bool saveChanges = (!Application.isPlaying) && (writePermissions == WritePermissions.ReadWrite);
				
				if(saveChanges)
				{
					CommitChanges();
				}
				else
				{
					DiscardChanges();
				}
				
				CubiquityDLL.DeleteColoredCubesVolume(volumeHandle.Value);
				volumeHandle = null;
			}
		}
		/// \endcond
	}
}