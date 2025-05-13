using UnityEngine;

namespace vz777.Foundations
{
    public class TransformUtils
    {
        public static Vector3 GetLocalScaleFromOtherParent(Vector3 localScale, Transform parentA, Transform parentB)
        {
            Vector3 scale;
            if (parentA)
            {
                var parentScale = parentA ? parentA.lossyScale : Vector3.one;

                // Calculate desired world scale under otherParent
                var desiredWorldScale = new Vector3(
                    parentB.lossyScale.x * localScale.x,
                    parentB.lossyScale.y * localScale.y,
                    parentB.lossyScale.z * localScale.z
                );

                // Calculate Child's new local scale under Parent A
                scale = new Vector3(
                    desiredWorldScale.x / parentScale.x,
                    desiredWorldScale.y / parentScale.y,
                    desiredWorldScale.z / parentScale.z
                );
            }
            else
            {
                scale = new Vector3(
                    parentB.lossyScale.x * localScale.x,
                    parentB.lossyScale.y * localScale.y,
                    parentB.lossyScale.z * localScale.z
                );
            }

            return scale;
        }
    }
}