using System.Collections.Generic;
using UnityEngine;

public abstract class bl_ItemManagerBase : bl_MonoBehaviour
{

    /// <summary>
    /// Queue the given item and respawn after the defined time
    /// </summary>
    /// <param name="item"></param>
    /// <param name="delay">if 0 the time defined in the bl_ItemManager inspector will be used</param>
    public abstract void RespawnAfter(bl_NetworkItem item, float delay = 0);

    /// <summary>
    /// Add the given item to the pool network items
    /// </summary>
    /// <param name="itemName">pool list name</param>
    /// <param name="item"></param>
    public abstract void PoolItem(string itemName, bl_NetworkItem item);

    /// <summary>
    /// Cache generic items
    /// Usefull when you are syncing items by name
    /// </summary>
    /// <param name="uniqueName"></param>
    /// <param name="go"></param>
    public abstract void RegisterGeneric(string uniqueName, GameObject go);

    /// <summary>
    /// Remove generic item from cache
    /// </summary>
    /// <param name="uniqueName"></param>
    public abstract void UnregisterGeneric(string uniqueName);

    /// <summary>
    /// Registers a local item by associating the specified game object with the given item name.
    /// </summary>
    /// <remarks>If the item name has not been registered previously, a new entry is created. Multiple game
    /// objects can be associated with the same item name.</remarks>
    /// <param name="itemName">The name used to identify the local item. Cannot be null.</param>
    /// <param name="go">The game object to associate with the specified item name. Cannot be null.</param>
    public abstract void RegisterLocalItem(string itemName, GameObject go);

    /// <summary>
    /// Unregisters a local item instance associated with the specified name and game object.
    /// </summary>
    /// <remarks>If the specified item name is not registered, or the game object is not associated with the
    /// item, no action is taken. This method is not thread-safe.</remarks>
    /// <param name="itemName">The name of the item to unregister. Cannot be null or empty.</param>
    /// <param name="go">The game object instance to remove from the local item registry. Cannot be null.</param>
    /// <returns><see langword="true"/> if the list still contains items after unregister this one; otherwise, <see langword="false"/>.</returns>
    public abstract bool UnregisterLocalItem(string itemName, GameObject go);

    public abstract Dictionary<string, List<GameObject>> GetLocalItems();

    private static bl_ItemManagerBase _instance;
    public static bl_ItemManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_ItemManagerBase>(); }
            return _instance;
        }
    }
}