using System;
using UnityEngine;

public abstract class bl_PlayerAnimationsBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    [SerializeField] private Animator m_animator = null;
    public Animator Animator
    {
        get => m_animator;
        set => m_animator = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public PlayerState BodyState
    {
        get;
        set;
    } = PlayerState.Idle;

    /// <summary>
    /// 
    /// </summary>
    public PlayerFPState FPState
    {
        get;
        set;
    } = PlayerFPState.Idle;

    /// <summary>
    /// Is this player touching the ground?
    /// This value should be provided by bl_PhotonNetwork.cs
    /// </summary>
    public bool IsGrounded
    {
        get;
        set;
    }

    /// <summary>
    /// The velocity of this player
    /// </summary>
    public Vector3 Velocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// The local velocity of this player
    /// </summary>
    public Vector3 LocalVelocity
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// Invoked when a custom command is executed
    /// </summary>
    public Action<PlayerAnimationCommands, string> OnCustomCommand;

    /// <summary>
    /// Since the script could be disabled at start, we call this from an external script
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Called when the player has changed of weapon
    /// </summary>
    public abstract void SetNetworkGun(GunType weaponType, bl_NetworkGun networkGun);

    /// <summary>
    /// Play the fire animation for a specific gun type.
    /// </summary>
    /// <param name="gunType"></param>
    public abstract void PlayFireAnimation(GunType gunType, FireReplicationFlag flag = FireReplicationFlag.None);

    /// <summary>
    /// Execute a custom command and make sure to sync over network.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="callFromLocal"></param>
    public abstract void CustomCommand(PlayerAnimationCommands command, string arg = "", bool callFromLocal = true);

    /// <summary>
    /// Update the player animator parameters.
    /// </summary>
    public abstract void UpdateAnimatorParameters();

    /// <summary>
    /// Called when the player get hit by an enemy.
    /// </summary>
    public abstract void OnGetHit();

    /// <summary>
    /// Block / Unequipped the weapons
    /// </summary>
    public abstract void BlockWeapons(int blockType);

    /// <summary>
    /// Rotate the player model to match the given yaw angle.
    /// </summary>
    /// <param name="y"></param>
    public virtual void ApplyModelYaw(float y, bool force = false)
    {
        if (BodyState == PlayerState.InVehicle || BodyState == PlayerState.Dropping) return;
        if (!force && BodyState != PlayerState.Idle)
        {
            CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.identity, Time.deltaTime * 16);
            return;
        }

        var e = CachedTransform.eulerAngles;
        e.y = y;
        CachedTransform.rotation = Quaternion.Slerp(CachedTransform.rotation, Quaternion.Euler(e), Time.deltaTime * 16);
    }

    public virtual void HandleRemoteTurnIndex(byte newIdx) { }

    public virtual byte GetTurnStepIndex() { return 0; }

    /// <summary>
    /// Manually update the turn in place from the first person view
    /// Since the code that handle the turn in place is located in the player animation script
    /// and that script is only enabled when the local player is in third person view
    /// we need to manually update the turn in place when in first person view from an external script,
    /// this function works as the entry point for that.
    /// </summary>
    public virtual void UpdateTIPFromFPV() { }

    /// <summary>
    /// Pause/Resume the turn in place rotation, this only has effect if the TIP is enabled in GameData.
    /// </summary>
    /// <param name="value"></param>
    public virtual void PauseTurnInPlace(bool value) { }
}