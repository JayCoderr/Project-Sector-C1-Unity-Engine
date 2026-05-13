using MFPS.Internal.Scriptables;
using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default MFPS pooling system
/// If you want to use your custom pool system, simply inherited your script from bl_ObjectPoolingBase.cs
/// And make sure you served the default prefabs from the default pool system with the same key/identifiers.
/// </summary>
public class bl_ObjectPooling : bl_ObjectPoolingBase
{
    [ScriptableDrawer] public bl_PrefabPoolContainer prefabContainer;

    private readonly Dictionary<string, PoolObject> pools = new();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (prefabContainer == null)
        {
            Debug.LogError("Prefab Container is null, assign it in the inspector.", gameObject);
            return;
        }

        foreach (var pooledPrefab in prefabContainer.Prefabs)
        {
            RegisterObject(pooledPrefab.Key, pooledPrefab.Prefab, pooledPrefab.PoolCount, pooledPrefab.InstanceMethod == bl_PrefabPoolContainer.InstanceMethod.OnStart);
        }
    }

    /// <summary>
    /// Add a new pooled prefab
    /// </summary>
    /// <param name="poolName">Identifier of this pool</param>
    /// <param name="prefab"></param>
    /// <param name="count"></param>
    public void RegisterObject(string poolName, GameObject prefab, int count, bool preInstance)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"Can't pool the prefab for '{poolName}' because the prefab has not been assigned.");
            return;
        }

        if (pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool for '{poolName}' already exists.");
            return;
        }

        var pool = new PoolObject(poolName, prefab, count, transform, preInstance);
        pools[poolName] = pool;
    }

    /// <summary>
    /// Instantiate a pooled prefab
    /// use this instead of GameObject.Instantiate(...)
    /// </summary>
    /// <returns></returns>
    public override GameObject Instantiate(string objectName, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(objectName, out var pool))
        {
            Debug.LogError($"Object '{objectName}' has not been registered for pooling.");
            return null;
        }

        var obj = pool.GetNextAvailable();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public class PoolObject
    {
        public string Name { get; }
        public GameObject Prefab { get; }
        private readonly GameObject[] poolItems;
        private int nextIndex;
        private readonly Transform parent;

        public PoolObject(string name, GameObject prefab, int count, Transform parent, bool preInstance)
        {
            Name = name;
            Prefab = prefab;
            poolItems = new GameObject[count];
            this.parent = parent;

            nextIndex = 0;

            if (!preInstance) return;

            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab, parent);
                instance.SetActive(false);
                poolItems[i] = instance;
            }
        }

        public GameObject GetNextAvailable()
        {
            GameObject obj = poolItems[nextIndex];
            nextIndex = (nextIndex + 1) % poolItems.Length;

            // if the object has not been instantiated yet or was destroyed, instance now.
            if (obj == null)
            {
                obj = Instantiate(Prefab, parent);
                poolItems[nextIndex] = obj;
            }

            return obj;
        }
    }

    [Serializable]
    public class PreRegister
    {
        public string Name;
        public GameObject Prefab;
        public int Lenght;
    }
}