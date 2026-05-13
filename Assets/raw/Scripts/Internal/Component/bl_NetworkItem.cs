#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class bl_NetworkItem : MonoBehaviour
{
    public ItemAuthority itemAuthority = ItemAuthority.All;
    [LovattoToogle] public bool isSceneItem = false;

    [SerializeField, HideInInspector]
    private bool isInitializated = false;

    /// <summary>
    /// The actor ID of the player who created this item (if was created by a player)
    /// -1 if this item was created by the scene.
    /// </summary>
    public int OwnerActorID { get; set; } = -1;

    [SerializeField, HideInInspector]
    private string m_itemName;
    public string ItemName
    {
        get
        {
            if (string.IsNullOrEmpty(m_itemName))
            {
                m_itemName = $"{gameObject.name.Replace(" (Clone)", "")} [{bl_StringUtility.GenerateKey()}]";
            }
            return m_itemName;
        }
        set => m_itemName = value;
    }

    private Collider _collider;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        Init();
    }

    /// <summary>
    /// This is called when the item is instanced by the local player
    /// after the drop delivery has arrived.
    /// </summary>
    public void OnLocalInstanced()
    {
        // ensure to reset the name of the item since the prefab could be already have an custom name.
        m_itemName = string.Empty;
        isInitializated = false;
    }

    /// <summary>
    /// If the local player create this item, this will be executed
    /// </summary>
    void Init()
    {
        if (isInitializated)
        {
            // in case this is a scene item, we add it to the pool so it can be easily identified later.
            bl_ItemManagerBase.Instance?.PoolItem(m_itemName, this);
            return;
        }

        string prefabName = gameObject.name.Replace(" (Clone)", "").Replace("(Clone)", "");
        gameObject.name = ItemName;
        OwnerActorID = bl_PhotonNetwork.LocalPlayer.ActorNumber;
        isInitializated = true;

        //add the item in the pool here since it won't be added for the local player
        bl_ItemManagerBase.Instance.PoolItem(ItemName, this);

        //instead of use a PhotonView for each item, we simple sync the information
        //and identify the item by an unique name
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("prefab", prefabName);
        data.Add("name", ItemName);
        data.Add("actorID", OwnerActorID);
        data.Add("position", transform.position);
        data.Add("rotation", transform.rotation);

        //this call is received and handled in bl_ItemManager.cs -> OnNetworkItemInstance()
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemInstance, data);
    }

    /// <summary>
    /// Setup this item.
    /// </summary>
    /// <param name="uniqueName"></param>
    /// <param name="actorID"></param>
    public void Setup(string uniqueName, int actorID)
    {
        ItemName = uniqueName;
        OwnerActorID = actorID;
        isInitializated = true;
        isSceneItem = false;
        gameObject.name = ItemName;
    }

    /// <summary>
    /// If this item was created for other player, this code will be executed
    /// </summary>
    public void OnNetworkInstance(ExitGames.Client.Photon.Hashtable data)
    {
        isSceneItem = false;
        ItemName = (string)data["name"];
        OwnerActorID = (int)data["actorID"];

        gameObject.name = ItemName;
        isInitializated = true;
    }

    /// <summary>
    /// Destroy this network item for all clients
    /// </summary>
    public void DestroySync()
    {
        if (!IsAuthorized()) return;

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("name", ItemName);
        data.Add("active", -1);
        data.Add("by", bl_PhotonNetwork.LocalPlayer.ActorNumber);
        data.Add("byTeam", bl_MFPS.LocalPlayer.Team);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemChange, data);
    }

    /// <summary>
    /// Active or disable this network item for all clients
    /// </summary>
    public void SetActiveSync(bool active)
    {
        if (!IsAuthorized()) return;

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("name", ItemName);
        data.Add("active", active ? 1 : 0);
        data.Add("by", bl_PhotonNetwork.LocalPlayer.ActorNumber);
        data.Add("byTeam", bl_MFPS.LocalPlayer.Team);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.NetworkItemChange, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsAuthorized()
    {
        if (itemAuthority == ItemAuthority.All) return true;
        if (itemAuthority == ItemAuthority.MasterClientOnly) return bl_PhotonNetwork.IsMasterClient;
        return bl_PhotonNetwork.LocalPlayer.ActorNumber == OwnerActorID;
    }

    /// <summary>
    /// Called before this item is destroyed
    /// </summary>
    /// <param name="byActorId">The actor ID who request destroy the item.</param>
    /// <param name="byTeam">The team of the actor who request.</param>
    public virtual void OnBeforeDestroy(int byActorId, Team byTeam)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void DrawGizmos()
    {
        if (Application.isPlaying) return;

        if (_collider == null) { _collider = GetComponent<Collider>(); }
        if (_collider == null) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        bl_GizmosUtility.DrawSolidRectangle(_collider.bounds.center, _collider.bounds.size, transform.rotation);
        Gizmos.color = Color.white;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isSceneItem) { isInitializated = false; }
        if (Application.isPlaying || !isSceneItem || bl_ItemManagerBase.Instance == null) return;

        EditorValidateName();
    }

    public void EditorValidateName()
    {
        if (string.IsNullOrEmpty(m_itemName))
        {
            m_itemName = ItemName;
            gameObject.name = m_itemName;
            isInitializated = true;
        }
        else if (gameObject.name.Contains("("))
        {
            int io = gameObject.name.LastIndexOf('[');
            m_itemName = "";
            if (io != -1)
            {
                string baseName = gameObject.name.Substring(0, io - 1);
                gameObject.name = baseName;
            }
            else
            {
                gameObject.name = "New Item";
            }
            m_itemName = ItemName;
            m_itemName = m_itemName.Replace("(", "");
            m_itemName = m_itemName.Replace(")", "");
            gameObject.name = m_itemName;
            isInitializated = true;
            EditorUtility.SetDirty(this);
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmos();
    }
#endif

    [System.Serializable]
    public enum ItemAuthority
    {
        All = 0,
        CreatorOnly,
        MasterClientOnly
    }
}