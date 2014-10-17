using UnityEngine;
using System.Collections;

using Cubiquity;
using Cubiquity.Impl;

namespace CogBlock
{
	/// <summary>
	/// This diabolical and evil class is the CogBlock variant of Cubiquity's Picking. In fact, it inherits from Cubiquity's Picking. Despite having the same name.
	/// It is evil. 
	/// </summary>
	public class Picking:Cubiquity.Picking
	{
		public static bool PickFirstSolidVoxel(CogBlockVolume volume, Ray ray, float distance, out PickVoxelResult pickResult)
		{
			//we want our ray divided into an origin and a direction
			return PickFirstSolidVoxel(volume, ray.origin, ray.direction, distance, out pickResult);
		}

		public static bool PickFirstSolidVoxel(CogBlockVolume volume, Vector3 origin, Vector3 direction, float distance, out PickVoxelResult pickResult)
		{			
			//initialization of an empty struct
			pickResult = new PickVoxelResult();
			
			// Can't hit it the volume if there's no data.
			if((volume.data == null) || (volume.data.volumeHandle == null))
			{
				return false;
			}

			//As noted by cubiquity, where cubqity's picking code works in volume space, Unity needs to work in world space...
			// Cubiquity's picking code works in volume space whereas we expose an interface that works in world

			//Grab our ray's components and transform them for volume space.
			Vector3 target = origin + direction * distance;				
			origin = volume.transform.InverseTransformPoint(origin);
			target = volume.transform.InverseTransformPoint(target);			
			direction = target - origin;
			
			// Now call through to the Cubiquity dll to do the actual picking.
			//Recall that CubiquityDLL will only have to work with ColoredCubeData, but that we are translating that boldface into CogBlockVolume data.
			uint hit = CubiquityDLL.PickFirstSolidVoxel((uint)volume.data.volumeHandle,
			                                            origin.x, origin.y, origin.z,
			                                            direction.x, direction.y, direction.z,
			                                            out pickResult.volumeSpacePos.x, out pickResult.volumeSpacePos.y, out pickResult.volumeSpacePos.z);
			
			// The result is in volume space, but again it is more convienient for Unity users to have the result
			// in world space. Therefore we apply the volume's volume-to-world transform to the volume space position.
			pickResult.worldSpacePos = volume.transform.TransformPoint((Vector3)(pickResult.volumeSpacePos));
			
			// Return true if we hit a surface.
			return hit == 1;
		}

		public static bool PickLastEmptyVoxel(CogBlockVolume volume, Ray ray, float distance, out PickVoxelResult pickResult)
		{
			//split ray into origin and direction again
			return PickLastEmptyVoxel(volume, ray.origin, ray.direction, distance, out pickResult);
		}
		

		public static bool PickLastEmptyVoxel(CogBlockVolume volume, Vector3 origin, Vector3 direction, float distance, out PickVoxelResult pickResult)
		{
			
			// This 'out' value needs to be initialised even if we don't hit
			// anything (in which case it will be left at it's default value).
			pickResult = new PickVoxelResult();
			
			// Can't hit it the volume if there's no data.
			if((volume.data == null) || (volume.data.volumeHandle == null))
			{
				return false;
			}
			
			//again, transforming to volume space
			Vector3 target = origin + direction * distance;				
			origin = volume.transform.InverseTransformPoint(origin);
			target = volume.transform.InverseTransformPoint(target);			
			direction = target - origin;
			
			// Now call through to the Cubiquity dll to do the actual picking.
			pickResult = new PickVoxelResult();
			uint hit = CubiquityDLL.PickLastEmptyVoxel((uint)volume.data.volumeHandle,
			                                           origin.x, origin.y, origin.z,
			                                           direction.x, direction.y, direction.z,
			                                           out pickResult.volumeSpacePos.x, out pickResult.volumeSpacePos.y, out pickResult.volumeSpacePos.z);
			
			//transform back out into worldspace
			pickResult.worldSpacePos = volume.transform.TransformPoint((Vector3)(pickResult.volumeSpacePos));
			
			// Return true if we hit a surface.
			return hit == 1;
		}
	}
}

