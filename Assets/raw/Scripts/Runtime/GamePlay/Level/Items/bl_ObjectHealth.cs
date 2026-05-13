using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach this script to any object that you want to have health and receive damage.
/// To sync over network, this object must have a PhotonView component.
/// </summary>
public class bl_ObjectHealth : bl_HitboxManagerBase
{
    public int Health = 100;
    public int ScoreForDestroy = 100;
    [LovattoToogle] public bool syncOverNetwork = true;
    [LovattoToogle] public bool DestroyOnDeath = true;
    public Team team = Team.None;
    public GameObject DestroyEffect;

    public bl_EventHandler.UEvent onDestroy;
    public UEventInt onDamage;

    private bool isDead = false;
    private PhotonView netView;
    private int maxHealth = -1;
    private int lastActor = -1;
    [Serializable] public class UEventInt : UnityEvent<int> { }

    public enum EventType
    {
        Damage,
        HealthSync,
    }

    private void OnEnable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnterInRoom;
    }

    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnterInRoom;
    }

    /// <summary>
    /// This is called locally when hit in a hitbox
    /// </summary>
    /// <param name="damageData"></param>
    /// <param name="hitbox"></param>
    public override void OnHit(DamageData damageData, bl_HitBoxBase hitbox)
    {
        if (isDead || team == bl_MFPS.LocalPlayer.Team) return; // do not apply damage if is dead or is the same team

        if (syncOverNetwork)
        {
            SendEvent(EventType.Damage, damageData.Damage, damageData.ActorViewID);
        }
        else
        {
            ApplyDamage(damageData.Damage);
        }
    }

    /// <summary>
    /// Apply damage to this object locally
    /// </summary>
    /// <param name="damage"></param>
    public void ApplyDamage(int damage)
    {
        if (isDead) return;

        if (maxHealth == -1) { maxHealth = Health; }

        Health -= damage;
        Health = Mathf.Clamp(Health, 0, maxHealth);

        if (LastActorIsLocalPlayer())
        {
            if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnHit(Health <= 0);
        }

        onDamage?.Invoke(Health);
        if (Health <= 0)
        {
            Destroy();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Destroy()
    {
        isDead = true;

        if (LastActorIsLocalPlayer())
        {
            if (ScoreForDestroy > 0)
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(ScoreForDestroy);
            }
        }

        if (DestroyEffect != null)
        {
            Instantiate(DestroyEffect, transform.position, Quaternion.identity);
        }

        onDestroy?.Invoke();

        if (DestroyOnDeath)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void ApplyHealth(int value)
    {
        Health += value;
        if (Health > maxHealth) { Health = maxHealth; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="value"></param>
    public void SendEvent(EventType eventType, int value, int actorViewId, bool ownerOnly = false)
    {
        if (syncOverNetwork)
        {
            netView = netView ?? GetComponent<PhotonView>();
            if (netView == null) return;
            if (ownerOnly && !netView.IsMine) return;

            netView.RPC(nameof(ObjectHealthEvent), RpcTarget.All, eventType, value, actorViewId);
        }
        else
        {
            ObjectHealthEvent(eventType, value);
        }
    }

    [PunRPC]
    public void ObjectHealthEvent(EventType eventType, int value, int fromActor = -1)
    {
        lastActor = fromActor;
        switch (eventType)
        {
            case EventType.Damage:
                ApplyDamage(value);
                break;
            case EventType.HealthSync:
                if (maxHealth == -1) { maxHealth = Health; }
                Health = value;
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    private void OnPlayerEnterInRoom(Player player)
    {
        // sync we do not buffer the health value, we sync it when a new player enter in room
        if (syncOverNetwork && bl_PhotonNetwork.IsMasterClient)
        {
            SendEvent(EventType.HealthSync, Health, -1);
        }
    }

    public bool LastActorIsLocalPlayer()
    {
        return lastActor == -1 || lastActor == bl_MFPS.LocalPlayer.ViewID;
    }
}