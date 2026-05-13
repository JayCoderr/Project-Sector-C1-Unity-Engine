using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerModelSkins : MonoBehaviour
{
    [SerializeField] private bl_PlayerReferences playerReferences;
    public int defaultSkin = 0;
    [LovattoToogle] public bool autoFetch = true;
    public List<PlayerModelSkinSetup> skins;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (autoFetch) FetchSyncSkin(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void FetchSyncSkin(bool isAutoCall = false)
    {
        if (transform.parent == null) { return; }
        if (playerReferences == null)
        {
            if (!isAutoCall) SetDefaultSkin();
            return;
        }

        if (playerReferences.photonView.InstantiationData == null || playerReferences.photonView.InstantiationData.Length < 2)
        {
            SetDefaultSkin();
            Debug.LogWarning("This player prefab does not have skin data, please check the player prefab setup.");
            return;
        }

        int skinID = (int)playerReferences.photonView.InstantiationData[1];
        SetActiveSkin(skinID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skinID"></param>
    public void SetActiveSkin(int skinID)
    {
        if (skinID < 0 || skinID >= skins.Count)
        {
            Debug.LogWarning($"Skin ID is out of range, the skin ID: {skinID} is either not setup yet in this player prefab or is out of range.");
            return;
        }

        for (int i = 0; i < skins.Count; i++)
        {
            skins[i].SetActive(false);
        }

        skins[skinID].SetActive(true);
    }

    [ContextMenu("Set Default Skin")]
    public void SetDefaultSkin()
    {
        SetActiveSkin(defaultSkin);
    }
}
