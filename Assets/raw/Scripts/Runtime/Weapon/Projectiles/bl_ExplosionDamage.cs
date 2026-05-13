using MFPS.Audio;
using MFPS.Core.Motion;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class bl_ExplosionDamage : bl_ExplosionBase
{
    #region Public members
    [FormerlySerializedAs("m_Type")]
    public ExplosionType explosionType = ExplosionType.Normal;
    public float explosionDamage = 50f;
    public float explosionRadius = 50f;
    [Tooltip("Time to destroy this object after instantiate.\n0 = Not to auto destroy.")]
    public float DisappearIn = 3f;
    [Tooltip("Layers of colliders to detect to apply damage.")]
    public LayerMask detectLayers;
    public ShakerPresent shakerPresent;
    public string shakerKey = "explosion";
    #endregion

    #region Private members
    private BulletData cachedData;
    private MFPSPlayer creator;
    private float effectiveRadius = 3;
    private readonly Vector3[] offsets = new Vector3[] { Vector3.zero, new(0, 0.6f, 0), new(0, 0.15f, 0) };
    private Vector3 framePosition;
    #endregion

    /// <summary>
    /// is not remote take damage
    /// </summary>
    void Start()
    {
        if (cachedData == null)
        {
            if (explosionType == ExplosionType.Level)
            {
                cachedData = new BulletData()
                {
                    MFPSActor = bl_GameManager.Instance.LocalActor,
                    isNetwork = false,
                    Damage = explosionDamage,
                    Position = transform.position,

                };
                creator = bl_MFPS.LocalPlayer.MFPSActor;
            }
            else
            {
                Debug.LogWarning("Explosion has not been initialized.");
                return;
            }
        }

        if (!cachedData.isNetwork)
        {
            DoDamage();
            ApplyShake();
        }

        if (DisappearIn > 0) Destroy(gameObject, DisappearIn);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void InitExplosion(BulletData bulletData, MFPSPlayer fromPlayer)
    {
        cachedData = bulletData;
        creator = fromPlayer;
        if (cachedData.Damage > 0) explosionDamage = cachedData.Damage;
        explosionRadius = cachedData.Range;
        effectiveRadius = cachedData.EffectiveFiringRange;

        SetupAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    public override void SetRadius(float radius)
    {
        explosionRadius = radius;
    }

    /// <summary>
    /// applying impact damage from the explosion to enemies
    /// </summary>
    private void DoDamage()
    {
        if (explosionType == ExplosionType.Shake)
            return;

        framePosition = transform.position;
        DoPlayersDamage();
        DoCollisionDamage();
    }

    /// <summary>
    /// Apply damage to the real players
    /// The splash calculation for players is due by distance instead of detect colliders
    /// due to its simpler and performs better.
    /// </summary>
    void DoPlayersDamage()
    {
        HashSet<Player> playersInRange = this.GetPlayersInRange();

        if (playersInRange == null || playersInRange.Count <= 0) return;

        foreach (Player player in playersInRange)
        {
            if (player == null) continue;

            GameObject p = FindPhotonPlayer(player);
            if (p == null) continue;

            var pt = p.transform;
            Vector3 pp = pt.position + Vector3.up;

            //check if there is an obstacle between player and explosion
            if (!ExplosionCanHitTarget(pt, offsets, pp)) continue;

            var pdm = p.transform.GetComponentInParent<bl_PlayerHealthManagerBase>();

            var odi = new DamageData
            {
                Damage = CalculatePlayerDamage(p.transform, player),
                OriginPosition = transform.position,
                From = creator.Name,
                IsHeadShot = false,
                Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion
            };
            odi.SetGunID(cachedData.WeaponID);

            pdm?.DoDamage(odi);
        }
    }

    /// <summary>
    /// Apply damage to objects, items, bots, etc... if the collider is inside the explosion radius
    /// </summary>
    void DoCollisionDamage()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, explosionRadius, detectLayers, QueryTriggerInteraction.Ignore);
        var Hited = new List<string>();

        var damageData = new DamageData()
        {
            OriginPosition = transform.position,
            MFPSActor = creator,
            ActorViewID = creator.ActorViewID,
            From = creator.Name,
        };
        damageData.SetGunID(cachedData.WeaponID);
        damageData.Cause = (!creator.isRealPlayer) ? DamageCause.Bot : DamageCause.Explosion;

        foreach (Collider c in colls)
        {
            //the damage to real players is handled separately
            if (c.isLocalPlayerCollider() || c.CompareTag("Untagged")) continue;

            if (!c.transform.TryGetComponent<IMFPSDamageable>(out var damageable))
            {
                if (c.attachedRigidbody != null)
                {
                    if (!c.attachedRigidbody.TryGetComponent(out damageable)) continue;
                }
                else continue;
            }

            // if the collider is a hitbox or an AI, we must check if the root is already hit
            if (c.CompareTag(bl_MFPS.HITBOX_TAG) || c.CompareTag(bl_MFPS.AI_TAG))
            {
                if (Hited.Contains(c.transform.root.name)) continue;
                if (!ExplosionCanHitTarget(c.transform.root, offsets)) continue;

                Hited.Add(c.transform.root.name);
            }
            else
            {
                if (c.attachedRigidbody != null)
                {
                    if (Hited.Contains(c.attachedRigidbody.name)) continue;
                }
                if (!ExplosionCanHitTarget(c.transform, offsets)) continue;
                if (c.attachedRigidbody != null)
                {
                    Hited.Add(c.attachedRigidbody.name);
                }
            }

            // if instead of calculate the damage to the hit collider you want to calculated to the root of the collider pass 'c.attachedRigidbody' instead of 'c.transform'
            int damage = CalculatePlayerDamage(c.transform, null);
            if (damage < 1) continue;

            damageData.Damage = (int)damage;

            if (damageData.MFPSActor == null)
            {
                Debug.Log($"Explosion actor '{creator.ActorViewID}' was not found in the scene, maybe left the match?");
                return;
            }
            damageable.ReceiveDamage(damageData);
            // Debug.Log($"Explosion hit {c.name} ({c.transform.root.name}) with {damage} damage, distance {bl_UtilityHelper.Distance(transform.position, c.transform.position):0.00}/{explosionRadius}");
        }
    }

    /// <summary>
    /// When Explosion is local, and take player hit
    /// Send only shake movement
    /// </summary>
    void ApplyShake()
    {
        float influence = GetLocalInfluence();
        if (influence > 0 && shakerPresent != null)
        {
            bl_EventHandler.DoPlayerCameraShake(shakerPresent, "shakerKey", influence);
        }
    }

    /// <summary>
    /// calculate the damage it generates, based on the distance
    /// between the player and the explosion
    /// </summary>
    private int CalculatePlayerDamage(Transform trans, Player p)
    {
        if (p != null)
        {
            if (!isOneTeamMode)
            {
                if (bl_GameData.CoreSettings.SelfGrenadeDamage && p == bl_PhotonNetwork.LocalPlayer)
                {
                    // if the self damage is applied differently, calculate it here
                }
                else
                {
                    if (p.IsTeamMate())
                    {
                        return 0;
                    }
                }
            }
        }

        float distance = bl_UtilityHelper.Distance(framePosition, trans.position);
        if (distance < effectiveRadius) return (int)explosionDamage;

        // if the player is outside the effective radius, the damage will be reduced based on the distance of the explosion
        return Mathf.Clamp((int)(explosionDamage * ((explosionRadius - distance) / explosionRadius)), 0, (int)explosionDamage);
    }

    /// <summary>
    /// Do a simple check to see if there's anything between the explosion and the collider target
    /// if that is the case, the explosion should not make any effect to the target.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="offset"></param>
    /// <param name="targetPosition"></param>
    /// <param name="rootOnly"></param>
    /// <returns></returns>
    private bool ExplosionCanHitTarget(Transform target, Vector3[] offset, Vector3 targetPosition = default)
    {
        if (targetPosition == default) targetPosition = target.position;

        Vector3 expOrigin = transform.position + (Vector3.up * 0.1f);
        for (int i = 0; i < offset.Length; i++)
        {
            Vector3 targetOffset = targetPosition + offset[i];
            // NOTE: for this to work correctly, the explosion obstacles layers must not include the player layers
            if (!Physics.Linecast(expOrigin, targetOffset, out RaycastHit hit, bl_GameData.TagsAndLayerSettings.ExplosionObstacles, QueryTriggerInteraction.Ignore))
            {
                // if there is no obstacles between the explosion and the target, return true = can hit
                return true;
            }
            if (hit.transform == target || hit.transform.IsChildOf(target))
            {
                // if the raycast hit the target, return true = can hit
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// get players who are within the range of the explosion
    /// </summary>
    /// <returns></returns>
    private HashSet<Player> GetPlayersInRange()
    {
        var list = new HashSet<Player>();
        Vector3 position = transform.position;
        foreach (var p in bl_PhotonNetwork.PlayerList)
        {
            GameObject player = FindPhotonPlayer(p);
            if (player == null)
                return null;

            float distance = bl_UtilityHelper.Distance(position, player.transform.position);
            if (!isOneTeamMode)
            {
                if (!creator.isRealPlayer)
                {
                    if (p.GetPlayerTeam() != creator.Team && (distance <= explosionRadius))
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (p != bl_PhotonNetwork.LocalPlayer)
                    {
                        if (p.GetPlayerTeam() != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && (distance <= explosionRadius))
                        {
                            list.Add(p);
                        }
                    }
                    else
                    {
                        if (bl_GameData.CoreSettings.SelfGrenadeDamage)
                        {
                            if (distance <= explosionRadius)
                            {
                                list.Add(p);
                            }
                        }
                    }
                }
            }
            else
            {
                if (p != bl_PhotonNetwork.LocalPlayer)
                {
                    if (distance <= explosionRadius)
                    {
                        list.Add(p);
                    }
                }
                else
                {
                    if (bl_GameData.CoreSettings.SelfGrenadeDamage)
                    {
                        if (distance <= explosionRadius)
                        {
                            list.Add(p);
                        }
                    }
                }
            }
        }
        return list;
    }

    /// <summary>
    /// Calculate if player local in explosion radius
    /// </summary>
    /// <returns></returns>
    private float GetLocalInfluence()
    {
        if (bl_MFPS.LocalPlayerReferences == null) return 0;

        Transform p = bl_MFPS.LocalPlayerReferences.transform;

        return bl_MathUtility.ExplosionInfluence(transform.position, p.position, explosionRadius * 0.5f, explosionRadius * 2, 1.5f);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupAudio()
    {
        if (!TryGetComponent<AudioSource>(out var Source) || bl_AudioController.Instance == null) return;

        Source.spatialBlend = 1;
        Source.maxDistance = bl_AudioController.Instance.maxExplosionDistance;
        Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        Source.minDistance = bl_AudioController.Instance.maxExplosionDistance * 0.09f;
        Source.spatialize = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectiveRadius);
    }

    [System.Serializable]
    public enum ExplosionType
    {
        Normal,
        Shake,
        Level
    }
}