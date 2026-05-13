using MFPS.Internal.Structures;
using MFPSEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(bl_Gun))]
public class bl_GunEditor : Editor
{
    private ReorderableList list;
    private bl_GameData GameData;
    private bl_Gun script;
    bool allowSceneObjects;
    bl_PlayerReferences playerReferences;
    bl_PlayerPlaceholder playerPlaceholder;
    Texture2D aimIcon;
    private int oldID = -1;
    private const int buttonWidth = 100;
    private const int buttonHeight = 20;
    private const int buttonMargin = 2;
    private readonly string[] tabOptions = new string[]
    {
        "General", "Bullet" , "Ammo", "Aim", "Muzzle", "Recoil", "Audio",
    };
    private readonly string[] fireTypeNames = new string[] { "Automatic", "Single", "Semi-Auto" };
    private bool[] attentionPoints;
    private bool searchIdOpen = false;
    private string searchID = "";
    private string[] weaponsNames;
    private bool missingWeaponLayer = false;
    private int CurrentTab
    {
        get => script._editorTabId;
        set => script._editorTabId = value;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty(Dependency.GOListPropiertie), true, true, true, true);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        list.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "On No Ammo Desactive"); };
        GameData = bl_GameData.Instance;
        script = (bl_Gun)target;
        playerReferences = script.transform.GetComponentInParent<bl_PlayerReferences>();
        playerPlaceholder = script.transform.GetComponentInParent<bl_PlayerPlaceholder>();
        aimIcon = Resources.Load("content/Images/editor-aim-icon", typeof(Texture2D)) as Texture2D;
        if (script != null)
        {
            script.weaponRenders = script.transform.GetComponentsInChildren<Renderer>();
            if (script.playerSettings == null) { script.playerSettings = script.transform.root.GetComponent<bl_PlayerSettings>(); }
        }

        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        SetAttentionPoints();
        CheckWeaponLayer();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();
        allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUI.BeginChangeCheck();

        TabMenu();
        GUILayout.Space(4);

        var r = EditorGUILayout.BeginVertical();
        {
            var hr = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton);
            EditorGUI.DrawRect(hr, new Color(0, 0, 0, 0.1f));
            EditorGUI.LabelField(hr, $"<b>{tabOptions[CurrentTab]}</b>", MFPSEditorStyles.CenterTextStyle);

            GUILayout.Space(2);
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.1f));
            if (CurrentTab == 0) DrawGlobalSettings();
            if (CurrentTab == 1) DrawBulletSettings();
            if (CurrentTab == 2) DrawAmmoSettings();
            if (CurrentTab == 3) DrawAimSettings();
            if (CurrentTab == 4) DrawMuzzle();
            if (CurrentTab == 5) DrawRecoil();
            if (CurrentTab == 6) DrawAudioSettings();
            GUILayout.Space(2);
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            SetAttentionPoints();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void TabMenu()
    {
        // Get the current inspector width
        var viewWidth = EditorGUIUtility.currentViewWidth;

        // Calculate how many buttons fit in a row
        var buttonsPerRow = Mathf.FloorToInt((viewWidth - buttonMargin) / (buttonWidth + buttonMargin));

        // Draw the buttons in a grid
        int i = 0;
        while (i < tabOptions.Length)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < buttonsPerRow && i < tabOptions.Length; j++, i++)
            {
                Rect br = GUILayoutUtility.GetRect(buttonWidth, buttonHeight);
                Color bColor = CurrentTab == i ? new Color(0.09803922f, 0.09803922f, 0.09803922f, 1.00f) : new Color(0.1568628f, 0.1568628f, 0.1568628f, 1.00f);
                EditorGUI.DrawRect(br, bColor);
                GUI.Label(br, tabOptions[i], TutorialWizard.Style.GetUnityStyle("it-center", MFPSEditorStyles.TextStyle, (ref GUIStyle style) =>
                {
                    style.alignment = TextAnchor.MiddleCenter;
                }));
                if (CurrentTab == i)
                {
                    var lr = br;
                    lr.height = 1;
                    lr.y += br.height - 1;
                    EditorGUI.DrawRect(lr, new Color(1f, 0.9882353f, 0.003921569f, 1.00f));
                }
                if (GUI.Button(br, GUIContent.none, GUIStyle.none))
                {
                    CurrentTab = i;
                }

                if (attentionPoints != null && attentionPoints.Length > 0 && attentionPoints[i])
                {
                    var ar = br;
                    ar.x += br.width - 10;
                    ar.y += (ar.height * 0.5f) - 3;
                    ar.width = 6;
                    ar.height = 6;

                    EditorGUI.DrawRect(ar, new Color(1f, 0.9882353f, 0.003921569f, 1.00f));
                }

                GUILayout.Space(2);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGlobalSettings()
    {
        DrawGunPreview();

        EditorGUILayout.BeginHorizontal();
        {
            script.GunID = EditorGUILayout.Popup($"Gun ID ({script.GunID})", script.GunID, GameData.AllWeaponStringList(false, true));
            if (oldID != script.GunID)
            {
                script.Info = null;
                oldID = script.GunID;
            }
            if (GUILayout.Button("Search", GUILayout.Width(60)))
            {
                searchIdOpen = !searchIdOpen;
            }
        }
        EditorGUILayout.EndHorizontal();

        SearchBox();

        script.CrossHairScale = EditorGUILayout.Slider("Crosshair Scale", script.CrossHairScale, 1, 30);
        if (script.Info.Type == GunType.Melee || script.Info.Type == GunType.Grenade)
        {
            script.m_AllowQuickFire = Toggle("Allow Quick Fire", script.m_AllowQuickFire);
            if (script.m_AllowQuickFire)
            {
                script.quickFireTimeMode = (WeaponQuickFireTimeMode)EditorGUILayout.EnumPopup(new GUIContent("Quick Fire Time Mode", "How is calculated the time that takes the quick fire."), script.quickFireTimeMode);
                if (script.quickFireTimeMode == WeaponQuickFireTimeMode.FixedTime)
                {
                    script.quickFireTime = EditorGUILayout.FloatField("Quick Fire Time", script.quickFireTime);
                }
            }
        }
        script.customWeapon = EditorGUILayout.ObjectField("Custom Weapon Logic", script.customWeapon, typeof(bl_CustomGunBase), true) as bl_CustomGunBase;

        script.overrideQuickMeleeAttack = Toggle("Has Melee Capability", script.overrideQuickMeleeAttack, "Indicates whether the weapon allows melee attacks or uses a separate weapon for quick melee attacks.");
        script.canBeThrown = Toggle("Can be Thrown", script.canBeThrown, "Indicates whether the weapon can be thrown by the player.");
        if (script.Info.Type == GunType.Grenade)
        {
            script.ThrowByAnimation = Toggle("Throw By Animation Event", script.ThrowByAnimation);
            script.canBeTakenWhenIsEmpty = Toggle("Can be taken when is empty", script.canBeTakenWhenIsEmpty);
            if (!script.ThrowByAnimation)
            {
                script.DelayFire = EditorGUILayout.FloatField("Throw delay", script.DelayFire);
                script.quickFireDelay = EditorGUILayout.FloatField("Quick throw delay", script.quickFireDelay);
            }
        }
        script.centerPivot = EditorGUILayout.ObjectField("Center Pivot", script.centerPivot, typeof(Transform), true) as Transform;

        if (missingWeaponLayer)
        {
            EditorGUILayout.HelpBox("Some child objects are not in the 'Weapon' layer. Make sure all weapon parts that are supposed to be seen in the player camera are in the 'Weapon' layer to avoid weapon clipping issues.", MessageType.Warning);
            if (GUILayout.Button("Fix Layers"))
            {
                foreach (Transform t in script.transform)
                {
                    SetLayerRecursively(t.gameObject, LayerMask.NameToLayer("Weapons"));
                    EditorUtility.SetDirty(t.gameObject);
                }
                missingWeaponLayer = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SearchBox()
    {
        if (!searchIdOpen) return;

        EditorGUILayout.BeginVertical("window");
        // draw a search box ui
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(4);
        GUI.SetNextControlName("SearchBox");
        searchID = EditorGUILayout.TextField("", searchID, "SearchTextField");
        if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18)))
        {
            searchID = "";
            searchIdOpen = false;
            GUI.FocusControl(null);
        }
        GUILayout.Space(4);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();

        EditorGUILayout.Separator();

        if (!string.IsNullOrEmpty(searchID))
        {

            weaponsNames ??= GameData.AllWeaponStringList();

            for (int i = 0; i < weaponsNames.Length; i++)
            {
                if (!weaponsNames[i].ToLower().Contains(searchID.ToLower())) continue;

                if (GUILayout.Button(new GUIContent(weaponsNames[i], $"Gun ID {i}"), "ButtonRight"))
                {
                    script.GunID = i;
                    searchID = "";
                    searchIdOpen = false;
                }
            }
            GUILayout.Space(16);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Separator();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawAimSettings()
    {
        script.allowAim = Toggle("Allow Aiming", script.allowAim);
        if (!script.allowAim) return;

        EditorGUILayout.BeginVertical();
        {
            script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
            script.aimRotation = EditorGUILayout.Vector3Field("Aim Rotation", script.aimRotation);
            if (script.gameObject.activeSelf)
            {
                EditorStyles.toolbarButton.richText = true;
                Color ac = (script._aimRecord) ? Color.red : Color.yellow;

                GUI.color = ac;
                if (GUILayout.Button(new GUIContent(script._aimRecord ? " Finish Adjust Aim Position" : " Adjust Aim Position", aimIcon, "Simulate Aim"), GUILayout.Height(20)))
                {
                    if (!script._aimRecord)
                    {
                        script._defaultPosition = script.transform.localPosition;
                        script._defaultRotation = script.transform.localEulerAngles;
                        script.transform.localPosition = script.AimPosition;
                        script.transform.localEulerAngles = script.aimRotation;
                        script._aimRecord = true;
                        ActiveEditorTracker.sharedTracker.isLocked = true;

#if LOVATTO_DEV
                        var adjuster = EditorWindow.GetWindow<AimAdjuster>();
                        adjuster.Set(script);
#endif
                    }
                    else
                    {
                        script.AimPosition = script.transform.localPosition;
                        script.aimRotation = script.transform.localEulerAngles;
                        script.transform.localPosition = script._defaultPosition;
                        script.transform.localEulerAngles = script._defaultRotation;
                        script._aimRecord = false;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                        ActiveEditorTracker.sharedTracker.isLocked = false;
                    }
                    if (playerPlaceholder != null) playerPlaceholder.CalibratingAim = script._aimRecord;
                }
            }
            GUI.color = Color.white;
        }
        EditorGUILayout.EndVertical();

        script.aimZoom = EditorGUILayout.Slider(new GUIContent("Aim Zoom", "Less is more zoom"), script.aimZoom, 0.0f, 179);
        script.AimSmooth = EditorGUILayout.Slider(new GUIContent("Aim Smoothness", "The smoothness of the aim position transition, higher value result in faster transition."), script.AimSmooth, 0.01f, 30f);
        script.useSmooth = Toggle("Smooth Aim Movement", script.useSmooth);
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawAmmoSettings()
    {
        script.useAmmo = Toggle("Use Ammo", script.useAmmo);
        if (!script.useAmmo) return;

        if (IsNotMeeleOrNade())
        {
            script.ammoType = (AmmoType)EditorGUILayout.EnumPopup(new GUIContent("Ammo Type", "The type of ammo that this weapon use."), script.ammoType);
        }
        script.bulletsPerClip = EditorGUILayout.IntField(new GUIContent(script.Info.Type == GunType.Grenade ? "Carry Capacity" : "Bullets Per Mag", "The number of bullet that each magazine have"), script.bulletsPerClip);
        script.maxNumberOfClips = EditorGUILayout.IntField(new GUIContent(script.Info.Type == GunType.Grenade ? "Max Carry Capacity" : "Max Number Of Mags", "The max number of magazines the player can carry."), script.maxNumberOfClips);
        script.numberOfClips = EditorGUILayout.IntSlider(new GUIContent(script.Info.Type == GunType.Grenade ? "Initial Carry" : "Initial Number Of Mags", "The number of clips/magazines the player will start with"), script.numberOfClips, 0, script.maxNumberOfClips);
        if (IsNotMeeleOrNade())
        {
            script.reloadPer = (ReloadPer)EditorGUILayout.EnumPopup(new GUIContent("Reload Per", "Determine how to reload the weapon, for bolt cycling weapons like snipers and shotguns ReloadPer = Bullet is indicated, for other rifles that reload the whole magazine at once, use Magazine mode."), script.reloadPer);
            GUILayout.Space(2);
        }
        else
        {
            script.reloadPer = ReloadPer.Magazine;
        }
        script.AutoReload = Toggle("Auto Reload", script.AutoReload);
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBulletSettings()
    {
        GunType gunType = script.Info.Type;
        if (gunType == GunType.Melee)
        {
            GUILayout.Label("Melee use hitscan to detect collisions.", MFPSEditorStyles.CenterTextStyle);
            return;
        }
        GUILayout.Space(2);
        if (script.bulletInstanceMethod == BulletInstanceMethod.Pooled)
        {
            script.BulletName = EditorGUILayout.TextField(new GUIContent("Projectile Prefab Name", "The pooled prefab name of the bullet, this is the name that you defined in the prefab pool list for the bullet."), script.BulletName, EditorStyles.helpBox);
        }
        else
        {
            script.bulletPrefab = EditorGUILayout.ObjectField("Projectile Prefab", script.bulletPrefab, typeof(GameObject), false) as GameObject;
        }
        script.bulletInstanceMethod = (BulletInstanceMethod)EditorGUILayout.EnumPopup(new GUIContent("Projectile Instance Method", "Is the bullet a pool prefab or it's instance a prefab on each fire?"), script.bulletInstanceMethod);
        if (gunType != GunType.Grenade && gunType != GunType.Melee)
        {
            script.supportedFireTypes = (bl_WeaponBase.FireType)EditorGUILayout.MaskField(new GUIContent("Supported Fire Modes", "Define the fire modes that the player can switch with this weapon"), (int)script.supportedFireTypes, fireTypeNames);
            script.defaultFireType = (bl_WeaponBase.FireType)EditorGUILayout.EnumPopup(new GUIContent("Default Fire Type", "Define the default fire mode of this weapon."), script.defaultFireType);
        }
        else
        {
            script.supportedFireTypes = bl_WeaponBase.FireType.Single;
            script.defaultFireType = bl_WeaponBase.FireType.Single;
        }
        script.shellShotOption = (ShellShotOptions)EditorGUILayout.EnumPopup(new GUIContent("Shell Mode", "Single Shot = One bullet per shot.\nPellet Shot = Multiple bullets (pellets) per shot, usually used for shotguns."), script.shellShotOption);
        if (script.shellShotOption == ShellShotOptions.PelletShot)
        {
            script.pelletsPerShot = EditorGUILayout.IntSlider("Pellet Per Shots", script.pelletsPerShot, 1, 10);
        }

        if (gunType == GunType.Grenade || gunType == GunType.Launcher)
        {
            script.projectileForceMode = (ForceMode)EditorGUILayout.EnumPopup(new GUIContent("Force Mode", "The force mode applied to the projectile upon instance it"), script.projectileForceMode);
            script.projectileDirectionMode = (ProjectileDirectionMode)EditorGUILayout.EnumPopup(new GUIContent("Projectile Direction Mode", "How the projectile direction is calculated."), script.projectileDirectionMode);
        }

        if (gunType == GunType.Grenade)
        {
            script.bulletDropFactor = EditorGUILayout.FloatField(new GUIContent("Upward Force", "The throw up force"), script.bulletDropFactor);
        }
        else
        {
            script.bulletDropFactor = EditorGUILayout.Slider(new GUIContent("Projectile Drop Factor", "How much the bullet drop over time, 0 = hitscan bullet"), script.bulletDropFactor, 0, 10);
        }

        script.effectiveFiringRange = EditorGUILayout.Slider(new GUIContent("Effective Firing Range", "maximum distance at which a weapon can be expected to be accurate and achieve the intended base damage."), script.effectiveFiringRange, 1, script.Info.Range);

        if (gunType != GunType.Grenade && gunType != GunType.Launcher)
        {
            if (script.effectiveFiringRange < script.Info.Range)
            {
                script.effectivinessDropoff = EditorGUILayout.CurveField(new GUIContent("Effectiviness Dropoff", "The damage dropoff curve after the effective firing range."), script.effectivinessDropoff);
            }
        }

        script.dragCoefficient = EditorGUILayout.Slider(new GUIContent("Drag Coefficient", "Projectile decelerating due to air resistance (drag)\ncontrol how quickly the bullet slows down, lower values (0-2) experiences less air resistance as it travels, resulting in slower deceleration\nThe bullet maintains its speed longer and travels farther before losing significant velocity.\n\nRifle bullets:\n0.2 - 0.3\n\nHandgun bullets:\n0.4−0.6\n\nShotgun pellets:\n0.5−0.7"), script.dragCoefficient, 0, 10);

        string ps = (gunType == GunType.Grenade || gunType == GunType.Launcher) ? "Projectile Force" : "Projectile Speed";
        script.bulletSpeed = EditorGUILayout.FloatField(new GUIContent(ps, "m/s"), script.bulletSpeed);
        if (script.supportedFireTypes.IsEnumFlagPresent(bl_WeaponBase.FireType.Semi))
        {
            script.roundsPerBurst = EditorGUILayout.IntSlider(new GUIContent("Projectiles Per Burst", "Number of bullets/projectiles shoot per burst."), script.roundsPerBurst, 1, 10);
            script.lagBetweenBurst = EditorGUILayout.FloatField(new GUIContent("Lag Between Burst", "The cooldown/lag between burst fires, the total time is calculated by the fire rate * number of shoot bullets + this value."), script.lagBetweenBurst);
        }

        string sf = gunType == GunType.Launcher ? "Impact Radius" : "Impact Force";
        script.impactForce = EditorGUILayout.IntSlider(new GUIContent(sf, "The force applied to the hit object (if that object have a Rigidbody)"), script.impactForce, 0, 30);
        script.delayFireOnSprinting = EditorGUILayout.Slider(new GUIContent("First Shot Delay On Sprint", "The time delay before shot when fire and the player is running."), script.delayFireOnSprinting, 0, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawMuzzle()
    {
        script.muzzlePoint = EditorGUILayout.ObjectField(new GUIContent("Fire Point", "Point from where the projectile will be instantiated."), script.muzzlePoint, typeof(Transform), allowSceneObjects) as Transform;
        script.weaponFX = EditorGUILayout.ObjectField(new GUIContent("Weapon FX", "The weapon VFX setup containing the muzzleflash and other weapon particle effects, this could be a prefab or an instance inside of the weapon, the "), script.weaponFX, typeof(bl_WeaponFXBase), true) as bl_WeaponFXBase;
        if (script.weaponFX == null)
        {
            script.muzzleFlash = EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
            script.shell = EditorGUILayout.ObjectField("Shell", script.shell, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
        }

        if (script.Info.Type == GunType.Grenade)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAmmoLauncher"), new GUIContent("Projectil Mesh", "Assign the grenade/projectile mesh that will be disable when run out of ammo."), true);
            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawRecoil()
    {
        if (script.Info.Type != GunType.Melee)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Spread Range", GUILayout.Width(EditorGUIUtility.labelWidth - 10));
                script.spreadMinMax.x = EditorGUILayout.FloatField(script.spreadMinMax.x, GUILayout.Width(30));
                EditorGUILayout.MinMaxSlider(ref script.spreadMinMax.x, ref script.spreadMinMax.y, 0, 7);
                script.spreadMinMax.y = EditorGUILayout.FloatField(script.spreadMinMax.y, GUILayout.Width(30));
            }
            script.spreadAimMultiplier = EditorGUILayout.Slider(new GUIContent("Aim Spread Multiplier", "How much aiming affect the aim spread, a values less than 1 reduce the spread, e.g: 0.5 = half of the base spread."), script.spreadAimMultiplier, 0, 1);
            script.spreadPerSecond = EditorGUILayout.Slider(new GUIContent("Spread Per Seconds", "How much does the spread increase over time on continuos shooting?"), script.spreadPerSecond, 0, 1);
            script.decreaseSpreadPerSec = EditorGUILayout.Slider("Decrease Spread Per Sec", script.decreaseSpreadPerSec, 0, 1);
        }
        GUILayout.Space(4);
        EditorGUILayout.HelpBox("IMPORTANT!, the following presets could be shared with other weapons, modifying those values will also affect the other weapons using these presets, if you want to use an unique setup for this weapon, create a new preset scriptable object and replace these.", MessageType.Info);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shakerPresent"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("recoilSettings"), true);
        if (script.recoilSettings == null)
        {
            script.RecoilAmount = EditorGUILayout.Slider("Recoil", script.RecoilAmount, 0.1f, 10);
            script.RecoilSpeed = EditorGUILayout.Slider("Recoil Speed", script.RecoilSpeed, 0.1f, 10);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawAudioSettings()
    {
        script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        EditorGUILayout.MinMaxSlider(new GUIContent($"Fire Pitch Range {script.fireSoundPitch.x:0.00}-{script.fireSoundPitch.y:0.00}", "Fire sound pitch range, to add variation each time the fire sound plays."), ref script.fireSoundPitch.x, ref script.fireSoundPitch.y, 0.5f, 2);
        if (script.overrideQuickMeleeAttack) script.bashAttackSound = EditorGUILayout.ObjectField("Bash Attack Sound", script.bashAttackSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        script.DryFireSound = EditorGUILayout.ObjectField("Empty Fire Sound", script.DryFireSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        script.TakeSound = EditorGUILayout.ObjectField("Drawing Weapon Sound", script.TakeSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
        script.aimSound = EditorGUILayout.ObjectField("Aim Sound", script.aimSound, typeof(AudioClip), allowSceneObjects) as AudioClip;

        if (script.Info.Type == GunType.Melee) return;

        script.SoundReloadByAnim = Toggle("Sounds Played by Animation?", script.SoundReloadByAnim);
        if (!script.SoundReloadByAnim)
        {
            script.ReloadSound = EditorGUILayout.ObjectField("Reload Start", script.ReloadSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.ReloadSound2 = EditorGUILayout.ObjectField("Reload Middle", script.ReloadSound2, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.ReloadSound3 = EditorGUILayout.ObjectField("Reload Finish", script.ReloadSound3, typeof(AudioClip), allowSceneObjects) as AudioClip;
            script.boltSound = EditorGUILayout.ObjectField("Bolt Cycling Sound", script.boltSound, typeof(AudioClip), allowSceneObjects) as AudioClip;
            if (script.boltSound != null)
            {
                script.delayForSecondFireSound = EditorGUILayout.Slider(new GUIContent("Delay for Bolt Cycling", "The delay before playing the bolt cycling sound after playing the fire sound"), script.delayForSecondFireSound, 0.0f, 2.0f);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGunPreview()
    {
        EditorGUILayout.Space(4);
        var fr = EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space(10);
            var info = script.Info;
            Rect iconRect;

            var weaponIcon = info.GunIcon;
            if (weaponIcon != null)
            {
                float ratio = 128f / weaponIcon.texture.width;
                if (weaponIcon.texture.height > weaponIcon.texture.width)
                {
                    ratio = 128f / weaponIcon.texture.height;
                }
                float height = weaponIcon.texture.height * ratio;
                float width = weaponIcon.texture.width * ratio;

                height = Mathf.Max(height, 50);

                iconRect = GUILayoutUtility.GetRect(width, height);
                GUI.DrawTexture(iconRect, weaponIcon.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                iconRect = GUILayoutUtility.GetRect(128, 80);
                EditorGUI.DrawRect(iconRect, new Color(0, 0, 0, 0.1f));
            }

            var r = iconRect;
            r.x += r.width + 5;
            r.width = fr.width - r.width - 5;
            r.height = 20;

            EditorGUI.LabelField(r, $"<b><size=14>{info.Name}</size></b>", MFPSEditorStyles.TextStyle);
            r.y += 12;
            EditorGUI.LabelField(r, $"<size=10><color=#B9B9B9FF>{info.Type}</color></size>", MFPSEditorStyles.TextStyle);

            r = fr;
            r.y += r.height + 6;
            r.height = 1;
            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.3f));

            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(12);

        var rr = fr;

        rr.x += rr.width - 110;
        rr.y += rr.height - 18;
        rr.width = 50;
        rr.height = 18;

        if (GUI.Button(rr, new GUIContent("Add", "Add a new weapon info")))
        {
            bl_AddWeaponWindow.ShowWindow(script);
        }

        rr.x += 50;
        if (GUI.Button(rr, "Edit"))
        {
            bl_MFPSManagerWindow.Open(bl_MFPSManagerWindow.WindowType.Weapons, new string[] { $"{script.GunID}" });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetAttentionPoints()
    {
        var info = script.Info;
        attentionPoints = new bool[tabOptions.Length];

        if (info.Type != GunType.Melee)
        {
            if (script.bulletInstanceMethod == BulletInstanceMethod.Instanced && script.bulletPrefab == null)
            {
                attentionPoints[1] = true;
            }
            if (script.supportedFireTypes == 0) attentionPoints[1] = true;
            if (script.muzzlePoint == null) attentionPoints[4] = true;
            if (script.recoilSettings == null || script.shakerPresent == null) attentionPoints[5] = true;
            if (script.FireAudioClip == null) attentionPoints[6] = true;
        }

    }

    private void CheckWeaponLayer()
    {
        // check if the tranform children are in the 'Weapon' layer
        foreach (Transform t in script.transform)
        {
            if (t.gameObject.layer != LayerMask.NameToLayer("Weapons"))
            {
                missingWeaponLayer = true;
            }
        }
    }

    private void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null) return;

        root.layer = layer;

        // Instead of foreach (allocates), use for-loop for performance.
        Transform t = root.transform;
        int childCount = t.childCount;
        for (int i = 0; i < childCount; i++)
        {
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!script._aimRecord || playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = playerReferences.playerCamera.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.CircleHandleCap(0, origin, Quaternion.identity, 0.2f, EventType.Repaint);
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Toggle(string title, bool value, string tooltip = "")
    {
        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toggle);
        GUILayout.Space(2);
        return MFPSEditorStyles.FeatureToogle(r, value, title, tooltip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Toggle(GUIContent title, bool value)
    {
        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toggle);
        GUILayout.Space(2);
        return MFPSEditorStyles.FeatureToogle(r, value, title.text);
    }

    private bool IsNotMeeleOrNade()
    {
        return script.Info.Type != GunType.Melee && script.Info.Type != GunType.Grenade;
    }

    /// <summary>
    /// Compact editor window for adding new weapons
    /// </summary>
    public class bl_AddWeaponWindow : EditorWindow
    {
        private bl_GunInfo newWeaponInfo;
        private Vector2 scrollPosition;
        private bl_Gun targetGun;
        private SerializedObject serializedWeaponInfo;
        private SerializedProperty nameProp;
        private SerializedProperty typeProp;
        private SerializedProperty activeProp;
        private SerializedProperty damageProp;
        private SerializedProperty fireRateProp;
        private SerializedProperty reloadTimeProp;
        private SerializedProperty rangeProp;
        private SerializedProperty accuracyProp;
        private SerializedProperty weightProp;
        private SerializedProperty unlockabilityProp;
        private SerializedProperty iconProp;

        public static void ShowWindow(bl_Gun gun)
        {
            var window = GetWindow<bl_AddWeaponWindow>(true, "Add New Weapon", true);
            window.targetGun = gun;
            window.minSize = new Vector2(350, 500);
            window.maxSize = new Vector2(350, 700);
            window.Initialize();
            window.ShowUtility();
        }

        private void Initialize()
        {
            newWeaponInfo = new bl_GunInfo
            {
                Name = "New Weapon",
                Type = GunType.Machinegun,
                Active = true,
                Damage = 30,
                FireRate = 0.1f,
                ReloadTime = 2.5f,
                Range = 100,
                Accuracy = 1f,
                Weight = 1f,
                Unlockability = new MFPSItemUnlockability()
            };

            CreateSerializedProperties();
        }

        private void CreateSerializedProperties()
        {
            var scriptableObject = ScriptableObject.CreateInstance<bl_GunInfoWrapper>();
            scriptableObject.gunInfo = newWeaponInfo;
            serializedWeaponInfo = new SerializedObject(scriptableObject);

            nameProp = serializedWeaponInfo.FindProperty("gunInfo.Name");
            typeProp = serializedWeaponInfo.FindProperty("gunInfo.Type");
            activeProp = serializedWeaponInfo.FindProperty("gunInfo.Active");
            damageProp = serializedWeaponInfo.FindProperty("gunInfo.Damage");
            fireRateProp = serializedWeaponInfo.FindProperty("gunInfo.FireRate");
            reloadTimeProp = serializedWeaponInfo.FindProperty("gunInfo.ReloadTime");
            rangeProp = serializedWeaponInfo.FindProperty("gunInfo.Range");
            accuracyProp = serializedWeaponInfo.FindProperty("gunInfo.Accuracy");
            weightProp = serializedWeaponInfo.FindProperty("gunInfo.Weight");
            unlockabilityProp = serializedWeaponInfo.FindProperty("gunInfo.Unlockability");
            iconProp = serializedWeaponInfo.FindProperty("gunInfo.GunIcon");
        }

        private void OnGUI()
        {
            if (newWeaponInfo == null || serializedWeaponInfo == null)
            {
                Initialize();
                return;
            }

            serializedWeaponInfo.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Add New Weapon", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nameProp, new GUIContent("Name"));
                EditorGUILayout.PropertyField(typeProp, new GUIContent("Type"));
                EditorGUILayout.PropertyField(activeProp, new GUIContent("Active"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(damageProp, new GUIContent("Damage"));
                EditorGUILayout.PropertyField(fireRateProp, new GUIContent("Fire Rate"));
                EditorGUILayout.PropertyField(reloadTimeProp, new GUIContent("Reload Time"));
                EditorGUILayout.PropertyField(rangeProp, new GUIContent("Range"));
                EditorGUILayout.PropertyField(accuracyProp, new GUIContent("Accuracy"));
                EditorGUILayout.PropertyField(weightProp, new GUIContent("Weight"));
                EditorGUILayout.PropertyField(unlockabilityProp, new GUIContent("Unlockability"), true);
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(iconProp, new GUIContent("Gun Icon"));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space(10);
            }
            EditorGUILayout.EndScrollView();

            serializedWeaponInfo.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
                if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    Close();
                }

                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUILayout.Button("Add", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    AddWeapon();
                }
                GUI.backgroundColor = Color.white;

                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private void AddWeapon()
        {
            if (string.IsNullOrEmpty(newWeaponInfo.Name))
            {
                EditorUtility.DisplayDialog("Invalid Name", "Weapon name cannot be empty.", "OK");
                return;
            }

            var gameData = bl_GameData.Instance;
            if (gameData == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find GameData instance.", "OK");
                return;
            }

            newWeaponInfo.Key = bl_StringUtility.GenerateKey();

            gameData.AllWeapons.Add(newWeaponInfo);

            int newIndex = gameData.AllWeapons.Count - 1;

            if (targetGun != null)
            {
                targetGun.GunID = newIndex;
            }

            EditorUtility.SetDirty(gameData);
            AssetDatabase.SaveAssets();

            Debug.Log($"Added new weapon '{newWeaponInfo.Name}' at index {newIndex}");

            Close();
        }

        private void OnDestroy()
        {
            if (serializedWeaponInfo != null && serializedWeaponInfo.targetObject != null)
            {
                DestroyImmediate(serializedWeaponInfo.targetObject);
            }
        }
    }

    /// <summary>
    /// Wrapper class to allow serialization of bl_GunInfo
    /// </summary>
    private class bl_GunInfoWrapper : ScriptableObject
    {
        public bl_GunInfo gunInfo;
    }
}