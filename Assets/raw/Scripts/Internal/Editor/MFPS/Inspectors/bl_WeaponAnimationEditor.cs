using MFPSEditor;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;

[CustomEditor(typeof(bl_WeaponAnimation))]
public class bl_WeaponAnimationEditor : Editor
{
    private bl_Gun Gun;
    GunType gType;
    private Animator _animator;
    bl_WeaponAnimation script;
    bool allowSceneObjects = false;
    AnimationClip WalkAnim;
    AnimationClip RunAnim;
    private SerializedProperty aimFireClipsProp;
    private SerializedProperty fireClipsProp;

    private void OnEnable()
    {
        script = (bl_WeaponAnimation)target;
        Gun = script.GetComponentInParent<bl_Gun>(true);
        gType = bl_GameData.Instance.GetWeapon(Gun.GunID).Type;
        if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animator)
        {
            _animator = script.GetComponent<Animator>();
        }
        aimFireClipsProp = serializedObject.FindProperty("aimFireAnimations");
        fireClipsProp = serializedObject.FindProperty("FireAnimations");
        if (script.FireAnimations == null) { script.FireAnimations = new AnimationClip[0]; }
    }

    public override void OnInspectorGUI()
    {
        if (script == null) { script = (bl_WeaponAnimation)target; }
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        allowSceneObjects = !EditorUtility.IsPersistent(script);

        GUILayout.BeginVertical("box");

        GUILayout.BeginVertical("box");
        script.m_AnimationType = (bl_WeaponAnimation.AnimationType)EditorGUILayout.EnumPopup("Animation Type", script.m_AnimationType);
        GUILayout.EndVertical();

        if (script.m_AnimationType != bl_WeaponAnimation.AnimationType.Generic)
        {
            if (script.m_AnimationType == bl_WeaponAnimation.AnimationType.Animation)
            {
                AnimationGUI();
            }
            else
            {
                AnimatorGUI();
            }

            GUILayout.EndVertical();

            if (Gun.SoundReloadByAnim && gType != GunType.Melee)
            {
                GUILayout.BeginVertical("box");
                script.Reload_1 = EditorGUILayout.ObjectField("Clip Out Audio", script.Reload_1, typeof(AudioClip), allowSceneObjects) as AudioClip;
                script.Reload_2 = EditorGUILayout.ObjectField("Clip In Audio", script.Reload_2, typeof(AudioClip), allowSceneObjects) as AudioClip;
                script.Reload_3 = EditorGUILayout.ObjectField("Slide Audio", script.Reload_3, typeof(AudioClip), allowSceneObjects) as AudioClip;
                GUILayout.EndVertical();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("With Generic animation type, the weapon will use some generic fire, reload, take in, and take out animations.", MessageType.Info);
            GUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }
    }

    void AnimatorGUI()
    {
        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbar);
        script.useMultipleAnimators = MFPSEditorStyles.FeatureToogle(r, script.useMultipleAnimators, "Use Multiple Animators");

        RuntimeAnimatorController runtimeController;
        if (!script.useMultipleAnimators)
        {
            if (_animator == null)
            {
                _animator = script.GetComponent<Animator>();
                if (_animator == null)
                {
                    EditorGUILayout.HelpBox("This weapon don't have a Animator Component!", MessageType.Warning);
                    return;
                }
            }
            runtimeController = _animator.runtimeAnimatorController;
        }
        else
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animators"), true);
            EditorGUI.indentLevel--;
            if (script.animators == null || script.animators.Length == 0)
            {
                EditorGUILayout.HelpBox("This weapon don't have Animators assigned", MessageType.Warning);
                return;
            }

            if (script.animators[0] == null)
            {
                EditorGUILayout.HelpBox("The first animator in the list is not assigned.", MessageType.Warning);
                return;
            }

            runtimeController = script.animators[0].runtimeAnimatorController;
        }

        if (script.useMultipleAnimators)
        {
            EditorGUILayout.HelpBox("When using multiple animators, the auto animator setup won't work since when use multiple animator usually means both parts use different animation clips, thefore a manual setup is required, you will have to create the animator controller and assign the animation clips manually, you still need to assign the animations below to get the length information.", MessageType.Info);
        }

        if (runtimeController == null)
        {
            if (!script.useMultipleAnimators)
            {
                EditorGUILayout.HelpBox("The animator for this weapons has not been assigned yet, if you already have it, assign it in the Animator Component, otherwise you can create it here," +
                "simply draw the AnimationClips in the respective field below and click in the button SetUp", MessageType.Info);
            }

            GUILayout.BeginVertical("box");
            script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            if (gType != GunType.Melee)
            {
                if (Gun.reloadPer == ReloadPer.Bullet)
                {
                    script.StartReloadAnim = EditorGUILayout.ObjectField("Begin Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("After Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
                else
                {
                    script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.emptyReloadClip = EditorGUILayout.ObjectField("Empty Reload Animation", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                }
            }
            else
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            script.IdleClip = EditorGUILayout.ObjectField("Idle Animation", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("Custom Animations For Movements", script.AnimatedMovements, EditorStyles.toolbarButton);
            GUILayout.Space(4);
            if (script.AnimatedMovements)
            {
                WalkAnim = EditorGUILayout.ObjectField("Walk Animation", WalkAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                RunAnim = EditorGUILayout.ObjectField("Run Animation", RunAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            }
            if (GUILayout.Button("SetUp", EditorStyles.toolbarButton))
            {
                CreateAnimator();
            }

            GUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginHorizontal("box");
            script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
            GUILayout.EndHorizontal();
            if (gType == GunType.Machinegun || gType == GunType.Pistol)
            {
                script.fireBlendMethod = (bl_WeaponAnimation.FireBlendMethod)EditorGUILayout.EnumPopup("Fire Blend Method", script.fireBlendMethod);
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if (script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeed || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
                    script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                if (script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireRateCrossFade || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
                {
                    script.crossfadeLenght = EditorGUILayout.Slider("Crossfade Lenght", script.crossfadeLenght, 0.0f, 0.5f);
                }
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();

                if (Gun.reloadPer == ReloadPer.Bullet)
                {
                    GUILayout.BeginHorizontal("box");
                    script.StartReloadAnim = EditorGUILayout.ObjectField("Start Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("End Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginVertical("box");
                    script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.emptyReloadClip = EditorGUILayout.ObjectField("Empty Reload Animation", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndVertical();
                }
            }
            else if (gType == GunType.Shotgun || gType == GunType.Sniper)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                if (Gun.reloadPer == ReloadPer.Bullet)
                {
                    GUILayout.BeginHorizontal("box");
                    script.StartReloadAnim = EditorGUILayout.ObjectField("Start Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                    script.AfterReloadAnim = EditorGUILayout.ObjectField("End Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginVertical("box");
                    script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    script.emptyReloadClip = EditorGUILayout.ObjectField("Empty Reload Animation", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndVertical();
                }
            }
            else if (gType == GunType.Grenade || gType == GunType.Launcher)
            {
                GUILayout.BeginHorizontal("box");
                script.SoloFireClip = EditorGUILayout.ObjectField("Fire Animation", script.SoloFireClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                script.HasParticles = EditorGUILayout.ToggleLeft("Use Particles", script.HasParticles, EditorStyles.toolbarPopup);
                if (script.HasParticles)
                {
                    script.ParticleRate = EditorGUILayout.Slider("Particle Rate", script.ParticleRate, 0.1f, 10);
                    var prop = serializedObject.FindProperty("Particles");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(prop, true);
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.Space(2);
                script.DrawAfterFire = EditorGUILayout.ToggleLeft("Draw After Fire", script.DrawAfterFire, EditorStyles.toolbarButton);
            }
            else if (gType == GunType.Melee)
            {
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                script.fireAnimationsCount = EditorGUILayout.IntSlider(new GUIContent("Fire Animations Count", "If you have multiple fire animations and you are using the Animator component > setup the fire animations in the Animator Controller with the name 'Fire 1', 'Fire 2', 'Fire 3', etc... "), script.fireAnimationsCount, 1, 10);
                GUILayout.BeginHorizontal("box");
                script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal("box");
            script.IdleClip = EditorGUILayout.ObjectField("Idle Animation", script.IdleClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("box");
            script.AnimatedMovements = EditorGUILayout.ToggleLeft("Custom Animations For Movements", script.AnimatedMovements, EditorStyles.toolbarButton);
            script.weaponBaseLayer = EditorGUILayout.IntField("Base Layer Index", script.weaponBaseLayer);
            script.movementLayer = EditorGUILayout.IntField("Movement Layer Index", script.movementLayer);
            GUILayout.EndVertical();
        }
    }

    void CreateAnimator()
    {
        string lastFolder = PlayerPrefs.GetString("mfpseditor.wanimator.save", "Assets/");
        string path = EditorUtility.SaveFolderPanel("Save Animator Folder", lastFolder, script.gameObject.name);
        if (string.IsNullOrEmpty(path)) { Debug.Log("Setup canceled"); return; }

        PlayerPrefs.SetString("mfpseditor.wanimator.save", path);

        path += string.Format("/{0}.controller", Gun.gameObject.name);
        string relativepath = "Assets" + path.Substring(Application.dataPath.Length);
        string copyName = string.Format("Assets/MFPS/Content/Prefabs/Weapons/Animators/FPWeapon [{0}].controller", gType.ToString());

        if (AssetDatabase.CopyAsset(copyName, relativepath))
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath(relativepath, typeof(AnimatorController)) as AnimatorController;
            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;
            var movementStateMachine = controller.layers[1].stateMachine;

            ChildAnimatorState s = rootStateMachine.states.ToList().Find(x => x.state.name == "Draw");
            s.state.motion = script.DrawName;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Hide");
            s.state.motion = script.TakeOut;
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Fire");
            s.state.motion = script.SoloFireClip;
            if (gType != GunType.Melee && gType != GunType.Grenade)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "AimFire");
                s.state.motion = script.FireAimAnimation;
            }
            else
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "QuickFire");
                s.state.motion = script.QuickFireAnim;
            }
            if (gType == GunType.Machinegun || gType == GunType.Pistol || gType == GunType.Grenade || gType == GunType.Launcher)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                s.state.motion = script.ReloadName;

                s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload Empty");
                s.state.motion = script.ReloadName;
            }
            else if (gType == GunType.Sniper || gType == GunType.Shotgun)
            {
                if (Gun.reloadPer != ReloadPer.Bullet)
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload");
                    s.state.motion = script.ReloadName;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Reload Empty");
                    s.state.motion = script.emptyReloadClip;
                }
                else
                {
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "StartReload");
                    s.state.motion = script.StartReloadAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "Insert");
                    s.state.motion = script.InsertAnim;
                    s = rootStateMachine.states.ToList().Find(x => x.state.name == "EndReload");
                    s.state.motion = script.AfterReloadAnim;
                }
            }
            s = rootStateMachine.states.ToList().Find(x => x.state.name == "Idle");
            s.state.motion = script.IdleClip;

            if (script.AnimatedMovements)
            {
                s = rootStateMachine.states.ToList().Find(x => x.state.name == "Run");
                s.state.motion = RunAnim;

                s = movementStateMachine.states.ToList().Find(x => x.state.name == "Movement");
                var moveBlend = (UnityEditor.Animations.BlendTree)s.state.motion;

                var childs = moveBlend.children;
                childs[0].motion = script.IdleClip;
                childs[1].motion = script.IdleClip;
                childs[2].motion = WalkAnim;
                childs[3].motion = RunAnim;
                moveBlend.children = childs;
            }

            EditorUtility.SetDirty(controller);
            if (script.useMultipleAnimators)
            {
                script.animators[0].runtimeAnimatorController = controller;
                EditorUtility.SetDirty(script.animators[0]);
            }
            else
            {
                _animator.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(_animator);
            }
        }
    }

    void AnimationGUI()
    {
        GUILayout.BeginHorizontal("box");
        script.DrawName = EditorGUILayout.ObjectField("Draw Animation", script.DrawName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.DrawSpeed = EditorGUILayout.Slider(script.DrawSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal("box");
        script.TakeOut = EditorGUILayout.ObjectField("Hide Animation", script.TakeOut, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
        script.HideSpeed = EditorGUILayout.Slider(script.HideSpeed, 0.1f, 3, GUILayout.Width(125));
        GUILayout.EndHorizontal();

        if (gType == GunType.Machinegun || gType == GunType.Pistol)
        {
            GUILayout.BeginVertical("box");
            script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.emptyReloadClip = EditorGUILayout.ObjectField("Empty Reload Animation", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndVertical();

            if (Gun.overrideQuickMeleeAttack) script.QuickFireAnim = EditorGUILayout.ObjectField("Meele Attack Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            // we keep showing this for backwards compatibility only.
            if (script.FireAimAnimation != null)
            {
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }

            script.fireBlendMethod = (bl_WeaponAnimation.FireBlendMethod)EditorGUILayout.EnumPopup("Fire Blend Method", script.fireBlendMethod);
            if (script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireRateCrossFade || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
            {
                script.crossfadeLenght = EditorGUILayout.Slider("Crossfade Lenght", script.crossfadeLenght, 0.0f, 0.5f);
            }
            if (script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeed || script.fireBlendMethod == bl_WeaponAnimation.FireBlendMethod.FireSpeedCrossFade)
                script.FireSpeed = EditorGUILayout.Slider("Fire Speed", script.FireSpeed, 0.1f, 3);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(aimFireClipsProp, true);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fireClipsProp, true);
            EditorGUI.indentLevel--;
        }
        else if (gType == GunType.Shotgun || gType == GunType.Sniper)
        {
            if (Gun.reloadPer == ReloadPer.Bullet)
            {
                GUILayout.BeginHorizontal("box");
                script.StartReloadAnim = EditorGUILayout.ObjectField("Start Reload", script.StartReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                script.InsertAnim = EditorGUILayout.ObjectField("Insert Bullet", script.InsertAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.InsertSpeed = EditorGUILayout.Slider(script.InsertSpeed, 0.1f, 3, GUILayout.Width(125));
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("box");
                script.AfterReloadAnim = EditorGUILayout.ObjectField("End Reload", script.AfterReloadAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.emptyReloadClip = EditorGUILayout.ObjectField("End Reload Empty", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box");
                script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                script.emptyReloadClip = EditorGUILayout.ObjectField("Empty Reload Animation", script.emptyReloadClip, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndVertical();
            }

            if (Gun.overrideQuickMeleeAttack) script.QuickFireAnim = EditorGUILayout.ObjectField("Meele Attack Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            // we keep showing this for backwards compatibility only.
            if (script.FireAimAnimation != null)
            {
                GUILayout.BeginHorizontal("box");
                script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                GUILayout.EndHorizontal();
            }

            script.FireSpeed = EditorGUILayout.Slider("Fire Speed", script.FireSpeed, 0.1f, 3);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(aimFireClipsProp, true);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(fireClipsProp, true);
            EditorGUI.indentLevel--;
        }
        else if (gType == GunType.Grenade || gType == GunType.Launcher)
        {
            if (gType == GunType.Grenade)
            {
                // we keep showing this for backwards compatibility only.
                if (script.FireAimAnimation != null)
                {
                    GUILayout.BeginHorizontal("box");
                    script.FireAimAnimation = EditorGUILayout.ObjectField("Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
                    GUILayout.EndHorizontal();
                }

                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(aimFireClipsProp, true);
                EditorGUI.indentLevel--;
            }
            GUILayout.BeginVertical("box");
            script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireAimAnimation = EditorGUILayout.ObjectField("Aim Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndVertical();
            GUILayout.BeginHorizontal("box");
            script.ReloadName = EditorGUILayout.ObjectField("Reload Animation", script.ReloadName, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            GUILayout.EndHorizontal();
            script.HasParticles = EditorGUILayout.ToggleLeft("Use Particles", script.HasParticles, EditorStyles.toolbarPopup);
            if (script.HasParticles)
            {
                script.ParticleRate = EditorGUILayout.Slider("Particle Rate", script.ParticleRate, 0.1f, 10);
                var prop = serializedObject.FindProperty("Particles");
                serializedObject.Update();
                EditorGUILayout.PropertyField(prop, true);
                serializedObject.ApplyModifiedProperties();
            }
            if (gType == GunType.Launcher)
            {
                script.FireSpeed = EditorGUILayout.Slider(script.FireSpeed, 0.1f, 3);
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(fireClipsProp, true);
                EditorGUI.indentLevel--;
            }
        }
        else if (gType == GunType.Melee)
        {
            script.QuickFireAnim = EditorGUILayout.ObjectField("Quick Fire Animation", script.QuickFireAnim, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireAimAnimation = EditorGUILayout.ObjectField("Single Fire Animation", script.FireAimAnimation, typeof(AnimationClip), allowSceneObjects) as AnimationClip;
            script.FireSpeed = EditorGUILayout.Slider("Fire Speed", script.FireSpeed, 0.1f, 3);

            GUILayout.BeginHorizontal("box");
            if (script.FireAnimations != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(fireClipsProp, true);
            }
            else if (script.QuickFireAnim != null)
            {
                script.FireAnimations = new AnimationClip[] { script.QuickFireAnim };
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndHorizontal();
        }
    }
}