using UnityEngine;

public abstract class bl_AIAnimationBase : bl_MonoBehaviour
{

    private Animator _animator = null;
    public Animator BotAnimator
    {
        get
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
            return _animator;
        }
        set
        {
            _animator = value;
        }
    }

    /// <summary>
    /// Make the bot player a ragdoll
    /// </summary>
    /// <param name="from"></param>
    /// <param name="isExplosion"></param>
    public abstract void Ragdolled(DamageData damageData);

    /// <summary>
    /// React to the bot being injured
    /// </summary>
    public abstract void OnGetHit();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bodyBone"></param>
    public virtual Transform GetHumanBone(HumanBodyBones bodyBone)
    {
        if (BotAnimator == null) return null;

        return BotAnimator.GetBoneTransform(bodyBone);
    }
}