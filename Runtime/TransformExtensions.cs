using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace vz777.Foundations
{
    public static class TransformExtensions
    {
        public static string GetScenePath(this Transform transform)
        {
            var current = transform;
            var inScenePath = new List<string> { current.name };
            while (current != transform.root)
            {
                current = current.parent;
                inScenePath.Add(current.name);
            }
            
            var builder = new StringBuilder();
            foreach (var item in Enumerable.Reverse(inScenePath)) 
                builder.Append($"\\{item}");
            
            return builder.ToString().TrimStart('\\');
        }

        /// <summary>
        /// Get the world transform of the other parent.
        /// </summary>
        public static (Vector3 position, Quaternion rotation, Vector3 scale) GetWorldTransformFromOtherParent(
            this Transform transform, Transform otherParent)
        {
            var scaledPosition = new Vector3(
                transform.localPosition.x * otherParent.lossyScale.x,
                transform.localPosition.y * otherParent.lossyScale.y,
                transform.localPosition.z * otherParent.lossyScale.z
            );
        
            var position = otherParent.position + otherParent.rotation * scaledPosition;
            var rotation = otherParent.rotation * transform.localRotation;
            var parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;
            var scale = new Vector3(
                otherParent.lossyScale.x / parentScale.x * transform.localScale.x,
                otherParent.lossyScale.y / parentScale.y * transform.localScale.y,
                otherParent.lossyScale.z / parentScale.z * transform.localScale.z
            );

            return (position, rotation, scale);
        } 
    }
}