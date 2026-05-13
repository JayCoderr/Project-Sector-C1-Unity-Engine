using UnityEngine;

public abstract class bl_AIShooterAttackBase : bl_PhotonHelper
{
    public enum FireReason
    {
        /// <summary>
        /// Only shoot when the target is in sight range
        /// </summary>
        Normal,
        /// <summary>
        /// Shoot random bursts when has a clear shot.
        /// </summary>
        OnMove,
        /// <summary>
        /// Consicutive shoots even if doesn't have a clear shot.
        /// </summary>
        Forced,
        /// <summary>
        /// Shot single bullet when has a clear shot even out of range.
        /// </summary>
        LongRange,
    }

    public enum AttackerRange
    {
        Close,
        Medium,
        Far,
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract bool IsFiring
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CanFire
    {
        get => canFire;
        set => canFire = value;
    }
    private bool canFire = true;

    /// <summary>
    /// 
    /// </summary>
    public abstract void Fire(FireReason fireReason = FireReason.Normal);

    /// <summary>
    /// 
    /// </summary>
    public abstract void OnDeath();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract Vector3 GetFirePosition();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public abstract bl_NetworkGun GetEquippedWeapon();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual AttackerRange GetAttackRange()
    {
        return AttackerRange.Medium;
    }
}