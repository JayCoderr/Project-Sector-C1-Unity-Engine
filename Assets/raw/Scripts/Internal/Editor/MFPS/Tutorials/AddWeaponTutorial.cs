using MFPSEditor;
using UnityEditor;
using UnityEngine;

public class AddWeaponTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-0.jpg", Image = null},
        new NetworkImages{Name = "img-5.png", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.jpg", Image = null},
        new NetworkImages{Name = "img-16.jpg", Image = null},
        new NetworkImages{Name = "img-17.jpg", Image = null},
        new NetworkImages{Name = "img-18.jpg", Image = null},
        new NetworkImages{Name = "img-19.jpg", Image = null},
        new NetworkImages{Name = "img-20.jpg", Image = null},
        new NetworkImages{Name = "img-21.jpg", Image = null},
        new NetworkImages{Name = "img-22.jpg", Image = null},
        new NetworkImages{Name = "img-23.jpg", Image = null},
        new NetworkImages{Name = "img-24.png", Image = null},
        new NetworkImages{Name = "img-25.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "gif-1.gif" },
        new GifData{ Path = "gif-2.gif" },
        new GifData{ Path = "gif-3.gif" },
        new GifData{ Path = "gif-4.gif"},
        new GifData{ Path = "gif-5.gif"},
        new GifData{ Path = "gif-6.gif"},
        new GifData{ Path = "gif-7.gif"},
        new GifData{ Path = "gif-8.gif"},
        new GifData{ Path = "gif-9.gif"},
        new GifData{ Path = "gif-10.gif"},
        new GifData{ Path = "gif-11.gif"},
        new GifData{ Path = "gif-12.gif"},
        new GifData{ Path = "gif-13.gif"},
        new GifData{ Path = "gif-14.gif"},
        new GifData{ Path = "gif-15.gif"},
        new GifData{ Path = "gif-16.gif"},
        new GifData{ Path = "gif-17.gif"},
        new GifData{ Path = "gif-18.gif"},
        new GifData{ Path = "gif-19.gif"},
        new GifData{ Path = "gif-20.gif"},
   };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Weapon Model", StepsLenght = 0, DrawFunctionName = nameof(DrawWeaponModel) },
    new Steps { Name = "Create Info", StepsLenght = 2, DrawFunctionName = nameof(DrawCreateInfo) },
    new Steps { Name = "FPV Weapon", StepsLenght = 7, DrawFunctionName = nameof(DrawFPWeapon) },
    new Steps { Name = "TPV Weapon", StepsLenght = 3, DrawFunctionName = nameof(DrawTPWeapon) },
    new Steps { Name = "PickUp Prefab", StepsLenght = 2, DrawFunctionName = nameof(DrawPickUpPrefab) },
    //new Steps { Name = "Export Weapons", StepsLenght = 0, DrawFunctionName = nameof(DrawExportWeapons) },
    new Steps { Name = "Weapon Animations", StepsLenght = 2, DrawFunctionName = nameof(AnimateWeaponDoc),
    SubStepsNames = new string[] {"FP Custom Animations", "FP Generic Animations" } },
    };
    //final required////////////////////////////////////////////////

    private GameObject PlayerInstantiated;
    private int animationType = 0;
    public AssetStoreAffiliate weaponList;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        if (weaponList == null)
        {
            weaponList = new AssetStoreAffiliate();
            weaponList.randomize = true;
            weaponList.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/114132/widget-medium");
            weaponList.FixedHeight = 400;
        }
        allowTextSuggestions = true;
        SetHTMLHeaderLinks(new System.Collections.Generic.Dictionary<string, string>() { { "Community", "https://discord.gg/8zF5B4G" }, { "Website", "https://www.lovattostudio.com/en/" }, { "Get MFPS", "https://www.lovattostudio.com/en/shop/template/mfps-2-0/" } });
        SetHTMLTitle("MFPS Add Weapon");
        SetHTMLLink("https://lovattostudio.com/documentations/mfps2/add-weapons.html");
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawWeaponModel()
    {
        if (subStep == 0)
        {
            DrawText("In order to add a new weapon you need of course a Weapon 3D model, now, there are different approaches to add a new weapon, someones just replace the weapon model and use the MFPS default hands model and animations <i>(basically just positioned in the hands the new weapon model)</i>, Although this is not prohibited or wrong, it definitely is not the best solution since the hand model and animations of MFPS are placeholders that work as an example only, and the animations will not look right with different weapon models.\n \nIs highly recommended that you use your own models and animations <i>(including the arms model)</i>, you can use the default MFPS hands but by doing this you will have to animate them for each weapon that you want to add and if you are not an Animator or you don't have experience animating, that could be a hard time for you since <b>you need at least 4 animations which are: Take In, Take Out, Fire and Reload</b> animations.\n \n<b>Optionally</b> if you want to save time and effort you can get weapons models packages that are compatible with MFPS and comes with the required animations and their own arms models, below I'll leave you an Asset Store collection list of those assets:");
            GUILayout.Space(5);
            Rect r = EditorGUILayout.BeginHorizontal();
            MFPSEditorStyles.DrawBackground(r, new Color(0, 0, 0, 0.3f));
            GUILayout.Space(10);
            weaponList.OnGUI();
            DrawAffiliateList("https://assetstore.unity.com/linkmaker/embed/list/114132/widget-medium");
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("<color=yellow>View online</color>", EditorStyles.label))
            {
                Application.OpenURL("https://www.lovattostudio.com/en/weapon-packs-for-mfps/");
            }
        }
    }

    void DrawCreateInfo()
    {
        if (subStep == 0)
        {
            DrawText("The first step to add a new weapon is to create the weapon basic information,\n for it you need go to the <b>GameData</b> and add a new field in\n the <b>AllWeapons</b> inspector list.");
            GUILayout.Space(10);
            if (DrawButton("Select GameData"))
            {
                bl_GameData gm = bl_GameData.Instance;
                Selection.activeObject = gm;
                EditorGUIUtility.PingObject(gm);
                subStep++;
            }
            if (IsHTMLPass()) DrawLinkText("Select GameData", "http://localhost:52888/ping?path=Assets/MFPS/Resources/GameData.asset");
        }
        else if (subStep == 1)
        {
            DrawText("The <b>GameData</b> should be focus in the inspector window.\n\nScroll down in the inspector window to the <b>Weapon</b> section, there you will have the <b>All Weapons</b> list, foldout the list and add a new field to the list");
            Space(10);
            DrawAnimatedImage(8);
            DownArrow();
            DrawText("Now adjust the rest of the weapon basic information in the new field");
            DrawPropertieInfo("Name", "string", "The name of this weapon, use a Unique name for each weapon so you can easily identify they.");
            DrawPropertieInfo("Type", "enum", "The type of this weapon, is a sniper, rifle, knife, etc...");
            DrawPropertieInfo("Damage", "int", "The base damage that this weapon will inflict.");
            DrawPropertieInfo("Fire Rate", "float", "Time (in seconds) between following shots.");
            DrawPropertieInfo("Reload Time", "float", "Time that take reload the weapon.");
            DrawPropertieInfo("Range", "int", "The maximum distance that the bullet of this weapon can travel (and hit something) before get destroyed.");
            DrawPropertieInfo("Accuracy", "float", "Representative value of the accuracy of this weapon, this doesn't actually define the accuracy of the weapon in game, it's just use to showcase the accuracy level.");
            DrawPropertieInfo("Weight", "float", "The 'weight' of this weapon, the weight affect the player speed when this gun is equipped");
            DrawPropertieInfo("Unlockability", "Class", "Information of the lock state of the weapon (is it free or it require to be unlocked in game?)");
            DrawPropertieInfo("Gun Icon", "Sprite", "A sprite icon that represent this weapon.");
            Space(10);
            DrawServerImage("img-27.png");
            GUILayout.Label("With this you have setup the weapon information, now you're ready to integrate the weapon model, check the next section.");
        }
    }

    void DrawFPWeapon()
    {
        if (subStep == 0)
        {
            DrawTitleText("First Person Weapon Integration");
            DrawNote("To proceed with this step, is recommended to do it in a empty scene for clarity, lets open a new empty scene\nyou can create a new scene in (top navigation menu) <b>File -> New Scene.</b>");
            DrawText("The first model we have to integrate is the <b>FP Weapon</b>, the FPV Weapon <i>(First Person View Weapon)</i> is the weapon model that the local player see, which is the weapon with the first person arm model, we have to integrate it in the <i>View Weapon Container</i>, even if you are developing a 3rd person view only game, by default is required to integrate the first person weapon in order the weapon system to work.\n \nTo start, in order to preview the first person view perspective and the already integrated weapons, go to the editor top navigation menu > <b>MFPS > Actions > Preview FP Weapons</b>, or click in the button below.");

            Space(10);
            if (DrawButton("Preview FP Weapons"))
            {
                EditorApplication.ExecuteMenuItem("MFPS/Actions/Preview FP Weapons");
                NextStep();
            }
            if (IsHTMLPass()) DrawLinkText("Preview FP Weapons", "http://localhost:52888/menuitem?path=MFPS/Actions/Preview FP Weapons");
        }
        else if (subStep == 1)
        {
            DrawTitleText("Create Weapon Setup");
            DrawText("Now, the player first person view placeholder should be instanced in the hierarchy window along with all the integrated FP Weapons, the FP Weapon Root <i>(<b>FP Weapons</b>)</i> should be automatically selected, if you foldout it in the hierarchy window you should see all the integrated FP Weapons");
            DrawServerImage("img-28.png");
            DownArrow();
            DrawText("To proceed, duplicate an existing FP Weapon that matches the type you're integrating. For instance, if you're adding a rifle, duplicate an existing rifle weapon. If it's a sniper, duplicate a sniper weapon, and so forth.\n\nYou can duplicate the weapons by selecting the weapon in the hierarchy window > Right Mouse Click > <i>(context menu)</i> <b>Duplicate</b> or Ctrl+D.\n\n Then rename the duplicated weapon instance in the hierarchy");
            DrawAnimatedImage(9);
            DownArrow();
            DrawText("With the duplicate weapon selected in the hierarchy window, navigate to the <b>inspector window > bl_Gun > General</b> > assign the <b>Gun ID</b> of this weapon which is the name of the weapon you define in the GameData All Weapon list early.");
            DrawServerImage("img-29.png");
            DrawText("Continue with the next step.");
        }
        else if (subStep == 2)
        {
            DrawTitleText("Setup Weapon Model");
            DrawText("Now is time to add your new FP Weapon model which is the weapon model with the first person arms.\n \nDrag your FP Weapon model inside your new weapon object in the hierarchy, with the new model root selected > change the <b>Layer</b> to the <b>'Weapons'</b> layer and apply to all the children objects from the dialog window.");
            DrawAnimatedImage(10);
            DownArrow();
            DrawText("Reset the position and rotation of the model instance to (0,0,0), and adjust the position and rotation of the weapon root <i>(where <b>bl_Gun</b> is attached)</i> to the position you want it in the first person view, use the <b>Game View</b> window to see how it looks from the fp perspective.");
            DrawText("Transfer the Fire Point from the old model of the duplicated weapon to your new weapon model, for it, in the inspector of <b>bl_Gun</b> > select the <b>Muzzle</b> tab > click in the <b>Fire Point</b> field value <i>(the value not the title)</i>, this will ping and select the Fire Point object in the hierarchy window, drag the selected object inside the barrel transform of your new weapon in the hierarchy.\n \nFine tune the position of the fire point to align it with the end of your weapon barrel or wherever you want the bullet origin to be.\n \nDelete the old weapon model of your duplicate weapon to leave just your new weapon model in the hierarchy.");
            DrawAnimatedImage(11);
            DrawText("Continue with the next step.");
        }
        else if (subStep == 3)
        {
            DrawTitleText("Setup Weapon Animations");
            DrawText("It's time to setup the fp weapon animations, start by attaching the <b>bl_WeaponAnimation</b> script to your weapon model where the <b>Animator</b> or <b>Animation</b> component is attached to, you can attached this script by click on the <b>Add Component</b> button in the inspector > write <i>bl_WeaponAnimation</i> in the search bar > select the <b>bl_WeaponAnimation</b> script on the search result");
            DrawAnimatedImage(12);
            DownArrow();
            if (IsHTMLPass()) animationType = 2;
            if (animationType == 0)
            {
                DrawText("Now select which Animation system your weapon model is using, select <b>Generic Animations</b> if you don't have or don't want to use custom animations for this weapon model yet <i>(or never)</i>");
                Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Animation (Legacy)", EditorStyles.toolbarButton))
                {
                    animationType = 1;
                }
                GUILayout.Space(2);
                if (GUILayout.Button("Animator (Mecanim)", EditorStyles.toolbarButton))
                {
                    animationType = 2;
                }
                GUILayout.Space(2);
                if (GUILayout.Button("Generic Animations", EditorStyles.toolbarButton))
                {
                    animationType = 3;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else if (animationType == 1)
            {
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Back"))
                    {
                        animationType = 0;
                    }
                }
                Space(5);
                DrawText("Now in the inspector of the script you have to assign the animation clips\n\n<b>Draw</b> = Equip weapon animation clip.\n<b>Hide</b> = Unequip weapon animation clip.\n<b>Fire</b> = Fire animation clip.\n<b>Fire Aim</b> = This can be the same as normal fire animation clip but it is recommended that you use an animation with a low kick back movement.\n<b>Reload:</b> Weapon reload animation clip.\n<b>Reload Empty:</b> Weapon reload when the magazine is empty, it can be the same as the normal reload clip.\n\nIn the case where your weapon have a split reload animation <i>(Start, Insert Bullet and Finish)</i> Simple set the <b>Reload Per</b> dropdown as <b>Bullet</b> in the inspector of <b>bl_Gun</b> > <b>Ammo</b> tab  and assign the animations in bl_WeaponAnimation script.\n\n<b>NOTE:</b> All animations that are assigned in bl_WeaponAnimation should be listed in the <b>Animations</b> list of the <b>Animation</b> Component");
                DownArrow();
                DrawServerImage(3);
            }
            else if (animationType == 2)
            {
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Back"))
                    {
                        animationType = 0;
                    }
                }
                Space(5);
                DrawText("First, make sure that the attached Animator component doesn't have a <b>Animator Controller</b> assigned yet. If it already has one, remove it.\n \nNow in the inspector of <b>bl_WeaponAnimation ➔ Animation Type</b>, select the option <b>Animator</b>, you will see some empty Animation clip fields. In these, you have to assign the respective animation clips.\n\n<b>Draw</b> = Equip weapon animation clip.\n<b>Hide</b> = Unequip weapon animation clip.\n<b>Fire</b> = Fire animation clip.\n<b>Fire Aim</b> = This can be the same as normal fire animation clip but it is recommended that you use an animation with a low kick back movement.\n<b>Reload:</b> Weapon reload animation clip.\n<b>Reload Empty:</b> Weapon reload when the magazine is empty, it can be the same as the normal reload clip.\n\nIn the case where your weapon have a split reload animation <i>(Start, Insert Bullet and Finish)</i> Simple set the <b>Reload Per</b> dropdown as <b>Bullet</b> in the inspector of <b>bl_Gun</b> > <b>Ammo</b> tab  and assign the animations in bl_WeaponAnimation script inspector.\n \nOnce you have assigned all required animations, press the button <b>SetUp</b> in the inspector ➔ a Window will open, select a folder in the project to save the Animator Controller.");
                Space(5);
                DrawServerImage(23);
                DrawNote("<b>In case your weapon have multiple Animator</b> component as is the case with various weapon pack assets, you simply have to attach the <b>bl_WeaponAnimation</b> script to one of the objects that have the Animator component attached and turn on the <b>Use Multiple Animators</b> toggle in the inspector > then add all the animators in the <b>Animators</b> list that will appear.");
            }
            else if (animationType == 3)
            {
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Back"))
                    {
                        animationType = 0;
                    }
                }
                Space(5);
                DrawText("To use these Generic Animations all you have to do is set the <b>Animation Type</b> in <b>bl_WeaponAnimation</b> inspector to <b>Generic</b> and make sure the Transform position and rotation of where the <b>bl_WeaponAnimation</b> script is attached to is set to <b>(0,0,0)</b>");
                DrawServerImage("img-26.png");
            }
        }
        else if (subStep == 4)
        {
            DrawTitleText("Setup Weapon Movement");
            DrawText("In MFPS the <i>Walking</i> and <i>Running</i> animations of the FP Weapons are procedurally generated so is not required having these as animation clips, it gives more flexibility and most cases looks even better than baked animations.\n \nAll you have to do to setup the movement animations to the new added weapon is to attach the script <b>bl_WeaponMovement</b> in the same object where you attach the <b>bl_WeaponAnimation</b> script early");
            DrawAnimatedImage(13);
            DownArrow();
            DrawText("Now you have to define the run weapon pose, is the base position and rotation that the weapon will have when the player is running.\n \nFor this, first click in the button '<b>Edit Pose</b>' in the top of the <b>bl_WeaponMovement</b> script inspector, this will active the edit mode.");
            DrawServerImage("img-30.png");
            DrawText("Adjust the position and rotation of weapon as you want the running pose to be, use the <b>Game View window to preview the first person perspective</b> which is as it will look in-game.");
            DrawAnimatedImage(14);
            DrawText("Once you have the running pose, navigate to the inspector window of <b>bl_WeaponMovement</b> again > click in the first '<b>Get Actual Position</b>' button > then click in the red '<b>Edit Pose</b>' button again.");
            DrawServerImage("img-31.png");
            DrawText("Now the weapon will be back to it's idle pose and the changes will be saved, so you are done with this part, you can continue with the next step.");
            DrawHorizontalSeparator();
            DrawSuperText("<?background=#FFCF92FF>Use Weapon Movement Animations.</background>\n \nIn the case where you want to use Animation clips for your weapon walk and running movements instead of the procedural ones:\n \n1. <b>Delete the bl_WeaponMovement</b> script <i>(if you have attached)</i>\n \n2. <b>Unassign</b> the <b>Animator Controller</b> in the <b>Animator</b> component <i>(leave the field empty)</i>\n \n3. Navigate to the inspector of <b>bl_WeaponAnimation</b> and active the <b>Custom Animations For Movement</b> toggle.\n \n4. Assign the animation clips in their respective fields and click in the <b>SetUp</b> button > select the save folder and that's.");
        }
        else if (subStep == 5)
        {
            DrawTitleText("Setup Weapon Properties");
            DrawText("Now, you have to setup the weapon properties, the stats, ammo, bullet properties, etc... you do all this in the inspector of the <b>bl_Gun</b> script which attached in the root of your new weapon setup.\n \nLet's start with the weapon <b>Aim Position</b>, which is of course the position of the weapon when the player aims, to set this up, go to the <b>Aim</b> tab in the inspector of <i>bl_Gun</i> > make sure <b>Allow Aiming</b> is on > and click in the yellow button '<b>Adjust Aim Position</b>'.");
            DrawServerImage("img-32.png");
            DrawText("The system will auto-position the weapon to its current aiming stance. You'll need to fine-tune the weapon's position and rotation to ensure the iron sight, scope, or any attachment sight aligns precisely with the screen's center. The <b>Game View window</b> provides guide lines to indicate the exact center of the screen, assisting you in this alignment.");
            DrawAnimatedImage(15);
            DrawText("Once you have the aim position, back to the bl_Gun inspector and click in the red '<b>Finish Adjust Aim Position</b>' button and that's.");
            DrawServerImage("img-33.png");
            DownArrow();
            DrawText("Now tweak the rest of the weapon properties in the inspector of <b>bl_Gun</b>, switch to the different tabs to setup the ammunition, bullet, sounds, recoil, etc... almost all the properties field in the inspector have an tooltip explanation, hover your mouse over a field name that you are not sure what is for and you will see it's description in a tooltip.");
            DrawServerImage("img-34.png");
            DrawHorizontalSeparator();
            DrawText("If the weapon is a Sniper, attach the script bl_SniperScope.cs,  in the list 'OnScopeDisable' add all meshes of the sniper model including hands");
        }
        else if (subStep == 6)
        {
            DrawTitleText("Finish");
            DrawText("Now the FP Weapon setup is ready, all you have to do now and save the changes to the weapon container prefab, for this simply select the <b>FP Weapons</b> object in the hierarchy window > <b>bl_ViewWeaponsContainer</b> inspector > click on the <b>Commit Changes</b> button > and click yes in the display dialog");
            DrawServerImage("img-35.png");
            DrawText("That's, you have integrated your new FP Weapon, now if you did the integration in a working scene, delete the <b>Player Placeholder</b> instance from the scene hierarchy, if you need to make changes to the weapon <i>(or any FP Weapon)</i> in the future, simply instance the player placeholder again with the top editor navigation menu <b>MFPS > Actions > Preview FP Weapons</b>, then do the modifications and apply the changes <i>(by clicking in the <b>Commit Changes</b> button)</i>");
            DrawHorizontalSeparator();
            DrawText("If you want to assign this new weapon to one of the default player loadouts, open the <b>MFPS Manager window</b> (Ctrl+M) > go to the <b>Loadouts</b> tab on the upper right corner > select the player class loadout <i>(Assault, Recon, Support or Engineer)</i> for the player prefab you want > assign the weapon in the desired slot by clicking in the <b>Change</b> button and selecting the weapon in the list.");
            DrawSuperText("If you want to allow the players to change their weapon loadout in-game, you can use the <?link=https://www.lovattostudio.com/en/shop/addons/class-cutomization/>Class Customization</link> addon.");
        }
    }

    void DrawTPWeapon()
    {
        if (subStep == 0)
        {
            DrawTitleText("Third Person Weapon Integration");
            DrawText("Each weapon has two different points of view: The <b>First Person View</b> which is what the local player sees and what you integrated early, and the <b>Third Person View</b> which is the weapon that other players see in the player full body, in this part we're going to set up this last one, the <b>Third Person Weapon</b>, for this, what you need is the same weapon model that you use for the FPV weapon but without the hands/arms mesh, just the weapon body mesh, even though <b>is recommended to use a more optimized / less detailed model of the weapon than the first person model.</b>");
            DrawServerImage("img-36.png");
            DownArrow();

            DrawText("to proceed, you will need one of the player prefabs, to automatically instance it, go to the editor <b>top navigation menu > MFPS > Actions > Preview TP Weapons</b>, or click in the button below");
            Space(10);
            if (DrawButton("Instance Player Prefab"))
            {
                EditorApplication.ExecuteMenuItem("MFPS/Actions/Preview TP Weapons");
            }
            if (IsHTMLPass()) DrawLinkText("Instance Player Prefab", "http://localhost:52888/menuitem?path=MFPS/Actions/Preview TP Weapons");
            DownArrow();
            DrawText("Now a window with all your player prefab options should shown, simply click in the player prefab icon that you want to use for the integration and this should instance that player prefab in the active scene hierarchy with the Remote Weapon root selected.\n\nDrag your weapon model inside this remote weapon object <i>(TP Weapons)</i>");
            DrawAnimatedImage(16);
            DownArrow();

            DrawText("In the root of the new weapon you just drag, add the script <b>bl_NetworkGun</b>.\n \nIn the inspector of this script you will see the <b>FPWeapon</b> dropdown field, select the FP Weapon that you integrated in the previous section <i>(the first person weapon)</i>, the weapons are listed by their game object name so pick the correct one from the list and then click in the button <b>Select</b> next to it.");
            DrawAnimatedImage(17);
            DownArrow();
            DrawText("If you see the Muzzleflash field, drag your muzzleflash particle effect and put inside of the weapon object, if you don't have your a custom one, you have the MFPS default one located in: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Particles ➔ Prefabs ➔ Misc ➔ <b>MuzzleFlashEffect</b></i>.\nDrag it to the hierarchy and put it inside the weapon and position at the end of the gun barrel.\nThen assign in the 'MuzzleFlash' field of the bl_NetworkGun inspector.");
            DrawText("Continue with the next step.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("Weapon Position");
            DrawText("If you followed the last step plain and simple, your weapon should be in it's default instanced position, what you have to do fine tune it's position and rotation in the player model hands ensuring it looks as though the player is aiming with the weapon.");
            DrawText("In the <b>Scene View</b> window you should see a dotted yellow line origin from the weapon in direction to the player front, use that line as reference of where the weapon should be looking at with the barrel <i>(in the case where the weapon have a barrel).</i>");
            DrawAnimatedImage(18);
            DownArrow();
            DrawText("Once you have the desired weapon position, you have to set up the left player hand position to simulating the hand grip in the weapon.\n \nFor this, navigate to the inspector window of <b>bl_NetworkGun</b> > Click in the <b>Set Up Hand IK</b> button > this will automatically create and select the left hand IK target, do not select anything else and in the Scene View window move and rotate the selected object, you will see how the left arm follows using inverse kinematic, so fine tune the position until you have the desired hand pose, then click in the yellow button <b>'Done'</b>.");
            DrawAnimatedImage(19);
            DrawText("Continue with the next step.");
        }
        else if (subStep == 2)
        {
            DrawText("Finally, save the changes to the weapon container, for this, select the <b>TP Weapon</b> object in the hierarchy window > go to the inspector window of <b>bl_WorldWeaponsContainer</b> > click in the <b>Commit Changes</b> button > click yes in the dialog");
            DrawServerImage("img-37.png");
            DrawText("Delete the player instance from the scene hierarchy and that's.\n \nIf you want to do modifications to this TP Weapon later, simply instance the player prefab using the top navigation menu (<i>MFPS > Actions > Preview TP Weapons)</i> > apply the changes > and save the changes by clicking in the <b>Commit Changes</b> button again.");

            DrawHorizontalSeparator();

            DrawText("While the main integration is complete, there's an additional step for any other player prefabs with a different character model than the one used in this integration. You'll need to manually adjust the position of the new Third Person (TP) Weapon to fit in the hands of each player model. Since there's no automatic method, this manual adjustment is crucial. Essentially, it involves repositioning the weapon and clicking a button. Here's how you proceed:");
            DrawText("1. Instance the player prefab using the top navigation menu > <b>MFPS > Actions > Preview TP Weapons</b> > select the player prefab <i>(one that you haven't adjust the position of the new weapon).</i>\n \n2. Unfold the <b>TP Weapons</b> object in the hierarchy > select your new weapon > active it if it is disabled so you can see it > fine tune the position and rotation of the weapon to simulate the player is holding it just as you did in the original integration.\n \n3. Adjust the left hand position if necessary, in the inspector of <b>bl_NetworkGun</b> of your weapon > click in the <b>Edit Positions</b> button > fine tune the left hand pose just as you did in the original integration. <i>(Don't forget to click in the <b>DONE</b> yellow button once finish)</i>\n \n4. Select the <b>TP Weapons</b> object in the hierarchy again > go to the inspector of <b>bl_WorldWeaponsContainer</b> > click in the <b>Commit Changes</b> button and that's.");
        }
    }

    void DrawPickUpPrefab()
    {
        if (subStep == 0)
        {
            DrawText("The last thing that you need set up is the 'PickUp prefab' of the weapon. This prefab is instanced when players pick up other weapons or when a player dies with the weapon active." +
                " So what you need is the weapon model. Just like for setting up the TPWeapon, you only need the weapon model mesh without hands. Again, it is recommended that you use a low poly model for this one.");
            DownArrow();
            DrawText("Drag your weapon model into the hierarchy window (You don't need the player prefab for this one). Select it and Add these Components:\n\n-<b>RigidBody</b>\n-<b>Sphere Collider</b>: in the sphere collider check" +
                " 'IsTrigger', this collider is the area where the gun will be detected, when player enter in this, so modify the position and radius if is necessary.\n-<b>Box Collider</b>: " +
                " with 'IsTrigger' unchecked, make the Bounds of this collider fit exactly with the weapon mesh.\n\nSo the inspector should look like this:");
            DrawServerImage(20);
            DownArrow();
            DrawText("and the Colliders Bounds should look like this: ");
            DownArrow();
            DrawServerImage(21);

        }
        else if (subStep == 1)
        {
            DrawText("Now Add the script <b>bl_GunPickUp.cs</b> and set up the variables");
            DownArrow();
            DrawPropertieInfo("GunID", "enum", "Select the Weapon ID of this weapon, the one that you set up in GameData");
            DrawPropertieInfo("Bullets", "int", "The bullets that contains this weapon when someone pickup");
            DrawPropertieInfo("DestroyAfterTime", "bool", "Will the prefab get destroyed automatically after some time since was instantiated?");
            DownArrow();
            DrawSuperText("Create a prefab of this weapon pick up by dragging the weapon object from hierarchy window to a folder in the Project Window.\n \nThen select the <b>TP Weapon Container</b> prefab which is located in: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Weapons ➔ Containers ➔ TP Weapons</i> <?link=asset:Assets/MFPS/Content/Prefabs/Weapons/Containers/TP Weapons.prefab>(click here to ping)</link>.\n \nWith the prefab selected, navigate to the inspector of <b>bl_WorldWeaponsContainer</b> > foldout the <b>Weapons</b> list > foldout the field of your weapon target > assign the just created weapon pick up prefab in the <b>Weapon Pick Up Prefab</b> field and that's.\n\nWith that you have completely finish the integration of your weapon!");
            DrawServerImage("img-38.png");
        }
    }

    void DrawExportWeapons()
    {
        DrawText("The weapon system count with an useful feature which is the ability to <b>Export</b> and <b>Import</b> weapons setups, and with <b>\"Weapons Setups\"</b> I refer to the FPWeapon, TPWeapon, GunInfo, Position, Rotation, etc.. of a weapon from a player prefab.\n\nThis feature is especially useful when you need to port a weapon from a player prefab to another or even to another Unity Project, e.g: lets say you just integrated a new weapon in the player prefab 1, but you also want to integrate that weapon in the player prefab 2, instead of do all the steps again in the player 2 since you already did in player 1 you only have to export the setup (from player 1) and import in the player 2, that way you only have to worry about integrating a weapon one time, good right?\n\nOk, to the point, the first step is export the weapon from the player prefab where you already integrated it, so open the player prefab either dragging it to the scene hierarchy or opening it in the prefab scene, then select the FPWeapon (under WeaponManager) on the top the inspector of bl_Gun.cs you'll see a button called <b>Export</b>, Do click on it, after that a new small window will appear, on that window click on the <b>EXPORT WEAPON</b> button, a Window dialog will appear to select the folder where to save the exported weapon, so yeah, select the folder where you'll save the exported weapon.\n");
        DrawAnimatedImage(6);

        DrawText("Ok, now you have the exported weapon ready, now you can <b>Import</b> in any player prefab, for it simple open the Player prefab where you want to import the weapon, but this time select the <b>WeaponManager</b> object and in the inspector of bl_GunManager click on the <b>Import</b> button <i>(top right)</i>, after this a new window will open, on this window you'll have a empty field <b>Weapon To Import</b> in this field drag the <b>Exported Weapon</b> prefab that you just saved and click on the <b>Import</b> button.\n\nThat's now the weapon will be fully integrated in this player prefab too.\n");
    }

    void AnimateWeaponDoc()
    {
        if (subStep == 0)
        {
            DrawText("A frequently asked question about the weapons is <b>\"How to animate the weapons\"</b>, as is mentioned before, in order to add integrate a new weapon in MFPS for the first-person view, you need:\n \n•  The Arms/Hand Model\n•  The Weapon Model\n•  4 Animations <b>(Draw, Hide, Fire, and Reload)</b>");

            DrawText("<b><size=16>Custom First Person Animations</size></b>\n \nNow, either way, if you use the MFPS example arms/hand model or a custom arm model if you want your new weapon animations to look good, you will have to create custom animations for that pair <i>(arm + weapon model)</i>.\n \nWith <b>Custom Animations</b> I refer to animations that are specifically created for one arm + weapon model pair, it's necessary to be created for a specific weapon model since most of the weapons don't have the same shape which causes the animation arm poses to not align correctly with a different weapon model of which the animation was created for.");

            DrawText("<b><size=16>How to create custom first-person animations?</size></b>\n \n<b>The animated process is independent of MFPS</b>, and animation is a skill that required a learning curve and a lot of practice to create good animations, if you are not an animator and it's under your capabilities, probably hiring an animator to create the animations could potentially save you time and work.\n \nBut if you can or are willing to create the animations yourself, below you will find some useful tutorials on how to animate first-person weapons inside of Unity or with third-party programs like Blender.");

            DrawNote("Once you have the required animations, check again the <b>FPV Weapon > Step 6</b> section to see how to assign the animations to your weapon.");

            Space(10);
            DrawHyperlinkText("<b><size=22>INSIDE UNITY</size></b>\n\nIf you want to create animations inside Unity for quick prototyping and fast development the build-in Unity Animation system comes in handy, but for more complex cases like this where there're a lot of bones and you want to use Inverse Kinematics, constrains, etc... Unity animation is not the ideal, instead, you can use an external editor tool available in the Asset Store with a free edition called \"UMotion\", you can check it out here: <link=https://assetstore.unity.com/packages/tools/animation/umotion-community-animation-editor-95986?aid=1101lJFi>UMotion Pro</link>\n\nand it's free edition: <link=https://assetstore.unity.com/packages/tools/animation/umotion-community-animation-editor-95986?aid=1101lJFi>UMotion Community</link>");
            DrawText("Here a video of how UMotion can be used to animated first person weapons:");
            DrawYoutubeCover("(FPS) - UMotion In Practice", GetServerImage("https://img.youtube.com/vi/nZPWVPYw41Y/0.jpg"), "https://www.youtube.com/watch?v=nZPWVPYw41Y&ab_channel=SoxwareInteractive");
            Space(10);
            DrawHyperlinkText("<b><size=22>ANIMATION SOFTWARE</size></b>\n \nA more advanced solution and more commonly used by artists for these kinds of animations is to use third-party programs that specialize in modeling and animation, but of course, these require a learning curve more extended, and in order to get to use it will require some practice, here are a list the most popular programs:\n \n<link=https://www.blender.org/download/>Blender (Free)</link>\n<link=https://www.autodesk.co.uk/products/maya/free-trial>Maya (Paid or free w student edition)</link>\n<link=https://www.autodesk.co.uk/products/3ds-max/free-trial>3D Max (Paid or free w student edition)</link>\n\nBelow some useful tutorials for animate first-person weapons:");

            DrawYoutubeCover("Blender FPS Rigging & Animation Tutorial", GetServerImage("https://img.youtube.com/vi/L2ZqWDUVWoY/0.jpg"), "https://www.youtube.com/watch?v=L2ZqWDUVWoY&list=PLn8ROcXT8fZgjdv4w6FczF8ziXkavUxdu&ab_channel=DavidStenfors");
            DrawYoutubeCover("How to make FPS Animations in Blender 2.8+", GetServerImage("https://img.youtube.com/vi/IV6XP-EDzw8/0.jpg"), "https://www.youtube.com/watch?v=IV6XP-EDzw8&ab_channel=thriftydonut");
            DrawYoutubeCover("How to create animation and animation group for FPS arms and guns inside blender 2.8", GetServerImage("https://img.youtube.com/vi/DWOWdZf8MDA/0.jpg"), "https://www.youtube.com/watch?v=DWOWdZf8MDA&ab_channel=SaqibHussain");
            DrawYoutubeCover("How Great First-Person Animations are Made", GetServerImage("https://img.youtube.com/vi/dclA9iwZB_s/0.jpg"), "https://www.youtube.com/watch?v=dclA9iwZB_s&ab_channel=CGCookie");

            DrawHTMLYoutube("L2ZqWDUVWoY");
            DrawHTMLYoutube("IV6XP-EDzw8");
            DrawHTMLYoutube("DWOWdZf8MDA");
            DrawHTMLYoutube("dclA9iwZB_s");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=16>Generic First-Person Animations</size></b>\n \nSince version 1.10, MFPS includes some generic first-person animations that allow you to integrate weapons without the need to create custom first-person animations right away, instead, generic animations will be used.");
            DrawText("This is a great solution for fast prototyping, but keep in mind that these generic animations are designed as placeholders, they can be used as the final solution if you want but that is not the intention since these animations do not animate any of the arms or weapon bones, the animations are really simplistic, if you want the animation looks better with your new weapon <b>Custom Animations</b> is the only way.\n \nTo use these Generic Animations all you have to do is set the <b>Animation Type</b> in <b>bl_WeaponAnimation</b> inspector to <b>Generic</b> and make sure the Transform position and rotation of where the <b>bl_WeaponAnimation</b> script is attached to is set to <b>(0,0,0)</b>");
            DrawServerImage("img-26.png");
        }
    }

    [MenuItem("MFPS/Tutorials/Add Weapon", false, 500)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AddWeaponTutorial));
    }
}