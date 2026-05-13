using MFPSEditor;
using UnityEditor;
using UnityEngine;

public class AddPlayerTutorial : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/player/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "dDHltGGDrAA", Image = null, Type = NetworkImages.ImageType.Youtube},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "https://www.lovattostudio.com/en/wp-content/uploads/2017/03/player-selector-product-cover-925x484.png",Type = NetworkImages.ImageType.Custom},
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "3DModel", StepsLenght = 0, DrawFunctionName = nameof(DrawModelInfo) },
    new Steps { Name = "Ragdolled", StepsLenght = 3, DrawFunctionName = nameof(DrawRagdolled) },
    new Steps { Name = "Player Prefab", StepsLenght = 6, DrawFunctionName = nameof(DrawPlayerPrefab) },
    new Steps { Name = "Player Models Assets", StepsLenght = 1, DrawFunctionName = nameof(PlayerModelAssetsDoc) },
    new Steps { Name = "Animation Assets", StepsLenght = 1, DrawFunctionName = nameof(AnimationAssetsDoc) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "addpt3.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
        new GifData{ Path = "apm-atpw.gif" },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private GameObject PlayerModel;
    private Animator PlayerAnimator;
    private Avatar PlayerModelAvatar;
    private string LogLine = "";
    private ModelImporter ModelInfo;
    Editor p1editor;
    AssetStoreAffiliate playerAssets;
    AssetStoreAffiliate animationAssets;
    public TPWeaponOrientationMode weaponOrientationMode = TPWeaponOrientationMode.KeepSameLocation;
    public bool autoPoseAiming = true;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        if (playerAssets == null)
        {
            playerAssets = new AssetStoreAffiliate();
            playerAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/157287/widget-medium");
            playerAssets.FixedHeight = 0;
            playerAssets.selfScroll = false;
            playerAssets.randomize = true;
            playerAssets.onSwitchPage = () => { SetContentScrollPosition(Vector2.zero); };
        }
        if (animationAssets == null)
        {
            animationAssets = new AssetStoreAffiliate();
            animationAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555464133/widget-medium");
            animationAssets.FixedHeight = 0;
            animationAssets.selfScroll = false;
            animationAssets.randomize = true;
            animationAssets.onSwitchPage = () => { SetContentScrollPosition(Vector2.zero); };
        }
        allowTextSuggestions = true;
        SetHTMLHeaderLinks(new System.Collections.Generic.Dictionary<string, string>() { { "Community", "https://discord.gg/8zF5B4G" }, { "Website", "https://www.lovattostudio.com/en/" }, { "Get MFPS", "https://www.lovattostudio.com/en/shop/template/mfps-2-0/" } });
        SetHTMLTitle("MFPS Add Players");
        SetHTMLLink("https://lovattostudio.com/documentations/mfps2/add-players.html");
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawModelInfo()
    {
        DrawText("This tutorial will guide you step by step to replace or add the player models to the MFPS player prefabs, what you need is:");
        DrawHorizontalColumn("Player Model", "A Humanoid <b>Rigged</b> 3D Model with the standard rigged bones or any rigged that work with the unity re-targeting animator system.");
        DrawText("The Model Import <b>Rig</b> setting has to be set as <b>Humanoid</b> in order to work with retargeting animations, for it select the player model <i>(the model not a prefab)</i> and in the inspector window you will see a toolbar, go to the Rig tab and set the <b>Animation Type</b> as Humanoid, the settings should look like this:");
        DrawServerImage("img-0.png");
        DownArrow();
        DrawNote("<b>Important:</b> your model should have a correct <b>T-Pose skeleton</b> to work correctly with the re-targeting animations, if your character model have a wrong posed skeleton the animations will look weird in the player model, in order to fix the skeleton pose you can follow this video tutorial:");
        DrawYoutubeCover("Adjusting Avatar for correct animation retargeting", GetServerImage(1), "https://www.youtube.com/watch?v=dDHltGGDrAA");
        DrawHTMLYoutube("dDHltGGDrAA");
    }

    void DrawRagdolled()
    {
        if (subStep == 0)
        {
            HideNextButton = true;
            DrawText("All right, with the model ready it's time to start setting it up.\n \nThe first thing that you need to do is make a ragdoll of your new player model. Normally in Unity, you make a ragdoll manually with GameObject ➔ 3D Object ➔ Ragdoll, and then assign every player bone in the wizard window manually, but this tool will make this automatically, you simply need to drag the player model below.");
            DownArrow();
            DrawText("Drag here your player model from the <b>Project View</b>");
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), false) as GameObject;
            DrawHTMLPlaceholder();
            GUI.enabled = PlayerModel != null;
            if (DrawButton("Continue"))
            {
                AssetImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PlayerModel));
                if (importer != null)
                {
                    ModelInfo = importer as ModelImporter;
                    if (ModelInfo != null)
                    {
                        if (ModelInfo.animationType != ModelImporterAnimationType.Human)
                        {
                            ModelInfo.animationType = ModelImporterAnimationType.Human;
                            EditorUtility.SetDirty(ModelInfo);
                            ModelInfo.SaveAndReimport();
                        }
                        if (ModelInfo.animationType == ModelImporterAnimationType.Human)
                        {
                            InstancePlayerPrefab();
                        }
                        else
                        {
                            LogLine = "Your models is not setup as a <b>Humanoid</b> rig, setup it:";
                        }
                    }
                    else
                    {
                        InstancePlayerPrefab();
                    }
                }
                else
                {
                    InstancePlayerPrefab();
                }
            }
            GUI.enabled = true;
            if (!string.IsNullOrEmpty(LogLine))
            {
                GUILayout.Label(LogLine);
                if (LogLine.Contains("Humanoid"))
                {
                    DrawServerImage(0);
                }
            }
        }
        else if (subStep == 1)
        {
            HideNextButton = false;
            GUI.enabled = false;
            GUILayout.BeginVertical("box");
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), false) as GameObject;
            PlayerModelAvatar = EditorGUILayout.ObjectField("Avatar", PlayerModelAvatar, typeof(Avatar), true) as Avatar;
            if (ModelInfo != null) GUILayout.Label(string.Format("Model Rig: {0}", ModelInfo.animationType.ToString()));
            GUI.enabled = true;
            MFPSEditor.MeshSizeChecker meshChecker = null;
            if (PlayerInstantiated != null)
            {
                meshChecker = PlayerInstantiated.GetComponent<MFPSEditor.MeshSizeChecker>();
                if (meshChecker == null) meshChecker = PlayerInstantiated.AddComponent<MFPSEditor.MeshSizeChecker>();
                meshChecker.CalculateHeight();

                Space(20);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label(string.Format("Model Height: <b>{0:0.00}m</b> | Expected Height: <b>2.00m</b>", meshChecker.Height), Style.TextStyle);
                    GUILayout.Label("<i><size=10><color=#90909094>By default, the model's height is calculated by the default mesh bounds. However, on some occasions, these bounds might not align accurately with the actual boundaries of the mesh vertices. To see if this is happening with your model, look for any empty space above or below the green line gizmos. If you notice such a gap, use the <b>Mesh Vertice</b> calculation method.</color></size></i>", Style.TextStyle);
                    DrawServerImage("img-10.png");
                }
                EditorGUILayout.EndVertical();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginVertical();
                {
                    meshChecker.calculateMethod = (MFPSEditor.MeshSizeChecker.CalculateMethod)EditorGUILayout.EnumPopup("Calc Method", meshChecker.calculateMethod);
                    Space(10);

                    if (meshChecker.Height < 1.9f)
                    {
                        GUILayout.Label("<size=10><color=#FFCF92FF>The height of your character model is shorter than the default. Would you like to automatically adjust it to the standard size?</color></size>", Style.TextStyle);
                    }
                    else if (meshChecker.Height > 2.25f)
                    {
                        GUILayout.Label("<size=10><color=#FFCF92FF>The height of your character model is greater than the default. Would you like to automatically adjust it to the standard size?</color></size>", Style.TextStyle);
                    }

                    if (DrawButton("Auto Resize"))
                    {
                        if (meshChecker.Height < 2)
                        {
                            Vector3 v = PlayerInstantiated.transform.localScale;
                            float dif = 2f / meshChecker.Height;
                            v = v * dif;
                            PlayerInstantiated.transform.localScale = v;
                        }
                        else if (meshChecker.Height > 2)
                        {
                            Vector3 v = PlayerInstantiated.transform.localScale;
                            float dif = meshChecker.Height / 2;
                            v = v / dif;
                            PlayerInstantiated.transform.localScale = v;
                        }
                    }
                }
                EditorGUILayout.EndVertical();

                if (EditorGUI.EndChangeCheck())
                {
                    meshChecker.CalculateHeight(true);
                    SceneView.RepaintAll();
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

            }

            GUILayout.EndVertical();
            GUI.enabled = true;
            if (PlayerModelAvatar != null && PlayerAnimator != null)
            {
                DownArrow();
                DrawText("Once your model is sized correctly, click the button below to automatically ragdolled.");
                if (DrawButton("Build Ragdoll"))
                {
                    if (MFPSEditor.AutoRagdoller.Build(PlayerAnimator))
                    {
                        if (meshChecker != null) DestroyImmediate(meshChecker);
                        else
                        {
                            meshChecker = PlayerInstantiated.GetComponent<MFPSEditor.MeshSizeChecker>();
                            if (meshChecker != null) DestroyImmediate(meshChecker);
                        }

                        Selection.activeTransform = PlayerInstantiated.transform;

                        var view = (SceneView)SceneView.sceneViews[0];
                        if (view != null) view.ShowNotification(new GUIContent("Ragdoll Created!"));
                        NextStep();
                    }
                }
            }
            else
            {
                GUILayout.Label("<color=yellow>Hmm... something is happening here, can't get the model avatar.</color>", EditorStyles.label);
            }
            DrawHTMLPlaceholder();
        }
        else if (subStep == 2)
        {
            DrawText("Right now your player model <i>(in the scene)</i> should look similar to this:");
            DrawServerImage(2);
            DrawText("Now, these <b>Box</b> and <b>Capsule</b> Colliders are the player HitBoxes <i>(the colliders that detect when a bullet hit the player)</i>, in some models these colliders may not be place/oriented in the right axes causing a problem which will be that some parts of the player will not be hitteable in game.\n\nIt's crucial to ensure that these colliders envelop the player model accurately. You might need to adjust their parameters or orientation, especially for the head's Sphere Collider. Often, it doesn't fit properly by default, so manual adjustments to its radius might be necessary.\n \nOnce everything appears correctly set up, you're prepared to move on to the next step.");

        }
    }

    private void InstancePlayerPrefab()
    {
        PlayerInstantiated = PrefabUtility.InstantiatePrefab(PlayerModel) as GameObject;
        UnPackPrefab(PlayerInstantiated);
        PlayerInstantiated.transform.rotation = Quaternion.identity;
        PlayerAnimator = PlayerInstantiated.GetComponent<Animator>();
        PlayerModelAvatar = PlayerAnimator.avatar;
        var view = (SceneView)SceneView.sceneViews[0];
        view.camera.transform.position = PlayerInstantiated.transform.position + ((PlayerInstantiated.transform.forward * 10) + Vector3.up);
        view.LookAt(PlayerInstantiated.transform.position);
        EditorGUIUtility.PingObject(PlayerInstantiated);
        Selection.activeTransform = PlayerInstantiated.transform;
        subStep++;
    }

    void DrawPlayerInstanceButton(GameObject player)
    {
        if (player == null) return;

        if (GUILayout.Button(player.name, GUILayout.Width(150)))
        {
            PlayerModel = PlayerInstantiated;
            PlayerInstantiated = PrefabUtility.InstantiatePrefab(player) as GameObject;
            UnPackPrefab(PlayerInstantiated);
            Selection.activeObject = PlayerInstantiated;
            EditorGUIUtility.PingObject(PlayerInstantiated);
            NextStep();
        }
        GUILayout.Space(4);
    }

    void DrawPlayerPrefab()
    {
        if (subStep == 0)
        {
            DrawText("Okay, now that we have the player model ragdoll, we can integrate it to a player prefab, for this we would need to open one of the existing player prefabs.\n\nBelow you will have a list of all your available player prefabs in your project, click on the one that you want to use as reference to replace it's model.");
            DrawHTMLPlaceholder();
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                {
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player1.gameObject);
                    DrawPlayerInstanceButton(bl_GameData.Instance.Player2.gameObject);
#if PSELECTOR
                    foreach (var p in bl_PlayerSelector.Data.AllPlayers)
                    {
                        if (p == null || p.Prefab == null) continue;
                        DrawPlayerInstanceButton(p.Prefab);
                    }
#endif
                }
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        else if (subStep == 1)
        {
            GUI.enabled = (PlayerInstantiated == null || PlayerModel == null);
            PlayerInstantiated = EditorGUILayout.ObjectField("Player Prefab", PlayerInstantiated, typeof(GameObject), true) as GameObject;
            if (PlayerModel == null)
            {
                GUILayout.Label("<color=yellow>Select the ragdolled player model (from hierarchy)</color>");
            }
            DrawHTMLPlaceholder();
            PlayerModel = EditorGUILayout.ObjectField("Player Model", PlayerModel, typeof(GameObject), true) as GameObject;
            GUI.enabled = true;
            if (PlayerModel != null && PlayerInstantiated != null)
            {
                DownArrow();
                DrawText("All good, click in the button below to setup the model in the player prefab.");
                GUILayout.Space(10);
                var r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
                autoPoseAiming = MFPSEditorStyles.FeatureToogle(r, autoPoseAiming, "Automatically Pose Aiming");
                GUILayout.Space(4);
                weaponOrientationMode = (TPWeaponOrientationMode)EditorGUILayout.EnumPopup("TPWeapon Reposition Method", weaponOrientationMode);
                GUILayout.Space(20);

                using (new CenteredScope())
                {
                    if (Buttons.GlowButton("<color=#1e1e1e>SETUP MODEL</color>", Style.highlightColor, GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        SetUpModelInPrefab();
                        NextStep();
                    }
                }
            }
        }
        else if (subStep == 2)
        {
            string pin = PlayerInstantiated == null ? "MPlayer" : PlayerInstantiated.name;
            DrawText($"If all works as expected, you should see <b>just</b> a log in the console: <b><i>Player model integrated</i></b>.\n\nIf it's so, you also should see inside the player prefab instanced in the scene hierarchy: <b>{pin} -> RemotePlayer -></b> both models the old one <i>(marked with <b>(DELETE THIS)</b> at the end of the name) </i> and the new one.");
            DrawServerImage("img-3.png");
            DrawNote("The old model is not automatically deleted just in case you see a noticeable difference in the position, scale, or rotation between both models, if is this the case you can manually adjust the position, rotation, or scale of the new model using the old model as a reference, if that is not the case is everything seems correct, you can simply delete the old model.");
            DownArrow();
            DrawText("Next up, there's an essential manual adjustment you'll need to make. When we moved the <i>TPWeapons</i> from the old player model to the new one, the unique local axis orientations of each model might cause misalignment. The weapons could end up not sitting correctly in the new player's hands. To address this, you'll have to manually reposition and reorient the weapons. A good starting point is to adjust the <b>TPWeapon Root</b>, which is the central transform where all the TPWeapons are grouped. You'll identify this as the object named <b>RemoteWeapons</b> in the hierarchy window.");

            DrawText("In order to repositioned/re-oriented them, select the <b>RemoteWeapons</b> object which is inside of the player prefab <i>(inside of the right hand of the player model)</i>, or click in the button below to try to ping it automatically on the hierarchy window.");
            using (new CenteredScope())
            {
                if (DrawButton("Ping RemoteWeapons"))
                {
                    if (PlayerInstantiated != null && PlayerInstantiated.GetComponent<bl_PlayerReferences>().RemoteWeapons != null)
                    {
                        Transform t = PlayerInstantiated.GetComponent<bl_PlayerReferences>().RemoteWeapons.transform;
                        Selection.activeTransform = t;
                        EditorGUIUtility.PingObject(t);
                    }
                }
            }
            DrawAnimatedImage(2);
            DrawText("After you have fine-tuned the position and orientation, continue with the next step");

        }
        else if (subStep == 3)
        {
            DrawTitleText("ADJUST AIM POSITION");
            DrawSuperText("The arms aim position is controlled by IK and the arms aim position can be customized from the inspector, for it select the player model inside the player prefab the one marked with <b>(NEW)</b> inside the RemotePlayer object ➔ then go to the inspector window ➔ <b>bl_PlayerIK</b> ➔ at the bottom of the script inspector ➔ click on the button <b>Preview Aim Position</b> ➔ move the auto-selected pivot and you will see how the arms move with it ➔ positioned the pivot in the place that you want to be the Aim position ➔ once you got it, click on the <b>DONE</b> yellow button and that's.");
            DrawNote("Make sure the <b>Gismoz</b> is enabled in the Editor otherwise you won't be able to move the pivot.");
            DrawAnimatedImage(0);
            DownArrow();
            DrawText("Now select the <b>TP Weapons</b> object in the hierarchy window > <i>(inspector window)</i> <b>bl_WorldWeaponsContainer</b> > click in the <b>Update Child Weapons</b> button > and then click in the <b>Commit Changes</b> button <i>(click yes in the dialog)</i>.");
            DrawServerImage("img-12.png");
            DrawText("When you are done, make sure that if you haven't deleted the old model yet, you should do it now:");
            DrawServerImage(6);
            DrawText("Also, delete the <b>TP Weapons</b> containers from the player prefab since it doesn't have to be included in the prefab.");
            DrawServerImage("img-11.png");
        }
        else if (subStep == 4)
        {
            DrawText("Now you need to copy this prefab inside the <b>Resources</b> folder, by dragging it to: MFPS -> Resources. Rename it if you wish.");
            DrawServerImage(7);
            DownArrow();
            DrawText("Now you need assign this new player prefab for use by one of the Teams (team 1 or team 2). To do this, go to GameData (in Resources folder too) -> Players section, and in the corresponding field (Team1 or Team2), " +
                "drag the new player prefab.");
            DrawServerImage(8);
        }
        else if (subStep == 5)
        {
            DrawText("That's it! You have your new player model integrated!.\n\n Please note: Some models are not fully compatible with the default player animations re-targeting, causing " +
                "some of your animations to look awkward. Unfortunately, there is nothing we can do to fix it automatically. To fix it you have two options: Edit the animation or replace with another that you know" +
                " works in your model, check the documentation for more info of how replace animations.");
            GUILayout.Space(7);
            DrawText("Do you want to have multiple player options so a player has more players to choose from?, Check out <b>Player Selector</b> Addon, with which you can add as many player models as you want: ");
            GUILayout.Space(5);
            if (DrawButton("PLAYER SELECTOR"))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/shop/addons/player-selector/");
            }
            DrawServerImage(9);
        }
    }

    void PlayerModelAssetsDoc()
    {
        DrawText("Here you have a list of Asset Store player model assets that you can use to integrate in MFPS");
        Space(10);
        playerAssets.OnGUI();
        DrawAffiliateList(playerAssets.autoUrl);
    }

    void AnimationAssetsDoc()
    {
        DrawText("Here you have a list of Asset Store animation assets that you can use with your character models");
        Space(10);
        animationAssets.OnGUI();
        DrawAffiliateList(animationAssets.autoUrl);
    }

    void UnPackPrefab(GameObject prefab)
    {
#if UNITY_2018_3_OR_NEWER
        if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.Connected)
        {
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
#endif
    }

    void SetUpModelInPrefab()
    {
        UnPackPrefab(PlayerModel);
        UnPackPrefab(PlayerInstantiated);
        GameObject TempPlayerPrefab = PlayerInstantiated;
        GameObject TempPlayerModel = PlayerModel;

        //change name of prefabs to identify
        PlayerInstantiated.gameObject.name += " [NEW]";
        PlayerInstantiated.transform.SetAsLastSibling();
        PlayerModel.name += " [NEW]";

        // get the current player model
        GameObject RemoteChildPlayer = TempPlayerPrefab.GetComponentInChildren<bl_PlayerAnimationsBase>().gameObject;
        GameObject ActualModel = TempPlayerPrefab.GetComponentInChildren<bl_PlayerIKBase>().gameObject;
        Transform NetGunns = null;
        var tempPlayerReferences = TempPlayerPrefab.GetComponent<bl_PlayerReferences>();

        if (tempPlayerReferences.RemoteWeapons != null)
        {
            NetGunns = tempPlayerReferences.RemoteWeapons.transform;
        }
        else if (tempPlayerReferences.GetComponent<bl_PlayerNetwork>().NetworkGuns.Count > 0)
        {
            NetGunns = tempPlayerReferences.GetComponent<bl_PlayerNetwork>().NetworkGuns[0].transform.parent;
        }

        //set the new model to the same position as the current model
        TempPlayerModel.transform.parent = RemoteChildPlayer.transform;
        TempPlayerModel.transform.localPosition = ActualModel.transform.localPosition;
        TempPlayerModel.transform.localRotation = ActualModel.transform.localRotation;

        //add and copy components of actual player model
        bl_PlayerIK ahl = ActualModel.GetComponent<bl_PlayerIK>();
        if (TempPlayerModel.GetComponent<Animator>() == null) { TempPlayerModel.AddComponent<Animator>(); }
        Animator NewAnimator = TempPlayerModel.GetComponent<Animator>();

        if (ahl != null)
        {
            bl_PlayerIK newht = TempPlayerModel.AddComponent<bl_PlayerIK>();
            bl_UtilityHelper.CopyComponentFields(ahl, newht);

            Animator oldAnimator = ActualModel.GetComponent<Animator>();
            NewAnimator.runtimeAnimatorController = oldAnimator.runtimeAnimatorController;
            NewAnimator.applyRootMotion = oldAnimator.hasRootMotion;
            if (NewAnimator.avatar == null)
            {
                NewAnimator.avatar = oldAnimator.avatar;
                Debug.LogWarning("Your new model doesn't have a avatar, that can cause some problems with the animations, be sure to add it manually.");
            }
        }
        Transform RightHand = NewAnimator.GetBoneTransform(HumanBodyBones.RightHand);

        if (RightHand == null)
        {
            Debug.Log("Can't get right hand from new model, are u sure that is an humanoid rig?");
            return;
        }


        tempPlayerReferences.PlayerAnimator = NewAnimator;
        var pa = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerAnimationsBase>();
        var tempRagdoll = TempPlayerPrefab.transform.GetComponentInChildren<bl_PlayerRagdoll>();
        pa.Animator = NewAnimator;
        ActualModel.SetActive(false);
        tempRagdoll.SetUpHitBoxes();
        var hbm = (bl_HitBoxManager)tempPlayerReferences.hitBoxManager;
        hbm.SetupHitboxes(NewAnimator);
        tempPlayerReferences.playerSettings.carrierPoint = NewAnimator.GetBoneTransform(HumanBodyBones.UpperChest);

        if (tempPlayerReferences.gunManager != null)
        {
            // hide the FPWeapons so the TPWeapons can be seen clearly.
            foreach (var weapon in tempPlayerReferences.gunManager.AllGuns)
            {
                if (weapon == null) continue;
                weapon.gameObject.SetActive(false);
            }
        }

        EditorUtility.SetDirty(tempPlayerReferences);

        if (autoPoseAiming)
        {
            if (pa.Animator != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    pa.Animator.Update(0.2f);
                }
            }
        }

        if (RightHand != null)
        {
            var npos = NetGunns.localPosition;
            var nrot = NetGunns.localRotation;

            NetGunns.parent = RightHand;
            if (weaponOrientationMode == TPWeaponOrientationMode.SameLocalAsOldModel)
            {
                NetGunns.localPosition = npos;
                NetGunns.localRotation = nrot;
            }
            else if (weaponOrientationMode == TPWeaponOrientationMode.ResetInNewModel)
            {
                NetGunns.localPosition = Vector3.zero;
                NetGunns.rotation = RightHand.rotation;
            }
            else
            {

            }
        }
        else
        {
            Debug.Log("Can't find right hand");
        }

        if (NetGunns != null && NetGunns.GetComponent<bl_RemoteWeapons>() != null)
        {
            NetGunns.GetComponent<bl_RemoteWeapons>().InstanceContainer(null, null, true, true);
        }

        ActualModel.name += " (DELETE THIS)";
        ActualModel.SetActive(false);

        var view = (SceneView)SceneView.sceneViews[0];
        var pbounds = MFPSEditor.MFPSEditorUtils.GetTransformBounds(tempPlayerReferences.gameObject);
        pbounds.center += Vector3.up * 0.5f;
        view.LookAt(pbounds.center);
        //view.Frame(pbounds);

        view.ShowNotification(new GUIContent("Player Setup"));
        Debug.Log("Player model integrated.");
    }

    private Rigidbody[] GetRigidBodys(Transform t)
    {
        Rigidbody[] R = t.GetComponentsInChildren<Rigidbody>();
        return R;
    }

    private Collider[] GetCollider(Transform t)
    {
        Collider[] R = t.GetComponentsInChildren<Collider>();
        return R;
    }

    public enum TPWeaponOrientationMode
    {
        SameLocalAsOldModel,
        ResetInNewModel,
        KeepSameLocation
    }

    [MenuItem("MFPS/Tutorials/Add Player", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddPlayerTutorial));
    }
}