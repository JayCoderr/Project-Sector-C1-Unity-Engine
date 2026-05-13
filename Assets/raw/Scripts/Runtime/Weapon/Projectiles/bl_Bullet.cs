using UnityEngine;

/// <summary>
/// This the default bullet handler of MFPS
/// If you want to use your custom bullet script, simply append the bl_ProjectileBase class to it
/// Handle the override functions and attach it to a bullet prefab
/// Follow the Bullet documentation to see how to use a custom bullet prefab.
/// You can use this script as reference to see how to apply damage to players and bots.
/// </summary>
public class bl_Bullet : bl_ProjectileBase
{
    #region Public members
    /// <summary>
    /// If is enabled, on all collisions will check if the hitted object have a IMFPSDamageable component
    /// </summary>
    [LovattoToogle] public bool checkDamageables = false;
    public LayerMask HittableLayers;
    public TrailRenderer Trail = null;
    #endregion

    #region Public properties
    public int ActorViewID { get; set; }
    #endregion

    #region Private members
    private Vector3 dir = Vector3.zero;
    private RaycastHit hit;
    private Transform m_Transform;
    private float distanceSinceLastCheck = 0;
    private float totalTraveledDistance = 0;
    private BulletData bulletData;
    private Vector3 velocity, newVelEuler = Vector3.zero; // bullet velocity
    private Vector3 newPos = Vector3.zero;   // bullet's new position
    private Vector3 oldPos = Vector3.zero;   // bullet's previous location
    private bool hasHit = false;             // has the bullet hit something?
    private Vector3 direction, gravity;               // direction bullet is travelling
    private float bulletRange, timeStep = 0;
    private bool realisticBallistics = false;
    private int fixedFrame = 0;
    private int detectionStep = 1;
    private LayerMask bulletMask;
    private int localLayer = -2;
    private Vector3 lastPoint = Vector3.zero;
    private float dragCoefficient = 0.1f;
    #endregion

    /// <summary>
    /// Initialize the bullet with the given info from the weapon
    /// </summary>
    public override void InitProjectile(BulletData data)
    {
        // Since the bullet is pooled, make sure to reset all the variables to its initial values.
        ResetBullet();

        m_Transform = transform;
        bulletData = data;

        ActorViewID = data.ActorViewID;

        // direction bullet is traveling, add some spread to it by the weapon accuracy
        direction = m_Transform.TransformDirection(data.Inaccuracity);
        lastPoint = m_Transform.position;
        bulletRange = data.Range;
        newPos = m_Transform.position;
        oldPos = newPos;
        velocity = data.Speed * m_Transform.forward; // bullet's velocity determined by direction and bullet speed
        // apply the spread direction
        velocity += direction;
        gravity = bl_MathUtility.Gravity * data.DropFactor;
        dragCoefficient = data.DragCoefficient;

        realisticBallistics = bl_GameData.CoreSettings.realisticBullets;
        detectionStep = bl_GameData.CoreSettings.BulletHitDetectionStep;

        bulletMask = HittableLayers;
        if (data.IsLocalPlayer)
        {
            if (localLayer == -2) localLayer = bl_GameData.TagsAndLayerSettings.GetLocalPlayerLayerIndex();
            bulletMask = HittableLayers & ~(1 << localLayer);
        }

        if (Trail != null)
        {
            if (!bl_GameData.CoreSettings.BulletTracer) { Destroy(Trail); }
            else
            {
                if (IsFlagSet(ProjectileFlags.NoTrail))
                {
                    Trail.Clear();
                    Trail.enabled = false;
                }
                else
                {
                    Trail.enabled = true;
                }
            }
        }

        // schedule for destruction if bullet never hits anything
        Invoke(nameof(Disable), 5);//TTL

        bl_EventHandler.Weapon.onBulletFired?.Invoke(this);
        //bl_BulletDebugger.CreateBullet(this);
    }

    /// <summary>
    /// Set all the bullet properties to their initial values.
    /// </summary>
    void ResetBullet()
    {
        bulletData = null;
        hasHit = false;
        ActorViewID = 0;
        totalTraveledDistance = 0;
        fixedFrame = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (hasHit)
            return; // if bullet has already hit its max hits... exit

        Travel();
    }

    /// <summary>
    /// Determine the ballistic movement
    /// Detect if the bullet hit anything during the travel.
    /// </summary>
    void Travel()
    {
        timeStep = Time.fixedDeltaTime;
        if (!realisticBallistics)
        {
            // Calculate the linear ballistic trajectory
            // No external or gravity forces applied here.
            newPos += velocity * timeStep;
        }
        else
        {
            // Calculate bullet deceleration due to drag
            float drag = Mathf.Exp(-dragCoefficient * timeStep);
            velocity *= drag;

            //Calculate the new velocity and position using Heuns method's
            //y_k+1 = y_k + timeStep * 0.5 * (f(t_k, y_k) + f(t_k+1, y_k+1))
            //Where f(t_k+1, y_k+1) is calculated with Forward Euler: y_k+1 = y_k + timeStep * f(t_k, y_k)
            newVelEuler = velocity + (timeStep * gravity);
            Vector3 newVel = velocity + (timeStep * 0.5f * (gravity + gravity));

            newPos += (timeStep * 0.5f * (velocity + newVelEuler));
            velocity = newVel;
        }

        if (fixedFrame % detectionStep == 0)
        {
            // get the traveled trajectory since last check
            dir = newPos - oldPos;
            distanceSinceLastCheck = dir.magnitude;
            totalTraveledDistance += distanceSinceLastCheck;

            if (distanceSinceLastCheck > 0)
            {
                //bl_BulletDebugger.UpdateBullet(this);

                // Check if we hit anything on the way
                dir /= distanceSinceLastCheck;
                if (Physics.Raycast(oldPos, dir, out hit, distanceSinceLastCheck, bulletMask, QueryTriggerInteraction.Ignore))
                {
                    newPos = hit.point;
                    if (OnHit(hit))
                    {
                        hasHit = true;
                        Disable();
                    }
                }
            }

            if (bulletRange > 0 && totalTraveledDistance > bulletRange)
            {
                Disable();
            }

            oldPos = m_Transform.position;  // set old position to current position
        }

        fixedFrame++;
        m_Transform.position = newPos;  // set current position to the new position
    }

    /// <summary>
    /// 
    /// </summary>
    private void Disable()
    {
        onBeforeDisable?.Invoke();
        ResetBullet();
        gameObject.SetActive(false);
    }

    #region Bullet On Hits

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    private bool OnHit(RaycastHit hit)
    {
        Ray mRay = new(m_Transform.position, m_Transform.forward);
        lastPoint = hit.point;

        switch (hit.collider.tag) // decide what the bullet collided with and what to do with it
        {
            case "IgnoreBullet":
                return false;
            case "Projectile":
                // do nothing if 2 bullets collide
                break;
            case bl_MFPS.HITBOX_TAG://Send Damage for other players
                SendPlayerDamage(hit);
                break;
            case bl_MFPS.AI_TAG:
                //Bullet hit a bot collider, check if we have to apply damage.
                SendBotDamage(hit);
                break;
            case bl_MFPS.LOCAL_PLAYER_TAG:
                //A bot hit a real player
                SendPlayerDamageFromBot(hit);
                break;
            default:
                InstanceHitParticle(hit);

                if (hit.rigidbody != null)
                {
                    float mAdjust = 1.0f / (0.2f / Time.fixedDeltaTime);
                    hit.rigidbody.AddForceAtPosition(mRay.direction * bulletData.ImpactForce / Time.timeScale / mAdjust, hit.point);
                }
                break;
        }

        bl_EventHandler.Weapon.onBulletHit?.Invoke(lastPoint);
        Disable();
        return true;
    }
    #endregion

    /// <summary>
    /// Apply damage to real players from a bot or another real player
    /// </summary>
    void SendPlayerDamage(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;

        if (hit.transform.TryGetComponent<IMFPSDamageable>(out var damageable))
        {
            DamageData damageData = BuildBaseDamageData();
            //check if the bullet comes from a bot or a real player.
            damageData.Cause = bulletData.MFPSActor != null ? bulletData.MFPSActor.isRealPlayer ? DamageCause.Player : DamageCause.Bot : DamageCause.Player;
            damageable.ReceiveDamage(damageData);
        }

        if (bl_GameData.CoreSettings.ShowBlood)
        {
            GameObject go = bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            go.transform.parent = hit.transform;
        }
        Disable();
    }

    /// <summary>
    /// Apply damage to a bot from a Real Player or Another bot
    /// </summary>
    void SendBotDamage(RaycastHit hit)
    {
        //apply damage only if this is the Local Player, we could use only On Master Client to have a more "Authoritative" logic but since the
        //Ping of Master Clients can be volatile since it depend of the client connection, that could affect all the players in the room.
        if (!bulletData.isNetwork)
        {
            if (bulletData.MFPSActor == null)
            {
                Debug.LogWarning($"Bullet ownerID {bulletData.ActorViewID} not found, ignore this if a player just enter in the match.");
            }
            if (hit.transform.root != null && bulletData.MFPSActor.Name == hit.transform.root.name) { return; }

            if (hit.transform.TryGetComponent<IMFPSDamageable>(out var damageable))
            {
                //callback is in bl_HitBox.cs
                var damageData = BuildBaseDamageData();
                damageable.ReceiveDamage(damageData);
            }
        }

        GameObject go = bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
        go.transform.parent = hit.transform;
        Disable();
    }

    /// <summary>
    /// Apply damage to real players FROM bots.
    /// </summary>
    void SendPlayerDamageFromBot(RaycastHit hit)
    {
        if (bulletData.isNetwork) return;
        //Bots doesn't hit the players HitBoxes like other real players does, instead they hit the Character Controller Collider,
        //So instead of communicate with the hit box script we have to communicate with the player health script directly
        var pdm = hit.transform.GetComponent<bl_PlayerReferences>();
        if (pdm != null && bulletData.MFPSActor != null && !bulletData.MFPSActor.isRealPlayer)
        {
            if (!isOneTeamMode)
            {
                if (!bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire && pdm.playerSettings.PlayerTeam == bulletData.MFPSActor.Team)//if hit a team mate player
                {
                    Disable();
                    return;
                }
            }
            DamageData info = BuildBaseDamageData();
            info.Cause = DamageCause.Bot;
            pdm.playerHealthManager.DoDamage(info);
            Disable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceHitParticle(RaycastHit hit, bool overrideDamageable = false)
    {
        // instance decal
        bl_BulletDecalManagerBase.InstantiateDecal(hit);

        if (checkDamageables || overrideDamageable)
        {
            if (!hit.transform.TryGetComponent<IMFPSDamageable>(out var damageable)) return;
            DamageData damageData = BuildBaseDamageData();
            damageData.Cause = DamageCause.Player;
            damageable.ReceiveDamage(damageData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    DamageData BuildBaseDamageData()
    {
        var data = new DamageData()
        {
            Damage = (int)bulletData.GetDamage(bl_UtilityHelper.Distance(bulletData.Position, lastPoint)),
            OriginPosition = bulletData.Position,
            TravelDirection = direction,
            HitPoint = lastPoint,
            MFPSActor = bulletData.MFPSActor,
            ActorViewID = bulletData.ActorViewID,
        };
        data.SetGunID(bulletData.WeaponID);
        if (data.MFPSActor != null) { data.From = data.MFPSActor.Name; }
        return data;
    }
}