using System.Collections;
using UnityEngine;

public class bl_DropCaller : bl_DropCallerBase
{
    public GameObject DestroyEffect;
    private int KitID = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dropData"></param>
    public override void SetUp(DropData dropData)
    {
        KitID = dropData.KitID;
        StartCoroutine(CallProcess(dropData.Delay));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator CallProcess(float delay)
    {
        string uniqueId = $"K{KitID}-{bl_StringUtility.GenerateKey(5)}";
        yield return new WaitForSeconds(delay);
        bl_EventHandler.Gameplay.onAirDropRequested?.Invoke(new bl_EventHandler.Gameplay.AirDropData()
        {
            ItemId = KitID,
            UniqueID = uniqueId,
            ActorId = bl_PhotonNetwork.LocalPlayer.ActorNumber,
            Position = transform.position,
        });

        if (DestroyEffect != null)
        {
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}