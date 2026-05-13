using UnityEngine;

public class bl_MapMods : MonoBehaviour
{
    [LovattoToogle] public bool infinityAmmo = false;
    [LovattoToogle] public bool disableProtectionTime = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (disableProtectionTime)
        {
            bl_Hook.AddFilter("SpawnProtectedTime", input =>
            {
                return 0;
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.Player.onLocalWeaponLoadoutReady += OnLocalWeaponLoadoutReady;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.Player.onLocalWeaponLoadoutReady -= OnLocalWeaponLoadoutReady;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalWeaponLoadoutReady()
    {
        var p = bl_MFPS.LocalPlayerReferences;
        //if (p == null) return;

        if (infinityAmmo) p.gunManager.SetInfinityAmmoToAllEquippeds(true);
    }
}