using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_KillCam : bl_KillCamBase
{

    #region Public members
    [SerializeField] private GameObject content = null;
    public Transform target = null;
    public float distance = 10.0f;
    public float distanceMax = 15f;
    public float distanceMin = 0.5f;
    public float xSpeed = 120f;
    public float yMaxLimit = 80f;
    public float yMinLimit = -20f;
    public float ySpeed = 120f;
    public LayerMask layers;
    #endregion

    #region Private members
    float x = 0;
    float y = 0;
    private int CurrentTarget = 0;
    private bool canManipulate = false;
    private KillCameraType cameraType = KillCameraType.ObserveDeath;
    private KillCameraType gameModeOverrideType = KillCameraType.None;
    private bool isActive = false;
    private string currentSpectatingName;
    private bool waitingForTarget = false;
    private Transform lookAtTarget;
    private float smoothDistance;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onRemotePlayerDeath += OnRemotePlayerDie;
        bl_EventHandler.onRemoteActorChange += OnRemotePlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_EventHandler.onRemoteActorChange -= OnRemotePlayerSpawn;
        bl_EventHandler.onRemotePlayerDeath -= OnRemotePlayerDie;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        transform.parent = null;
        cameraType = bl_GameData.CoreSettings.killCameraType;
        gameModeOverrideType = bl_MFPS.RoomGameMode.CurrentGameModeData.OverrideKillCamMode;
        smoothDistance = distance;
        if (target != null)
        {
            transform.LookAt(target);
            StartCoroutine(ZoomOut());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!isActive) return;

        Orbit();
        if (lookAtTarget != null) PositionedAndLookAt(lookAtTarget, false);
        if (CurrentMode == KillCameraType.OrbitTarget)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ChangeTarget(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        target = null;
        waitingForTarget = false;
        canManipulate = false;
        bl_KillCamUIBase.Instance?.Hide();
        SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    void OnRemotePlayerDie(bl_EventHandler.PlayerDeathData data)
    {
        if (!isActive) return;

        if (data.Player.Name == currentSpectatingName)
        {
            ChangeTarget(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRemotePlayerSpawn(bl_EventHandler.PlayerChangeData e)
    {
        if (!isActive) return;

        if (e.IsAlive && waitingForTarget)
        {
            SetTarget(e.MFPSActor.Actor, e.PlayerName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeTarget(bool next, bool isTargetOnly = true)
    {
        List<MFPSPlayer> spectateList = GetSpectateList();
        if (spectateList.Count <= 0)
        {
            return;
        }

        if (next) { CurrentTarget = (CurrentTarget + 1) % spectateList.Count; }
        else
        {
            if (CurrentTarget > 0) { CurrentTarget--; } else { CurrentTarget = spectateList.Count - 1; }
        }

        var nextTarget = spectateList[CurrentTarget];
        SetTarget(nextTarget.Actor, nextTarget.Name, isTargetOnly);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_target"></param>
    /// <param name="name"></param>
    private void SetTarget(Transform _target, string name, bool isTargetOnly = true)
    {
        target = _target;
        currentSpectatingName = name;
        waitingForTarget = false;

        if (target == null)
        {
            waitingForTarget = true;
        }
        else
        {
            var ki = new KillCamInfo()
            {
                TargetName = name,
                Target = _target,
                IsTargetChangeOnly = isTargetOnly,
            };
            bl_KillCamUIBase.Instance.Show(ki);
        }
    }

    /// <summary>
    /// update camera movements
    /// </summary>
    void Orbit()
    {
        if (target == null) return;
        if (!canManipulate || CurrentMode != KillCameraType.OrbitTarget)
            return;

        float targetDistance = distance;
        var ray = new Ray(target.position, CachedTransform.position - target.position);
        if (Physics.SphereCast(ray, 0.2f, out var hit, distance, layers))
        {
            targetDistance = bl_MathUtility.Distance(target.position, hit.point) - 0.21f;
        }

        smoothDistance = Mathf.Lerp(smoothDistance, targetDistance, Time.deltaTime * 5);
        x += bl_GameInput.MouseX * this.xSpeed * smoothDistance * 0.02f;
        y -= bl_GameInput.MouseY * this.ySpeed * 0.02f;
        y = bl_MathUtility.ClampAngle(this.y, this.yMinLimit, this.yMaxLimit);
        Quaternion quaternion = Quaternion.Euler(this.y, this.x, 0f);

        Vector3 vector = new(0f, 0f, -smoothDistance);
        Vector3 vector2 = target.position;
        vector2.y = target.position.y + 1f;
        Vector3 vector3 = (quaternion * vector) + vector2;
        CachedTransform.SetPositionAndRotation(vector3, quaternion);
    }

    /// <summary>
    /// Set the kill cam focus/expectate target
    /// </summary>
    /// <param name="targetName"></param>
    public override bl_KillCamBase SetTarget(KillCamInfo info)
    {
        // if the camera type is custom, don't do anything since the custom kill cam will handle it.
        if (CurrentMode == KillCameraType.Custom) { return this; }

        currentSpectatingName = info.TargetName;
        // if the target is already provided
        if (info.Target != null && (CurrentMode == KillCameraType.ObserveDeath || string.IsNullOrEmpty(info.TargetName)))
        {
            if (CanTargetBySpectated(info.Target.gameObject))
            {
                target = info.Target;
                ReadyToShow(info);
                return this;
            }
            else
            {
                // if the provided target can't be spectated (because is an enemy and spectating enemies is not allowed)
                // find a friendly target to spectate
                var friend = GetAFriendInstance();
                if (friend != null)
                {
                    target = friend.Actor;
                    ReadyToShow(info);
                    return this;
                }

                // if no friendly target was found, let the logic below decide what to do but make sure to not spectate the provided target
                info.Target = null;
            }
        }

        bool foundButNotSpectated = false;
        //if the player just send the name of the target
        var targetName = info.TargetName;
        if (string.IsNullOrEmpty(targetName)) return this;

        //try to find it by the name
        var targetInstance = GameObject.Find(targetName);

        // if the target is the local player
        if (targetName == LocalName)
        {
            // and there is not a fallback target, don't spectate anyone
            if (info.FallbackTarget == null) return this;

            targetInstance = info.FallbackTarget.gameObject;
        }

        // if the target instance was not found by its name, try to find it the player info.
        if (targetInstance == null)
        {
            var playerActor = bl_GameManager.Instance.FindActor(targetName);
            if (playerActor != null && playerActor.Actor != null)
            {
                targetInstance = playerActor.Actor.gameObject;
            }
        }

        // if the target instance was found, but it can't be spectated, don't spectate it.
        if (targetInstance != null)
        {
            if (!CanTargetBySpectated(targetInstance))
            {
                targetInstance = null;
                foundButNotSpectated = true;

                var friend = GetAFriendInstance();
                if (friend != null)
                {
                    targetInstance = friend.Actor.gameObject;
                }
            }
        }

        // if the target instance and info was not found, but there is a fallback target, use it.
        if (targetInstance == null && CurrentMode == KillCameraType.OrbitTarget && info.FallbackTarget)
        {
            targetInstance = info.FallbackTarget.gameObject;
        }

        if (targetInstance == null)
        {
            if (!foundButNotSpectated) Debug.LogWarning($"Couldn't found the kill cam target '{targetName}'.");
        }
        else
        {
            target = targetInstance.transform;
            ReadyToShow(info);
        }
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void FindATarget()
    {
        ChangeTarget(true, false);
    }

    /// <summary>
    /// 
    /// </summary>
    private void ReadyToShow(KillCamInfo info)
    {
        canManipulate = true;
        bl_KillCamUIBase.Instance?.Show(info);
        if (CurrentMode == KillCameraType.ObserveDeath && target != null)
        {
            lookAtTarget = info.FallbackTarget != null ? info.FallbackTarget : target;
            PositionedAndLookAt(target);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        content.SetActive(active);
        isActive = active;
        if (!active)
        {
            bl_KillCamUIBase.Instance?.Hide();
            waitingForTarget = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PositionedAndLookAt(Transform reference, bool extrapolate = true)
    {
        float distanceFromLocal = 2.5f;
        Vector3 position = reference.position + (Vector3.up * 2);
        CachedTransform.position = position - (reference.forward * distanceFromLocal);
        if (Physics.Raycast(position, -reference.forward, out RaycastHit rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(rayHit.point, reference.position, 0.05f);
        }

        var up = CachedTransform.position + (Vector3.up * distanceFromLocal);
        if (Physics.Raycast(up, Vector3.down, out rayHit, distanceFromLocal, layers, QueryTriggerInteraction.Ignore))
        {
            CachedTransform.position = Vector3.Lerp(up, rayHit.point, 0.4f);
        }

        if (extrapolate) CachedTransform.LookAt(position + (Vector3.up * 0.25f));
        else
        {
            Vector3 direction = reference.position - CachedTransform.position;
            // Calculate the rotation to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Smoothly rotate towards the target rotation
            CachedTransform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_target"></param>
    /// <returns></returns>
    private bool CanTargetBySpectated(GameObject _target)
    {
        if (_target == null) { return false; }

        var playerRefs = _target.GetComponentInParent<bl_PlayerReferencesCommon>();
        if (playerRefs != null)
        {
            if (!bl_MFPS.RoomGameMode.CurrentGameModeData.AllowSpectateEnemies && !playerRefs.IsTeamMateOfLocalPlayer())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ZoomOut()
    {
        float d = 0;
        Vector3 next = target.position + transform.TransformDirection(new Vector3(0, 0, -3));
        Vector3 origin = target.position;
        transform.position = target.position;
        while (d < 1)
        {
            d += Time.deltaTime * 1.25f;
            transform.position = Vector3.Lerp(origin, next, d);
            transform.LookAt(target);
            yield return null;
        }
        x = CachedTransform.eulerAngles.y;
        y = CachedTransform.eulerAngles.x;
    }

    private KillCameraType CurrentMode
    {
        get
        {
            return gameModeOverrideType != KillCameraType.None ? gameModeOverrideType : cameraType;
        }
    }
}