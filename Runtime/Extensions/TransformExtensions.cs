using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace vz777.Foundation
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Get the scene path of this transform.
        /// </summary>
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
        public static TrsData GetWorldTransformFromOtherParent(
            this Transform transform, Transform otherParent)
        {
            var scaledPosition = new Vector3(
                transform.localPosition.x * otherParent.lossyScale.x,
                transform.localPosition.y * otherParent.lossyScale.y,
                transform.localPosition.z * otherParent.lossyScale.z
            );
        
            var position = otherParent.position + otherParent.rotation * scaledPosition;
            var rotation = otherParent.rotation * transform.localRotation;
            var scale = TransformUtils.GetLocalScaleFromOtherParent(transform.localScale, transform.parent, otherParent);
            
            return new TrsData(position, rotation, scale);
        }
    }
}