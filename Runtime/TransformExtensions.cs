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
    }
}