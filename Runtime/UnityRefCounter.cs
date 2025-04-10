using UnityEngine;

namespace AYip.Foundations
{
    /// <summary>
    /// Responsible for counting reference for Unity's object, it will destroy the instance if there is no reference left. 
    /// </summary>
    public class UnityRefCounter<TUnityObject> : RefCounter<TUnityObject> where TUnityObject : Object
    {
        public UnityRefCounter(TUnityObject instance) : base(instance) { }

        protected override void ReleaseInstance()
        {
            Object.DestroyImmediate(instance);
        }
    }
}