using UnityEngine;

namespace vz777.Foundations
{
	public static class Vector4Extensions
	{
		/// <summary>
		/// Convert blender's quaternion to unity's quaternion.
		/// </summary>
		/// <param name="blenderQuaternion">Order in wxyz</param>
		/// <returns>A unity quaternion</returns>
		public static Quaternion ConvertBlenderQuaternion(this Vector4 blenderQuaternion, Vector3? eulerOffset = null)
		{
			// Correctly map WXYZ to Unity's XYZW
			var quaternion = new Quaternion(
				blenderQuaternion.y, // == blender.x
				blenderQuaternion.z, // == blender.y
				blenderQuaternion.w, // == blender.z
				blenderQuaternion.x  // == blender.w
			);

			// Conversion quaternion: 90° around X
			var convertQuaternion = eulerOffset.HasValue ? 
				Quaternion.Euler(eulerOffset.Value.x, eulerOffset.Value.y, eulerOffset.Value.z) : 
				Quaternion.Euler(90, 0, 0);

			// Transform quaternion
			var unityQuaternion = convertQuaternion * quaternion * Quaternion.Inverse(convertQuaternion);
			return unityQuaternion.normalized;
		}
	}
}