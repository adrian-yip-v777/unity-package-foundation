using UnityEngine;

namespace vz777.Foundation.PrefabDatabases
{
	/// <summary>
	/// Responsible for serving prefab by given ID with specific type
	/// </summary>
	public interface IPrefabDatabase
	{
		bool TryGetPrefabById(object id, out GameObject prefab);
	}
	
	/// <summary>
	/// Responsible for serving prefab by given ID with specific type
	/// </summary>
	/// <typeparam name="TKey">The type of the key for accessing the prefab.</typeparam>
	public interface IPrefabDatabase<in TKey> : IPrefabDatabase
	{
		bool TryGetPrefabById(TKey id, out GameObject prefab);
	}
}