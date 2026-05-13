using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This script handle the instantiation of network items
/// Network items are those game objects that instantiation and or destruction is replicated on all other clients in the room.
/// With this script, those network items doesn't have to have a PhotonView or be located in a Resources folder
/// Making way more network and memory efficient.
/// 
/// In order to instance a network item, there's only one requirement:
/// 1. the item prefab has to be listed in the 'Network Items Prefabs' list
/// </summary>
public class bl_ItemManager : bl_ItemManagerBase
{
    [Header("Network Prefabs")]
    public List<bl_NetworkItem> networkItemsPrefabs = new();

    [Header("Settings")]
    public float respawnItemsAfter = 12;

    //private
    private readonly Dictionary<string, bl_NetworkItem> networkItemsPool = new();
    private readonly Dictionary<string, GameObject> genericItems = new();
    private readonly List<RespawnItems> respawnItems = new();
    private Dictionary<string, List<GameObject>> localInstanceItems = new();

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        if (!bl_PhotonNetwork.IsConnected) return;

        base.OnEnable();
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.NetworkItemInstance, OnNetworkItemInstance);
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.NetworkItemChange, OnNetworkItemChange);
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.EventItemSync, OnItemEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonNetwork.Instance?.RemoveCallback(OnNetworkItemInstance);
        bl_PhotonNetwork.Instance?.RemoveCallback(OnNetworkItemChange);
        bl_PhotonNetwork.Instance?.RemoveCallback(OnItemEvent);
    }

    /// <summary>
    /// Handles the instantiation of a network item for remote players based on the provided data.
    /// </summary>
    /// <remarks>This method ensures that the item is not instantiated for the player who created it, as it is
    /// already instantiated locally for them. If the specified prefab is not found in the list of registered network
    /// item prefabs, a warning is logged, and no item is instantiated.</remarks>
    /// <param name="data">A <see cref="Hashtable"/> containing the necessary information to instantiate the network item.  Expected keys
    /// include: <list type="bullet"> <item><description><c>"actorID"</c>: The ID of the player who created the item.
    /// The item will not be instantiated for this player.</description></item> <item><description><c>"prefab"</c>: The
    /// name of the prefab to instantiate.</description></item> <item><description><c>"position"</c>: The <see
    /// cref="Vector3"/> position where the item should be instantiated.</description></item>
    /// <item><description><c>"rotation"</c>: The <see cref="Quaternion"/> rotation to apply to the instantiated
    /// item.</description></item> </list></param>
    void OnNetworkItemInstance(Hashtable data)
    {
        //don't instance for the player that create the item since it's already instance for him
        int actorID = (int)data["actorID"];
        if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actorID) return;

        string prefabName = (string)data["prefab"];
        bl_NetworkItem prefab = networkItemsPrefabs.Find(x =>
        {
            return (x != null && x.gameObject.name == prefabName);
        });

        if (prefab == null)
        {
            Debug.LogWarning($"The network prefab {prefabName} is not listed in the bl_ItemManager of this scene.");
            return;
        }

        prefab = Instantiate(prefab.gameObject, (Vector3)data["position"], (Quaternion)data["rotation"]).GetComponent<bl_NetworkItem>();
        prefab.OnNetworkInstance(data);
        //pool this network item
        PoolItem(prefabName, prefab);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void PoolItem(string itemName, bl_NetworkItem item)
    {
        if (networkItemsPool.ContainsKey(itemName)) return;
        networkItemsPool.Add(itemName, item);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueName"></param>
    /// <param name="go"></param>
    public override void RegisterGeneric(string uniqueName, GameObject go)
    {
        if (genericItems.ContainsKey(uniqueName)) return;
        genericItems.Add(uniqueName, go);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uniqueName"></param>
    public override void UnregisterGeneric(string uniqueName)
    {
        if (genericItems.ContainsKey(uniqueName))
        {
            genericItems.Remove(uniqueName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnItemEvent(Hashtable data)
    {
        byte command = (byte)data["c"];

        switch (command)
        {
            case 0: // sync projectile detonation
                OnSyncDetonation(data);
                break;
            case 1: // destroy projectile
                DestroyProjectile(data);
                break;
            case 2: // custom projectile command
                OnCustomProjectileCommand(data);
                break;
            default:
                Debug.LogWarning("Undefined item event " + command);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnSyncDetonation(Hashtable data)
    {
        string itemName = (string)data[(byte)0];
        Vector3 pos = (Vector3)data[(byte)1];
        Quaternion rot = (Quaternion)data[(byte)2];
        BulletData bulletData = (BulletData)data[(byte)3];

        if (!genericItems.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return;
        }
        if (genericItems[itemName] == null)
        {
            Debug.LogWarning("The item was null, maybe it was destroyed before the network command reach this client.");
            genericItems.Remove(itemName);
            return;
        }

        if (genericItems[itemName].TryGetComponent(out bl_ProjectileBase projectileBase))
        {
            projectileBase.Detonate(pos, rot, bulletData, false);
        }
    }

    /// <summary>
    /// Destroys the projectile associated with the specified data if it exists.
    /// </summary>
    /// <param name="data">A <see cref="Hashtable"/> containing information used to identify the projectile to destroy. Must include an
    /// entry at key <c>(byte)0</c> with the projectile's name as a <see cref="string"/>.</param>
    private void DestroyProjectile(Hashtable data)
    {
        string itemName = (string)data[(byte)0];
        var item = TryGetGenericItem(itemName);
        if (item == null) return;

        if (item.TryGetComponent(out bl_ProjectileBase projectileBase))
        {
            projectileBase.DestroyProjectile(false);
        }
    }

    /// <summary>
    /// Processes a custom command for a projectile item using the provided data payload.
    /// </summary>
    /// <remarks>The method attempts to locate the specified item and, if it contains a projectile component,
    /// forwards the custom command and parameters to it. If the item is not found or does not support custom projectile
    /// commands, no action is taken.</remarks>
    /// <param name="data">A <see cref="Hashtable"/> containing the command data. The table must include the item name, command identifier,
    /// and an array of parameters at keys 0, 1, and 2 respectively.</param>
    private void OnCustomProjectileCommand(Hashtable data)
    {
        string itemName = (string)data[(byte)0];
        string commandId = (string)data[(byte)1];
        object[] parameters = (object[])data[(byte)2];
        var item = TryGetGenericItem(itemName);
        if (item == null) return;

        if (item.TryGetComponent(out bl_ProjectileBase projectileBase))
        {
            projectileBase.OnReceiveCustomCommand(commandId, parameters);
        }
    }

    /// <summary>
    /// Handles changes to a network item's state, such as activation, deactivation, or removal.
    /// </summary>
    /// <remarks>If the specified network item does not exist in the pool, a warning is logged, and no further
    /// action is taken. For removal operations (state = -1), the item's <c>OnBeforeDestroy</c> method is called before
    /// it is destroyed and removed from the pool. For activation or deactivation, the item's <c>GameObject</c> is set
    /// to active or inactive based on the state value.</remarks>
    /// <param name="data">A <see cref="Hashtable"/> containing information about the network item change.  Expected keys include: <list
    /// type="bullet"> <item><term>"name"</term><description>The name of the network item as a <see
    /// cref="string"/>.</description></item> <item><term>"active"</term><description>An <see cref="int"/> indicating
    /// the item's state: 1 for active, 0 for inactive, and -1 for removal.</description></item>
    /// <item><term>"by"</term><description>An <see cref="int"/> representing the ID of the player initiating the change
    /// (used for removal).</description></item> <item><term>"byTeam"</term><description>A <see cref="Team"/> value
    /// representing the team of the player initiating the change (used for removal).</description></item> </list></param>
    void OnNetworkItemChange(Hashtable data)
    {
        string itemName = (string)data["name"];

        if (!networkItemsPool.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return;
        }

        int state = (int)data["active"];
        if (state == -1)
        {
            var item = networkItemsPool[itemName];

            if (item != null)
            {
                item.OnBeforeDestroy((int)data["by"], (Team)data["byTeam"]);
                Destroy(item.gameObject);
            }
            networkItemsPool.Remove(itemName);
        }
        else
        {
            networkItemsPool[itemName].gameObject.SetActive(state == 1 ? true : false);
        }
    }

    /// <summary>
    /// Registers a local item by associating the specified game object with the given item name.
    /// </summary>
    /// <remarks>If the item name has not been registered previously, a new entry is created. Multiple game
    /// objects can be associated with the same item name.</remarks>
    /// <param name="itemName">The name used to identify the local item. Cannot be null.</param>
    /// <param name="go">The game object to associate with the specified item name. Cannot be null.</param>
    public override void RegisterLocalItem(string itemName, GameObject go)
    {
        if (!localInstanceItems.ContainsKey(itemName))
        {
            localInstanceItems.Add(itemName, new List<GameObject>());
        }
        localInstanceItems[itemName].Add(go);
    }

    /// <summary>
    /// Unregisters a local item instance associated with the specified name and game object.
    /// </summary>
    /// <remarks>If the specified item name is not registered, or the game object is not associated with the
    /// item, no action is taken. This method is not thread-safe.</remarks>
    /// <param name="itemName">The name of the item to unregister. Cannot be null or empty.</param>
    /// <param name="go">The game object instance to remove from the local item registry. Cannot be null.</param>
    public override bool UnregisterLocalItem(string itemName, GameObject go)
    {
        if (localInstanceItems.ContainsKey(itemName))
        {
            localInstanceItems[itemName].Remove(go);
            if (localInstanceItems[itemName].Count == 0)
            {
                localInstanceItems.Remove(itemName);
                return false;
            }

            return true;
        }
        return false;
    }

    public override Dictionary<string, List<GameObject>> GetLocalItems()
    {
        return localInstanceItems;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnSlowUpdate()
    {
        CheckTimers();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckTimers()
    {
        if (respawnItems.Count <= 0) return;

        int c = respawnItems.Count;
        for (int i = c - 1; i >= 0; i--)
        {
            if (Time.time - respawnItems[i].AddedTime >= respawnItems[i].RespawnAfter)
            {
                respawnItems[i].Item.SetActiveSync(true);
                respawnItems.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Add an item to the waiting list to respawn it after certain time
    /// </summary>
    public override void RespawnAfter(bl_NetworkItem item, float respawnAfter = 0)
    {
        respawnItems.Add(new RespawnItems()
        {
            Item = item,
            AddedTime = Time.time,
            RespawnAfter = respawnAfter <= 0 ? respawnItemsAfter : respawnAfter
        });
        item.SetActiveSync(false);
    }

    private GameObject TryGetGenericItem(string itemName)
    {
        if (!genericItems.ContainsKey(itemName))
        {
            Debug.LogWarning($"The network item {itemName} couldn't be found, maybe was instanced before this player enter in the room.");
            return null;
        }
        if (genericItems[itemName] == null)
        {
            Debug.LogWarning("The item was null, maybe it was destroyed before the network command reach this client.");
            genericItems.Remove(itemName);
            return null;
        }
        return genericItems[itemName];
    }

    /// <summary>
    /// 
    /// </summary>
    public class RespawnItems
    {
        public float AddedTime;
        public float RespawnAfter;
        public bl_NetworkItem Item;
    }
}