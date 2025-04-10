using UnityEngine;

namespace vz777.Foundations
{
    /// <summary>
    /// Responsible for counting reference for Unity's object, it will destroy the instance if there is no reference left. 
    /// </summary>
    public class UnityRefCounter<TUnityObject> : RefCounter<TUnityObject> where TUnityObject : Object
    {
        public UnityRefCounter(TUnityObject instance) : base(instance) { }

        protected override void DisposeUnManagedResources()
        {
            Object.DestroyImmediate(instance);
            instance = null;
        }
    }
}