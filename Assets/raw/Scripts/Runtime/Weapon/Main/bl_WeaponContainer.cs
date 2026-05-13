using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class bl_WeaponContainer : MonoBehaviour
{
    [Serializable]
    public class WeaponData
    {
        [MFPSEditor.ReadOnly] public string Name;
        public bl_WeaponBase Weapon;
        public Vector3 Position;
        public Quaternion Rotation;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetGunID()
        {
            if (Weapon == null) return -1;
            return Weapon.GunID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        public void FetchWeapon(bl_WeaponBase weapon)
        {
            if (weapon == null) return;

            Name = weapon.name;
            Weapon = weapon;
            Position = weapon.transform.localPosition;
            Rotation = weapon.transform.localRotation;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public abstract bl_WeaponBase GetWeapon(int gunID, Transform parent, Avatar avatar = null);

    /// <summary>
    /// The the list of all weapons prefabs in this container
    /// This wont instance the weapons, just return the list of prefabs
    /// </summary>
    /// <returns></returns>
    public virtual List<bl_WeaponBase> GetAllWeaponsPrefabs(bool includeChildContainers = true) { return null; }

    /// <summary>
    /// 
    /// </summary>
    public void SetDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}