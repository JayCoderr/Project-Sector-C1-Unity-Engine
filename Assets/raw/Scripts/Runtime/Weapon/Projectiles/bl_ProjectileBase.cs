using System;
using UnityEngine;

/// <summary>
/// Base class for all the weapon projectiles (bullets, grenades, etc...)
/// </summary>
public abstract class bl_ProjectileBase : bl_MonoBehaviour
{
    private ProjectileFlags Flags = ProjectileFlags.None;
    public Action<string, object[]> onReceiveCustomCommand;

    /// <summary>
    /// Called just before disable the projectile
    /// </summary>
    public Action onBeforeDisable;

    protected bool hasUniqueID = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (hasUniqueID && bl_ItemManagerBase.Instance != null)
        {
            bl_ItemManagerBase.Instance.UnregisterGeneric(gameObject.name);
        }
    }

    /// <summary>
    /// Initialize the projectile
    /// </summary>
    public abstract void InitProjectile(BulletData data);

    /// <summary>
    /// Detonate the projectile
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="callFromLocal"></param>
    public virtual void Detonate(Vector3 position, Quaternion rotation, BulletData bulletData, bool callFromLocal = true)
    {
        Debug.LogWarning("Method not implemented.");
    }

    /// <summary>
    /// Destroys the projectile instance and removes it from the scene.
    /// </summary>
    /// <param name="callFromLocal">Indicates whether the destruction is initiated locally. If <see langword="true"/>, the method is called from the
    /// local context; otherwise, it may be triggered remotely or by another system.</param>
    public virtual void DestroyProjectile(bool callFromLocal = true)
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Sends a command and its associated parameters over the network for synchronization with other clients.
    /// </summary>
    /// <remarks>This method requires the projectile to have a unique ID; otherwise, the command will not be
    /// sent. Use this method to synchronize custom actions or state changes for the projectile across the
    /// network.</remarks>
    /// <param name="commandId">The identifier of the command to be sent. This value determines the type of action to synchronize across the
    /// network.</param>
    /// <param name="parameters">An array of parameters to include with the command. The contents of this array are transmitted to other clients
    /// and should match the expected format for the specified command.</param>
    public void SetCommandOverNetwork(string commandId, object[] parameters)
    {
        if (!hasUniqueID)
        {
            Debug.LogWarning($"The projectile {gameObject.name} does not have a unique ID. Cannot send command over network.");
            return;
        }

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("c", (byte)2);
        data.Add((byte)0, gameObject.name);
        data.Add((byte)1, commandId);
        data.Add((byte)2, parameters);

        bl_PhotonNetwork.SendNetworkEvent(PropertiesKeys.EventItemSync, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="parameters"></param>
    public virtual void OnReceiveCustomCommand(string commandId, object[] parameters)
    {
        onReceiveCustomCommand?.Invoke(commandId, parameters);
    }

    /// <summary>
    /// Set the unique id for this projectile
    /// </summary>
    /// <param name="id"></param>
    public virtual void SetUniqueID(string id)
    {
        gameObject.name = $"{gameObject.name.Replace("(Clone)", "")} [{id}]";
        hasUniqueID = true;
        if (bl_ItemManagerBase.Instance != null)
        {
            bl_ItemManagerBase.Instance.RegisterGeneric(gameObject.name, gameObject);
        }
    }

    /// <summary>
    /// Add a flag to the projectile
    /// </summary>
    /// <param name="flag"></param>
    public void SetFlag(ProjectileFlags flag)
    {
        Flags |= flag;
    }

    /// <summary>
    /// is the flag set
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    public bool IsFlagSet(ProjectileFlags flag)
    {
        return (Flags & flag) == flag;
    }

    /// <summary>
    /// Determines whether the current object was created by the local player.
    /// </summary>
    /// <returns>true if the object was created by the local player; otherwise, false.</returns>
    public virtual bool IsCreatedByLocalPlayer()
    {
        return true;
    }

    /// <summary>
    /// Gets the team associated with the projectile.
    /// </summary>
    /// <returns>A <see cref="Team"/> value representing the projectile's creator team. Returns <see cref="Team.None"/> if the projectile
    /// is not assigned to any team.</returns>
    public virtual Team GetProjectileTeam()
    {
        return Team.None;
    }
}