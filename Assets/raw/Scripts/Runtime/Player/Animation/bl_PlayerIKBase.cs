using System;
using UnityEngine;

public abstract class bl_PlayerIKBase : bl_MonoBehaviour
{
    /// <summary>
    /// When this is true, the IK will be enabled.
    /// </summary>
    public bool EnableIK
    {
        get;
        set;
    } = true;

    /// <summary>
    /// When this is true, you shouldn't control the arms with IK.
    /// </summary>
    public bool ControlArmsWithIK
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Return the player IK head look transform reference.
    /// </summary>
    /// <returns></returns>
    public abstract Transform HeadLookTarget
    {
        get;
        set;
    }

    /// <summary>
    /// When CustomArmsIKHandler is != null
    /// You should stop controlling the Arms with IK in your inherited script.
    /// </summary>
    public bl_BodyIKHandler CustomArmsIKHandler
    {
        get;
        set;
    } = null;

    public Action<int> onAnimatorIK;

    /// <summary>
    /// Invoked when an animation event calls the <see cref="OnAnimationEvent(string)"/> function."/>
    /// </summary>
    public Action<string> onAnimationEvent;

    /// <summary>
    /// Initialize the IK Solver
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// Use this function to receive animation events from the animator.
    /// </summary>
    /// <param name="eventName"></param>
    public virtual void OnAnimationEvent(string eventName)
    {
        onAnimationEvent?.Invoke(eventName);
    }

    /// <summary>
    /// Sets the target that the head should look at.
    /// </summary>
    /// <param name="target">The <see cref="Transform"/> representing the new look target. Can be <see langword="null"/> to clear the current
    /// target.</param>
    public virtual void SetHeadLookTarget(Transform target)
    {
    }
}