using System;
using System.Linq;
using UnityEngine;

namespace vz777.Foundation.PrefabDatabases
{
	/// <summary>
	/// The prefab database presented as a scriptable object.<seealso cref="IPrefabDatabase"/>
	/// </summary>
	// [CreateAssetMenu(menuName = "vz777/Scriptable Object Prefab Database")]
	public abstract class ScriptableObjectPrefabDatabase<TKey> : ScriptableObject, IPrefabDatabase<TKey>
	{
		[SerializeField]
		private RegistryPair<TKey>[] registry;
		
		/// <summary>
		/// Get the prefab by the window's ID (must be string). The look-up of ID is ignoring case by default.
		/// </summary>
		public bool TryGetPrefabById(object id, out GameObject prefab)
		{
			prefab = null;
			
			if (id.GetType() != typeof(TKey))
			{
				Debug.LogError($"Getting prefab with given ID that is not a {nameof(TKey)}.");
				return false;
			}

			return TryGetPrefabById((TKey)id, out prefab);
		}

		/// <summary>
		/// Get the prefab by the window's ID. The look-up of ID is ignoring case by default.
		/// </summary>
		public bool TryGetPrefabById(TKey id, out GameObject prefab)
		{
			prefab = null;
			
			var pair = registry.FirstOrDefault(pair => Equals(pair.Id, id));
			if (!pair.Prefab)
				return false;
			
			prefab = pair.Prefab;
			return true;
		}
		
		/// <summary>
		/// Custom equal function for the key.
		/// </summary>
		protected abstract bool Equals(TKey id1, TKey id2);
		
		[Serializable]
		private struct RegistryPair<TId>
		{
			/// <summary>
			/// The ID of the registry of the prefab, make it string to be readable on the inspector.
			/// </summary>
			public TId Id;
			
			/// <summary>
			/// The prefab of the window.
			/// </summary>
			public GameObject Prefab;
		}
	}
}