using MFPSEditor;
using UnityEditor;
using UnityEngine;

public class TutorialBots : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/bots/";
    private readonly NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
    };
    private readonly Steps[] AllSteps = new Steps[] {
         new() { Name = "MFPS Bots", StepsLenght = 1, DrawFunctionName = nameof(IntroductionDoc) },
     new() { Name = "Change Bot Model", StepsLenght = 1, DrawFunctionName = nameof(DrawModel) },
     new() { Name = "Bots Weapons", StepsLenght = 0, DrawFunctionName = nameof(BotWeaponsDoc)},
     new() { Name = "Bots Difficulty", StepsLenght = 0, DrawFunctionName = nameof(BotsDifficultyDoc)},
     new() { Name = "Cover Points", StepsLenght = 0, DrawFunctionName = nameof(CoverPointDoc) },
     new() { Name = "AI Areas", StepsLenght = 2, DrawFunctionName = nameof(AIAreasDoc), SubStepsNames = new string[] { "AI Areas", "Setup AI Areas"} },
     new() { Name = "bl_AICoverPointManager", StepsLenght = 0, DrawFunctionName = nameof(AICoverPointManagerDoc) },
     new() { Name = "Bots Names", StepsLenght = 0, DrawFunctionName = nameof(BotsNameDoc) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new(){ Path = "bot-add-w1.gif" },
        new(){ Path = "bot-add-w2.gif" },
   };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        allowTextSuggestions = true;
        FetchWebTutorials("mfps2/tutorials/");
        SetHTMLHeaderLinks(new System.Collections.Generic.Dictionary<string, string>() { { "Community", "https://discord.gg/8zF5B4G" }, { "Website", "https://www.lovattostudio.com/en/" }, { "Get MFPS", "https://www.lovattostudio.com/en/shop/template/mfps-2-0/" } });
        SetHTMLTitle("MFPS Change Bots");
        SetHTMLLink("https://lovattostudio.com/documentations/mfps2/change-bots.html");
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void IntroductionDoc()
    {
        DrawText("<size=18><b>MFPS Bots</b></size>\n\n\n<size=16><b>Overview</b></size>\n\n\nMFPS includes AI bots that provide a basic shooter experience in party matches. These bots are primarily designed for the game's core modes like Team DeathMatch and Free For All, as well as certain addon modes.\n\n<size=16><b>AI Behavior and Limitations</b></size>\n\n\nThe AI of these bots is tailored for specific game modes. However, their behavior is not fully adapted for all game modes available in MFPS. For example, in objective-based modes like Capture the Flag, the bots default to basic actions such as seeking and shooting enemies, without engaging in the mode's specific objectives like flag capturing. To have bots function appropriately in these modes, additional AI programming is necessary.\n\n<size=16><b>Bot Prefabs</b></size>\n\n\nMFPS provides default bot prefabs for team-based gameplay, with one prefab for each team. In solo modes, the game uses the prefab for Team 1. Post the MFPS 1.10.0 update, the process of integrating player models for these bots has been streamlined. The same player models used for real player prefabs can now be applied to bot prefabs, eliminating the need for separate integration.");
    }

    void BotWeaponsDoc()
    {
        DrawText("<size=18><b>Bot Weapons</b></size>\n\n\n<size=16><b>Overview</b></size>\n\n\nIn MFPS 2.0, bots are programmed to select weapons randomly from a predefined set available in their respective prefabs. By default, they can use four types of weapons: Machineguns, Pistols, Shotguns, and Grenades.\n\n<size=16><b>Simplified Weapon Integration (Post 1.10.0 Update)</b></size>\n\n\nStarting from MFPS 1.10.0, integrating weapons directly into each bot prefab is no longer necessary. Instead, bots use the weapons from the linked player prefab. Your task is to specify which weapons from the player prefab are available for the bot's use.\n\n<size=16><b>Setting Up Available Weapons for Bots</b></size>\n\n\nThe process to define the bot's weapon arsenal is straightforward:\n1. <b>Select Bot Prefab</b>:\n  • Locate and select the bot prefab you wish to modify. By default, these are found in the MFPS's Resources folder.\n2. <b>Accessing Weapon Settings</b>:\n  • With the bot prefab selected, navigate to the inspector and find the `<b>bl_AIShooterAttack</b>` component.\n  • Look for the 'Weapons Container' section.\n3. <b>Assigning Weapons</b>:\n  • In the 'Weapons Container', you'll find a list where you can add new entries.\n  • For each new entry, assign the GunID corresponding to the weapon you want to be available for this bot.\n\n<size=16><b>Customizing Weapons for Different Bots</b></size>\n\n\nIf you desire distinct weapon sets for different bots:\n• Assign a unique 'weapon container scriptable object' to each bot prefab. This allows you to have a different array of weapons available for each type of bot, providing more diversity in your game's AI.");
        DrawServerImage("img-12.png");
    }

    void BotsDifficultyDoc()
    {
        DrawText("By default, the bots are designed to provide a challenging but enjoyable gameplay experience.\n\nYou can customize various bot settings to adjust their difficulty. For example, you can tweak their shot accuracy, aggressiveness, tactical behavior, and other attributes that influence their actions. These settings are accessible in the inspector of Bot Prefabs, located in the MFPS Resources folder. Specifically, check the inspector for the `<b>bl_AIShooterAgent</b>` and `<b>bl_AIShooterAttack</b>` scripts, where the property names indicate their purposes.\n\nAdditionally, the placement of cover points significantly impacts bot behavior and perceived intelligence. Adding cover points in strategic locations on your map can enhance their tactical performance.\n\nFinally, the new <b>AI Areas</b> introduced in MFPS 1.10.0 changes how bots play in team dynamics. When AI Areas are utilized, bots avoid reckless engagements. Instead, they hold positions or find safe spots to shoot from a distance until they can advance. However, there is still some randomness, so occasionally, bots may directly rush the enemy.\n");
    }

    void CoverPointDoc()
    {
        DrawSuperText("The MFPS AI system comes with support for Cover Points, in essence, <b>are points strategically placed around the map which improves the AI navigation path maker</b>, based on some conditions in the battlefield, bots used these points to add some sort of randomness to their behavior when they are in battle, bots used these points to cover from enemies or as a random navigation target.\n \nThe usage of these points is recommended but not obligatory, more Cover Points you add to your map, more randomness, and realistic bot navigation you will get.\n \n<?title=18>ADD A NEW COVER POINT</title>\n \nAdd a new Cover Points is simply as duplicate one of the existing ones and manually placed in your map.\n \nIn order to easily preview all your Cover Points, you can turn on the gizmos for it in <i><b>(Your Map scene hierarchy) ➔ AIManager ➔ bl_AICoverPointManager ➔ Show Gizmos.</b></i>");
        DrawServerImage("img-5.png");
        DownArrow();
        DrawText("Each Cover Point reference must have attached the script <b>bl_AICoverPoint</b>, otherwise it wont work as a cover point, this scripts have a few public properties in the inspector:");
        DrawServerImage("img-6.png");
        DrawPropertieInfo("Crouch", "bool", "Tell is the bot should crouch (or stand up) while is using this cover point");
        DrawPropertieInfo("Neighbord Points", "List", "List with near by cover points that will be used as fallback in case this cover point is being used.");
    }

    void AIAreasDoc()
    {
        if (subStep == 0)
        {
            DrawTitleText("AI Areas");
            DrawServerImage("img-13.png");
            DrawText("<b>AI Areas</b> are dynamically determined zones on the map where a high concentration of bots <i>(AI-controlled characters)</i> are located. These areas are identified and managed by the AI Manager to enhance gameplay dynamics and strategic decision-making for AI bots. Here’s how AI Areas work:\n\n<size=16><b>What are AI Areas?</b></size>\n\n\nAI Areas are specific regions on the map where there is a significant presence of bots from one or both teams. These zones are identified based on the density of bots within a defined grid system. The AI Manager tracks the number of bots in each grid cell and designates cells with a high concentration of bots as AI Areas.\n\n<size=16><b>How AI Areas are Determined</b></size>\n\n1. <b>Grid System</b>: The map is divided into a grid with each cell having a defined size (cell size). The grid helps in efficiently managing and tracking the positions of bots.\n2. <b>Bot Density Calculation</b>: During each update cycle, the AI Manager counts the number of bots in each grid cell.\n3. <b>Hot Area Threshold</b>: A grid cell is marked as an AI Area if it contains a number of bots equal to or greater than a predefined threshold (e.g., 4 bots).\n4. <b>Team-specific Zones</b>: AI Areas are further categorized based on team presence. For example, an area dominated by bots from Team 1 will be considered a hot zone for Team 2 and vice versa.\n\n<size=16><b>Bot Behavior in AI Areas</b></size>\n\n\nWhen bots approach or detect an AI Area dominated by the enemy team, their behavior is influenced by their `<b>engagementBias</b>` parameter, which determines their tendency to rush into enemy zones or take a more cautious approach:\n• <b>engagementBias</b>: This parameter defined in the bot <b>Behavior Settings</b>, ranges from 0 to 1 and indicates the bot's likelihood of rushing enemy areas (1) versus holding strategic positions (0). A higher value makes the bot more aggressive, while a lower value makes it more cautious.");
            DrawText("<size=16><b>Bot Decision-making</b></size>\n\n\nWhen a bot is moving towards an enemy position and detects an AI Area dominated by the enemy team within a given detection range, it makes a decision based on its `<b>AggressionLevel</b>`:\n1. <b>Hold Position</b>: The bot may choose to hold its current position to avoid direct engagement in the hot zone.\n2. <b>Find Strategic Position</b>: Alternatively, the bot may move to a random point near the AI Area to take a strategic position, avoiding the direct line of confrontation.\n\nThis decision-making process ensures that bots exhibit more realistic and varied behaviors, enhancing the overall gameplay experience.\n\n<size=16><b>Visualizing AI Areas</b></size>\n\n\nFor development and debugging purposes, AI Areas can be visualized in the Unity Editor using gizmos when the bl_AIAreas > <b>Show Debug</b> toggle is on. The script draws wireframe cubes around hot zones with different colors to indicate team dominance.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("Setup AI Areas");
            DrawText("When you add a new map, the AI Areas are not set up by default. You'll need to configure them manually, but it's a straightforward process:\n\n1. <b>Open the target map scene.</b>\n2. <b>Create an empty GameObject.</b>\n3. <b>Attach the `<b>bl_AIAreas</b>` script</b> to the GameObject.\n4. <b>Configure the inspector properties</b> of the `<b>bl_AIAreas</b>` script as needed.\n\nTo help visualize and adjust the AI Areas:\n\n1. <b>Enable the \"Show Debug\" toggle</b> in the inspector of the `<b>bl_AIAreas</b>` script.\n2. <b>Adjust the \"Cell Size\" value</b> until the grid covers the entire map.");

            DrawHTMLPlaceholder();
            if (bl_AIAreas.Instance == null)
            {
                Space(20);
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Create AI Areas"))
                    {
                        GameObject go = new("AI Areas");
                        var script = go.AddComponent<bl_AIAreas>();
                        script.ShowDebug = true;
                        Selection.activeGameObject = go;

                        EditorGUIUtility.PingObject(go);
                    }
                }
            }
        }
    }

    void AICoverPointManagerDoc()
    {
        DrawText("The script <b>bl_AICoverPointManager.cs</b> is attached in each <i><b>map scene ➔ AIManager ➔ bl_AICoverPointManager</b></i>, this script handle the logic behind the cover point selection, when a bot request a cover point, this script is responsible for determine which cover point in the scene should be used based on the requester bot conditions.\n\nThis script has some public properties in the inspector that you can tweak:");
        DrawServerImage("img-7.png");
        DrawPropertieInfo("Max Distance", "float", "The max distance for which a cover point is consider a neighbor from another cover point.");
        DrawPropertieInfo("Usage Time", "float", "The 'cooldown' time that takes for the cover point to be used again after being used.");
        DrawPropertieInfo("Show Gizmos", "bool", "Show gizmos for each cover point in the map.");
        DrawPropertieInfo("Bake Neighbors Points", "Button", "Automatically calculate the neighbord cover points for each cover point in the scene, you should use this everytime you edit the cover points in your scene.");
        DrawPropertieInfo("Align point to floor", "Button", "Automatically vertical re-positione the cover point in the scene so it is right above the floor below the point (and not floating)");
    }

    GameObject ModelPrefab = null;
    void DrawModel()
    {
        DrawText("<size=18><b>Tutorial: Changing Bot Character Model</b></size>\n\n\n<size=16><b>Introduction</b></size>\n\n\nFrom version 1.10.0 onwards, MFPS has streamlined the process of replacing or adding new character models to bot prefabs. This tutorial guides you through the steps to customize your bots with different character models.\n\n<size=16><b>Prerequisites</b></size>\n\n• <b>Player/Character Model Integration</b>: Before proceeding, ensure that your desired player or character model is already integrated into a player prefab. If this step is not yet completed, please refer to the \"<b>Add Player</b>\" tutorial for guidance.\n\n<size=16><b>Step-by-Step Process</b></size>\n\n1. <b>Integrate the Character Model</b>:\n  • First, integrate your player or character model into a player prefab if you haven't already.\n2. <b>Locate the Bot Prefab</b>:\n  • Navigate to the MFPS's Resources folder to find the bot prefabs. By default, they are named \"<b>BotPlayer [1]</b>\" and \"<b>BotPlayer [2]</b>\".\n  • Select the bot prefab for which you wish to replace the character model.\n\n<i><b>Note</b>: If your goal is to create a new bot prefab with a different model, rather than replacing an existing one, simply duplicate one of the existing bot prefabs.</i>\n\n3. <b>Assign the Player Prefab</b>:\n  • With the desired bot prefab selected, go to the inspector window.\n  • Locate the `<b>bl_BotModel</b>` script.\n  • In the '<b>Player Prefab Binding</b>' field, assign the player prefab that contains the character model you want to use for this bot.\n  • This action links your chosen character model to the selected bot prefab and that's all you have to do.");
        DrawServerImage("img-11.png");
    }

    void BotsNameDoc()
    {
        DrawHyperlinkText("The bots are named randomly from a predefined list of names that you as the developer can easily modify.\n \nFirst, you can define the prefix that goes before the random name, this by default is <b>BOT</b>, you can change it to whatever you want in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ <b>Bots Name Prefix</b>.\n \nTo modify the list of the random names, open the script <b>bl_GameTexts.cs</b> ➔ <b>RandomNames</b>, in this list, add, remove or edit any element of the list.");
        DrawServerImage("img-8.png");
    }

    void ReplaceBotModel()
    {
        if (ModelPrefab == null) return;
        GameObject model = ModelPrefab;
        if (PrefabUtility.IsPartOfAnyPrefab(ModelPrefab))
        {
            model = PrefabUtility.InstantiatePrefab(ModelPrefab) as GameObject;
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        }
        model.name += " [NEW]";
        GameObject botPrefab = PrefabUtility.InstantiatePrefab(bl_GameData.Instance.BotTeam1.gameObject) as GameObject;
#if UNITY_2018_3_OR_NEWER
        PrefabUtility.UnpackPrefabInstance(botPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
#endif
        botPrefab.name = "AISoldier [NEW]";
        var oldModel = botPrefab.GetComponentInChildren<bl_AIAnimationBase>();
        oldModel.name += " [OLD]";
        Animator modelAnimator = model.GetComponent<Animator>();
        modelAnimator.applyRootMotion = false;
        modelAnimator.runtimeAnimatorController = oldModel.GetComponent<Animator>().runtimeAnimatorController;
        if (!AutoRagdoller.Build(modelAnimator))
        {
            Debug.LogError("Could not build a ragdoll for this model");
            return;
        }

        bl_AIShooterAgent aisa = botPrefab.GetComponent<bl_AIShooterAgent>();
        var botReferences = botPrefab.GetComponent<bl_AIShooterReferences>();

        model.transform.parent = oldModel.transform.parent;
        model.transform.localPosition = oldModel.transform.localPosition;
        model.transform.localRotation = oldModel.transform.localRotation;
        var aia = model.AddComponent<bl_AIAnimation>();
        botReferences.aiAnimation = aia;
        aia.rigidbodies.Clear();
        aia.rigidbodies.AddRange(model.transform.GetComponentsInChildren<Rigidbody>());
        Collider[] allColliders = model.transform.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].gameObject.layer = LayerMask.NameToLayer("Player");
        }
        var hbm = botReferences.hitBoxManager as bl_HitBoxManager;
        hbm.SetupHitboxes(modelAnimator);
        botReferences.PlayerAnimator = modelAnimator;
        EditorUtility.SetDirty(botReferences.hitBoxManager);
        Transform weaponRoot = botReferences.remoteWeapons.transform;
        Vector3 wrp = weaponRoot.localPosition;
        Quaternion wrr = weaponRoot.localRotation;
        weaponRoot.parent = modelAnimator.GetBoneTransform(HumanBodyBones.RightHand);
        weaponRoot.localRotation = wrr;
        weaponRoot.localPosition = wrp;
        DestroyImmediate(oldModel.gameObject);

        var view = (SceneView)SceneView.sceneViews[0];
        view.LookAt(botPrefab.transform.position);
        EditorGUIUtility.PingObject(botPrefab);
        Selection.activeTransform = botPrefab.transform;
    }

    [MenuItem("MFPS/Tutorials/ Change Bots", false, 501)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TutorialBots));
    }
}