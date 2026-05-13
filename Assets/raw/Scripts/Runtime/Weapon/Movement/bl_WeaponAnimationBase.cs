using System;
using UnityEngine;

/// <summary>
/// Base class with the required functions to handle the FPWeapons animations.
/// Inherit this class in your custom weapon animation script.
/// </summary>
public abstract class bl_WeaponAnimationBase : MonoBehaviour
{
    /// <summary>
    /// Play a custom animation
    /// </summary>
    /// <param name="motionName"></param>
    /// <param name="length"></param>
    /// <param name="speed"></param>
    /// <param name="crossfade"></param>
    /// <returns>The estimated time that the animation will take to finish</returns>
    public abstract float PlayAnimation(string motionName, float length, float speed = 1, bool crossfade = false);

    /// <summary>
    /// Play the Fire animation
    /// </summary>
    /// <returns>The time that will take the whole animation.</returns>
    public abstract float PlayFire(AnimationFlags flags = AnimationFlags.None);

    /// <summary>
    /// Play the reload animation
    /// </summary>
    /// <param name="onFinish">Callback to invoke once the reload animation finish.</param>
    public abstract void PlayReload(float reloadDuration, int[] data, AnimationFlags flags = AnimationFlags.None, Action onFinish = null);

    /// <summary>
    /// Play the take in animation
    /// </summary>
    /// <returns>The time that will take the whole animation.</returns>
    public abstract float PlayTakeIn();

    /// <summary>
    /// Play the take out animation
    /// </summary>
    /// <returns>The time that will take the whole animation.</returns>
    public abstract float PlayTakeOut();

    /// <summary>
    /// Handle cancel animations.
    /// </summary>
    /// <param name="animationType"></param>
    public virtual void CancelAnimation(WeaponAnimationType animationType) { }

    /// <summary>
    /// Should return the animation time that takes play the whole sequence.
    /// </summary>
    /// <returns></returns>
    public abstract float GetAnimationDuration(WeaponAnimationType animationType, float[] data = null);

    [Flags]
    public enum AnimationFlags
    {
        None = 0,
        Aiming = 1,
        QuickFire = 2,
        SplitReload = 4,
        EmptyReload = 8,
        Cook = 16,
        BashAttack = 32,
    }

    public enum WeaponAnimationType : short
    {
        TakeIn,
        TakeOut,
        Reload,
        Fire,
        AimFire,
        Bash,
    }
}