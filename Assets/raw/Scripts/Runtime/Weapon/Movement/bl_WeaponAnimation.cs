using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MFPS default script to handle the FPWeapon animations.
/// You can use your own script to handle the animations if you inherit your script from bl_WeaponAnimationBase.cs
/// </summary>
public class bl_WeaponAnimation : bl_WeaponAnimationBase
{
    #region Public members
    public AnimationType m_AnimationType = AnimationType.Animation;
    public FireBlendMethod fireBlendMethod = FireBlendMethod.FireSpeed;
    public AnimationClip DrawName;
    public AnimationClip TakeOut;
    public AnimationClip SoloFireClip;
    public AnimationClip[] FireAnimations;
    public AnimationClip FireAimAnimation;
    public AnimationClip[] aimFireAnimations;
    public AnimationClip ReloadName;
    public AnimationClip emptyReloadClip;
    public AnimationClip IdleClip;
    [Range(0.1f, 5)] public float FireSpeed = 1.0f;
    [Range(0.1f, 5)] public float DrawSpeed = 1.0f;
    [Range(0.1f, 5)] public float HideSpeed = 1.0f;
    public float crossfadeLenght = 0.025f;
    public AnimationClip StartReloadAnim;
    public AnimationClip InsertAnim;
    public AnimationClip AfterReloadAnim;
    [Range(0.1f, 5)] public float InsertSpeed = 1.0f;
    public AnimationClip QuickFireAnim;
    public ParticleSystem[] Particles;
    [Range(1, 10)] public float ParticleRate = 5;
    public bool HasParticles = false;
    public bool AnimatedMovements = false;
    public bool DrawAfterFire = false;
    public int fireAnimationsCount = 1;
    public int weaponBaseLayer = 0;
    public int movementLayer = 1;

    public bool useMultipleAnimators = false;
    public Animator[] animators;

    public AudioClip Reload_1;
    public AudioClip Reload_2;
    public AudioClip Reload_3;
    public AudioClip m_Fire;

    public Action<float, int, AnimationFlags> reloadOverrideFunc;
    #endregion

    #region Private members
    [NonSerialized] private bl_Gun m_linkWeapon = null;
    private bl_GunManager GunManager;
    private bool cancelReload = false;
    private bl_PlayerReferences playerReferences;
    private AudioSource m_source;
    private GunType gunType = GunType.Machinegun;
    private readonly Dictionary<string, int> stateHashes = new();
    private bool hasAimParameter = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        gunType = ParentGun.Info.Type;
        GunManager = transform.GetComponentInParent<bl_GunManager>();
        playerReferences = GunManager.PlayerReferences;

        if (!TryGetComponent(out m_source))
        {
            m_source = gameObject.AddComponent<AudioSource>();
            m_source.playOnAwake = false;
            m_source.AssignMixerGroup("FP Weapon");
        }
        CreateGenericObject();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.wrapMode = WrapMode.Once;
            playerReferences.weaponBob.AnimatedThis(null, false);
        }
        else if (m_AnimationType == AnimationType.Animator)
        {
            if (AnimatedMovements) { playerReferences.weaponBob.AnimatedThis(UpdateAnimated, true); } else { playerReferences.weaponBob.AnimatedThis(null, false); }
            hasAimParameter = HasParameter("Aiming");
            bl_EventHandler.onLocalAimChanged += OnAim;
        }
        else
        {
            bl_EventHandler.onLocalAimChanged += OnAim;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        if (m_AnimationType == AnimationType.Animator || m_AnimationType == AnimationType.Generic)
        {
            bl_EventHandler.onLocalAimChanged -= OnAim;
        }
    }

    #region Base interface
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aiming"></param>
    public override float PlayFire(AnimationFlags flags)
    {
        switch (gunType)
        {
            case GunType.Melee:
                return MeleeFire(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            case GunType.Grenade:
                return FireGrenade(flags.IsEnumFlagPresent(AnimationFlags.QuickFire));
            default:
                if (!flags.IsEnumFlagPresent(AnimationFlags.Aiming))
                    return Fire(flags);
                else return AimFire();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override float PlayAnimation(string motionName, float length, float speed = 1, bool crossfade = false)
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (!crossfade) Anim.Stop();
            Anim.Rewind(motionName);
            Anim[motionName].speed = speed;
            if (crossfade) Anim.Play(motionName);
            else Anim.Play(motionName, PlayMode.StopAll);

            return length / speed;
        }
        else
        {
            SetAnimatorFloat("FireSpeed", speed);
            if (crossfade) CrossfadeAnimator(motionName, 0.025f, 0, 0.02f);
            else PlayAnimator(motionName, 0);

            return length / speed;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onFinish"></param>
    public override void PlayReload(float reloadTime, int[] data, AnimationFlags flags, Action onFinish = null)
    {
        if (flags.IsEnumFlagPresent(AnimationFlags.SplitReload))
            SplitReload(reloadTime, data[0]); // data[0] = number of bullets to add to the magazine.
        else
            Reload(reloadTime, flags);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float PlayTakeIn()
    {
        // the code is not handle here directly because the DrawWeapon() function was there since version 1.
        return DrawWeapon();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float PlayTakeOut()
    {
        // the code is not handle here directly because the HideWeapon() function was there since version 1.
        return HideWeapon();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animationType"></param>
    public override void CancelAnimation(WeaponAnimationType animationType)
    {
        if (animationType == WeaponAnimationType.Reload) cancelReload = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override float GetAnimationDuration(WeaponAnimationType animationType, float[] data = null)
    {
        switch (animationType)
        {
            case WeaponAnimationType.Fire:
                return GetFireLenght;
            case WeaponAnimationType.AimFire:
                var fireAim = GetAimFireClip();
                return fireAim == null ? 0 : fireAim.length / FireSpeed;
            case WeaponAnimationType.TakeIn:
                return DrawName.length / DrawSpeed;
            case WeaponAnimationType.TakeOut:
                return TakeOut.length / HideSpeed;
            case WeaponAnimationType.Reload:
                if (ParentGun.reloadPer == ReloadPer.Magazine)
                    return ReloadName.length / ParentGun.Info.ReloadTime;
                else
                {
                    float insertD = InsertAnim.length / InsertSpeed;
                    if (data != null && data.Length > 0)
                    {
                        insertD *= data[0];
                    }
                    return StartReloadAnim.length + AfterReloadAnim.length + insertD;
                }
            default:
                return 0;
        }
    }

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isAiming"></param>
    void OnAim(bool isAiming)
    {
        if (m_AnimationType == AnimationType.Animator)
        {
            if (ParentGun.FPState == PlayerFPState.Reloading) return;

            if (isAiming)
            {
                PlayAnimator("Idle", weaponBaseLayer, 0);
                if (hasAimParameter)
                {
                    PlayAnimator("Hold", weaponBaseLayer, 0);
                }
            }

            if (hasAimParameter) SetAnimatorBool("Aiming", isAiming);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerState"></param>
    void UpdateAnimated(PlayerState playerState)
    {
        if (AnimatedMovements)
        {
            float speedPercentage = playerReferences.firstPersonController.VelocityMagnitude / playerReferences.firstPersonController.GetSpeedOnState(PlayerState.Running, true);
            SetAnimatorFloat("Speed", speedPercentage);
            SetAnimatorInteger("PlayerState", (int)playerState);

            if (speedPercentage > 0.55f && ParentGun.FPState != PlayerFPState.Reloading && ParentGun.FPState != PlayerFPState.Aiming && ParentGun.WeaponType != GunType.Melee && ParentGun.WeaponType != GunType.Grenade)
            {
                CrossfadeAnimator("Run", 0.2f);
            }
        }
    }

    /// <summary>
    /// Play the fire animation one time
    /// </summary>
    public float Fire(AnimationFlags flags)
    {
        if (flags.IsEnumFlagPresent(AnimationFlags.BashAttack))
        {
            return MeleeFire(true);
        }

        if (m_AnimationType == AnimationType.Animation)
        {
            if (FireAnimations.Length <= 0)
                return 0;

            int id = UnityEngine.Random.Range(0, FireAnimations.Length);
            if (FireAnimations[id] == null) { id = 0; }
            var fireClip = FireAnimations[id];
            float fireLength = fireClip.length;

            string n = FireAnimations[id].name;
            float fireSpeed = FireSpeed;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = (fireLength * 0.5f / ParentGun.Info.FireRate);
            }

            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                Anim.Rewind(n);
                Anim[n].speed = fireSpeed;
                Anim.Play(n);
            }
            else
            {
                Anim[n].speed = fireSpeed;
                Anim.CrossFadeQueued(n, crossfadeLenght, QueueMode.PlayNow);
            }

            return fireLength / fireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            float length = SoloFireClip == null ? 0.1f : SoloFireClip.length;

            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = (length * 0.5f / ParentGun.Info.FireRate);
            }

            SetAnimatorFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                PlayAnimator("Fire", weaponBaseLayer, 0.03f);
            }
            else
            {
                CrossfadeAnimator("Fire", crossfadeLenght, weaponBaseLayer, 0.03f);
            }

            return length / fireSpeed;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public float AimFire()
    {
        var fireAimClip = GetAimFireClip();

        if (fireAimClip == null && !IsGeneric)
            return Fire(AnimationFlags.None);

        if (m_AnimationType == AnimationType.Animation)
        {
            float fireSpeed = FireSpeed;
            float fireLength = fireAimClip.length;

            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = fireLength * 0.5f / ParentGun.Info.FireRate;
            }

            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                Anim.Rewind(fireAimClip.name);
                Anim[fireAimClip.name].speed = fireSpeed;
                Anim.Play(fireAimClip.name);
            }
            else
            {
                Anim[fireAimClip.name].speed = fireSpeed;
                Anim.CrossFadeQueued(fireAimClip.name, crossfadeLenght, QueueMode.PlayNow);
            }

            return fireLength / fireSpeed;
        }
        else
        {
            float fireSpeed = FireSpeed;
            float length = FireAimAnimation == null ? 0.1f : FireAimAnimation.length;
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireRateCrossFade)
            {
                fireSpeed = length * 0.5f / ParentGun.Info.FireRate;
            }

            SetAnimatorFloat("FireSpeed", fireSpeed);
            if (fireBlendMethod == FireBlendMethod.FireRate || fireBlendMethod == FireBlendMethod.FireSpeed)
            {
                PlayAnimator("AimFire", weaponBaseLayer, 0.02f);
            }
            else
            {
                CrossfadeAnimator("AimFire", crossfadeLenght, weaponBaseLayer, 0.02f);
            }
            return length / fireSpeed;
        }
    }

    /// <summary>
    /// Play the fire animation for the knife
    /// </summary>
    public float MeleeFire(bool Quickfire)
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            var animClip = FireAimAnimation;
            if (FireAnimations.Length > 0)
            {
                animClip = FireAnimations[UnityEngine.Random.Range(0, FireAnimations.Length)];
            }

            if (animClip == null)
                return 0;

            string an = Quickfire ? QuickFireAnim.name : animClip.name;
            Anim[an].speed = FireSpeed;
            Anim.Play(an);

            return Anim[an].length / FireSpeed;
        }
        else
        {
            SetAnimatorFloat("FireSpeed", FireSpeed);
            string an = Quickfire ? "QuickFire" : "Fire";
            if (!Quickfire && fireAnimationsCount > 1)
            {
                int fireID = UnityEngine.Random.Range(0, fireAnimationsCount);
                if (fireID > 0) { an = "Fire " + fireID; }
            }

            PlayAnimator(an, weaponBaseLayer, 0);
            if (SoloFireClip == null) return FireSpeed;
            return SoloFireClip.length / FireSpeed;
        }
    }

    /// <summary>
    /// Play the fire animation for the grenade
    /// </summary>
    /// <param name="fastFire"></param>
    /// <returns></returns>
    public float FireGrenade(bool fastFire)
    {
        if (fastFire)
        {
            if (QuickFireAnim != null) PlayAnimation(QuickFireAnim.name, QuickFireAnim.length);
            else AimFire();

            return GetFireLenght + (DrawName.length / DrawSpeed);
        }
        else
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                return AimFire();
            }
            else
            {
                float length = SoloFireClip == null ? 0.2f : SoloFireClip.length;
                SetAnimatorFloat("FireSpeed", FireSpeed);
                PlayAnimator("Fire", weaponBaseLayer, 0);
                float t = length / FireSpeed;
                SetLayerWeight(movementLayer, 0);
                if (DrawAfterFire) { Invoke(nameof(DrawWeapon), t + ParentGun.Info.ReloadTime - (GetAnimationDuration(WeaponAnimationType.TakeIn) * 0.5f)); }
                return t;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FastGrenade()
    {
        yield return new WaitForSeconds(DrawWeapon());
        AimFire();
    }

    /// <summary>
    /// Throw the grenade prefab (from the bl_Gun.cs)
    /// This should be called from an animation event
    /// </summary>
    public void ThrowProjectile()
    {
        StartCoroutine(ParentGun.ThrowGrenade(false, false));
    }

    /// <summary>
    /// Play the draw/take in animation
    /// </summary>
    public float DrawWeapon()
    {
        if ((DrawName == null || !gameObject.activeInHierarchy) && !IsGeneric)
            return 0.5f;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Rewind(DrawName.name);
            Anim[DrawName.name].speed = DrawSpeed;
            Anim[DrawName.name].time = 0;
            Anim.Play(DrawName.name);

            return Anim[DrawName.name].length / DrawSpeed;
        }
        else
        {
            if (animator == null && IsGeneric)
            {
                CreateGenericObject();
            }
            float length = DrawName == null ? 0.5f : DrawName.length;
            SetAnimatorFloat("DrawSpeed", DrawSpeed);
            PlayAnimator("Draw", weaponBaseLayer, 0);
            SetLayerWeight(movementLayer, 1);
            return length / DrawSpeed;
        }
    }

    /// <summary>
    ///  Play the hide/take out animation
    /// </summary>
    public float HideWeapon()
    {
        if (m_AnimationType == AnimationType.Animation)
        {
            if (TakeOut == null)
                return 0;

            Anim[TakeOut.name].speed = HideSpeed;
            Anim.CrossFade(TakeOut.name, Mathf.Min(0.2f, (TakeOut.length / HideSpeed) * 0.7f));
        }
        else
        {
            SetAnimatorFloat("HideSpeed", HideSpeed);
            CrossfadeAnimator("Hide", 0.25f, weaponBaseLayer);
        }
        return TakeOut == null ? 0.3f : TakeOut.length / HideSpeed;
    }

    /// <summary>
    /// event called by animation when is a reload state
    /// </summary>
    /// <param name="ReloadTime"></param>
    public void Reload(float ReloadTime, AnimationFlags flags)
    {
        if (reloadOverrideFunc != null)
        {
            reloadOverrideFunc.Invoke(ReloadTime, 0, flags);
            return;
        }

        bool emptyReload = flags.IsEnumFlagPresent(AnimationFlags.EmptyReload) && emptyReloadClip != null;
        var clip = emptyReload ? emptyReloadClip : ReloadName;
        if (clip == null && !IsGeneric)
            return;

        if (m_AnimationType == AnimationType.Animation)
        {
            Anim.Stop(clip.name);
            Anim[clip.name].wrapMode = WrapMode.Once;
            Anim[clip.name].speed = (clip.length / ReloadTime);
            Anim.Play(clip.name);
        }
        else
        {
            float length = clip == null ? 1.20f : clip.length;
            SetAnimatorFloat("ReloadSpeed", length / ReloadTime);
            PlayAnimator(emptyReload ? "Reload Empty" : "Reload", weaponBaseLayer, 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CreateGenericObject()
    {
        if (m_AnimationType == AnimationType.Generic)
        {
            Transform animatedParent = transform.CreateParent("Generic Animated", true, false);
            if (animator != null) animator.enabled = false;
            if (Anim != null) Anim.enabled = false;

            var genericAnimator = animatedParent.gameObject.AddComponent<Animator>();
            genericAnimator.runtimeAnimatorController = bl_GlobalReferences.GetGenericWeaponAnimationFor(ParentGun.Info.Type);
            animator = genericAnimator;
        }
    }

    /// <summary>
    /// event called by animation when is fire
    /// </summary>
    public void FireAudio()
    {
        if (m_source != null && m_Fire != null)
        {
            m_source.clip = m_Fire;
            m_source.pitch = UnityEngine.Random.Range(1, 1.5f);
            m_source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ReloadTime"></param>
    /// <param name="Bullets"></param>
    public void SplitReload(float ReloadTime, int bulletsToAdd)
    {
        if (reloadOverrideFunc != null)
        {
            reloadOverrideFunc.Invoke(ReloadTime, bulletsToAdd, AnimationFlags.None);
            return;
        }

        StartCoroutine(StartShotgunReload());
        IEnumerator StartShotgunReload()
        {
            if (m_AnimationType == AnimationType.Animation)
            {
                Anim.CrossFade(StartReloadAnim.name, 0.2f);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                int added = 0;
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    Anim[InsertAnim.name].wrapMode = WrapMode.Loop;
                    float speed = Anim[InsertAnim.name].length / InsertSpeed;
                    Anim[InsertAnim.name].speed = speed;
                    Anim.CrossFade(InsertAnim.name);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    added++;
                    float wait = InsertAnim.length / speed;
                    // if this is the last bullet to add, start the crossfade to the end reload animation early
                    if (added == bulletsToAdd) { wait *= 0.75f; }

                    yield return new WaitForSeconds(wait);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        Anim.CrossFade(AfterReloadAnim.name, 0.2f);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }

                var endClip = AfterReloadAnim;
                if (emptyReloadClip != null && bulletsToAdd == ParentGun.bulletsPerClip)
                {
                    endClip = emptyReloadClip;
                }

                Anim.CrossFade(endClip.name, 0.2f);
                GunManager.HeadAnimation(0, endClip.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(endClip.length);
                ParentGun.FinishReload();
            }
            else
            {
                if (animator != null) animator.speed = 1;
                PlayAnimator("StartReload", 0, 0);
                ParentGun.PlayReloadAudio(0);
                yield return new WaitForSeconds(StartReloadAnim.length);
                for (int i = 0; i < bulletsToAdd; i++)
                {
                    float speed = InsertAnim.length / InsertSpeed;
                    SetAnimatorFloat("ReloadSpeed", InsertSpeed);
                    CrossfadeAnimator("Insert", 0.2f, 0);
                    GunManager.HeadAnimation(3, speed);
                    ParentGun.PlayReloadAudio(1);
                    yield return new WaitForSeconds(speed);
                    ParentGun.AddBullet(1);
                    if (cancelReload)
                    {
                        CrossfadeAnimator("EndReload", 0.32f, 0);
                        GunManager.HeadAnimation(0, AfterReloadAnim.length);
                        yield return new WaitForSeconds(AfterReloadAnim.length);
                        ParentGun.FinishReload();
                        cancelReload = false;
                        yield break;
                    }
                }
                PlayAnimator("EndReload", 0, 0);
                GunManager.HeadAnimation(0, AfterReloadAnim.length);
                ParentGun.PlayReloadAudio(2);
                yield return new WaitForSeconds(AfterReloadAnim.length);
                ParentGun.FinishReload();
            }
        }
    }

    /// <summary>
    /// Use this for greater coordination
    /// reload sounds with animation
    /// </summary>
    public void ReloadSound(int index)
    {
        if (m_source == null)
            return;

        switch (index)
        {
            case 0:
                m_source.clip = Reload_1;
                m_source.Play();
                break;
            case 1:
                m_source.clip = Reload_2;
                m_source.Play();
                GunManager.HeadAnimation(3, 1);
                break;
            case 2:
                if (Reload_3 != null)
                {
                    m_source.clip = Reload_3;
                    m_source.Play();
                }
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="motionName"></param>
    public void PlayHeadAnimation(string motionName)
    {
        var animator = GunManager.HeadAnimator;

        if (animator == null) return;

        if (!animator.IsPlaying())
        {
            animator.Play(motionName, 0, 0);
        }
        else
        {
            animator.CrossFade(motionName, 0.2f, 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void PlayParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = ParticleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void StopParticle(int id)
    {
        ParticleSystem.EmissionModule m = Particles[id].emission;
        m.rateOverTime = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void PlayAnimator(string stateName, int layer, float normalizedTime = 0)
    {
        if (!useMultipleAnimators)
        {
            animator.Play(GetStateHashOf(stateName), layer, normalizedTime);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.Play(GetStateHashOf(stateName), layer, normalizedTime);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="crossfade"></param>
    /// <param name="normalizedTime"></param>
    public void CrossfadeAnimator(string stateName, float crossfade, int layer = 0, float offset = 0)
    {
        if (!useMultipleAnimators)
        {
            animator.CrossFade(GetStateHashOf(stateName), crossfade, layer, offset);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.CrossFade(GetStateHashOf(stateName), crossfade, layer, offset);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="value"></param>
    public void SetAnimatorFloat(string stateName, float value)
    {
        if (!useMultipleAnimators)
        {
            animator.SetFloat(GetStateHashOf(stateName), value);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.SetFloat(GetStateHashOf(stateName), value);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="value"></param>
    public void SetAnimatorBool(string stateName, bool value)
    {
        if (!useMultipleAnimators)
        {
            animator.SetBool(GetStateHashOf(stateName), value);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.SetBool(GetStateHashOf(stateName), value);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="value"></param>
    public void SetAnimatorInteger(string stateName, int value)
    {
        if (!useMultipleAnimators)
        {
            animator.SetInteger(GetStateHashOf(stateName), value);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.SetInteger(GetStateHashOf(stateName), value);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="weight"></param>
    public void SetLayerWeight(int layerIndex, float weight)
    {
        if (!useMultipleAnimators)
        {
            animator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            foreach (var item in animators)
            {
                if (item == null) continue;
                item.SetLayerWeight(layerIndex, weight);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <returns></returns>
    private int GetStateHashOf(string stateName)
    {
        if (!stateHashes.ContainsKey(stateName))
        {
            stateHashes.Add(stateName, Animator.StringToHash(stateName));
        }
        return stateHashes[stateName];
    }

    /// <summary>
    /// 
    /// </summary>
    private bool IsGeneric => m_AnimationType == AnimationType.Generic;

    public void PlayHeadShake() => GunManager.HeadAnimator.Play("shake-small", 0, 0);

    public float GetFireLenght { get { return FireAimAnimation == null ? 0 : FireAimAnimation.length / FireSpeed; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="stateName"></param>
    /// <returns></returns>
    public bool HasState(int layerIndex, string stateName)
    {
        // Check if the animator has the state in the given layer
        return animator.HasState(layerIndex, Animator.StringToHash(stateName));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paramName"></param>
    /// <returns></returns>
    public bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    private AnimationClip GetAimFireClip()
    {
        if (aimFireAnimations.Length == 0) return FireAimAnimation;
        return aimFireAnimations[UnityEngine.Random.Range(0, aimFireAnimations.Length)];
    }

    [Serializable]
    public enum AnimationType
    {
        Animation,
        Animator,
        Generic,
    }

    public enum FireBlendMethod
    {
        FireRate,
        FireSpeed,
        FireRateCrossFade,
        FireSpeedCrossFade
    }

    public bl_Gun ParentGun
    {
        get
        {
            if (m_linkWeapon == null)
            {
                m_linkWeapon = transform.GetComponentInParent<bl_Gun>(true);
            }
            return m_linkWeapon;
        }
    }

    private Animator _Animator;
    private Animator animator
    {
        get
        {
            if (_Animator == null)
            {
                _Animator = this.GetComponent<Animator>();
            }
            if (_Animator == null && useMultipleAnimators)
            {
                _Animator = animators[0];
            }
            return _Animator;
        }
        set { _Animator = value; }
    }

    private Animation _Anim;
    private Animation Anim
    {
        get
        {
            if (_Anim == null)
            {
                _Anim = this.GetComponent<Animation>();
            }
            return _Anim;
        }
        set { _Anim = value; }
    }
}