using UnityEngine;

public abstract class bl_HitboxManagerBase : MonoBehaviour
{
    /// <summary>
    /// Should be called when a hitbox is hit
    /// </summary>
    /// <param name="damageData"></param>
    /// <param name="hitbox"></param>
    public abstract void OnHit(DamageData damageData, bl_HitBoxBase hitbox);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="includeDeactive"></param>
    /// <param name="selfAssign"></param>
    public virtual void FetchAllChildHitBoxes(bool includeDeactive = true, bool selfAssign = true)
    {
    }

    /// <summary>
    /// Get the hitbox of a specific bone
    /// </summary>
    /// <param name="bone"></param>
    /// <param name="hitbox"></param>
    public virtual bl_HitBoxBase GetHitbox(HumanBodyBones bone)
    {
        return null;
    }
}