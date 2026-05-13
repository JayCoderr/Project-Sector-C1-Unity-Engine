using UnityEngine;

public abstract class bl_EquippedWeaponUIBase : MonoBehaviour
{

    /// <summary>
    /// Update the ammo information in the local player interface
    /// </summary>
    /// <param name="gun"></param>
    public virtual void SetAmmoOf(bl_Gun gun) { }

    /// <summary>
    /// Update the fire type text for the local player interface.
    /// </summary>
    /// <param name="fireType"></param>
    public virtual void SetFireType(bl_WeaponBase.FireType fireType) { }

    /// <summary>
    /// 
    /// </summary>
    private static bl_EquippedWeaponUIBase _instance;
    public static bl_EquippedWeaponUIBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_EquippedWeaponUIBase>(); }
            if (_instance == null && bl_UIReferences.Instance != null) { _instance = bl_UIReferences.Instance.GetComponentInChildren<bl_EquippedWeaponUIBase>(true); }
            return _instance;
        }
    }
}
