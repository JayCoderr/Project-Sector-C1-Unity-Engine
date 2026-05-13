using MFPSEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(bl_FirstPersonController))]
public class bl_FirstPersonControllerEditor : Editor
{
    bl_FirstPersonController script;
    public Dictionary<string, AnimBool> animatedBools = new()
    {
        {"move", null },  {"jump", null }, {"fall", null }, {"mouse", null }, {"bob", null }, {"sound", null }, {"misc", null }, {"slide", null }, {"stamina", null }
    };

    SerializedProperty moveProp;
    SerializedProperty jumpProp;
    SerializedProperty fallProp;
    SerializedProperty mouseProp;
    SerializedProperty bobProp;
    SerializedProperty soundProp;
    SerializedProperty miscProp;
    SerializedProperty slideProp;
    SerializedProperty staminaProp;
    SerializedProperty playerAudioBankProp;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_FirstPersonController)target;

        moveProp = serializedObject.FindProperty("WalkSpeed");
        jumpProp = serializedObject.FindProperty("jumpSpeed");
        fallProp = serializedObject.FindProperty("FallDamage");
        mouseProp = serializedObject.FindProperty("mouseLook");
        bobProp = serializedObject.FindProperty("headBobMagnitude");
        soundProp = serializedObject.FindProperty("footstep");
        miscProp = serializedObject.FindProperty("runToAimBehave");
        slideProp = serializedObject.FindProperty("canSlide");
        staminaProp = serializedObject.FindProperty("useStamina");
        playerAudioBankProp = serializedObject.FindProperty("playerAudioBank");

        animatedBools["move"] = new AnimBool(moveProp.isExpanded, Repaint);
        animatedBools["jump"] = new AnimBool(jumpProp.isExpanded, Repaint);
        animatedBools["fall"] = new AnimBool(fallProp.isExpanded, Repaint);
        animatedBools["mouse"] = new AnimBool(mouseProp.isExpanded, Repaint);
        animatedBools["bob"] = new AnimBool(bobProp.isExpanded, Repaint);
        animatedBools["sound"] = new AnimBool(soundProp.isExpanded, Repaint);
        animatedBools["misc"] = new AnimBool(miscProp.isExpanded, Repaint);
        animatedBools["slide"] = new AnimBool(slideProp.isExpanded, Repaint);
        animatedBools["stamina"] = new AnimBool(staminaProp.isExpanded, Repaint);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        MovementSpeeds();
        MouseLookBox();
        JumpBox();
        SlideBox();
        StaminaBox();
        FallBox();
        HeadBobBox();
        MiscBox();
        SoundBox();
    }

    void MovementSpeeds()
    {
        moveProp.isExpanded = animatedBools["move"].target = MFPSEditorStyles.ContainerHeaderFoldout("Speed", moveProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["move"].faded))
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("box");
            script.WalkSpeed = EditorGUILayout.Slider("Walk Speed", script.WalkSpeed, 2, 12);
            script.runSpeed = EditorGUILayout.Slider("Run Speed", script.runSpeed, script.WalkSpeed, 16);
            script.stealthSpeed = EditorGUILayout.Slider("Stealth Speed", script.stealthSpeed, 1, 3);
            script.acceleration = EditorGUILayout.Slider("Acceleration", script.acceleration, 1, 30);
            script.crouchSpeed = EditorGUILayout.Slider("Crouch Speed", script.crouchSpeed, 1, 8);
            script.crouchTransitionSpeed = EditorGUILayout.Slider("Crouch Transition Speed", script.crouchTransitionSpeed, 0.01f, 0.5f);
            script.slideSpeed = EditorGUILayout.Slider("Slide Speed", script.slideSpeed, 10, 20);
            EditorGUILayout.EndVertical();
            EndChangeCheck();
        }
        EditorGUILayout.EndFadeGroup();
    }

    void JumpBox()
    {
        jumpProp.isExpanded = animatedBools["jump"].target = MFPSEditorStyles.ContainerHeaderFoldout("Jump", jumpProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["jump"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.jumpSpeed = EditorGUILayout.Slider("Jump Force", script.jumpSpeed, 4, 30);
            script.doubleJumpSpeed = EditorGUILayout.Slider("Double Jump Force", script.doubleJumpSpeed, 4, 30);
            script.JumpMinRate = EditorGUILayout.Slider(new GUIContent("Jump Rate", "Minimum time delay between jumps"), script.JumpMinRate, 0.2f, 1.5f);
            script.jumpMomentumBooster = EditorGUILayout.Slider("Jump Momentum Booster", script.jumpMomentumBooster, 0.2f, 4.5f);
            script.doubleJumpBoost = EditorGUILayout.Slider("Double Jump Momentum Booster", script.doubleJumpBoost, 0.2f, 10);
            script.momentunDecaySpeed = EditorGUILayout.Slider(new GUIContent("Momentum Decay Speed", "Speed which the momentum velocity is reset."), script.momentunDecaySpeed, 0.2f, 12f);
            script.m_GravityMultiplier = EditorGUILayout.Slider("Gravity Multiplier", script.m_GravityMultiplier, 0.1f, 5);
            script.m_StickToGroundForce = EditorGUILayout.Slider("Stick To Ground Force", script.m_StickToGroundForce, 4, 12);
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.doubleJumpResetFall = MFPSEditorStyles.FeatureToogle(r, script.doubleJumpResetFall, "Double Jump Reset Fall", "Does double jump reset the fall distance?");
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    void SlideBox()
    {
        slideProp.isExpanded = animatedBools["slide"].target = MFPSEditorStyles.ContainerHeaderFoldout("Slide", slideProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["slide"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.canSlide = MFPSEditorStyles.FeatureToogle(r, script.canSlide, "Player Can Slide");
            script.slideTime = EditorGUILayout.Slider("Slide Time", script.slideTime, 0.2f, 1.5f);
            script.slideCoolDown = EditorGUILayout.Slider("Slide Cool-down", script.slideCoolDown, 0.1f, 2.5f);
            script.slideFriction = EditorGUILayout.Slider("Slide Friction", script.slideFriction, 1, 12);
            script.slideCameraTiltAngle = EditorGUILayout.Slider("Camera Tilt Angle", script.slideCameraTiltAngle, -35, 35);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    void StaminaBox()
    {
        staminaProp.isExpanded = animatedBools["stamina"].target = MFPSEditorStyles.ContainerHeaderFoldout("Stamina", staminaProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["stamina"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.useStamina = MFPSEditorStyles.FeatureToogle(r, script.useStamina, "Use Stamina");
            GUI.enabled = script.useStamina;
            script.maxStamina = EditorGUILayout.Slider("Stamina", script.maxStamina, 50, 500);
            script.staminaRegenRate = EditorGUILayout.Slider("Stamina Regeneration Rate", script.staminaRegenRate, 1, 50);
            script.staminaDrainRate = EditorGUILayout.Slider("Stamina Drain Rate", script.staminaDrainRate, 1, 15);
            script.staminaJumpDrain = EditorGUILayout.Slider("Stamina Jump Cost", script.staminaJumpDrain, 1, 50);
            script.staminaSlideDrain = EditorGUILayout.Slider("Stamina Slide Cost", script.staminaSlideDrain, 1, 50);
            GUI.enabled = true;
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void FallBox()
    {
        fallProp.isExpanded = animatedBools["fall"].target = MFPSEditorStyles.ContainerHeaderFoldout("Fall", fallProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["fall"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.FallDamage = MFPSEditorStyles.FeatureToogle(r, script.FallDamage, "Fall Damage");
            script.SafeFallDistance = EditorGUILayout.Slider("Safe Distance", script.SafeFallDistance, 0.1f, 7f);
            script.DeathFallDistance = EditorGUILayout.Slider("Deathly Distance", script.DeathFallDistance, script.SafeFallDistance, 25);
            script.stairDetectionGracePeriod = EditorGUILayout.Slider("Stair Detection Grace Period", script.stairDetectionGracePeriod, 0, 0.5f);
            script.minFallDistanceForLanding = EditorGUILayout.Slider("Min Fall Distance For Landing", script.minFallDistanceForLanding, 0, 1);
            script.AirControlMultiplier = EditorGUILayout.Slider("Air Control Multiplier", script.AirControlMultiplier, 0, 2);
            GUILayout.Space(10);
            GUILayout.Label("Dropping", EditorStyles.boldLabel);
            script.dropControlSpeed = EditorGUILayout.Slider("Drop Control Speed", script.dropControlSpeed, 15, 40);
            EditorGUILayout.MinMaxSlider("Drop Angle Speed Range", ref script.dropTiltSpeedRange.x, ref script.dropTiltSpeedRange.y, 10, 75);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MouseLookBox()
    {
        mouseProp.isExpanded = animatedBools["mouse"].target = MFPSEditorStyles.ContainerHeaderFoldout("Mouse Look", mouseProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["mouse"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            if (script.mouseLook == null) script.mouseLook = new MFPS.PlayerController.MouseLook();
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.mouseLook.clampVerticalRotation = MFPSEditorStyles.FeatureToogle(r, script.mouseLook.clampVerticalRotation, "Clamp Vertical Rotation");
            if (script.mouseLook.clampVerticalRotation)
            {
                EditorGUILayout.LabelField($"Vertical Rotation Clamp ({script.mouseLook.MinimumX.ToString("0.0")},{script.mouseLook.MaximumX.ToString("0.0")})");
                EditorGUILayout.MinMaxSlider(ref script.mouseLook.MinimumX, ref script.mouseLook.MaximumX, -180, 180);
            }
            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            GUILayout.Label("You can modify the default sensitivity settings in GameData -> Default Settings.", EditorStyles.helpBox);
            var prop = serializedObject.FindProperty("headRoot");
            EditorGUI.indentLevel++;
            prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, "References");
            if (prop.isExpanded)
            {
                EditorGUILayout.PropertyField(prop);
                script.CameraRoot = EditorGUILayout.ObjectField("Camera Root", script.CameraRoot, typeof(Transform), true) as Transform;
            }
            EditorGUI.indentLevel--;
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void HeadBobBox()
    {
        bobProp.isExpanded = animatedBools["bob"].target = MFPSEditorStyles.ContainerHeaderFoldout("Head Bob", bobProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["bob"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.headBobMagnitude = EditorGUILayout.Slider("Head Bob Magnitude", script.headBobMagnitude, 0, 1.2f);
            script.headVerticalBobMagnitude = EditorGUILayout.Slider("Vertical Bob Magnitude", script.headVerticalBobMagnitude, 0, 1f);
            GUILayout.Label("You can modify the Head Bob properties in bl_WeaponBob.", EditorStyles.helpBox);
            if (GUILayout.Button("Ping bl_WeaponBob.cs", EditorStyles.toolbarButton))
            {
                var wb = script.transform.GetComponentInChildren<bl_WeaponBobBase>(true);
                if (wb != null)
                {
                    Selection.activeObject = wb.gameObject;
                    EditorGUIUtility.PingObject(wb.gameObject);
                }
            }
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void SoundBox()
    {
        soundProp.isExpanded = animatedBools["sound"].target = MFPSEditorStyles.ContainerHeaderFoldout("Sounds", soundProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["sound"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.footstep = EditorGUILayout.ObjectField("FootStep Controller", script.footstep, typeof(bl_Footstep), true) as bl_Footstep;
            EditorGUILayout.PropertyField(playerAudioBankProp, new GUIContent("Player Audio Bank"), true);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    void MiscBox()
    {
        miscProp.isExpanded = animatedBools["misc"].target = MFPSEditorStyles.ContainerHeaderFoldout("Misc", miscProp.isExpanded);
        if (EditorGUILayout.BeginFadeGroup(animatedBools["misc"].faded))
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            script.runToAimBehave = (PlayerRunToAimBehave)EditorGUILayout.EnumPopup("Aim While Running Behave", script.runToAimBehave);

            var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.canStealthMode = MFPSEditorStyles.FeatureToogle(r, script.canStealthMode, "Can Use Stealth Mode");

            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.canDoubleJump = MFPSEditorStyles.FeatureToogle(r, script.canDoubleJump, "Can Double Jump");

            r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            script.RunFovEffect = MFPSEditorStyles.FeatureToogle(r, script.RunFovEffect, "Sprint FoV Effect");

            script.crouchHeight = EditorGUILayout.Slider("Crouch Height", script.crouchHeight, 0.2f, 3);
            if (script.RunFovEffect)
            {
                script.runFOVAmount = EditorGUILayout.Slider("Run FOV Amount", script.runFOVAmount, 0, 12);
            }
            script.maxSlopeAngle = EditorGUILayout.Slider("Max Slope Angle", script.maxSlopeAngle, 0, 90);
            EndChangeCheck();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
    }

    /// <summary>
    /// 
    /// </summary>
    private void EndChangeCheck()
    {
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}