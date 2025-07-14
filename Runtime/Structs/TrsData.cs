using UnityEngine;

namespace vz777.Foundation
{
    /// <summary>
    /// Containing translation, rotation, and scale data.
    /// </summary>
    public readonly struct TrsData
    {
        public TrsData(Transform transform, bool isWorldSpace = true)
        {
            IsWorldSpace = isWorldSpace;
            LocalScale = transform.localScale;
            
            if (isWorldSpace)
            {
                Position = transform.position;
                Rotation = transform.rotation;
                return;
            }
            
            Position = transform.localPosition;
            Rotation = transform.localRotation;
        }

        public TrsData(Vector3 position, Quaternion rotation, Vector3 localScale, bool isWorldSpace = true)
        {
            Position = position;
            Rotation = rotation;
            LocalScale = localScale;
            IsWorldSpace = isWorldSpace;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 LocalScale { get; }
        public bool IsWorldSpace { get; }

        public override string ToString()
        {
            return $"T: {Position} R: {Rotation} LS: {LocalScale}";
        }
    }
}