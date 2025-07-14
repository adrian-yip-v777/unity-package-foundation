# Vector4 Extensions
## ConvertBlenderQuaternion
Specifically for converting `Blender's Quaternion` to `Unity's Quaternion`.
```C#
// Blender uses w,x,y,z
var blenderQuaternion = new Vector4 (w, x, y, z);

// Unity uses x,y,z,w
var unityQuaternion = blenderQuaternion.ConvertBlenderQuaternion();

// Custom euler angle offset if the default one is not working for your case.
var eulerAngleOffset = new Vector3 (-90, 0, 0);
var unityQuaternion2 = blenderQuaternion.ConvertBlenderQuaternion(eulerAngleOffset);
```