using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class bl_Projectile : bl_ProjectileBase
{
    #region Public members
    [FormerlySerializedAs("explosionMethod")]
    public ExplosionMethod triggerMethod = ExplosionMethod.Timer;
    [FormerlySerializedAs("m_Type")]
    public ProjectileType detonateMethod = ProjectileType.Explosion;
    [FormerlySerializedAs("TimeToExploit")]
    public float timeToDetonate = 10;
    [Tooltip("After detonation, the time the shell will remain visible before being destroyed, set -1 to never destroy it.")]
    public float destroyAfterDetonate = 0;
    [LovattoToogle] public bool syncDetonation = false;
    [LovattoToogle] public bool stopPhysicsAfterDetonate = false;
    [Tooltip("Link the detonation object position to this projectile shell position?")]
    [LovattoToogle] public bool linkDetonationToProjectile = false;
    [Tooltip("If true, the explosion instance will be attached to the collider that trigger the explosion.")]
    [LovattoToogle] public bool attachExplosionToTarget = false;
    [LovattoToogle] public bool ignoreCollidersOnAttach = true;
    [Tooltip("The layer mask to use when checking for colliders to attach the explosion instance.")]
    public LayerMask layerMask = ~0;
    public GameObject explosion;   // instanced explosion 
    public TrailRenderer trailRenderer;
    public Vector3 stickFaceDirection = Vector3.forward;
    [Tooltip("Offset from the surface when stick")]
    public float surfaceOffset = 0f;
    [Tooltip("Objects to destach from this projectile and instanced when explode.")]
    public GameObject[] destachOnDetonate; // objects to destach from this projectile and instanced on explosion
    #endregion

    #region Public properties
    public int ID { get; set; }
    public bool IsNetwork { get; set; } = false;
    #endregion

    #region Private members
    private BulletData m_bulletData;
    private bool canDetectCollision = true;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public override void InitProjectile(BulletData data)
    {
        m_bulletData = data;
        ID = data.WeaponID;
        IsNetwork = data.isNetwork;

        if (triggerMethod == ExplosionMethod.Timer || triggerMethod == ExplosionMethod.CollisionAndTimer)
        {
            this.InvokeAfter(timeToDetonate, () =>
            {
                Detonate(transform.position, Quaternion.identity, m_bulletData, !IsNetwork);
            });
        }

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
            if (bl_GameData.CoreSettings.showProjectilesTrails)
            {
                this.InvokeAfter(0.2f, () =>
                {
                    trailRenderer.enabled = true;
                });
            }
        }

        var nadeData = new bl_EventHandler.Weapon.NadeThrowData()
        {
            ProjectileTransform = transform,
            ActorID = m_bulletData.ActorViewID,
            GunID = m_bulletData.WeaponID,
            IsRemote = m_bulletData.isNetwork,
            FromTeam = m_bulletData.MFPSActor == null ? Team.None : m_bulletData.MFPSActor.Team,
        };
        bl_EventHandler.Weapon.onNadeThrow?.Invoke(nadeData);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCollisionEnter(Collision enterObject)
    {
        if (!canDetectCollision) return;
        if (detonateMethod == ProjectileType.Explosion || detonateMethod == ProjectileType.Splash)
        {
            ProccessCollision(enterObject);
        }
        else if (detonateMethod == ProjectileType.Stick)
        {
            ProcessStickCollision(enterObject);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enterObject"></param>
    private void ProccessCollision(Collision enterObject)
    {
        if (triggerMethod != ExplosionMethod.Collision && triggerMethod != ExplosionMethod.CollisionAndTimer) return;
        // if the projectile is from a local player and hit the local player, ignore it
        if (m_bulletData.IsLocalPlayer && enterObject.transform.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG)) { return; }

        switch (enterObject.transform.tag)
        {
            case "Projectile":
                break;
            default:

                ContactPoint contact = enterObject.contacts[0];
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, contact.normal);
                Detonate(contact.point, rotation, m_bulletData, !IsNetwork);

                if (enterObject.rigidbody)
                {
                    enterObject.rigidbody.AddForce(CachedTransform.forward * m_bulletData.ImpactForce, ForceMode.Impulse);
                }
                canDetectCollision = false;
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collision"></param>
    private void ProcessStickCollision(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(stickFaceDirection, contact.normal);

        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.mass = 0.001f;
            if (ignoreCollidersOnAttach) rb.excludeLayers = ~0;
        }

        Vector3 offsetPos = contact.point + (contact.normal * surfaceOffset);
        transform.SetPositionAndRotation(offsetPos, rotation);
        transform.parent = collision.transform;
        canDetectCollision = false;
    }

    public override void DestroyProjectile(bool callFromLocal = true)
    {
        if (callFromLocal)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("c", (byte)1);
            data.Add((byte)0, gameObject.name);

            bl_PhotonNetwork.SendNetworkEvent(PropertiesKeys.EventItemSync, data);
            return;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Detonate(Vector3 position, Quaternion rotation, BulletData bulletData, bool callFromLocal = true)
    {
        if (bulletData == null && m_bulletData != null)
        {
            bulletData = m_bulletData;
        }

        if (syncDetonation && callFromLocal)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("c", (byte)0);
            data.Add((byte)0, gameObject.name);
            data.Add((byte)1, position);
            data.Add((byte)2, rotation);
            data.Add((byte)3, bulletData);

            bl_PhotonNetwork.SendNetworkEvent(PropertiesKeys.EventItemSync, data);
            return;
        }

        GameObject e = Instantiate(explosion, position, rotation) as GameObject;
        if (detonateMethod == ProjectileType.Explosion || detonateMethod == ProjectileType.Stick)
        {
            if (e.TryGetComponent<bl_ExplosionBase>(out var blast))
            {
                var actor = bulletData.MFPSActor;
                if (actor != null && actor.ActorView != null)
                {
                    blast.InitExplosion(bulletData, actor);
                }
            }
        }
        else if (detonateMethod == ProjectileType.Splash)
        {
            var da = e.GetComponent<bl_DamageArea>();
            if (bulletData.MFPSActor != null)
            {
                da.SetInfo(bulletData, IsNetwork);
            }
        }

        if (attachExplosionToTarget)
        {
            Collider[] touchingColliders = new Collider[3];
            // check with an sphere cast if there is a collider in the explosion radius
            if (Physics.OverlapSphereNonAlloc(CachedTransform.position, 1, touchingColliders, layerMask, QueryTriggerInteraction.Ignore) > 0)
            {
                // sort the colliders by distance to the explosion point
                Array.Sort(touchingColliders, CompareByDistance);

                e.transform.SetParent(touchingColliders[0].transform);
            }
        }

        if (stopPhysicsAfterDetonate)
        {
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
            }
        }

        if (linkDetonationToProjectile)
        {
            PositionConstraint pc = e.AddComponent<PositionConstraint>();
            pc.constraintActive = true;
            pc.translationOffset = Vector3.zero;
            pc.AddSource(new ConstraintSource()
            {
                sourceTransform = transform,
                weight = 1
            });
        }

        if (destachOnDetonate.Length > 0)
        {
            foreach (GameObject g in destachOnDetonate)
            {
                if (g == null) { continue; }
                g.transform.parent = null;
            }
        }

        if (destroyAfterDetonate == -1)
        {
            // manual destroy
        }
        else if (destroyAfterDetonate == 0) Destroy(gameObject);
        else Destroy(gameObject, destroyAfterDetonate);
    }

    public override bool IsCreatedByLocalPlayer()
    {
        return m_bulletData == null || m_bulletData.IsLocalPlayer;
    }

    public override Team GetProjectileTeam()
    {
        if (m_bulletData == null) return Team.None;
        return m_bulletData.MFPSActor == null ? Team.None : m_bulletData.MFPSActor.Team;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int CompareByDistance(Collider a, Collider b)
    {
        if (a == null || b == null) return 0;

        float distA = bl_UtilityHelper.SquaredDistance(CachedTransform.position, a.transform.position);
        float distB = bl_UtilityHelper.SquaredDistance(CachedTransform.position, b.transform.position);

        return distA.CompareTo(distB);
    }

    [System.Serializable]
    public enum ProjectileType
    {
        Explosion,
        Splash,
        Stick
    }

    [System.Serializable]
    public enum ExplosionMethod
    {
        Timer,
        Collision,
        CollisionAndTimer,
        Manual
    }
}