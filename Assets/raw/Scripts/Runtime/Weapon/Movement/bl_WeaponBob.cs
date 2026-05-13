using UnityEngine;
using System;
using MFPS.Runtime.Motion;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Random = UnityEngine.Random;

/// <summary>
/// Default weapon walk bob movement
/// You use your custom weapon script by inherited your script from the base class bl_WeaponBobBase.cs
/// NOTE: this default script also control the head bob movement, so if you replace this with you own script
/// make sure you also handle the head bob.
/// </summary>
public class bl_WeaponBob : bl_WeaponBobBase
{
    public bl_WeaponBobSettings settings;

    #region Private members
    Vector3 midpoint;
    Vector3 localRotation;
    float timer = 0.0f;
    private bool matchEnd = false;
    float lerp = 2;
    float bobbingSpeed;
    bl_FirstPersonControllerBase motor;
    float BobbingAmount;
    float tempWalkSpeed = 0;
    float tempRunSpeed = 0;
    float tempIdleSpeed = 0;
    float waveslice = 0.0f;
    float waveslice2 = 0.0f;
    float eulerZ = 0;
    float eulerX = 0;
    private bool rightFoot = false;
    Vector3 currentPosition, currentRotation = Vector3.zero;
    Vector3 bobEuler = Vector3.zero;
    private Action<PlayerState> AnimateCallback = null;
    private bl_PlayerReferences playerReferences;
    private bool useAnimation = false;
    private Vector3 bobRotation = Vector3.zero;
    private float rotationIntensity = 1;
    private bl_SpringTransform spring;
    private bool isAiming = false;
    private float currentNoiseFrequency = 0f;
    private float targetNoiseFrequency = 0f;
    private float timeUntilNextNoiseEvent = 0f;
    private float noiseEventDuration = 0f;
    private bool isInNoiseEvent = false;
    private bl_SpringVector3 noiseSpring;
    private bool wasMoving = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
        motor = playerReferences.firstPersonController;
        midpoint = transform.localPosition;
        localRotation = transform.localEulerAngles;
        Intensitity = 1;
        rotationIntensity = 1;
        spring = settings.spring.InitAndGetInstance(CachedTransform);
        noiseSpring = new bl_SpringVector3(Vector3.zero, settings.noiseStiffness, settings.noiseDamping, 1);
        noiseSpring.Enable = true;
        ScheduleNextNoiseEvent();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalAimChanged += OnLocalAimChange;
        bl_EventHandler.onRoundEnd += OnMatchEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalAimChanged -= OnLocalAimChange;
        bl_EventHandler.onRoundEnd -= OnMatchEnd;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (motor == null || matchEnd) return;

        if (useAnimation)
        {
            if (AnimateCallback == null) return;

            AnimateCallback.Invoke(motor.State);
            MoveToDefault();
            CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, midpoint, Time.deltaTime * lerp);
        }

        StateControl();
        if (settings.useNoiseMovement)
        {
            UpdateNoiseFrequency();
            ApplyRandomNoise();
        }
        spring.Update();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        Movement();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Stop()
    {
        bobbingSpeed = tempIdleSpeed;
        BobbingAmount = settings.WalkOscillationAmount * 0.1f;
        lerp = settings.WalkLerpSpeed;
        eulerZ = settings.EulerZAmount;
        eulerX = settings.EulerXAmount;
        MoveToDefault();
    }

    /// <summary>
    /// 
    /// </summary>
    void StateControl()
    {
        if (motor.State == PlayerState.Jumping || motor.State == PlayerState.InVehicle)
        {
            SetToIdle();
            return;
        }

        if (motor.VelocityMagnitude > 0.1f && !motor.IsSprinting())
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = settings.WalkOscillationAmount;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
            wasMoving = true;
        }
        else if (motor.IsSprinting())
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = settings.RunOscillationAmount;
            lerp = settings.RunLerpSpeed;
            eulerZ = settings.RunEulerZAmount;
            eulerX = settings.RunEulerXAmount;
            wasMoving = true;
        }

        if ((!motor.IsSprinting() && motor.VelocityMagnitude < 0.1f) || !bl_GameInput.IsCursorLocked)
        {
            SetToIdle();
            if (wasMoving)
            {
                wasMoving = false;
                /// motor.PlayFootStepSound();
                if (Random.value > 0.75f) timeUntilNextNoiseEvent = 0;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetToIdle()
    {
        bobbingSpeed = tempIdleSpeed;
        BobbingAmount = settings.WalkOscillationAmount * 0.1f;
        lerp = settings.WalkLerpSpeed;
        eulerZ = settings.EulerZAmount;
        eulerX = settings.EulerXAmount;
    }

    /// <summary>
    /// 
    /// </summary>
    void Movement()
    {
        if (motor == null) { return; }

        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if ((!motor.IsSprinting() && motor.VelocityMagnitude < 0.1f) || !bl_GameInput.IsCursorLocked)
        {
            IdleMovement();
            return;
        }

        if (tempIdleSpeed != 0.2354f)
        {
            tempWalkSpeed = motor.GetCurrentSpeed() * 0.06f * settings.WalkSpeedMultiplier;
            tempRunSpeed = motor.GetCurrentSpeed() * 0.03f * settings.RunSpeedMultiplier;
            tempIdleSpeed = 0.2354f;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer += bobbingSpeed;
        if (timer > Mathf.PI * 2)
        {
            timer -= (Mathf.PI * 2);
        }

        float w1v = (waveslice + 1) / 2;
        w1v = settings.pitchCurve.Evaluate(w1v);
        waveslice = Mathf.Lerp(-1, 1, w1v);

        float w2v = (waveslice2 + 1) / 2;
        w2v = settings.rollCurve.Evaluate(w2v);
        waveslice2 = Mathf.Lerp(-1, 1, w2v);

        ApplyMovement();
        UpdateFootStep();
    }

    /// <summary>
    /// 
    /// </summary>
    void IdleMovement()
    {
        if (useAnimation) return;

        float multiplier = isAiming ? settings.idleAimMultiplier : 1;
        float sineValue = Mathf.Sin(Time.time * settings.idleBreathingFrequency);
        float verticalOffset = settings.idleBreathingAmplitude * sineValue * multiplier;
        float rotationOffset = settings.idleSwayAmplitude * sineValue * multiplier;

        spring.SetPositionTarget(midpoint + new Vector3(0f, verticalOffset, 0f));
        currentRotation.Set(rotationOffset, 0, 0);
        CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), Time.smoothDeltaTime * lerp);
        playerReferences.cameraMotion.AddCameraRotation(1, currentRotation * settings.idleCameraMultiplier);
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMovement()
    {
        if (useAnimation) return;
        float time = Time.smoothDeltaTime;
        if (waveslice != 0)
        {
            float verticalMovement = waveslice * BobbingAmount * Intensitity;
            if (!settings.pitchTowardUp) verticalMovement = -verticalMovement;
            float horizontalMovement = waveslice2 * BobbingAmount * Intensitity;
            float zFlow = waveslice2 * eulerZ * rotationIntensity;
            float xFlow = waveslice * eulerX * rotationIntensity;

            if (motor.isGrounded && motor.State != PlayerState.InVehicle)
            {
                //if player is moving
                if (motor.VelocityMagnitude > 0.1f && motor.State != PlayerState.Idle)
                {
                    currentPosition.Set(midpoint.x + horizontalMovement, midpoint.y + verticalMovement, midpoint.z);
                    currentRotation.Set(localRotation.x + xFlow, localRotation.y, localRotation.z + zFlow);
                    bobEuler.Set(xFlow * 0.33f * motor.GetHeadBobMagnitudes(true), 0, zFlow * motor.GetHeadBobMagnitudes(false));
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
                    bobRotation = bobEuler;
                }
                else//if player is idle
                {
                    currentPosition.Set(midpoint.x, midpoint.y + verticalMovement, midpoint.z);
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
                    bobRotation = Vector3.zero;
                }
            }
            else
            {
                //Player not moving
                MoveToDefault();
            }
        }
        else
        {
            //Player not moving
            MoveToDefault();
        }

        if (spring != null) spring.SetPositionTarget(currentPosition);
        else CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, currentPosition, time * lerp);
        playerReferences.cameraMotion.AddCameraRotation(1, bobRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateNoiseFrequency()
    {
        // Update timers
        if (isInNoiseEvent)
        {
            // We are in a noise event
            noiseEventDuration -= Time.deltaTime;

            if (noiseEventDuration <= 0f)
            {
                // End the noise event
                isInNoiseEvent = false;
                targetNoiseFrequency = 0f;
                spring.RotationSpring.SetTargetLayer(1, Vector3.zero);
                ScheduleNextNoiseEvent();
            }
        }
        else
        {
            // We are waiting for the next noise event
            timeUntilNextNoiseEvent -= Time.deltaTime;

            if (timeUntilNextNoiseEvent <= 0f)
            {
                // Start a new noise event
                isInNoiseEvent = true;
                targetNoiseFrequency = Random.Range(0f, settings.maxNoiseFrequency);
                noiseEventDuration = Random.Range(settings.noiseDurationRange.x, settings.noiseDurationRange.y); // Random duration for the noise event
            }
        }

        // Smoothly transition the current noise frequency towards the target
        currentNoiseFrequency = Mathf.MoveTowards(currentNoiseFrequency, targetNoiseFrequency, settings.noiseFrequencyTransitionSpeed * Time.deltaTime);
        noiseSpring.Update(Time.deltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyRandomNoise()
    {
        if (currentNoiseFrequency > 0f)
        {
            float noiseY = (Mathf.PerlinNoise(Time.time * currentNoiseFrequency, 0f) - 0.5f) * 2f * settings.maxNoiseIntensity;
            float noiseZ = (Mathf.PerlinNoise(0f, Time.time * currentNoiseFrequency * settings.noiseZMultiplier) - 0.5f) * 2f * settings.maxNoiseIntensity * settings.noiseZMultiplier;
            float noiseX = (Mathf.PerlinNoise(Time.time * currentNoiseFrequency, Time.time * currentNoiseFrequency) - 0.5f) * 2f * settings.maxNoiseIntensity;

            float multiplier = isAiming ? settings.noiseAimMultiplier : 1;
            noiseSpring.Target = new Vector3(noiseX, noiseY, noiseZ) * multiplier;
            noiseSpring.Stiffness = settings.noiseStiffness;
            noiseSpring.Damping = settings.noiseDamping;
            CachedTransform.localRotation *= Quaternion.Euler(noiseSpring.Current);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void MoveToDefault()
    {
        currentPosition = midpoint;
        CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.identity, Time.smoothDeltaTime * 12);
        bobRotation = Vector3.zero;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateFootStep()
    {
        if (!motor.isGrounded || motor.State == PlayerState.InVehicle) return;

        if (motor.VelocityMagnitude > 0.1f)
        {
            if (waveslice2 >= 0.97f && !rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = true;
            }
            else if (waveslice2 <= (-0.97f) && rightFoot)
            {
                motor.PlayFootStepSound();
                rightFoot = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aiming"></param>
    void OnLocalAimChange(bool aiming)
    {
        isAiming = aiming;
        Intensitity = aiming ? settings.AimIntensity : 1;
        rotationIntensity = aiming ? settings.aimRotationIntensity : 1;

        if (aiming)
        {
            CachedTransform.localPosition = currentPosition = midpoint;
            CachedTransform.localRotation = Quaternion.identity;
            bobRotation = Vector3.zero;
        }
    }

    void ScheduleNextNoiseEvent()
    {
        if (!settings.useNoiseMovement) return;

        // Schedule the next noise event after a random interval
        timeUntilNextNoiseEvent = Random.Range(settings.noiseIntervalRange.x, settings.noiseIntervalRange.y); // Most of the time, we wait before the next event
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchEnd()
    {
        matchEnd = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="useAnim"></param>
    public override void AnimatedThis(Action<PlayerState> callback, bool useAnim)
    {
        AnimateCallback = callback;
        useAnimation = useAnim;
    }

#if ENABLE_SIMULATION
    public float simulationSpeed = 0;
    public bool isSimulating = false;

    public void SimulateMovement()
    {
        float time = 0.018f;

        if (simulationSpeed > 0.1f && simulationSpeed <= 4)
        {
            bobbingSpeed = tempWalkSpeed;
            BobbingAmount = settings.WalkOscillationAmount;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }
        else if (simulationSpeed > 4)
        {
            bobbingSpeed = tempRunSpeed;
            BobbingAmount = settings.RunOscillationAmount;
            lerp = settings.RunLerpSpeed;
            eulerZ = settings.RunEulerZAmount;
            eulerX = settings.RunEulerXAmount;
        }

        if (simulationSpeed < 0.1f)
        {
            bobbingSpeed = tempIdleSpeed;
            BobbingAmount = settings.WalkOscillationAmount * 0.1f;
            lerp = settings.WalkLerpSpeed;
            eulerZ = settings.EulerZAmount;
            eulerX = settings.EulerXAmount;
        }

        tempWalkSpeed = 0;
        tempRunSpeed = 0;
        tempIdleSpeed = 0;

        if (tempIdleSpeed != settings.idleBobbingSpeed)
        {
            tempWalkSpeed = simulationSpeed * 0.06f * settings.WalkSpeedMultiplier;
            tempRunSpeed = simulationSpeed * 0.03f * settings.RunSpeedMultiplier;
            tempIdleSpeed = settings.idleBobbingSpeed;
        }

        waveslice = Mathf.Sin(timer * 2);
        waveslice2 = Mathf.Sin(timer);
        timer = timer + bobbingSpeed;

        float w1v = (waveslice + 1) / 2;
        w1v = settings.pitchCurve.Evaluate(w1v);
        waveslice = Mathf.Lerp(-1, 1, w1v);

        float w2v = (waveslice2 + 1) / 2;
        w2v = settings.rollCurve.Evaluate(w2v);
        waveslice2 = Mathf.Lerp(-1, 1, w2v);

        if (waveslice != 0)
        {
            float verticalMovement = -(waveslice * BobbingAmount * Intensitity);
            float horizontalMovement = waveslice2 * BobbingAmount * Intensitity;
            float zFlow = waveslice2 * eulerZ;
            float xFlow = waveslice * eulerX;

            //if player is moving
            if (simulationSpeed > 0.1f)
            {
                currentPosition.Set(midpoint.x + horizontalMovement, midpoint.y + verticalMovement, currentPosition.z);
                currentRotation.Set(localRotation.x + xFlow, localRotation.y, localRotation.z + zFlow);
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(currentRotation), time * lerp);
            }
            else//if player is idle
            {
                currentPosition.Set(midpoint.x, midpoint.y + verticalMovement, currentPosition.z);
                CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(Vector3.zero), time * 10);
            }
        }
        else
        {
            //Player not moving
            MoveToDefault();
        }

        CachedTransform.localPosition = Vector3.Lerp(CachedTransform.localPosition, currentPosition, time * lerp);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_WeaponBob))]
public class bl_WeaponEditorBob : Editor
{
    bl_WeaponBob script;

    private void OnEnable()
    {
        script = (bl_WeaponBob)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(5);
        SerializedProperty so = serializedObject.FindProperty("settings");
        EditorGUILayout.PropertyField(so);
        if (so != null && so.objectReferenceValue != null)
        {
            var editor = Editor.CreateEditor(so.objectReferenceValue);
            if (editor != null)
            {
                EditorGUILayout.BeginVertical("box");
                editor.DrawDefaultInspector();
                EditorGUILayout.EndVertical();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
#if ENABLE_SIMULATION
        if (!script.isSimulating)
        {
            if (GUILayout.Button("Simulate"))
            {
                script.isSimulating = true;
                script.Init();
                EditorApplication.update += OnEditorUpdate;
            }
        }
        else
        {
            script.simulationSpeed = EditorGUILayout.Slider("Simulation Speed", script.simulationSpeed, 0, 8);
            if (GUILayout.Button("Stop Simulate"))
            {
                EditorApplication.update -= OnEditorUpdate;
                script.isSimulating = false;
            }
        }
#endif
    }

#if ENABLE_SIMULATION
    void OnEditorUpdate()
    {
        script.SimulateMovement();
    }
#endif
}
#endif