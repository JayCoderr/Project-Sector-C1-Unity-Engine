using Photon.Pun;
using System;
using UnityEngine;

public abstract class bl_PlayerReferencesCommon : MonoBehaviour
{
    // Player events specifically for this player

    /// <summary>
    /// Invoked when this player fire a weapon (local or remote)
    /// </summary>
    public Action<MFPSPlayerFireEventData> onWeaponFire;

    /// <summary>
    /// Called when this player dies
    /// </summary>
    public Action onDie;

    /// <summary>
    /// 
    /// </summary>
    public abstract string PlayerName
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Collider[] AllColliders
    {
        get;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Animator PlayerAnimator
    {
        get;
        set;
    }

    /// <summary>
    /// The reference of this player that the bots will use to aim at.
    /// </summary>
    public abstract bl_AIPlayerTarget BotAimTarget
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract Team PlayerTeam
    {
        get;
    }

    private PhotonView _networkView = null;
    /// <summary>
    /// Player network view
    /// </summary>
    public PhotonView NetworkView
    {
        get
        {
            if (_networkView == null) _networkView = GetComponent<PhotonView>();
            return _networkView;
        }
    }

    /// <summary>
    /// Local position
    /// </summary>
    public Vector3 Position
    {

        get => transform.localPosition;
    }

    /// <summary>
    /// Get a specific hitbox of this player
    /// </summary>
    /// <param name="bone"></param>
    /// <returns></returns>
    public virtual bl_HitBoxBase GetBodyHitbox(HumanBodyBones bone)
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <param name="ignore"></param>
    public abstract void IgnoreColliders(Collider[] list, bool ignore);

    /// <summary>
    /// Determine if this player is death
    /// Since when the player dies it still exists in the game as a ragdoll for a short period of time
    /// </summary>
    /// <returns></returns>
    public abstract bool IsDeath();

    /// <summary>
    /// Get the health of this player
    /// </summary>
    /// <returns></returns>
    public abstract int GetHealth();

    /// <summary>
    /// Get the max health of this player
    /// </summary>
    /// <returns></returns>
    public virtual int GetMaxHealth() { return 100; }

    /// <summary>
    /// Is a local player?
    /// </summary>
    public virtual bool IsLocal()
    {
        return NetworkView.IsMine;
    }

    /// <summary>
    /// Is this player a real player or a bot?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsRealPlayer()
    {
        return true;
    }

    /// <summary>
    /// Is this player in the same team of the local player?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsTeamMateOfLocalPlayer()
    {
        return (bl_RoomSettings.Instance == null || !bl_RoomSettings.Instance.isOneTeamMode) && PlayerTeam == bl_MFPS.LocalPlayer.Team;
    }

    /// <summary>
    /// This should be called when this player is attacked (hit or almost hit)
    /// This is only used for bots so in real players this method do nothing.
    /// </summary>
    public virtual void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
    }

    /// <summary>
    /// Return the parent of the third person model
    /// </summary>
    /// <returns></returns>
    public abstract Transform GetThirdPersonModelParent();
}