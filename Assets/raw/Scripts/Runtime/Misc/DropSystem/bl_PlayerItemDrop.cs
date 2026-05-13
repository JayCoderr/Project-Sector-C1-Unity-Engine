using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_PlayerItemDrop : bl_MonoBehaviour
{
    /// <summary>
    /// When True, Block the throw of kits
    /// </summary>
    public bool BlockThrow { get; set; } = false;

    private int dropItemID = -1;
#if !ACTK_IS_HERE
    private int remaingKits;
#else
    private ObscuredInt remaingKits;
#endif

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (bl_GameData.isChatting || BlockThrow) return;

        if (bl_GameInput.ThrowKit())
        {
            DispatchThrow();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DispatchThrow()
    {
        if (bl_DropDispacher.Instance == null) return;

        if (dropItemID == -1)
        {
            dropItemID = PlayerReferences.gunManager.CurrentLoadout.DropKit;

            // get the item data from the container based in the equipped player loadout
            var dropItemData = bl_DropDispacher.Instance.GetItemContainer().GetItem(dropItemID);
            if (dropItemData != null) { remaingKits = dropItemData.Count; }
            else
            {
                dropItemID = -1;
                Debug.LogWarning($"The drop item with index {dropItemID} couldn't be found.");
            }
        }
        if (dropItemID == -1 || remaingKits <= 0) return;

        bl_DropDispacher.Instance.ThrowIndicator(PlayerReferences, dropItemID);
        remaingKits--;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    public void AddKits(int amount)
    {
        remaingKits += amount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemId"></param>
    public void SetDropItemID(int itemId)
    {
        dropItemID = itemId;
    }

#if MFPSM
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_TouchHelper.OnKit += OnMobileClick;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_TouchHelper.OnKit -= OnMobileClick;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMobileClick()
    {
        DispatchThrow();
    }
#endif

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}