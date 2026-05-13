using MFPSEditor;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class MFPSGeneralDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/general/";
    private readonly NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new() {Name = "img-0.jpg", Image = null},
        new() {Name = "img-1.jpg", Image = null},
        new() {Name = "img-2.jpg", Image = null},
        new() {Name = "img-3.png", Image = null},
        new() {Name = "img-5.jpg", Image = null},
        new() {Name = "img-6.jpg", Image = null},
        new() {Name = "img-7.jpg", Image = null},
        new() {Name = "img-8.png", Image = null},
        new() {Name = "img-9.png", Image = null},
        new() {Name = "img-4.jpg", Image = null},
        new() {Name = "img-10.png", Image = null},
        new() {Name = "img-11.png", Image = null},
        new() {Name = "img-12.png", Image = null},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_33.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_6.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_27.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_14.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_31.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_22.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_24.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_19.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_29.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://img.youtube.com/vi/ysYqI4w1vq4/0.jpg", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "https://www.lovattostudio.com/documentations/mfps2/assets/images/image_23.png", Image = null, Type = NetworkImages.ImageType.Custom},
        new() {Name = "img-13.jpg", Image = null},
        new() {Name = "img-14.jpg", Image = null},
        new() {Name = "img-15.png", Image = null},
        new() {Name = "img-16.png", Image = null},
        new() {Name = "img-17.png", Image = null},
        new() {Name = "img-18.png", Image = null},
        new() {Name = "img-19.png", Image = null},
        new() {Name = "img-20.png", Image = null},//31
        new() {Name = "img-21.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new(){ Path = "createwindowobj.gif" },
        new(){ Path = "addnewwindowfield.gif" },
        new(){ Path = "createwindowbutton.gif" },
        new(){ Path = "addonintegrateprevw.gif"},
        new(){ Path = "mfps-urpcsps.gif"},
        new(){ Path = "playerai-1.gif"},
        new(){ Path = "cpaoc.gif"},
        new(){ Path = "adcwtp.gif"},
        new(){ Path = "adcwtp2.gif"},
    };
    private readonly Steps[] AllSteps = new Steps[] {
    new() { Name = "Resume", StepsLenght = 0, DrawFunctionName = nameof(Resume) },
    new() { Name = "Terminology", StepsLenght = 0, DrawFunctionName = nameof(TerminologyDoc) },
    new() { Name = "How to Modify MFPS", StepsLenght = 3, DrawFunctionName = nameof(HowToEditMFPSDoc),
     SubStepsNames = new string[]{ "How to Modify", "Code Modifications", "Other Modifications" } },
    new() { Name = "GameData", StepsLenght = 0, DrawFunctionName = nameof(GameDataDoc) },
    new() { Name = "Photon PUN", StepsLenght = 4, DrawFunctionName = nameof(DrawPhotonPunDoc),
    SubStepsNames = new string[]{ "Photon PUN", "Photon Server", "What is CCU?", "Chinese Mainland Region"}},
    new() { Name = "Offline", StepsLenght = 0, DrawFunctionName = nameof(OfflineDoc) },
    new() { Name = "URP", StepsLenght = 4, DrawFunctionName = nameof(UniversalRPDoc) },
    new() { Name = "HDRP", StepsLenght = 5, DrawFunctionName = nameof(HDRPDoc) },
    new() { Name = "Player Prefabs", StepsLenght = 0, DrawFunctionName = nameof(PlayerPrefabsDoc) },
    new() { Name = "Player Classes", StepsLenght = 2, DrawFunctionName = nameof(PlayerClassesDoc),
    SubStepsNames = new string[] { "Player Classes", "Loadouts" } },
    new() { Name = "Game Audio", StepsLenght = 4, DrawFunctionName = nameof(AudioDoc),
    SubStepsNames = new string[]{ "Audio Reskin", "Audio Ranges", "Audio Volumes", "Audio Assets"}},
    new() { Name = "Game Texts", StepsLenght = 2, DrawFunctionName = nameof(DrawGameTexts),
    SubStepsNames = new string[] { "Coded Text", "Profanity Filter" } },
    new() { Name = "Game Input", StepsLenght = 5, DrawFunctionName = nameof(GameInputDoc),
    SubStepsNames = new string[]{ "Game Input", "Default Mapped", "Add Input", "GampePad", "Add Mapped" }},
    new() { Name = "Game UI", StepsLenght = 2, DrawFunctionName = nameof(GameUIDoc),
    SubStepsNames = new string[]{ "UI Reskin", "UI Assets" }},
    new() { Name = "Teams", StepsLenght = 0, DrawFunctionName = nameof(DrawTeamsDoc) },
    new() { Name = "Coins", StepsLenght = 2 , DrawFunctionName = nameof(DrawCoins),
    SubStepsNames = new string[]{ "Properties", "Operations" } },
    new() { Name = "Item Unlock", StepsLenght = 0, DrawFunctionName = nameof(ItemUnlockDoc) },
    new() { Name = "Game Modes", StepsLenght = 3, DrawFunctionName = nameof(GameModesDoc),
    SubStepsNames = new string[]{ "Game Modes", "Custom Mode", "Per Map Modes" } },
    new() { Name = "Player Animations", StepsLenght = 4, DrawFunctionName = nameof(DrawPlayerAnimationDoc),
    SubStepsNames = new string[]{ "Basic", "Advance", "Weapon Animations", "Animation Assets" } },
    new() { Name = "Name Plates", StepsLenght = 0, DrawFunctionName = nameof(NamePlatesDoc) },
    new() { Name = "Bullets", StepsLenght = 3 , DrawFunctionName = nameof(DrawBullets),
    SubStepsNames = new string[]{ "Bullet Prefab", "Bullet Decals", "Custom Bullet" } },
        new() { Name = "Player Hitbox", StepsLenght = 2, DrawFunctionName = nameof(PlayerHitboxDoc),
    SubStepsNames = new string[]{ "Colliders", "Damage"}},
    new() { Name = "Deal Damage", StepsLenght = 2, DrawFunctionName = nameof(PlayerDamageDoc),
     SubStepsNames = new string[]{ "Player Damage", "Object Damage"}},
    new() { Name = "Kill Zones", StepsLenght = 0, DrawFunctionName = nameof(DrawKillZones) },
    new() { Name = "Room Properties", StepsLenght = 0, DrawFunctionName = nameof(RoomPropertiesDoc) },
    new() { Name = "Game Settings", StepsLenght = 0, DrawFunctionName = nameof(DrawGameSettings) },
    new() { Name = "Mouse Look", StepsLenght = 0, DrawFunctionName = nameof(MouseLookDoc) },
    new() { Name = "Object Pooling", StepsLenght = 0, DrawFunctionName = nameof(DrawObjectPooling) },
    new() { Name = "Add New Menu", StepsLenght = 0, DrawFunctionName = nameof(AddNewMenu) },
    new() { Name = "Player Arms IK", StepsLenght = 0, DrawFunctionName = nameof(PlayerIKDoc) },
    new() { Name = "Crosshair", StepsLenght = 0, DrawFunctionName = nameof(CrosshairDoc) },
    new() { Name = "Mobile", StepsLenght = 0, DrawFunctionName = nameof(DrawMobileDoc) },
    new() { Name = "Particles & Decals", StepsLenght = 2, DrawFunctionName = nameof(ParticlesDecalsDoc) ,
    SubStepsNames = new string[]{ "Particles & Decals", "FX Assets" }},
    new() { Name = "Post Processing", StepsLenght = 3, DrawFunctionName = nameof(PostProcessingDoc),
        SubStepsNames = new string[]{ "Post Processing", "Custom Profile", "Error Handling" } },
    new() { Name = "Friend List", StepsLenght = 0, DrawFunctionName = nameof(DrawFriendListDoc) },
    new() { Name = "InGame Chat", StepsLenght = 0, DrawFunctionName = nameof(InGameChatDoc) },
    new() { Name = "FootStep", StepsLenght = 0, DrawFunctionName = nameof(FootStepsDoc) },
    new() { Name = "Head Bob", StepsLenght = 0, DrawFunctionName = nameof(HeadBobDoc) },
    new() { Name = "Kill Feed", StepsLenght = 2, DrawFunctionName = nameof(KillFeedDoc) },
    new() { Name = "Doors", StepsLenght = 0, DrawFunctionName = nameof(DoorsDoc) },
    new() { Name = "Item Drop", StepsLenght = 3, DrawFunctionName = nameof(DrawKitsSystem),
        SubStepsNames = new string[] { "Item Drop", "Replace Item Models", "Add new item" } },
    new() { Name = "Map Camera", StepsLenght = 4, DrawFunctionName = nameof(MapCameraDoc),
        SubStepsNames = new string[] { "Map Camera", "Initial POV", "Match Camera Positions", "Spectator" } },
    new() { Name = "Server Regions", StepsLenght = 0, DrawFunctionName = nameof(ServerRegionDoc) },
    new() { Name = "Local Notifications", StepsLenght = 0, DrawFunctionName = nameof(LocalNotificationsDoc) },
    new() { Name = "Kick Vote", StepsLenght = 0, DrawFunctionName = nameof(KickVotationDoc) },
    new() { Name = "Network Stats", StepsLenght = 0, DrawFunctionName = nameof(NetworkStats) },
    new() { Name = "Weapon Recoil", StepsLenght = 0, DrawFunctionName = nameof(WeaponRecoilDoc) },
    new() { Name = "Ladder", StepsLenght = 0, DrawFunctionName = nameof(LadderDoc) },
    new() { Name = "Events", StepsLenght = 0, DrawFunctionName = nameof(MFPSEventsDoc) },
    new() { Name = "Hooks", StepsLenght = 2, DrawFunctionName = nameof(HookDoc),
     SubStepsNames = new string[] { "Hook System", "Defaul Hooks" }},
    new() { Name = "Addons", StepsLenght = 3, DrawFunctionName = nameof(DrawAddonsDoc),
    SubStepsNames = new string[] { "MFPS Addons", "Addons Manager", "How to Integrate" } },
    new() { Name = "Editor Menus", StepsLenght = 2, DrawFunctionName = nameof(EditorMenusDoc),
     SubStepsNames = new string[] { "Menu Options", "Overlay Toolbar" }},
    new() { Name = "Update MFPS", StepsLenght = 2, DrawFunctionName = nameof(UpdateMFPSDoc),
    SubStepsNames = new string[]{ "Upgrade Version", "Patch Update" }},
    new() { Name = "Anti-Cheat", StepsLenght = 0, DrawFunctionName = nameof(AntiCheatDoc) },
    new() { Name = "FP Arms Material", StepsLenght = 0, DrawFunctionName = nameof(FPArmsMaterial) },
    new() { Name = "AFK", StepsLenght = 0, DrawFunctionName = nameof(AfkDoc) },
    new() { Name = "Lobby Chat", StepsLenght = 0, DrawFunctionName = nameof(DrawLobbyChat) },
    new() { Name = "Common Q/A", StepsLenght = 0, DrawFunctionName = nameof(CommonQADoc) },
    new() { Name = "Known Issues", StepsLenght = 0, DrawFunctionName = nameof(KnownIssuesDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        FetchWebTutorials("mfps2/tutorials/");
        allowTextSuggestions = true;
        SetHTMLHeaderLinks(new System.Collections.Generic.Dictionary<string, string>() { { "Community", "https://discord.gg/8zF5B4G" }, { "Website", "https://www.lovattostudio.com/en/" }, { "Get MFPS", "https://www.lovattostudio.com/en/shop/template/mfps-2-0/" } });
        SetHTMLTitle("MFPS Doc");
        SetHTMLLink("https://lovattostudio.com/documentations/mfps2/general.html");

    }

    void Resume()
    {
        DrawTitleText($"MFPS 2.0 Version {MFPSEditor.AssetData.Version}");
        DrawSimpleText("Stuck or need more clarity on something not covered in the docs or tutorials? <b>Join our Discord community!</b> Get direct help from us or other community members, connect with fellow creators, and explore a large amount of resources for development, custom mods, and tools to enhance your work with MFPS.");
        if (!IsHTMLPass()) DrawServerImage("joind.png", TextAlignment.Left, false, () =>
         {
             Application.OpenURL("https://discord.gg/8zF5B4G");
         });
        DrawRawHtml(@"<iframe src=""https://discord.com/widget?id=728294461389668413&theme=dark"" width=""350"" height=""500"" allowtransparency=""true"" frameborder=""0"" sandbox=""allow-popups allow-popups-to-escape-sandbox allow-same-origin allow-scripts""></iframe>");

        DrawYoutubeCover("MFPS Get Started Video", GetServerImage(22), "https://www.youtube.com/watch?v=ysYqI4w1vq4");
        DrawTitleText("Hot Tutorials");

        if (DrawLinkText("Add Maps"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Map");
        }
        if (DrawLinkText("Add Weapon"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Weapon");
        }
        if (DrawLinkText("Add Players"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Add Player");
        }
        if (DrawLinkText("Change Bots"))
        {
            EditorApplication.ExecuteMenuItem("MFPS/Tutorials/Change Bots");
        }

        if (IsHTMLPass())
        {
            DrawLinkText("Add Maps", "https://lovattostudio.com/documentations/mfps2/add-maps.html");
            DrawLinkText("Add Weapons", "https://lovattostudio.com/documentations/mfps2/add-weapons.html");
            DrawLinkText("Add Players", "https://lovattostudio.com/documentations/mfps2/add-players.html");
            DrawLinkText("Change Bots", "https://lovattostudio.com/documentations/mfps2/change-bots.html");
        }

        DrawTitleText("Custom Integration Tutorials");
        DrawLinkText("Integrate Loading Screen to MFPS", "https://www.lovattostudio.com/en/integrate-loading-screen-to-mfps-2-0/");
        DrawLinkText("Integrate DestroyIt to MFPS", "https://www.lovattostudio.com/en/integrate-destroyit-to-mfps-2-0/");
    }

    void TerminologyDoc()
    {
        DrawText("In this documentation, you might come across some terms or phrases that are unfamiliar to you. Here, you'll find a concise explanation of what each one means.");
        DrawHorizontalSeparator();
        Space(10);
        DrawHorizontalColumn("GameData", "Is a <i>ScriptableObject</i> that contain many front-end settings of MFPS that you can easily tweak to fit your needs and reskin the game, the options go from simple toggles to show or hide blood in the game up to the Weapon and Game Mode information, it can be found in the <b>Resources</b> folder of MFPS.\n\nFor more information check the <b>GameData</b> section.");

        DrawHorizontalColumn("Player Prefab", "Are the unity prefabs or prefab that contain all the required scripts, objects, and structure that make up the MFPS player controller,\nby default the player prefabs are located in the <b>Resources</b> folder of MFPS.\n\nFor more information, check the <b>Player Prefab</b> section.");

        DrawHorizontalColumn("FPWeapon", "Stands for '<b>First Person Weapon</b>' is referring to the weapon that it seems for the local player camera, also referenced as the <b>view model</b>, What differentiates this weapon from the <i>TPWeapon</i> is that it contains the weapon and the arms/hands model and it only shows for the local player camera.\n \nThe FPWeapons can be found inside the <b>Local</b> child of each player prefab.");

        DrawHorizontalColumn("TPWeapon", "Stands for '<b>Third Person Weapon</b>' is referring to the weapon that it seems in the other player's soldier model, also referenced as the <b>world model</b>, What differentiates this weapon from the <i>FPWeapon</i> is that it is a single weapon model and is placed inside the player/soldier model hands.\n \nThe TPWeapons can be found inside the <b>Remote</b> child of each player prefab, specifically in the right-hand bone of the player model.");

        DrawHorizontalColumn("Unity Top Menu", "Is referencing the Menu Items located at the top of the Unity Editor, <i><b>File, Edit, Assets, GameObject, Components, MFPS, etc...</b></i>");

        DrawHorizontalColumn("Index (List)", "In the context of a list or an array, the terms 'Index' or 'ID' refer to the specific position of an item within that list or array. The counting of these positions starts from 0, not 1. This is known as zero-based indexing.\n \nFor example, the first item in the list or array is at index 0, the second item is at index 1, and so on.");
    }

    void HowToEditMFPSDoc()
    {
        if (subStep == 0)
        {
            DrawText("This title might suggest there are limitations when modifying MFPS, but that’s not the case at all. If you know what changes you want to make and how to implement them, MFPS gives you complete freedom. It includes the full source code and game content, so you can modify everything to suit your needs.\n\nThe purpose of this guide is to provide recommendations for the <b>ideal way to modify MFPS</b>, especially if you want to make it easier to merge updates in the future.\n\nIf you’ve been using MFPS for a while, you might already know that merging updates has been one of its most challenging aspects. Each major update typically needs to be imported into a fresh Unity project. Trying to import a new update into a project with an older MFPS version will overwrite your custom changes, potentially breaking the game. As a result, the only way to apply improvements and fixes from new updates to older projects has been through manual merging—checking scripts, comparing prefabs, and so on—a tedious and time-consuming process.\n\nThis challenge was partly due to how MFPS was originally designed. The only way to modify the code was by editing the default scripts directly. The tightly coupled code and intricate web of references further compounded the difficulty. Unlike tools or extension assets, game templates are intended for developers to modify the core content, which inherently makes updating to newer versions more complex.\n\nStarting from version 1.9, the game’s backend design has been overhauled to address these issues. The codebase is now designed to be more modular and extensible. Most of the core scripts are inheritable, allowing developers to implement custom changes without directly modifying the default scripts. Instead, you can:\n\n1. Create a new script.\n\n2. Inherit from the relevant base class.\n\n3. Implement your custom changes.\n\nOn the next page, you’ll find more details about this updated design and how to effectively work with it.");
        }
        else if (subStep == 1)
        {
            DrawText("<size=20><b>Code Modifications</b></size>\n\n\nStarting with MFPS 1.9, most default scripts are built using inheritance to enable a polymorphic design.\n\n<b>Inheritance allows you (or your programmers) to build upon existing game classes to create new implementations while maintaining their core behavior. This approach enables code reuse, simplifies extending functionality, and keeps the original code intact.</b>\n\n<size=18><b>Example: Benefits of Code-Inheritance Design</b></size>\n\n\nBefore MFPS 1.9, if you wanted to modify the weapon pickup system, you would need to edit the default script, <b>`<b>bl_GunPickUp.cs</b>`</b>. However, this approach made it impossible to update that script in the future without losing your custom changes.\n\nWith MFPS 1.9 and later versions, the process is much simpler and more update-friendly. To make the same modification:\n\n1. Create a new script that inherits from the base class (<b>`<b>bl_GunPickUpBase</b>`</b>).\n\n2. Copy the relevant code from the default script into your new script.\n\n3. Implement your desired changes.\n\nYou don’t need to worry about breaking references because all connections point to the base class. The only additional step is to attach your new script to the GameObject where the default script was originally attached and then remove the default script.\n\n___________________________________________\n\n<size=20><b>How to Inherit a Script?</b></size>\n\n\nIf you are an intermediate or advanced programmer, you may already be familiar with this design pattern. It’s a widely used approach in good software design, as it makes code easier to maintain, scale, and extend. In addition, inheritance provides a polymorphic structure, enabling the implementation of new features or variations with minimal effort.\n\nIf you are less experienced or unfamiliar with code inheritance, here’s a simple example to guide you. Suppose you want to modify the weapon pickup logic. After creating a new script, you’ll start with a basic structure like this:");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : MonoBehaviour\n{\n    ...\n}");
            DrawText("What you have to do is change the base class <i>(the class name next to your class name)</i>, in this case, it's <color=#0E6148FF>MonoBeheaviour</color>, you have to change to the base class of <i>bl_GunPickUp</i> which is <color=#0E6148FF>bl_GunPickUpBase</color>, so you will have something like this:");
            DrawCodeText("using UnityEngine;\n \npublic class bl_GunPickUp2 : bl_GunPickUpBase\n{\n    ...\n}");
            DrawNote("Almost all the default scripts have this same name pattern of the base classes, they just end with the \"Base\" word at the end of the name, but you can always make sure which is the base of a script by opening the default script.");
            DrawText("Once you change the base class, you will have to override the base class functions/methods, if you don't know how to do this, simply copy the code of the original class <i>(in this case <b>bl_GunPickUp.cs</b>)</i> and paste it your script,\nThat is all you need, know you can make the changes that you want in your new script.");
            DrawHorizontalSeparator();
            DrawText("As said before, this is not a requirement to make code changes but is the recommended way, although not all code MFPS allow inheritance, as of version 1.9 more than half of the code has been redesigned to work that way but the works are still in progress, the target is that the whole code or at least most of it allows inheritance and be modular in future updates.");
        }
        else if (subStep == 2)
        {
            DrawText("The same principle outlined in <b>Code Modifications</b> applies to other game assets such as prefabs, menus, scenes, and more.\n\n<b>Ideally, you should avoid modifying default content directly. Instead, create a duplicate and work with the duplicated version.</b>\n\nFor example:\n• <b>MainMenu Scene</b>: Duplicate the `<b>MainMenu</b>` scene and keep the default scene untouched as a reference for future updates. This way, when a new MFPS update is released, your custom changes won't be overwritten, and you can compare the new version's changes to the default scene.\n\n• <b>Player Prefabs</b>: Instead of directly editing the default player prefabs (<b>`<b>MPlayer</b>`</b> and <b>`<b>MPlayer2</b>`</b>), create duplicates of these prefabs. Assign the duplicates in <b>GameData</b>, and use them for your modifications.\n\nThe takeaway is simple: <b>Don’t edit the default content—duplicate it and use the copy instead.</b> This approach not only protects your custom work during updates but also ensures you have a reference point to understand and integrate new changes.");
            DrawNote("In the editor, you can duplicate almost anything by selecting it in the Project View window and pressing <b>Ctrl + D</b> on Windows or <b>Command + D</b> on Mac.");
        }
    }

    void GameDataDoc()
    {
        DrawTitleText("GameData");
        DrawText("With MFPS you will notice that <b>GameData</b> is mentioned a lot in the documentation, readMe.txt, and many other comments.\nIf you don't know what <b>GameData</b> is, how it works or where it's located, here is a brief explanation:\n\n<b>GameData</b> is a <i>ScriptableObject</i> that contain a lot of front-end settings that you can easily tweak to fit your needs and reskin the game, the options go from simple toggles to show or hide blood in the game up to the Weapon and Game Mode information.");
        DrawHyperlinkText("<link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> is located in the <b>Resources</b> folder of MFPS:");
        DrawServerImage(3);
        DrawNote("You can also open this quickly using the <b>MFPS Manager</b> window by clicking <b>Ctrl + M</b> in the editor using Windows or <b>Command + M</b> on Mac.");
    }

    void DrawPhotonPunDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b>Photon Unity Networking (PUN)</b> is the networking solution used by MFPS to manage all network and server-side operations. It's widely regarded as one of the most robust and reliable options available for Unity. PUN is solid, scalable, and reliable, offering multiple server locations worldwide to ensure the lowest possible ping for players.\n\nIn Unity, PUN is provided as a third-party plugin that you can download for free from the Unity Asset Store.\n\n<size=18><b>Common Questions About PUN</b></size>\n\nWhen it comes to server-side solutions, costs are often a concern. With PUN, the Photon team handles all the heavy lifting, including server hosting, maintenance, scaling, and operational services. This means you don’t need to worry about managing servers yourself. However, because these services are provided by Photon, <b>PUN is a paid service</b>.\n\nThe good news is that PUN offers a <b>Free Plan</b> that’s perfect for development. You can use this plan during the early stages of your project and then upgrade to a paid plan when you’re ready to release your game");

            DrawHyperlinkText("You can see all the available Plans in their website:\n<link=https://www.photonengine.com/en-US/PUN/pricing>Photon Pun Plans</link>\n");
            DrawHorizontalSeparator();
            DrawText("A common question I receive is:\n\n<b>What About Authoritative Servers?</b>\n\nIf you have prior experience with networking systems, you might have noticed that Photon uses a <b>Client-Authority model</b> rather than a traditional <b>Authoritative Server model</b>. In this setup, a designated <b>Master Client</b> (one of the players) is responsible for authorizing and managing the game, instead of an independent <b>Master Server</b>.\n\nWhile this approach simplifies development and reduces server costs, it does introduce potential vulnerabilities. Since the Master Client resides on a player's device, it creates an opportunity for cheaters to manipulate gameplay locally, which can then be mirrored to other clients. Without an independent server to validate and enforce game logic, these exploits can sometimes go unchecked.");
            DrawHyperlinkText("Out of the box, Photon PUN doesn't offer a solid solution for fix this problem, instead they offer <link=https://www.photonengine.com/en-us/Server>Photon OnPremise</link> aka Photon Server with which you can host the server side code/sdk and make changes in the serve code and create your own authoritative server by modifying the server-side code but that also require some knowledge in the are, if you are interested in using Photon Server check the next section for more information regarding.");
        }
        else if (subStep == 1)
        {
            DrawText("As explained in the previous section, <b>Photon Server is an alternative to Photon PUN</b> which have some advantages but also some other things to take into consideration, some of the benefits of use Photon Server over Photon PUN are:\n \n- Self-Hosted servers and more control over the server-side code.\n- More affordable CCU plan prices with an unlimited plan option.\n- Allow hosting a server in a specific region not available in PUN.\n- Cheaper overall than using a Photon PUN plans.");
            DrawText("But these are things you have to take into consideration when using Photon Server:\n \n- You are in charge of keeping your server running and handling any server crash, shutdown, clean-up, etc...\n\n- Regions availability depends on where you host the server, at difference of Photon PUN which includes the option to easily change over 13 different regions around the globe, with Photon Server it will depend on where you host your server and that will be the unique region available unless you also add a server selection system in your game.\n\n- Scalability will depend on your type of hosting and plan, at the difference of Photon PUN where you can easily upgrade your plan if you require more CCUs, with Photon Server even if you have the unlimited CCU plan, if your hosting server is not prepared for auto-scalability you will have troubles growing your game, because of that is recommended that you use a resizable server hosting plan like EC2 of AWS.");
            DrawSuperText("For more detail information about Photon Server, check the website page here:\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-intro</link>");
            Space(18);
            DrawSuperText("<b><size=16>Use Photon Server with MFPS</size></b>\n \nUsing Photon Server with MFPS instead of Photon PUN doesn't require code changes in MFPS but you have to set up the server SDK manually and that may require some experience or knowledge in the area, fortunately, the official documentation explains the process clearly and step by step to set up and deploy your own server, you can found the guide here:\n<?link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min</link>");
            DrawSuperText("Once you have set up the Photon Server SDK and have it running, there're just a few things to do in your project to start using it:\n \n- In your MFPS Unity project, go to the <?link=asset:Assets/MFPS/Content/Required/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset>PhotonServerSettings (Click Here)</link> or located by default in <i>Assets ➔ MFPS ➔ Content ➔ Required ➔ Photon ➔ PhotonUnityNetworking ➔ Resources ➔ PhotonServerSettings</i>.\n\n- Foldout the <b>Server/Cloud Settings</b> > uncheck the <b>Use Name Server</b> toggle.\n\n- In the <b>Server</b> text field paste your server <i>(where you setup the Photon Server SDK)</i> public IP or domain name if apply.\n \n- In the <b>Port</b> input field set the port number that you open for your server or if you didn't change it set the default one which is <b>5055 for UDP</b> or <b>4530 for TCP</b>.\n \nThat's all, if everything was set up correctly you should not be able to play the game and it will be connecting to your server.");
            DrawServerImage("img-49.png", TextAlignment.Center);
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>What is CCU?</size></b>\n \n<b>CCU</b> stands for <b>Concurrently Connected Users</b> and with Photon and other networking plugins it refers to the limit number of concurrent players that the server or plan allows or supports.\n \nWith Photon PUN each plan has a CCU limit and when that limit is reached it will refuse more connections causing new players not able to connect until a slot gets free, the default plan which is for development purposes counts with a 20 CCU limit which means only 20 players can be connected at the same time.");
            DrawNote("Do not confuse <b>CCU</b> with <b>DAU</b> <i>(Daily Active Users)</i> or <b>MAU</b> <i>(Monthly Active Users)</i>, CCU is only the players connected at the same time.");
            DrawText("<b><size=16>How to know which CCU limit I need?</size></b>\n \nSince the higher the CCU limit a Photon plan has higher the monthly cost is, it's important to select the right plan, for it you can use the formula based on the average of CCU with respect to the DAU and MAU so based on how many daily or monthly players play your game, you can get an idea of which CCU plan will be enough for you.");
            DrawNote("This data is based on official Photon stats collected across many titles and games working with Photon.");
            DrawText("CCU to DAU has a factor of 10 to 100 (depending on the game).\nDAU to MAU has a factor of 10.\n \nSo <b>100 CCU might work for as many as 100k monthly active users</b>.\nThese numbers are distorted for new games that get a lot of attention and playtime initially, so it's never really a fixed factor");
        }
        else if (subStep == 3)
        {
            DrawSuperText("With Photon PUN by default you would not be able to connect to PUN regions if you are located in Chinese mainland region, this due to China restrictions, in order to be able to use Photon in China you need to request access to the Chinese Mainland region for your Photon application to the Photon Team.\n \nFor more details on this, check the Photon official documentation:\n <?link=https://doc.photonengine.com/realtime/current/connection-and-authentication/regions#using_the_chinese_mainland_region>https://doc.photonengine.com/realtime/current/connection-and-authentication/regions</link>");
        }
    }

    void OfflineDoc()
    {
        DrawText("MFPS supports the Photon <b>Offline mode</b>, this allows you to test the map scene without the need to go to the lobby -> create a room -> load the map scene, instead you play the scene directly.\n\nThis feature is especially useful when, for example, you make changes to the player's prefab or a weapon and want to test them at runtime, save lot of time and improve the development work-flow.\n\nTo enable or disable this feature go to <b>GameData</b> -> Offline Mode\nAfter you enable it, simply open the map scene and Play.");
        DrawNote("The offline mode <b>is not</b> designed or intended to develop an offline game using MFPS, but rather to facilitate the development of the multiplayer gameplay.");
    }

    void UniversalRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("By default MFPS use the legacy build-in render pipeline, MFPS will be using URP <i>(Universal Render Pipeline)</i> as the default Render Pipeline in the future when it's more standardized, but for the moment in order to use MFPS with URP or HDRP you have to manually convert the project, in this doc I'll teach you how you can do it:\n\n<b><size=20>Convert MFPS project to URP:</size></b>\n\n*<i>This tutorial takes for granted that you have an MFPS project working with the build-in render pipeline in Unity 2018.4 or later</i>*\n\nFirst of all, you have to remove the Post-Processing package, for this simply go to (Unity Top Menu) <b>MFPS -> Tools -> Delete Post-Processing</b> -> Wait until script compilation finish.\n\n<i>Continue in the next step.</i>");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=22>Installing URP</size></b>\n\n1. In Unity, open your Project.\n2. In the top navigation bar, select Window > Package Manager to open the Package Manager window.\n3. Select the All tab. This tab displays the list of available packages for the version of Unity that you are currently running.\n4. Select Universal RP from the list of packages.\n5. In the bottom right corner of the Package Manager window, select Install. Unity installs URP directly into your Project.\n\n<b><size=22>Configuring URP</size></b>\n\nBefore you can start using URP, you need to configure it. To do this, you need to create a Scriptable Render Pipeline Asset and adjust your Graphics settings.\n\nCreating the Universal Render Pipeline Asset\nThe Universal Render Pipeline Asset controls the global rendering and quality settings of your Project, and creates the rendering pipeline instance. The rendering pipeline instance contains intermediate resources and the render pipeline implementation.\n\n<b><size=16>To create a Universal Render Pipeline Asset:</size></b>\n\n1. In the Editor, go to the Project window.\n2. Right-click in the Project window, and select Create > Rendering: Universal Render    Pipeline: Pipeline Asset. Alternatively, navigate to the menu bar at the top, and select Assets: Create: Rendering: Universal Render Pipeline: Pipeline Asset.\n\nYou can either leave the default name for the new Universal Render Pipeline Asset, or type a new one.");
            DrawText("<b><size=16>Adding the Asset to your Graphics settings</size></b>\n\nTo use URP, you need to add the newly created Universal Render Pipeline Asset to your Graphics settings in Unity. If you don't, Unity still tries to use the Built-in render pipeline.\n\nTo add the Universal Render Pipeline Asset to your Graphics settings:\n\nNavigate to<b> Edit > Project Settings... > Graphics.</b>\nIn the <b>Scriptable Render Pipeline Settings</b> field, add the Universal Render Pipeline Asset you created earlier. When you add the Universal Render Pipeline Asset, the available Graphics settings immediately change. Your Project is now using URP.\n\n<b>Now you will see some (a lot) pink objects</b>, this is because the shaders from the build-in RP doesn't work on URP or HDRP, you have to upgrade the material shaders, in the next step, I'll show you how to convert them.");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=22>Upgrading your Shaders</size></b>\n\nIf your Project uses shaders from the built-in render pipeline, and you want to switch your Project to use the Universal Render Pipeline instead, you must convert those Shaders to the URP Shaders. This is because built-in Lit shaders are not compatible with URP Shaders. For an overview of the mapping between built-in shaders and URP Shaders, see Shader mappings.\n\nTo upgrade built-in Shaders:\n\n1. Open your Project in Unity, and go to Edit > Render Pipeline > Universal Render Pipeline.\n2. Select <b>Upgrade Project Materials to URP Materials</b>\n\n\n<b>Note:</b> These changes cannot be undone. Backup your Project before you upgrade it.\n\n<b>Tip:</b> If the Preview thumbnails in Project View are incorrect after you've upgraded, try right-clicking anywhere in the Project View window and selecting Reimport All.");
            Space(10);
            DrawText("After this, you still may see some pink objects, those objects were using a custom shader, so in order to fix them simply select them and change their material shader to a Universal RP Shader.\n");
            DrawServerImage(31);
            DrawText("There's one last thing you have to do, see the next step.");
        }
        else if (subStep == 3)
        {
            DrawText("Finally, there's one last thing that you have to set up.\nin URP and HDRP Camera's work different than the build-in RP, in URP/HDRP there's a 'Base' camera and if you want to render any other camera at the same time, you have to set up it as an '<b>Overlay Camera</b>' and add it on the <b>'Stack'</b> camera list of the '<b>Base Camera</b>'\n\nMFPS players use 2 cameras, one that draws only the FP Weapons and the other that draws everything else, so you have to configure the one that draws the FP Weapons as an 'Overlay Camera', <b>you have to do the following for each player prefab that you are using:</b>\n\n1. In the <b>Project Window</b>, select the player prefab <i>(the default MFPS player prefabs are located in the Resources folder of MFPS)</i> ➔ Click on <b>Open Prefab</b> button.");

            DrawNote("<color=#FFFC01FF>NOTE:</color> You may see a warning message when you select the player prefab, this is because there's a null component attached in the 'Weapon Camera' because the Post-Processing package was removed, to fix this simply remove the null component from the Weapon Camera.");
            DrawText("With the player prefab open do the following:");
            DrawAnimatedImage(4);
            DrawText("And that's, you can start using MFPS with URP, just remember that this last step has to be done in all the player prefabs that you are using.");
        }
    }

    private void SetupURPCamera(bl_PlayerReferences player)
    {
        if (player == null) return;

#if MFPS_URP
        UniversalAdditionalCameraData additionalCameraData = player.weaponCamera.GetUniversalAdditionalCameraData();
        UniversalAdditionalCameraData playerAdditionalCameraData = player.playerCamera.GetUniversalAdditionalCameraData();
        if (additionalCameraData == null) return;

        additionalCameraData.renderType = CameraRenderType.Overlay;
        playerAdditionalCameraData.cameraStack.Add(player.weaponCamera);

        EditorUtility.SetDirty(additionalCameraData);
        EditorUtility.SetDirty(playerAdditionalCameraData);
        EditorUtility.SetDirty(player.gameObject);
#endif
    }

    void HDRPDoc()
    {
        if (subStep == 0)
        {
            DrawText("HDRP is one of the new Unity's render pipelines aiming for high-end platforms which allow access to cutting-edge real-time 3D rendering technology designed to deliver high-fidelity graphics and uncompromising GPU performance. By default, MFPS uses the built-in Render Pipeline, but you can manually convert the project to HDRP, in this guide you will learn how to do it.");
            DrawSuperText("<b><size=16>Convert MFPS to HDRP</size></b>\n \nThis tutorial takes for granted that you have an MFPS project using the built-in render pipeline in Unity 2020.1 or later, some options features mentioned in this tutorial may not be located or called the same in newest versions of the editor, for these cases you can refer to the official Unity guide for your specific Unity version here:\n<?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html>https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Upgrading-To-HDRP.html</link>");
            DrawText("Before start setting up the render pipeline, is necessary to remove the <b>Post Processing</b> package since this is not supported in HDRP (Which includes their own PP system), to automatically remove the package, go to <i>(Unity Top Menu)</i> MFPS ➔ Tools ➔ <b>Delete Post-Processing</b> ➔ Wait until script compilation finish and then continue with the next step.");
        }
        else if (subStep == 1)
        {
            DrawSuperText("<b><size=16>Setting up HDRP</size></b>\n\nFirstly, to install HDRP, add the High Definition RP package to your Unity Project:\n \n<?list=•>Open your Unity project.\nTo open the Package Manager window, go to Window > Package Manager.\nIn the Package Manager window, in the Packages: field, select <b>Unity Registry</b> from the menu.\nSelect <b>High Definition RP</b> from the list of packages.\nIn the bottom right corner of the Package Manager window, select <b>Install.</b></list>");
            DrawNote("<i><size=8><color=#76767694>Note:</color></size></i> When you install HDRP, Unity automatically attaches two HDRP-specific components to GameObjects in your Scene. It attaches the <b>HD Additional Light Data</b> component to Lights, and the <b>HD Additional Camera Data</b> component to Cameras. If you don't set your Project to use HDRP, and any HDRP component is present in your Scene, Unity throws errors. To fix these errors, see the following instructions on how to set up HDRP in your Project.");
            DrawSuperText("To set up HDRP in your project, use the <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Render-Pipeline-Wizard.html>HDRP Wizard.</link>\n \nTo open the <b>HD Render Pipeline Wizard</b> window, go to <b>Window > Rendering > HD Render Pipeline Wizard</b>.\nIn the <b>Configuration Checking</b> section, go to the <b>HDRP</b> tab and click <b>Fix All</b>. This fixes every HDRP configuration issue with your Project.\nYou have fixed your Project's HDRP configuration issues, but your Scene doesn't render correctly because GameObjects in the Scene still use Shaders made for the Built-in Render Pipeline. To find out how to upgrade Built-in Shaders to HDRP Shaders, see Upgrading Materials in the next step.");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>Upgrading Materials</size></b>\n\nTo upgrade the Materials in your Scene to HDRP-compatible Materials:\n \n1. Go to <b>Edit > Rendering > Materials</b>\n2. Choose one of the following options:\n \n ■ <b>Convert All Built-in Materials to HDRP</b>: Converts every compatible Material in your Project to an HDRP Material.\n\n ■ <b>Convert Selected Built-in Materials to HDRP</b>: Converts every compatible Material currently selected in the Project window to an HDRP Material.\n\n ■ <b>Convert Scene Terrains to HDRP Terrains</b>: Replaces the built-in default standard terrain Material in every Terrain in the scene with HDRP default Terrain Material.");
            DrawText("<b><size=16>Limitations</size></b>\n\nThe automatic upgrade options described above can't upgrade all Materials to HDRP correctly:\n \nYou can't automatically upgrade custom Materials or Shaders to HDRP. You must convert custom Materials and Shaders manually, in MFPS the water shader is a custom shader that can't be automatically upgrade so you will have to either replace it with a HDRP water sahder or remove the water from your scene.\n\nHeight mapped Materials might look incorrect. This is because HDRP supports more height map displacement techniques and decompression options than the Built-in Render Pipeline. To upgrade a Material that uses a heightmap, modify the Material's Amplitude and Base properties until the result more closely matches the Built-in Render Pipeline version.\n\n<b>You can't upgrade particle shaders</b>. HDRP doesn't support particle shaders, but it does provide Shader Graphs that are compatible with the Built-in Particle System. These Shader Graphs work in a similar way to the built-in particle shaders. To use these Shader Graphs, import the Particle System Shader Samples sample:\n \n 1. Open the <b>Package Manager</b> window (menu: <b>Window > Package Manager</b>).\n 2. Find and click the <b>High Definition RP</b> entry.\n 3. In the package information for <b>High Definition RP</b>, go to the <b>Samples</b> section and click the <b>Import into Project</b> button next to <b>Particle System Shader Samples</b>.");
        }
        else if (subStep == 3)
        {
            DrawSuperText("<b><size=18>Adjusting lighting</size></b>\n\nHDRP uses <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Physical-Light-Units.html>physical Light un</link>its to control the intensity of Lights. These units don't match the arbitrary units that the Built-in render pipeline uses.\n \nFor light intensity units, Directional Lights use Lux and all other Light types can use Lumen, Candela, EV, or simulate Lux at a certain distance.\n \nTo set up lighting in your HDRP Project:\n \n1. To add the default sky Volume to your Scene and set up ambient lighting go to <b>GameObject > Volume > Sky and Fog Global Volume.</b>\n\n2. Set the <?link=https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@14.0/manual/Environment-Lighting.html>Environment Lighting</link> to use this new sky:\n \n - Open the Lighting window (menu: <b>Window > Rendering > Lighting Settings</b>).\n - In the <b>Environment</b> tab, set the <b>Profile</b> property to the same Volume Profile that the Sky and Fog Global Volume uses.\n - Set the <b>Static Lighting Sky</b> property to <b>PhysicallyBasedSky</b>.\n - Optionally, if you don't want Unity to re-bake the Scene's lighting when you make the rest of the changes in this section, you can disable the <b>Auto Generate</b> checkbox at the bottom of the window.");
            DrawText("3. Currently, the shadows are low quality. To increase the shadow quality:\n \nCreate a new <b>Global Volume</b> GameObject (menu: <b>GameObject > Volume > Global Volume</b>) and name it <b>Global Settings.</b>\nTo create a new Volume Profile for this Global Volume:\n \n - Open the Global Volume's Inspector window and go to the Volume component.\n - Go to <b>Profile</b> and select <b>New</b>.\n - To add a Shadows override:\n \n  - Go to Add <b>Override > Shadowing > Shadows</b>.\n  - Enable Max Distance.\n  - Set Max Distance to 50.\n\n4.Configure your Sun Light GameObject.\n \n - Select your Light GameObject that represents the Sun in your Scene to view it in the Inspector.\n - Go to <b>Emmision</b> and set the Intensity to 100000.\n - Set <b>Light Appearance</b> to <b>Color</b>.\n - Set <b>Color</b> to white.\n - To see the sun in the sky, go to <b>Shape</b> and set <b>Angular Diameter</b> to 3.\n\n5. The Scene is now over-exposed. To fix this:\n \n - Select the <b>Global Settings</b> GameObject you created in step 3.\n - Add an <b>Exposure</b> override to its Volume component (menu: Add <b>Override > Exposure</b>).\n - Enable <b>Mode</b> and set it to <b>Automatic</b>.\n - To refresh the exposure, go to the Scene view and enable <b>Always Refresh</b>.");
            DrawServerImage("img-50.png");
        }
        else if (subStep == 4)
        {
            DrawText("Finally, there's a manual setup you have to do for all the MFPS player prefabs to stack the player cameras:\n \n1. Open the player prefab by opening it in the prefab editor or dragging it into a scene hierarchy.\n \n2. Inside the Player Prefab hierarchy, select the <b>WeaponCamera</b> located in <b>Local > Mouse > Animations > Main Camera > WeaponCamera</b>.\n \n3. In the inspector window of the <b>Camera</b> component > unfold the <b>Output</b> tab and set the <b>Depth</b> value to 2.\n \nApply/Save the player prefab changes and repeat this process with all the remaining player prefabs in your game; once you finish with this, you have complete the basic project conversion to HDRP.");
            DrawServerImage("img-51.png");
        }
    }

    void KillFeedDoc()
    {
        if (subStep == 0)
        {
            DrawTitleText("KILL FEED");
            DrawText("<i>Kill Feed</i> or <i>Who Kill Who</i> is the UI text notification panel where displays events of the match like player's kills and other in-game eliminations, showing to the players who get eliminated and who eliminated him.\n \nNormally, this panel is placed in a corner of the screen to not interfere with the gameplay but easily visible.\n  \nHere I'll show you a few options that you have to customize this system without requiring to touch the code and how you can display your own events on it.");
            DownArrow();
            DrawText("• MFPS comes with two modes to display kills events on the kill feed, in a kill event there are 3 parts of event information to display: the name of the player who was eliminated, the name of the player who did the elimination, and the weapon or cause of the elimination.\n \nYou have two ways of shows the cause of the eliminations:\n \n<b>Weapon Name:</b>");
            DrawServerImage(0);
            DrawText("<b>Weapon Icon:</b>");
            DrawServerImage(1);
            DrawText("By default <b>Weapon Icon</b> is the default option, you can change that in <b>GameData</b> -> KillFeedWeaponShowMode.\n \nAnother option to customize is the color to highlight the local player name when this " +
                "appear in the kill feed, for the context player names in kill feed are represented by the color of his Team but in order to the local player easily knows when an event that include him appear in the kill feed, his name " +
                "should be highlight with a different color, <b>to choose that color</b> go to GameData -> <b>HighLightColor.</b>\n \nOkay, that are the front-end customize options, if you want to customize the way that the UI looks" +
                " you have to do the in the UI prefab which is located in: <i>Assets -> MFPS -> Content -> Prefabs -> UI -> Instances -> <b>KillFeed</b></i>, drag this prefab inside the kill feed panel in canvas which is located by default in: ");
            DrawServerImage(2);
            DrawText("Right, these are all customize options that you have in front end, if you wanna create your own events to display, check the next step.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("CREATE KILLFEED EVENTS");
            DrawText("The kill feed system various type of events to display, use the one that fits your event:\n \n<b>Kill Event:</b>\n \n• This is should use when of course a kill event happen, but a kill that include two actors " +
                "the killer and the killed, to show that you have to call this:");
            DrawCodeText("bl_KillFeed.Instance.SendKillMessageEvent(string killer, string killed, int gunID, Team killerTeam, bool byHeadshot);");
            DrawText("<b>Message:</b>\n \n• If you want to show a simple text of an event in concrete that doesn't include a player in specific, use:");
            DrawCodeText("bl_KillFeed.Instance.SendMessageEvent(string message);");
            DrawText("<b>Team Highlight:</b>\n \n• If you want to show a text of an event in concrete that as subject have a team in specific and you wanna highlight a part of the text with the tam color, use:");
            DrawCodeText("bl_KillFeed.Instance.SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);");
        }
    }

    void PlayerClassesDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b><size=16>Player Classes</size></b>\n\n<b><size=12>Overview</size></b>\n\nMFPS features four distinct player classes, each designed with unique capabilities and roles. These classes are <b>Assault, Support, Recon, and Engineer</b>. A key feature of these classes is their ability to have customized weapon loadouts, aligning with their specialized roles or 'operator specialties'.\n \n<b><size=14>Class-Specific Weapon Loadouts</size></b>\n\n<b>Assault Class:</b> This class can be equipped with a weapon loadout tailored for assault or rush tactics. The loadout typically includes weapons and gear suited for close to mid-range combat.\n\n<b>Support Class:</b> The Support class loadout can be optimized for roles like providing cover fire or team support.\n\n<b>Recon Class:</b> Ideal for sniping or long-range engagements, the Recon class's loadout is designed for distance combat.\n\n<b>Engineer Class:</b> The Engineer's loadout can be tailored for tasks like technical roles on the battlefield.\n\n<b><size=14>Customization of Class Names</size></b>\n\n<b>Default Names:</b> By default, the classes are named <b>Assault, Support, Recon, and Engineer.</b>\n\n<b><size=12>Modifying Class Names:</size></b>\n\nYou can easily change these names to better suit your game's theme or style.\nTo do this, go to the inspector of <b>GameData</b> and locate the <b>Player Classes Name</b> section.\nHere, you can input new names for each class, customizing them as per your game's narrative or design");
        }
        else
        {
            DrawText("<b><size=18>Weapon Loadouts</size></b>\n\n<b><size=14>Overview</size></b>\n\nIn MFPS, weapon loadouts play a crucial role in defining a player's combat capabilities within a match. A weapon loadout refers to the specific set of weapons a player is equipped with upon spawning.\n \n<b><size=14>Configuration of Loadouts</size></b>\n\nEach Player Class in MFPS can be assigned a distinct weapon loadout, allowing for tailored combat styles and strategies, a loadout includes four weapon slots. Each slot is designated for a different weapon. Additionally, there is an extra slot allocated for a 'drop kit'.\n\nDefault Loadouts Location: By default, the four primary loadouts for all player classes are stored in a specific folder within the project (<i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Weapons ➔ Loadouts➔*</i>).\n\n<b><size=14>Modifying Default Weapons</size></b>\n\n- To change the default weapons in a loadout, begin by opening the relevant player prefab. This is particularly useful if you want each player prefab have a different set of weapon loadouts.\n\n- Navigate to the <b>WeaponManager</b> object within the player prefab.\n\n- In the inspector, locate the <b>bl_GunManager</b> and expand the foldout for the desired class loadout (e.g., Assault Class, Recon Class, etc...), Here, you can modify the weapon slots, allowing you to customize the weapons each class starts with.\n<i>(if you want to create a new present before edit, simply click in the <b>New</b> button</i>");
            DrawServerImage(7);
            DrawNote("You also can edit the default class loadouts from the Project Window in the folder: <i>Assets->MFPS->Content->Prefabs->Weapons->Loadouts</i>");
        }
    }

    void HeadBobDoc()
    {
        DrawText("<b>Head Bob</b> is the camera movement that simulates the reaction of the player head when walk or run, in MFPS this movement is procedurally generated by code and you can adjust the value to obtain the desired result.\n\nIn order to obtain a more realistic result in MFPS we have sync the weapon bob and the head bob movement, so the settings will apply to both movements.\n\nYou can modify the values in the bl_WeaponBob.cs<i> (Attached in WeaponsManager object inside the players prefabs')</i>, you can edit in runtime to preview the movement as you edit it.\n");
        DrawServerImage(8);
        DownArrow();
        DrawText("If you want to use different movements per player or just want to have a backup of the current movement settings you can create a new \"Present\" of the settings and assing it in the script instead of the current one.\n\nFor create a new present simple select the folder where you wanna create it <i>(In Project View)</i> -> Right Click -> MFPS -> Weapons -> Bob -> Settings -> Drag the created profile in the bl_WeaponBob -> Settings -> Them make the changes that you want.\n");
    }

    void AfkDoc()
    {
        DrawTitleText("AFK");
        DrawText("AFK is an abbreviation for <i>away from keyboard</i>, players are called AFK when they have not interact with the game in a extended period of time.  in multi player games AFK players could be a problem," +
            "like for example in MFPS where players play in teams, an AFK player represent free points for the enemy team, or in different context AFK player are used to leveling up, so that is way some games count with a system " +
            "to detect these AFK players and kick out of the server/room after a certain period of time begin AFK.  MFPS include this system but <b>is disable by default</b>.\n \n" +
            "In order to enable AFK detection, go to GameData -> turn on <b>Detect AFK</b>, -> set the seconds before kick out players after detected as AFK in <b>AFK Time Limit</b>");
    }

    void KickVotationDoc()
    {
        DrawTitleText("KICK VOTATION");
        DrawText("In order to give an option to players to get rip of toxic, hackers, non-rules players in a democratic way where a player put the option on the table and the majority of the players in room " +
            "decide to kick out or cancel the petition, MFPS include a voting system.\n \nTo start a vote in game, players have to open the menu -> in the scoreboard click / touch over the player that they want to request the vote -> " +
            "in the PopUp menu that will appear -> Click on <b>Request Kick</b> button.\n \nBy default the keys to vote are F1 for Yes and F2 for No, you can change these keys in bl_KickVotation.cs which is attached in <b>GameManager</b> " +
            "in maps scenes.");
        DownArrow();
        DrawText("If you want to implement your own way to start a voting request, you can do it by calling:");
        DrawCodeText("bl_KickVotation.Instance.RequestKick(Photon.Realtime.Player playerToKick);");
    }

    private AssetStoreAffiliate soundsAssets;
    void AudioDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b><size=22>BACKGROUND AUDIO</size></b>\n \nBy default MFPS only use <b>background audio in the Lobby/MainMenu</b> scene, in order to change that soundtrack or remove it, you can do it by:\n \n■ In the <b>MainMenu</b> scene -> Lobby -> Scene -> AudioController -> <i>(Inspector window)</i> bl_AudioController -> assign/remove the audio clip in the field '<b>Background Clip</b>'");
            Space(20);
            DrawText("<b><size=22>BULLET HITS</size></b>\n \nThere's a bullet hit sound effects that's played when the bullet hit particle is instanced, by design that sound volume is really low since it can be annoying for some persons, in order to change the audio sound effect or volume of these bullet impacts <b>you have to open the Bullet Impact particle prefab</b> which by default are located in the MFPS folder at <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>\n \nOpen the prefab of the impact you want to modify -> in the <b>Audio Source</b> component attached in the prefab, assign/replace the Audio Clip and adjust the volume as you please.");
            Space(20);
            DrawText("<b><size=22>PLAYER HIT</size></b>\n \nAnother hit sound that is played is when the local player gets hit, there are two different sounds played, one when is hit by a bullet and the other when is any other kinda injure.\n \nYou can change these sounds per player prefab, simply select the <b>Player Prefab</b> where you wanna change the sounds -> bl_PlayerHealthManager -> in the lists:\n \n<b>Hits Sounds:</b> for the bullet hit sound effect\n<b>Injure Sounds:</b> for... well, the injured sound effects :D");
            Space(20);
            DrawText("<b><size=22>ROOM GENERAL SOUNDS</size></b>\n\nThere are some general sound clips that are listed in the room Audio Controler, you can easily change these clips in the map scene hierarchy ><b>GameManager > Audio Manager > bl_AudioController > Audio Controler > Audio Bank > <i>*foldout the list items*</i> > Clip</b>");
            Space(20);
            DrawText("<b><size=22>PLAYER FOOTSTEPS</size></b>\n \nCheck the Footstep section for more detail information on this.");
        }
        else if (subStep == 1)
        {
            DrawText("You may encounter a scenario where in your custom map you can hear players/bots shooting, footsteps, explosions, etc... sounds far away as if it were near to you, this is because the default audio ranges are not ideal for your map size, but this can be easily modified.");
            DrawText("<b><size=16>Adjust Audio Ranges</size></b>\n \nTo adjust the audio ranges in a specific map > open your map scene in the editor > in the hierarchy window go to <b>GameManager</b> > <b>Audio Manager</b> > in the inspector window of this object > <b>bl_AudioController</b> > you will see some slider parameters that you can use to adjust the range of certain sound types, the ranges are defined in meters which means setting up a range to 50 means the audio will be heard only when the player is 50 meter or close to the audio source.\n \nthe audio volume will gradually fade out based on the origin and the max distance.");
            DrawServerImage("img-47.png");
        }
        else if (subStep == 2)
        {
            DrawSuperText("<b><size=22>MFPS Audio Mixer</size></b>\n\nTo modify the game audio volumes in general for certain categories like <i>Footsteps, First Person Weapon, Third Person Weapons, Bullet Hits, and UI</i> you can use the <b>Audio Mixer window</b> to adjust the MFPS default Audio Mixer setup.\n \nFor this, go to the <b>MFPS Audio Mixer</b> located in <i>Assets ➔ MFPS ➔ Content ➔ Sounds ➔ Mixer ➔ MFPS Audio Mix</i> <?link=asset:Assets/MFPS/Content/Art/Sounds/Mixer/MFPS Audio Mix.mixer>(click here to ping)</link>, then double click it to open the Audio Mixer window, there you will see the Audio Mixer Groups where you can adjust the volumes and audio blend or even apply custom sound effects.\n \nFor more information in the Unity Audio Mixer, refer to this:\n<?link=https://docs.unity3d.com/Manual/AudioMixer.html>https://docs.unity3d.com/Manual/AudioMixer.html</link>");
        }
        else if (subStep == 3)
        {
            DrawText("In case you are looking for sounds to replace the default ones in the game or maybe you need new sounds for your weapon or players, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (soundsAssets == null)
            {
                soundsAssets = new AssetStoreAffiliate
                {
                    randomize = true
                };
                soundsAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555517340/widget-medium");
                soundsAssets.FixedHeight = 400;
            }
            else
            {
                soundsAssets.OnGUI();
            }
            DrawAffiliateList("https://assetstore.unity.com/linkmaker/embed/list/4673555517340/widget-medium");
        }
    }

    void AntiCheatDoc()
    {
        DrawSuperText("Hacking is one of the obstacles that soon or later you will face as a game developer, but especially in competitive multiplayer games, you will have to deal with those modders/hackers trying to exploit your game to get some advantage over regular players, because that, you have to take some measures since the beginning of your project to protect your game.\n \nBy default, MFPS doesn't include any Anti-Cheat system because these are big and complex systems that by they selves worth more than the MFPS core price, but since version 1.9.2, <b>MFPS comes with the basic integration of a third-party anti-cheat asset</b> that solves most of the common hacking methods used nowadays, this asset is the\n<?link=https://assetstore.unity.com/packages/tools/utilities/anti-cheat-toolkit-2021-202695?aid=1101lJFi>Anti-Cheat Toolkit (ACTk)</link>\nfrom the Asset Store, with it you can prevent the players to be able to cheat in your game e.g modifying the player health, ammo, drops, coins, etc...\n \nBy default, the MFPS core just includes the basic integration that allows prevents the players to modify these values, but there's an addon that extends the features <b>allowing automatically banning players that use Speed hacks, memory injection, code injection, or walk hacks</b>, it is the <?link=https://www.lovattostudio.com/en/shop/addons/anti-cheat-and-reporting/>MFPS Anti Cheat addon</link>.\n \nAlthough this will drive away most of the wanna-be hackers, this would not be enough for more experienced/hardened modders, actually, nothing is unhackable, you can only make it harder so they give up trying, with that in mind here you have other implementations and changes that you can do to make your game more secure in any platform:");
        Space(20);
        DrawSuperText("<b><size=14>1. Use IL2CPP</size></b>\n \nUnity supports 2 scripting backend at the moment: <b>Mono</b> and <b>IL2CPP</b> where this last one is newer and more secure, IL2CPP does produce raw binary code with metadata instead of Mono’s IL bytecode making all IL reversing tools useless and making it much, much harder to get good decompilation of your game code.\n \nThis is built-in Unity and you simply have to change the scripting backend option in the <b>Project Settings > Player > Scripting Backend</b>.\n \n\n<b><size=14>2. Code Obfuscation</size></b>\n \nCode obfuscation is the process of randomly generating names for your script parameters, functions, properties, etc... in the build process in order to make it pretty much unreadable for anyone that decompiles your binaries, the code will become a mess of useless names making reconstructed IL assembly very hard to reverse-engineer and analyze a complete nightmare.\n \nUnfortunately, Unity doesn't come with such a feature included, and third-party systems are required, the most popular and easy to use obfuscator for Unity is available on the Asset Store:\n<?link=https://assetstore.unity.com/packages/tools/utilities/obfuscator-48919?aid=1101lJFi>Obfuscator</link>");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>Enable Anti-Cheat</size></b>\n \nIn order to enable the anti-cheat integration, if you are using the addon for the advance integration > follow the addon documentation, if you are not using the addon but you have installed the Anti-Cheat Toolking assets and want to enable the basic integration included in the core package > go to the editor top navigation menu > Tools > Code Stage > Anti-Cheat Toolking > Settings... > Conditional Compilation Symbols > check the <b>ACTK_IS_HERE</b> check box and that's.");
    }

    void FPArmsMaterial()
    {
        DrawText("Normally you will use the same hand model for all of your weapons model, using a different material and textures for each team, so you may encounter with the inconvenient of change the hand texture for each weapon in the player prefabs is a little bit annoying, well MFPS handle this.\nYou don't have two manually change the arms, sleeves, gloves, etc.. materials, you only" +
            "have to create a prefab and list all the arms materials along with the different textures per team.");
        DownArrow();
        DrawText("Let's start by creating a new \"Arms Material\" asset, in the <b>Project Window</b> select a folder where save the asset and do <b>Right Mouse Click</b> -> MFPS -> Player -> <b>Arm Material</b>");
        DrawServerImage(4);
        DownArrow();
        DrawText("Then select the created material and in the Inspector window you'll see a List, in this list you have to add all the materials your Arms model <b> that change of texture depending of the player team</b>, for example the default MFPS arms model have 3 materials: Sleeve, Skin and Gloves, but only the Sleeves and Gloves material change of texture, the skin is the same, so only those two materials are include in the list.\nProbably you only have to add the Gloves Material, so add a new field on the list and assign the material and add the different Textures depending of the Team.\n \nWith that the materials will automatically change of textures in runtime depending on which team the player spawm.");
        DrawServerImage(5);
    }

    void RoomPropertiesDoc()
    {
        DrawHyperlinkText("There're some properties that are different per room/match besides the game mode, that you can tweak, e.g: the max players options, max rounds time limits, game goals, etc... these properties options can be different per game mode and you can modify them in the game mode info like this:\n\n► Go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Game Modes ➔ <i>*Open a game mode*</i> ➔ there you will see the list and\noptions to modify these properties.");

        DrawHorizontalColumn("Max Players", "the maximum number of players options that can join in the room for the game mode, for two-team game modes, the max number of the player for each team is half of the max players allowed.");
        DrawHorizontalColumn("Game Goals Options:", "the score, point, or kills goals options of this game mode.");
        DrawHorizontalColumn("Time Limits:", "the round time limit options for this game mode (in seconds).");
        DrawServerImage("img-45.png");
    }

    void DrawTeamsDoc()
    {
        if (subStep == 0)
        {
            DrawText("On MFPS there are various game modes that use Team systems like CTF (Capture the Flag) or TDM (Team Death Match), for default these teams are named as \"Delta\" and \"Recon\", you can modify these team names and representative color, and team icon, for it go to <b>Game Data</b> > scroll down to the \"Team\" section:");
            DrawServerImage("img-56.png");

            DrawText("A common question about the team system is whether it's possible to have more than two teams. Unfortunately, this isn't supported by default at the moment. Implementing it would require significant changes to the core scripts.");

            DrawText("To access specific team information through code, you can use:");
            DrawCodeText("string team1Name = MFPSTeam.Get(Team.Team1).Name;\nSprite team1Icon = MFPSTeam.Get(Team.Team1).Icon;");
        }
        if (subStep == 1)
        {

        }
    }

    void DrawCoins()
    {
        if (subStep == 0)
        {
            DrawHyperlinkText("MFPS Integrate a virtual coin/currency system with two different coins, one that can be earned by playing the game <i>(with XP)</i> and another that should only be acquired by purchasing it with real money <i>(using the <link=https://www.lovattostudio.com/en/shop/network/shop/>Shop addon</link> or your custom IAP) </i>.\n \nYou can customize many aspects of these coins without touching any code, like <b>the names of the coins, color, icon, and value</b>.\n \nTo modify these properties, go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Game Coins ➔ unfold the coin you want to modify, most of the properties names are self-describing, but here is the explanation of the ones that could confuse you:");
            DrawHorizontalSeparator();
            DrawHorizontalColumn("Acronym", "An abbreviation of the coin name.");
            DrawHorizontalColumn("Coin Value", "The value of the coin with respect to 1, <b>e.g</b>: when you set the price for an in-game item let say 100 for a weapon if the coin value is equal to 0.25 it means that the player will need 400 of that coin to purchase that weapon <i>(0.25 = 1/4 ➔ 400/4 = 100)</i>, or if the coin value is equal to 2, the player just will need 50 of that coin.\n \nThe idea is that you only need to set a price for the in-game items, and the price for each coin is calculated automatically in-game.");
            DrawHorizontalColumn("Initial Coins", "The amount of that coins that new player will receive the first time that play the game.");
            Space(10);
            DrawServerImage("img-38.png", TextAlignment.Center);
            DownArrow();
            DrawSuperText("<?title=20>How to use the coins?</title>\n \nBy default <i>(in the core packages)</i> MFPS does not use the coins for anything, is up to you for what you wanna use them, either to buy game items like weapons, special skins, operators, etc... or implement a lootbox system or anything you can imagine.\n \nThere are some addons if you want to implement a shop system and allow players to buy weapons and coins packs:\n \n<?link=https://www.lovattostudio.com/en/shop/network/shop/>Shop System Addon.</link>\n \nAnd there are addons to integrate a payment system to allow purchases with real money/currencies:\n \n<?link=https://www.lovattostudio.com/en/shop/addons/unity-iap-for-shop/>Unity IAP Addon.</link>\n<?link=https://www.lovattostudio.com/en/shop/network/paypal-for-shop/>Paypal Addon.</link>");
        }
        else if (subStep == 1)
        {
            DrawSuperText("If you want to do basic coin operations like <b>add</b> or <b>deduct</b> a specific amount of coins to the player wallet you can do so with a single line of code:\n \n<?title=20>ADD COINS</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Add(100);");
            DrawSuperText("where 0 = the index of the coin in GameData ➔ GameCoins list,\nby default 0 = XP Coin, 1 = Gold Coin.\n \n<?title=20>DEDUCT COINS</title>");
            DrawCodeText("bl_MFPS.Coins.GetCoinData(0).Deduct(100);");
            DrawText("If you are using ULogin Pro addon the coin operation will be executed in the server and saved in the database, if you are not using it, the coins will be stored locally using <b>PlayerPrefs which is not secured</b>, that is why is recommended to save the coins externally in a dedicated database as ULogin Pro does.");
        }
    }

    void ItemUnlockDoc()
    {
        DrawText("<size=18><b>Unlockable Items in MFPS</b></size>\n\nIn MFPS, certain items can be <b>locked by default</b> and unlocked in-game through various methods that you, the developer, define. These methods can include:\n \n• Level progression\n• Special events\n• Loot boxes\n• Battle pass\n• Direct purchase from the shop\n\n\nThe following item types support unlock functionality:\n \n• Weapons\n• Player Skins\n• Weapon Camos\n• Emblems & Calling Cards\n• Emotes\n• Sprays\n• (And potentially more via addons)");
        DrawHorizontalSeparator();
        DrawText("<size=18><b>How Unlocking Works</b></size>\n\nAll unlockable items share the same logic. Each item has an <b>\"Unlockability\"</b> property in its data definition (ScriptableObject or list entry), where you can configure:\n \n• <b>Unlock Method</b> (e.g., Level, Purchase, VIP Only)\n• <b>Required Level</b> (if applicable)\n• <b>Price</b> (if applicable)\n\nThe unlock methods are mostly self-explanatory, but here are two special ones:\n\n<size=16><b>🔒 Hidden</b></size>\n\nHides the item from all default selectors (e.g., shop, loadout menus) until it's unlocked. Ideal for seasonal or event-based items.\n\n<size=16><b>⭐ VIP Only</b></size>\n\nMakes the item available only to players with an active VIP subscription. Requires the <b>VIP System addon</b>.");
        DrawServerImage("img-66.png");
        DrawHorizontalSeparator();
        DrawText("<size=18><b>Where to Set Unlockability</b></size>\n\nThe unlockability data is defined in the same place as the item itself. Each item type is stored in a different global settings asset, but they all follow the same structure. Examples:\n\n• <b>Weapons</b>: `<b>GameData > All Weapons</b>`\n• <b>Player Skins</b> <i>(requires Player Selector addon)</i>: `<b>PlayerSelector > All Players</b>`\n• <b>Weapon Camos</b> <i>(requires Customizer addon)</i>: `<b>CustomizerData > Global Camos</b>`\n• <b>Emblems, Emotes, Sprays</b>, etc.: Follow similar structure in their respective systems.\n\nAll these assets are located in a `<b>Resources</b>` folder, but you can easily access them through the <b>MFPS Manager</b> window (`Ctrl+M`).");
        DrawServerImage("img-67.png");
    }

    void GameModesDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS comes with 3 different game modes: <b>Team Deathmatch, Capture Of Flag, and Free For All</b>, each of these modes has its respective logic in separate scripts which can be found at: <i>Assets ➔ MFPS ➔ Scripts ➔ GamePlay ➔ GameModes➔*</i>\n \nEach game mode has some common properties that you can customize in the inspector in GameData ➔ Game Modes ➔ *,\nif you foldout one of the modes, you will find all the customizable properties");
            DrawServerImage("img-36.png");
            DrawText("Here what they are for:");
            DrawPropertieInfo("Mode Name", "string", "The name represent this mode that will be displayed in-game.");
            DrawPropertieInfo("Game Mode", "enum", "The internal id of this mode, it's use in code to identify a specific game mode, you can only have a mode in the Game Modes list with the same id.");
            DrawPropertieInfo("Is Enable", "bool", "Will this mode be available in your game?");
            DrawPropertieInfo("Support Bots", "bool", "Can games be created for this mode and filled them with bots? <b>NOTE:</b> that by default, bots are only supported in TDM and FFA modes.");
            DrawPropertieInfo("Auto Team Selection", "bool", "Force auto team selection to keep teams balanced?");
            DrawPropertieInfo("Required Players To Start", "int", "The minimum number of players that has to be joined in the match in order to start the game for this mode.");
            DrawPropertieInfo("On Round Started Spawn", "enum", "When a player join in a match with this mode but a round is already started, what to do?");
            DrawPropertieInfo("On Player Die", "enum", "What happens after a player die in this mode?");
            DrawPropertieInfo("Goal Name", "string", "The name of the points/objective of this game mode, e.g kills, captures, score, etc...");
            DrawPropertieInfo("Allow Pickup Weapons", "bool", "Can players pickup weapons in this mode?");
            DrawPropertieInfo("Max Players", "int[]", "The available options of max players for this mode, these options are the ones that appear in the room creator menu in the lobby.");
            DrawPropertieInfo("Game Goal Options", "int[]", "The available options for this mode goals (kills, captures, score, etc...), these options are the ones that appear in the room creator menu in the lobby.");
            DrawPropertieInfo("Time Limits", "int[]", "The available match times options for this mode set in seconds, these options are the ones that appear in the room creator menu in the lobby.");
        }
        else if (subStep == 1)
        {
            DrawHyperlinkText("Besides the 3 modes included in MFPS, there are more of the popular game modes available for MFPS as add-ons that you can acquire, like <i><b>Demolition/Bomb Defuse, Domination/Cover Point, Gun Race/Gun Play, Elimination, and Kill Confirm</b></i>, all those are available in <link=https://www.lovattostudio.com/en/shop/>our Shop</link>.\n \nBut let's say you want to create a custom game mode, where to start?\nWell, that is a difficult question since each game mode has its unique requirements, different logic, gameplay, etc... because of that, each approach is different, so I can't tell you how to create your game mode per se, what I can do, is tell you how to integrate it to MFPS.\n \nThe first thing is to create an enum identifier for your game mode, which will be used to identify the game mode in the code, for that simply add a pseudonym of the game mode name in the script <b>GameMode.cs</b>, e.g:");
            DrawCodeText("public enum GameMode\n{\n    TDM,\n    FFA,\n    CTF,\n    SND,\n    CP,\n    GR,\n    BR,\n    ELIM,\n    DM,\n    KC,\n    MyCustomMode,\n}");
            DrawText("In the above example I added the <i>\"MyCustomMode\"</i> as example, but you can use anything you want, even just the initial as the others mode.");
            DownArrow();
            DrawSuperText("The next thing is to define the script which will handle the integration of MFPS with your game mode, for that MFPS uses the abstract class <color=#238C2BFF>bl_GameModeBase</color> to define the functions that your game mode main script must implement, so what you have to do is <?link=https://www.w3schools.com/cs/cs_abstract.php>inherit that base class</link> in your main game mode script ➔ override the required functions of the abstract class in your script, to make easier, you can use the default game modes scripts <i>(bl_TeamDeathMatch.cs, bl_FreeForAll.cs, etc...)</i> as a reference and learn from its implementation\n  \nOnce you define the <b>bl_GameModeBase</b> base class and its functions, there's a must-have code that you have to implement, in the Initialize() function you have to register this mode with its enum identifier, like this:");
            DrawCodeText("if(IsThisModeActive(GameMode.MyCustomMode))\n        {\n            // Active your game mode\n            // Active all the game objects, props, UI, etc... that are for this specific mode\n            // Register to game events needed for this mode\n        }\n        else\n        {\n            // Disable any object/UI that are for this specific mode\n        }");
            DownArrow();
            DrawText("Below you will find a complete script will the default skeleton of the above explained, you can use it as the base and start to modify it with your custom requirements for your mode:");
            DrawCodeText("using UnityEngine;\n \npublic class MyCustomModeScript : bl_GameModeBase\n{\n \n    #region Interface Overrides\n    public override void Initialize()\n    {\n        if (IsThisModeActive(GameMode.MyCustomMode))// replace the identifier with your game mode\n        {\n            // if this game mode is active in this room\n            // active all the objects that should be active for this game mode only\n            // like UI, map objects, etc\n        }\n        else\n        {\n            // if this game mode is not active in this room\n            // deactive all the objects that should be active for this game mode only\n        }\n    }\n \n    public override bool IsLocalPlayerWinner()\n    {\n        // Determine if the local player is the winner\n        // Based in your custom game mode logic\n        return true;\n    }\n \n    public override void OnLocalPlayerDeath()\n    {\n        // if something should happens when the local player die\n        // in your game mode, put it here\n    }\n \n    public override void OnLocalPlayerKill(KillInfo info)\n    {\n        // if something should happens when the local player kill\n        // in your game mode, put it here\n    }\n \n    public override void OnLocalPlayerSpawn()\n    {\n        // if something should happens when the local player spawn\n        // in your game mode, put it here\n    }\n \n    public override MatchResult GetMatchResult()\n    {\n        // Determine the match result based in your game mode logic FOR THE LOCAL PLAYER\n        return MatchResult.Victory;\n    }\n    #endregion\n}");
            Space(10);
            DrawText("For guidance and reference, consider examining the scripts of the default game modes. Further, inspect all the functions within the <b>bl_GameModeBase.cs</b> script to gain a comprehensive understanding of all the override and utility functions available for use in your game mode.");
        }
        else if (subStep == 2)
        {
            DrawText("If you want to define which game modes will be allowed and which don't in certain maps of your game, you can do it by setting the games that are NOT allowed in the map info No Allowed Game Modes list, which is located in each map scene info in <i>GameData ➔ All Scenes ➔ *Scene info* ➔ <b>No Allowed Game Modes.</b></i>\n \nThe modes that you add to that list will not be available for that map, and all the rest will be available.");
            DrawServerImage("img-37.png");
        }
    }

    void DrawLobbyChat()
    {
        DrawText("MFPS 2.0 include a <b>lobby chat system</b>, for players by able to communicate between meanwhile search or wait for join to a match, this chat use Photon Chat plugin, for use it you need to have a Photon Chat AppID <i>(it's not the same that Photon PUN AppID)</i>, you can get this appid from your photon dashboard:\n\nGet your AppId from the Chat Dashboard:");
        if (Buttons.FlowButton("Chat Dashboard"))
        {
            Application.OpenURL("https://www.photonengine.com/en-US/Chat");
        }
        GetTutorialHTML()?.SetButtonBlock("Chat Dashboard", "https://www.photonengine.com/en-US/Chat");

        DrawText("when you have your Chat AppID, paste it on the PhotonServerSettings:");
        DrawServerImage("img-6.jpg");
    }

    public AnimatorController playerAnimatorController;
    public int customWeaponStep = 0;
    public string[] weaponNames;
    public int customWeaponID, selectedSubMachine = 0;
    public ChildAnimatorStateMachine[] upperSubMachineStates;
    public string[] upperSubMachineStatesNames;
    public string customAnimationFireName;
    private AssetStoreAffiliate playerAnimationAssets;
    void DrawPlayerAnimationDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("MFPS 2.0 uses Mecanim system to handle the tp player animations, so change animations clips is simple as drag and drop the animation clip in the motion state in the Animator window, you only need a humanoid animation clip and override it in the Animator Override Controller.\n\n\n<?title=16>CHANGE PLAYER ANIMATION</title>\n\nIn order to change a specific animation clip:\n\n- Open the player prefab that you wanna change the animation for <i>(Player prefabs are located in the Resources folder).</i>\n\n- In the Animator component of your soldier model of the player prefab <b>double click</b> on the Controller field:");
            DrawServerImage("img-27.png");
            DrawText("- Now in the <b>Inspector</b> window, you will see the list of the animations clips used in the player animator controller, next to the default animation clips names you can see a field to assign and override/replace the default animation with your custom animation clips.\n \nSo, you simply have to assign your custom animations in the corresponding box, <b>based on the default animation names you can figure out which animation is for what player motion</b>.");
            DrawServerImage("img-28.png");
            DownArrow();
            DrawSuperText("Now, if you want to use differents animations for an specific player prefab, <b>e.g</b> <i>each player prefab have a different soldier mode with custom movement animations for each model</i>, the solution is pretty easy.\n\nYou simply have to create or duplicate the <?underline=>Player Animations [Override]</underline> controller and assign it in the Animator component of your soldier model in the player prefab:");
            DrawAnimatedImage(6);
        }
        else if (subStep == 1)
        {
            DrawSuperText("<?title=18>CHANGE ANIMATIONS <b>ADVANCE</b></title>\n\n- Change the animations from the <?underline=>Animator Override Control</underline> <i>(from the Basic section)</i> have a limitation, and it's that you can only replace the animation clips, the problem is that by default some animations are used in multiple states of the Animator StateMachine, e.g <i>the 'reload-ar' which is the weapon reload animation is used for the Rifle, Pistol and Sniper weapons</i>, if you want to use a different animation for each type of weapon you will have to modify the Base <?underline=>Animator Controller</underline>.\n\n1. Duplicate the default Player Animator Controller asset (<?link=asset:Assets/MFPS/Content/Art/Animations/Player/Controllers/Player [Controller].controller>Player [Controller]</link>) ➔ Select the asset in the Project window ➔ Ctrl + D or Command + D on MAC.\n\n2. Assign the duplicated <b>Animator Controller</b> in the Animator component of the soldier model of the player prefab that you want to modify the animation for.\n\n3. Open the duplicated Animator Controller in the Animator window <i><size=9><color=#76767694>(double click on the Animator Controller)</color></size></i> ➔ Find the animation state that you want to change the animation clip, on the Animator view you need figure out for what part of player body is this animations Bottom or Upper body <i>(Legs or Arms)</i>, e.g the <b>Rifle Reload</b> motion is for the Arms, so it is for the Upper Body, so go to <b>Layers</b> - and select the <?underline=>Upper</underline> layer, there you will see various state machine with the name of the weapon types to which it belongs ➔ open the weapon state machine:");
            DrawServerImage("img-29.png");
            DrawSuperText("Now you will see others animation states, those represent the weapon motion clips, in this example we are looking for the \"Reload\" state, so, select the Reload state ➔ in the inspector view you will see the settings of this state, what we are interested in is the <?underline=>Motion</underline> field, in that field you to need to assign the animation clip with which you want to replace the default one.\n\nOnce you do that, you are ready to go, you can replace all other animation clips if you want.");
            DrawServerImage(16);
        }
        else if (subStep == 2)
        {
            DrawText("As you may already know in MFPS the first person and third person animations are different, the same applies to the weapon animations, you can't use the use FPWeapon animations for the TPWeapons, instead, humanoid animations are needed for the TPWeapons.");
            DrawNote("TPWeapon animation set = the player animations needed for a weapon (Idle, Reload, Run, and Fire).");
            DrawText("By default, MFPS uses different weapon animations for each weapon type <i>(Machinegun, Pistol, Sniper, Grenade, etc...)</i> a set of animations for all the weapons of the same type which means you have multiple <b>Sniper</b> weapons, all of them will play the same set of sniper TP animations.\n \nCould be the case in where you want to <b>use a custom animation set for a specific TPWeapon</b>, this will require creating a new <b>SubStateMachine</b> in the player <b>Animator Controller</b> and creating the necessary transitions, below you will find an automated guide in how to set up the SubStateMachine in order to use custom animations for a TPWeapon.");
            DrawHorizontalSeparator();
            DrawTitleText("ADD CUSTOM WEAPON ANIMATIONS");
            DrawSuperText("The first thing we need is to know in which <b>Animator Controller</b> we want to add SubMachineState for the animation set, by default, the default Animator Controller which is used for all the MFPS player prefabs is located in: <i>Assets ➔ MFPS ➔ Content ➔ Art ➔ Animations ➔ Player ➔ Controllers ➔ Player [Controller]</i> <?link=asset:Assets/MFPS/Content/Art/Animations/Player/Controllers/Player [Controller].controller>(Click here to ping it)</link>\n \nIf you haven't changed the Animator Controller in any player prefab simply drag the default Animator Controller <i>(from the above path)</i> in the field below, otherwise drag the Animator Controller that you are using.");
            DrawHTMLPlaceholder();
            Space(10);
            var lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.BeginHorizontal("box");
            Space(20);
            playerAnimatorController = EditorGUILayout.ObjectField("Player Animator Controller", playerAnimatorController, typeof(AnimatorController), true) as AnimatorController;
            if (customWeaponStep == 0)
            {
                GUI.enabled = playerAnimatorController != null;
                if (Buttons.FlowButton("Continue"))
                {
                    customWeaponStep++;
                }
                GUI.enabled = true;
            }
            Space(20);
            EditorGUILayout.EndHorizontal();

            if (customWeaponStep > 0 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Now, in the dropdown below select the <b>weapon information</b> for which you want to add the custom animations and then click on the <b>Select</b> button.");
                weaponNames ??= bl_GameData.Instance.AllWeaponStringList(false, true);
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                customWeaponID = EditorGUILayout.Popup("Weapon", customWeaponID, weaponNames);
                if (Buttons.FlowButton("Select"))
                {
                    customWeaponStep++;
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
            }

            if (customWeaponStep > 1 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Alright, now you have to do this manually:\nFirst, open the <b>Animator Controller</b> in the <b>Animator window</b>, for it select the Animator Controller <i>(the one you assigned in the field above)</i> in the <b>Project View window</b> ➔ double click over it or right mouse click > <b>Open</b>.\n \nIn the Animator window that should be opened, select the <b>Layers</b> tab in the top left corner ➔ Select the second layer <b>Upper</b> ➔");
                DrawServerImage("img-43.png");
                Space(10);
                DrawSuperText("<?background=#CCCCCCFF>Duplicate one of the SubStateMachines</background>\n \nDuplicate a SubMachineState of a weapon of the same type that you want to add the animations, <b>e.g</b> if your custom animations are for a shotgun > Duplicate the Shotgun SubMachineState.\n \nFor duplicate the SubMachineState simply select it > right mouse click > Copy > right mouse click in an empty space > Paste:");
                DrawAnimatedImage(7);
                DownArrow();
                DrawText("Once you duplicate it, click on the button below to fetch the Animator Controller data and proceed with the set up");
                if (GUILayout.Button("Fetch Animator Controller Data"))
                {
                    var rootStateMachine = playerAnimatorController.layers[1].stateMachine;
                    upperSubMachineStates = rootStateMachine.stateMachines;
                    upperSubMachineStatesNames = upperSubMachineStates.Select(x => x.stateMachine.name).ToArray();
                    customWeaponStep++;
                }
            }

            if (customWeaponStep > 2 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Alright, now in the dropdown below, select the <b>SubMachineState</b> that you just duplicate and click on the <b>Setup</b> Button.");
                GUI.enabled = customWeaponStep == 3;
                EditorGUILayout.BeginHorizontal("box");
                Space(20);
                selectedSubMachine = EditorGUILayout.Popup("SubMachineState", selectedSubMachine, upperSubMachineStatesNames);
                if (Buttons.FlowButton("SetUp"))
                {
                    if (SetupWeaponSubMachine(upperSubMachineStates[selectedSubMachine].stateMachine, playerAnimatorController))
                    {
                        customWeaponStep++;
                    }
                }
                Space(20);
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            if (customWeaponStep > 3 && playerAnimatorController != null)
            {
                Space(10);
                DrawText("Good, now the SubStateMachine is ready to use, the next step is to assign the player weapon animations ➔ in the Animator window select the <b>SubStateMachine</b> with the name of the weapon that the animations are for <i><b>(The name of the SubStateMachine that you just duplicated was automatically renamed to the weapon name that you selected)</b></i> ➔ Double click over SubStateMachine ➔ You should see some AnimatorStates <b>(Run, Idle, Reload, and Fire)</b> ➔ you can replace the Animation Clip of each state by selecting the State ➔ redirect to the Inspector window ➔ Assign the Animation Clip in the <b>Motion</b> field.");
                DrawAnimatedImage(8);
                DownArrow();

                DrawText("Once you finish assigning the animations, you are pretty much done, the last thing to do is to assign the custom animation information in the <b>TPWeapon > bl_NetworkGun</b> inspector, for it, open a player prefab <i>(you will have to do this for all the player prefabs that you are using)</i> ➔ Select the TPWeapon inside the player prefab ➔ in the inspector check the <b>Use Custom Player Animations</b> toggle ➔ Infill the next fields with this info:");
                Space(10);
                if (string.IsNullOrEmpty(customAnimationFireName))
                {
                    var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
                    customAnimationFireName = weaponInfo.Name;
                }
                DrawHorizontalColumn("Custom Animator State ID", (20 + customWeaponID).ToString(), 175);
                DrawHorizontalColumn("Custom Fire Animation Name", customAnimationFireName + "Fire", 175);
                DrawServerImage("img-44.png");
                DrawText("That's");
            }

            EditorGUIUtility.labelWidth = lw;
        }
        else if (subStep == 3)
        {
            DrawText("In case you are looking for player animations to replace the default ones in the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (playerAnimationAssets == null)
            {
                playerAnimationAssets = new AssetStoreAffiliate
                {
                    randomize = true
                };
                playerAnimationAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555464133/widget-medium");
                playerAnimationAssets.FixedHeight = 400;
            }
            else
            {
                playerAnimationAssets.OnGUI();
            }
            DrawAffiliateList("https://assetstore.unity.com/linkmaker/embed/list/4673555464133/widget-medium");
        }
    }

    private bool SetupWeaponSubMachine(AnimatorStateMachine stateMachine, AnimatorController animatorController)
    {
        if (stateMachine == null)
        {
            Debug.LogWarning($"The StateMachine couldn't be found in this Animator Controller.");
            return false;
        }

        var weaponInfo = bl_GameData.Instance.GetWeapon(customWeaponID);
        int upperID = 20 + customWeaponID;
        var rootStateMachine = animatorController.layers[1].stateMachine;
        var equipState = rootStateMachine.states.ToList().Find(x => x.state.name == "Equip").state;

        if (equipState == null)
        {
            Debug.LogWarning($"The Equip animation state couldn't be found in the animator controller.");
            return false;
        }

        stateMachine.name = weaponInfo.Name;

        var subMachineStates = stateMachine.states.ToList();
        var idleState = subMachineStates.Find(x => x.state.name == "Idle");

        if (idleState.state == null)
        {
            Debug.LogWarning($"The Idle animation state couldn't be found in the animator controller.");
            return false;
        }

        var transition = equipState.AddTransition(idleState.state);
        transition.AddCondition(AnimatorConditionMode.Equals, upperID, "GunType");

        foreach (var state in subMachineStates)
        {
            if (state.state.name.Contains("Fire"))
            {
                state.state.name = $"{weaponInfo.Name}Fire";
            }
            var ts = state.state.transitions;
            foreach (var transit in ts)
            {
                var conditions = transit.conditions;
                var newConditions = new AnimatorCondition[conditions.Length];
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i].parameter == "GunType" && conditions[i].mode == AnimatorConditionMode.NotEqual)
                    {
                        var outCondition = new AnimatorCondition
                        {
                            parameter = "GunType",
                            mode = AnimatorConditionMode.NotEqual,
                            threshold = upperID
                        };
                        newConditions[i] = outCondition;

                    }
                    else
                    {
                        newConditions[i] = conditions[i];
                    }
                }
                transit.conditions = newConditions;
            }
        }

        EditorUtility.SetDirty(animatorController);
        return true;
    }

    void NamePlatesDoc()
    {
        DrawSuperText("Name Plates = <b>Above Head Player Name</b>, is the GUI Text that appears on teammates players in-game,\nit's a pretty basic feature in all multiplayer games, in MFPS this GUI is rendering using the Unity legacy OnGUI system for its simplicity for the required purpose.\n \nCustomize the design of this GUI is quite simply:\n \nIn each player prefab you will found the script <?underline=>bl_NamePlateDrawer.cs</underline> attached to it, now in order to make easier to modify the look of the name plate UI you can preview it in the editor <i>(in edit mode)</i> by instance the player prefab in a scene ➔ select the player prefab instance ➔ bl_NamePlaterDrawer ➔ Click on the button <b>Simulate [OFF]</b>.");
        DrawServerImage("img-30.png");
        DrawText("Now with the preview On, you can edit the design with the frontend properties, for it on the inspector window of bl_NamePlateDrawer ➔ click on the button Edit Present ➔ this will open the GUI style properties of the name plate.");
        DrawServerImage("img-31.png");
        DrawText("Customize the properties as you desire and preview the change in realtime in the Scene View window ➔ Once you're done, turn off the Simulation by clicking again in the <b>Simulation [On]</b> button ➔ Apply the changes to the player prefab.");
        DownArrow();
        DrawHyperlinkText("<b><size=16>Hide Health Bar</size></b>\n\nIf you want to hide the health bar from the name plate of teammates and only show the player name, you can do it by turn off the toggle <b>Show Teammates Health Bar</b> in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link>.");
    }

    void PlayerPrefabsDoc()
    {
        DrawHyperlinkText("We called <b>Player Prefabs</b> to the unity prefab that contain all the required scripts, objects, and structure that make up the player controller.\n\nThe MFPS player prefabs are located in a special Unity folder called <b>'Resources'</b> <i>(you can find it inside the MFPS folder)</i>, if you wanna change anything related to one of the players like, modify a weapon position, animation, a script property, etc... you have to apply the change to these prefabs.\n\n<b>By default, MFPS uses 2 player prefabs</b> which are assigned in the <b><link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link></b> ➔ <b>Player1</b> and <b>Player2</b>, Player1 is used for Team 1 and Player2 for Team 2, in case the game mode is not a team-based mode like the Free For All game mode, the Player1 is used.\n\nSince version 1.8 you can also override the Player1 and Player2 per scene, which means that you can use different player prefabs for each team in each map, to do that you simply have to attach the script <b>bl_OverridePlayerPrefab.cs</b> in any object of your map scene ➔ then in the inspector of this script you will see the fields to assign the player prefabs and that's all.");
        DownArrow();
        DrawHyperlinkText("In case you are looking for a more advanced solution for the player selection or want to add more player prefabs where the players can select in-game their character, you should take a look at the <link=https://www.lovattostudio.com/en/shop/addons/player-selector/>Player Selector</link> addon.\n");
    }

    void DrawBullets()
    {
        if (subStep == 0)
        {
            DrawText("in MFPS bullets are pooled and like all other pooled objects in MFPS they are listed in <b>bl_ObjectPooling</b> script, which is attached in the <b>GameManager</b> on each map scene.\nin <b>bl_Gun</b> you assign only the \"Pooled Name\".\n\n\nYou may want to add a new bullet for a specific weapon, let's say you want add a different Trail Render, well for do it you can do this:\n\n<size=18>Duplicated a bullet prefab:</size>\n\nSelect one of the existing prefab located at MFPS ➔ Content ➔ Prefabs ➔ Weapon ➔ Projectiles ➔ *, select the prefab and Ctrl + D to Duplicated, or Command + D on Mac.\n\nThen make the changes that you want to this duplicated prefab and after this, add a new field in <b>bl_ObjectPooling</b> <i>(it is attached in GameManager object in room scenes)</i>, in the new field drag the bullet prefab and change the pooled name:");
            DrawServerImage(17);
            DrawText("then open a player prefab and select the <b>FPWeapon</b> that you want assign the bullet, in the bl_Gun script of that weapon, write the pooled name of the bullet in the field \"Bullet\".\n");
            DrawServerImage(18);
            DrawText("Apply changes to the player prefab and ready.");
        }
        else if (subStep == 1)
        {
            DrawSuperText("The bullet decals are pooled and automatically placed when a bullet hits a collider, the decal material will be selected based on the collider tag, you can define as many decals as you want.\n \nThe <b>Bullet Decal Manager</b> is located under the <b>GameManager</b> object in each map scene, <i><b>GameManager ➔ Bullet Decal Manager ➔ bl_BulletDecalManager</b></i>.\n \n<?title=18>In order to modify, add or remove a decal:</title>\n \n<?list=■>Create a new material with a simple alpha shader\nAssign the decal texture to this material.\nIn the<b>Bullet Decal Manager ➔ Decal List ➔ Surface Decals</b>, add a new tag field if the decal is for a new Tag, or if it is for an existing one, simply unfold the field.\nAdd the Decal material to the <b>Decal Materials</b> list.\nThat's.</list>");
            DrawServerImage("img-39.png");
        }
        else if (subStep == 2)
        {
            DrawText("If you need to create a different type of bullet, not just a trail renderer change but a different ballistic or a different hit detection, this is possible to do without breaking changes.\n \nThe bullet or projectiles can be extended and/or a custom class can be used, <b>you should not modify the default bullets/projectiles scripts</b>,\ninstead, create a new class and inherit it from the <color=#FFCC2AFF>bl_ProjectileBase</color> class, override the required functions and use the default scripts as reference only and code the logic of your bullet/projectile as needed.\n \nAttach your custom script to a game object and create a prefab of it, then add this prefab in the pooled list and you are ready to go.");
        }
    }

    void DrawKitsSystem()
    {
        if (subStep == 0)
        {
            DrawText("MFPS incorporates an in-match item drop system, inspired by the <i>Call of Duty</i> games, that allows players to request items during gameplay.\n \nThe operation of this feature is straightforward. Players can call for an item drop by pressing the drop button <i>(default key: H)</i>, which throws an indicator onto the map. This indicator marks the location where the item will be dropped. After a brief delay, the requested item arrives at the designated spot, ready for the player to collect.\n \nMFPS, by default, offers two types of items for drop requests: <b>Ammunition</b> and <b>Health</b> kits. The specific item that gets requested is determined by the player's class. However, if you're using the Class Customization add-on, the item drop will depend on the item you've equipped in your loadout.");
        }
        else if (subStep == 1)
        {
            DrawTitleText("Change the model of the kits");
            DrawServerImage(19);
            DrawText("•  You can find the kits prefabs in: <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Items->*</i>\n\n•  Select the kit that you want to change the model (Health or Ammunition) and drag to the scene hierarchy.\n\n•  Replace the mesh with you new model, apply the changes and save the prefab.");
            DownArrow();
            DrawTitleText("Change the model of Kit deploy indicator");
            DrawServerImage(20);
            DrawText("You can find the prefab in <i>Assets -> MFPS -> Content -> Prefabs -> Level -> AirDrop->*</i>");
        }
        else if (subStep == 2)
        {
            DrawText("To add a new drop item prefab, follow the steps below:\n \n<b>1. Create a Prefab:</b> Begin by creating a prefab of the item you want to be dropped. This prefab should include the item mesh, a Trigger collider, and the script that manages its pickup. You can use the default items (Health and Ammunition kit) prefabs as a reference.\n \n<b>2. List the Prefab in the Item Drop Container:</b> Once your prefab is ready, it needs to be added to the Item Drop Container list. By default, the container is located in *. To do this, select this object and proceed to the inspector of bl_ItemDropContainer.\n \n<b>3. Add a New Field in the Items List:</b> In the inspector of bl_ItemDropContainer, add a new field in the Items list.\n \n<b>4. Assign the Item Prefab:</b> Assign the item prefab in the Prefab field and fill in all the other required properties.");
            DrawServerImage("img-54.png");
            DrawText("<b>5. Assign this kit to one of the player classes:</b> Now you have to define which player class or classes will equipped this item, for it, select the a player prefab > go to the <b>bl_GunManager</b> of the player prefab > foldout the player class you want > and in the <b>DropKit</b> field > assign the <b>Index</b> of your new item prefab from the Item Container list.");
            DrawServerImage("img-55.png");
            DrawText("That's, now you will be able to request your new item drop when use the player classes where you assigned it.");
        }
    }

    void MapCameraDoc()
    {
        if (subStep == 0)
        {
            DrawText("<b><size=16>Map Camera</size></b>\n\n<b><size=12>Overview</size></b>\n\nThe Map Camera, often termed as the \"<b>Room Camera</b>\" is a specialized camera used within game to render the map scenes. It is particularly active when the player first joins the game and when the local player character is not instantiated.\n \n<b><size=12>Location and Access</size></b>\n\nBy default, the Map Camera is positioned under the \"<b>GameManager</b>\" object in the scene's hierarchy. It is specifically named \"Room Camera\" in this hierarchy.\n\nWhen the \"Room Camera\" object is selected in the hierarchy, you will be able to view and modify various properties of the camera in the inspector. The camera's functionalities and settings are accessible through the attached script, labeled as <b>bl_RoomCamera</b>.\n\n<b><size=12>Programmatic Access</size></b>\n\nTo interact with the Map Camera through code, you can reference its functions as follows:");
            DrawCodeText("bl_RoomCameraBase.Instance.FUNCTION_NAME();\nor\nbl_RoomCamera.Instance.FUNCTION_NAME();");
            DrawServerImage("img-57.png");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=16>Initial Camera Point of View (POV) in Map Scenes</size></b>\n \nThe \"<i>Initial Camera Point of View</i>\" refers to the default position and orientation of the camera at the beginning of a map scene. This default POV is set when the player first enters a room. In the editor mode, the camera's initial position is determined by the placement and rotation you specify for the map camera. To adjust this, position and rotate the map camera as desired in the editor mode.\n \nBy default, the initial camera POV includes an automatic rotation feature. This is designed to add dynamic movement to the scene, particularly useful in scenarios where there is minimal action, preventing the scene from appearing overly static. If needed, you can modify this feature through the bl_RoomCamera script's inspector. Options include changing the direction of the rotation or disabling the movement entirely.");
            DrawServerImage("img-58.png");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=16>Match Camera Positions</size></b>\n\nIn addition to the initial camera position, you can configure the Map Camera to display different points of view (POVs) for various match events. These events include the conclusion of a match, entering spectator mode, and other significant in-game occurrences.\n \nIn the MFPS example scene, different camera POVs for these match events positions are pre-configured by default.\n\nNew Map Scenes Integration: When integrating new map scenes, the camera position for all events will default to the initial POV. This may be suitable if uniformity across all events is your desired outcome, but if you want to set up the camera position for these different match events, this is how you do it:\n\n<b><size=14>Setting Up Different POVs</size></b>\n\n<b>Create an Empty GameObject:</b>\nIn your map scene, create a new empty GameObject.\n\n<b>Attach the Script:</b>\nAdd the bl_RoomCameraPositions script to this GameObject.\n\n<b>Configure Positions in Inspector:</b>\nAccess the inspector of the <b>bl_RoomCameraPositions</b> script.\nAdd new entries to the positions list. Each entry represents a different POV for a specific match event.\nCustomize these positions by specifying their fields, such as position coordinates, rotation, and other relevant parameters as follow:");

            DrawHorizontalColumn("Key", "The key name for this camera position data, for the default MFPS events you have to use these keys: \"spectator-start\", \"match-start\", and \"match-finish\" (it's name indicate the event it's for), for custom events you can use your custom key name.");
            DrawHorizontalColumn("Position", "The position of the camera in this POV, a Transform instance in your map scene that work as reference of the position and rotation where the map camera will be position when the event is triggered, you can preview it with a frustum gizmo when the bl_RoomCameraPositions object is selected.");
            DrawHorizontalColumn("Moving", "Does the map camera move when apply this position?");
            DrawHorizontalColumn("Move Direction", "If Moving is On, the direction where the camera will continuously move to, you can preview the direction with the green line gizmo that start from the frustum position.");
            DrawServerImage("img-59.png");
            Space(10);
            DrawText("To Manually trigger/set the map camera to one of the defined positions, you can use:");
            DrawCodeText("bl_RoomCameraPositions.Instance.SetRoomCameraToPosition(\"YOUR_POSITION_KEY\");");
        }
        else if (subStep == 3)
        {
            DrawText("<b><size=16>MFPS Spectator Mode</size></b>\n\nMFPS includes a Spectator Mode feature, allowing players to join a match solely as observers. This mode is designed for players who wish to view the game without actively participating in the match.\n \n<b><size=12>Accessing Spectator Mode</size></b>\n\nPlayers can access Spectator Mode from the Waiting Room menu.\nTo enter Spectator Mode, players should click on the \"<i>Enter As Spectator</i>\" button located in this menu.\n\n<b><size=12>Spectator Mode Features</size></b>\n\nThe Spectator Mode currently offers only one type of Point of View (POV) - the external flying camera. This camera provides an aerial view of the match, allowing spectators to freely navigate around the game scene.\nAs of now, Spectator Mode does not include a first-person perspective option. The view is strictly limited to the external flying camera.\n\n<b><size=12>Customizing Controls</size></b>\n\nThe controls for the spectator flying camera can be customized to suit user preferences.\nThese adjustments are made in the inspector of the bl_RoomCamera script.\nWithin the inspector, you can modify various settings related to camera movement, speed, and responsiveness, tailoring the flying camera's control scheme to your needs.");
        }
    }

    void DrawKillZones()
    {
        DrawText("<size=20><b>Kill Zones</b></size>\n\nYou can restrict player movement and define map boundaries using <b>Kill Zones</b>. When a player enters one of these areas, a warning message and a countdown timer appear. If the player fails to exit the zone before the timer hits zero, they are automatically eliminated and sent back to a spawn point.");
        DrawServerImage(21);
        DrawText("<size=18><b>Implementation</b></size>\n\nTo set up a Kill Zone, follow these steps:\n\n1. <b>Create the Volume</b>: Add a new GameObject to your scene and attach a <b>Box Collider</b>. Adjust the size of the collider to define the boundaries of the restricted area. Ensure <b>Is Trigger</b> is enabled.\n\n2. <b>Attach the Script</b>: Add the `<b>bl_DeathZone.cs</b>` component to the object.\n\n3. <b>Configure Settings</b>:\n\n4. <b>Message</b>: Enter the text you want displayed on the player's screen while they are inside the zone.");
    }

    void DrawGameSettings()
    {
        DrawText("MFPS allows players to modify some game settings of the game in-runtime like graphics quality and control settings, you as the developer are in charge to set the default values for these settings, the value that the player will have the first time that they play the game.");

        DrawHyperlinkText("To set the default values go to <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Setting Values ➔ *");
        DrawServerImage(28);
        DrawText("In this list, you will have all the available values, simply unfold the setting that you want to modify and set the desired value.");
        DownArrow();
        DrawTitleText("Add a new setting");
        DrawText("Add a new setting is really simple, in the same list <i><b>(Setting Values)</b></i> add a new field, set a unique name to the to identify the setting ➔ set the type of setting <i>(float, integer, bool, or string)</i> then set the default value.\n\nNow to use this value in-game you can load it with:");
        DrawCodeText("var val = bl_MFPS.Settings.GetSettingOf('THE_SETTING_NAME');");
        DrawText("once the setting is added in the list, it will automatically be saved when the player applies the settings in-game <i>(click on the <b>Save</b> button)</i>, but if you want to save the setting with your own rules you can do it with:");
        DrawCodeText("bl_MFPS.Settings.SetSettingOf('THE_SETTING_NAME', THE_SETTING_VALUE);");
        DrawText("As a reference of how you can use it in-game you can inspect the script <b><color=#00E9FFFF>bl_SingleSettingsBinding.cs</color></b>\n");
    }

    void MouseLookDoc()
    {
        DrawHyperlinkText("The Mouse Look/Camera controller is a key feature in shooter games or any fast-paced action game.\n \nIn MFPS a few techniques are used to improve the accuracy and smoothness of the look movement like <b>Frame Smoothing</b> and <b>Movement Smoothness</b>.\n \nIf you want to personalize the movement, there're some properties that you can modify in the inspector:\n \nFor the general setup, you can find the properties in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> -> Mouse Look Settings.");
        DrawPropertieInfo("Use Smoothing", "bool", "Use Frame Smoothing?, a technique that allows achieving a more fluid movement by using the average mouse movement calculated from a certain number of past frames.");
        DrawPropertieInfo("Frame Of Smoothing", "int", "The number of frames that will be buffer to calculate the average movement, more means more smooth but less precise.");
        DrawPropertieInfo("Lerp Movement", "bool", "Apply an extra smooth layer to the movement? good for touch devices, not recommended for mouse/gamepad controllers.");
        DrawPropertieInfo("Aim Sensitivity Adjust", "enum", "Determine how the mouse sensitivity will transition to the Aim sensitivity, <b>Fixed</b> = To the exact Aim sensitivity value, <b>Relative</b> = by calculating the Camera Field Of View change.");
        DrawHorizontalSeparator();
        DrawText("<b><size=22>PER PLAYER PROPERTIES</size></b>\n\nThere're some properties that you can modify per player, you can find them in the Player prefabs ➔ bl_FirstPersonController ➔ Mouse Look ➔ *,");
        DrawHorizontalSeparator();
        DrawHyperlinkText("<b><size=22>SENSITIVITY</size></b>\n\nPlayers can change the mouse/pad sensitivity in-game, but you can set up the default sensitivity in <link=asset:Assets/MFPS/Resources/GameData.asset>GameData</link> ➔ Default Settings ➔ Settings Values ➔ <b>Sensitivity</b> and <b>Aim Sensitivity</b>.");
    }

    void DrawObjectPooling()
    {
        DrawSuperText("<b><size=15>What is Object Pooling?</size></b>\n\n<b>Instantiate()</b> and <b>Destroy()</b> are useful and necessary methods during gameplay. Each generally requires minimal CPU time.\n\nHowever, for objects created during gameplay that have a short lifespan and get destroyed in vast numbers per second like Bullets per example, the CPU needs to allocate considerably more time.\n\nThere is when Object Pooling is enter, <b>Object pooling</b> is where you pre-instantiate all the objects you’ll need at any specific moment before gameplay, in MFPS bullets, decals and hit particles are pooled.\n\nThe bl_ObjectPooling.cs class is really easy to use, all what you have to do to add a new object to pooled is to listed the prefab in the <i>Prefab Pool Container</i>, by default the container is located in <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Presets ➔ Containers ➔ Default Prefab Pool</i>  <?link=asset:Assets/MFPS/Content/Prefabs/Presets/Containers/Default Prefab Pool.asset>(Click here to ping)</link>, once selected, in the inspector window you will have the list  <b>Prefabs</b>, add new item to the list > assign the prefab, key name and how many instances of this prefab pre-instantiate and that's.\n\nNow for instance this prefab from a script, before you normally will use something like:");
        DrawCodeText("GameObject ob = Instantiate(MyPrefab, position, rotation);");
        DrawText("with bl_ObjectPooling script you simply has to replace that with:");
        DrawCodeText("GameObject ob = bl_ObjectPooling.Instance.Instantiate(\"PrefabKey\", position, rotation);");
    }

    void AddNewMenu()
    {
        DrawText("If you want add a new menu/window in the Lobby UI, follow this:\n\n1 - Create the menu/window UI: make the design of UI with what you need, but make all the design as child of a parent under canvas, example \"<i>MyNewWindow</i>\" (this is a empty game object under canvas) put all the buttons, text, images, etc.. of your new menu/window under this object.\n\n");
        DrawAnimatedImage(0);
        DrawText("2 - Add a new field in the <b>Windows</b> list in Lobby -> Canvas [Default Menu] -> bl_LobbyUI -> Windows, in the new field in this list and add the \"<i>MyNewWindow</i>\" object in the field and assign an unique name.\n");
        DrawAnimatedImage(1);
        DrawText("3 - Create a menu button: add a new button that will open the new menu/window, all the other buttons are in: Lobby -> Canvas -> Lobby -> Content -> Top Menu -> Buttons -> *, so you can duplicate one of these buttons and change the title text.\n\nin this new button add as listener the function of bl_Lobby -> ChangeWindow(string) -> and set the name of new window in the list \"Windows\".That's.\n");
        DrawAnimatedImage(2);
    }

    void DrawAddonsDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS 2.0 is designed with a robust set of core functionalities, yet you might notice it doesn't include certain standard features commonly found in FPS games, such as Mini Map, Login System, Level System, Vehicles, Shop, etc... This isn't an oversight; MFPS indeed offers many of those features, but as extensions/add-ons, allowing you to select the system to your specific needs.\n \nYou might wonder why these aren't part of the default setup. Let's rationale:\n \nThe decision to separate these features as optional add-ons is twofold, benefiting both us (the MFPS devs) and you the end game developer. Primarily, it's about affordability and flexibility. Including every possible feature in the core package would unavoidably inflate its price, potentially pushing it beyond an accessible range for many — we're talking about a jump to at least $500 or more. By offering a lean core package, we keep the entry price low, ensuring accessibility for a wider range of developers, from hobbyists to professionals.\n \nSecondly, this modular approach respects the unique needs and constraints of different projects. Not every game requires a complex Account Systems, Vehicles, or Kill Streaks support. By allowing developers to select and integrate only the features they need, we ensure that you don't pay for, or wade through, unnecessary functionalities. This not only makes MFPS more customizable but also more efficient and lighter to work with.\n\n<b>It's important to note that the core package is entirely functional on its own, and the add-ons are optional.</b>\n \nDespite being optional, these add-ons are meticulously crafted to blend seamlessly with the core system. Most feature automatic integration, meaning you're spared the technical hassle of manual coding or configuration. Simply choose what you need, and the system integrates it smoothly into your project.\n \nTo make your development journey smoother and to explore the range of functionalities you can incorporate, check out the '<b>Addon Manager</b>' window. You can access it in MFPS by navigating to <b>MFPS ➔ Addons ➔ Addons Manager.</b> Here, you'll find a suite of add-ons designed to elevate your game vision, all while maintaining the balance between complexity, performance, and budget.");

            DrawNote("Due to Asset Store guidelines, we are not allowed to list the links to the addons directly in the editor, but you can find all the available addons in our website <b>lovattostudio.com</b>");
        }
        else if (subStep == 1)
        {
            DrawText("\n<b>Addons Manager</b>\n\nThe Addons Manager is a custom Unity editor window designed to provide a unified dashboard for managing all MFPS addons. With it, you can view a full list of available addons, see which addons are already in your project, check the latest versions, find available updates for your installed addons, and access detailed information about each one. Think of it like Unity’s Package Manager, but specifically for MFPS Addons.\n\nDue to Unity Asset Store guidelines, this window isn’t enabled by default after importing MFPS. You will need to manually activate it.\n\nTo enable the Addons Manager, simply delete the \"<i>_ASCOMPLIANCE_</i>\" prefab from your project. You can find this prefab in: `<b>Assets/MFPS/Scripts/Internal/Editor/MFPS/Resources/*</b>`.\n\nOnce enabled, you can access the Addons Manager window from the top navigation menu in Unity: <b>MFPS > Addons > Addons Manager</b>.");
            DrawServerImage("img-64.png");

            DownArrow();
            DrawServerImage("img-65.png");

            GUI.enabled = !MFPSEditor.AssetData.AssetStoreCompliant();
            if (GUILayout.Button("Open Addons Manager"))
            {
                GetWindow<MFPSEditor.Addons.MFPSAddonsWindow>("Addons Manager");
            }
            GUI.enabled = true;
            DrawText("\nIn the Addons Manager window, the full list of available addons is displayed in the left panel. Clicking on an addon will show detailed information about it. If an addon is already imported into your project and an update is available, an \"update\" icon will appear next to the addon name in the left panel.\n\nIf you purchased addons from lovattostudio.com, you can download them directly from this window by clicking the \"<b>Downloads</b>\" button in the top right corner. This will open a form where you can log in with the email and password of your lovattostudio.com account. After logging in, all the addons you own will be listed, along with a button to download them.");
        }
        else if (subStep == 2)
        {
            DrawText("<b><size=22>How Integrate Addons?</size></b>\n\nAll addons comes with a <b>ReadMe.txt</b> on the root folder of the addon with the instructions, but pretty much <b>all Addons comes with an Automatically integration</b>, you only have to enable and click on the Integrate MenuItem:\n");
            DrawAnimatedImage(3);
        }
    }

    void EditorMenusDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS comes with a series of uniqueness <b>Editor windows</b> that you can found in the Unity Editor top menu under the <b>MFPS</b> root menu:");
            DrawServerImage("img-26.png");
            DrawText("Here is a brief explaination of what each one of them is for:\n \n<b><size=18>MFPS ➔ Addons ➔ *</size></b>\n \n- In the submenu of this, you will find all the <i>quick actions</i> for all the addons in your project, listed by the addon name you will find the button for quick actions like enable or disable the addon, run the auto-integration and the documentation link.\n \n<b><size=18>MFPS ➔ Addons ➔ Addons Manager</size></b>\n \n- This will open the Addons Manager window, with which you can see the register of all the MFPS 2.0 available add-ons,\nwith information as the latest update of each addon, the changelogs, last versions, addons description, enable/disable addons, addons links, and in-project addons status.\n \n<b><size=18>MFPS ➔ Tutorials</size></b>\n \n- In the submenu of this, you will find listed all the build-in editor documentation and tutorials for MFPS and all the addons in your project, being the <b>Tutorials ➔ Documentation</b> the main or general documentation of MFPS.\n \n<b><size=18>MFPS ➔ Tools</size></b>\n \n- In the submenu of this, you will find various actions and helpers windows that provide useful automated operations for MFPS and or specific addons.");
            DrawText("<b><size=18>MFPS ➔ Manager</size></b>\n \n- This will open the MFPS Manager window with which you can find the front-end settings of MFPS and all the addons in your project, also, in this window you have a range of utility windows with you can manage some of the MFPS main settings as the default player weapons load-outs, weapons information, levels, etc...\n \n<b><size=18>MFPS ➔ Store</size></b>\n \n- This will open the MFPS Store window, in this window you can find listed all the available MFPS addons with their respective informations, preview images and links to acquire them.\n \n<b><size=18>MFPS ➔ MFPS</size></b>\n \n- This will open the MFPS window, which is the same window that appears automatically when you import MFPS for the first time, this window contains useful information about your current MFPS project as the MFPS version you are using, the changelog of the version, addons information, links for tutorials and contact information.\n \n<b><size=18>MFPS ➔ MFPS News</size></b>\n \n- This will open the MFPS News window, in which as the name says, you will find news about MFPS and the add-ons,\nlatest add-ons updates, third-party assets from the Asset Store that you may find interesting for shooter projects.\n \nThis information of this window is fetched from our server so you may want to check it regularly so you don't miss anything about MFPS.\n\n<b><size=18>MFPS ➔ Updates</size></b>\n\n- The window displays available patch updates for your current MFPS version, but only after your purchase is verified. If your purchase hasn't been verified yet, you'll be prompted to complete the verification form.");
        }
        else if (subStep == 1)
        {
            DrawText("Starting from version 1.10.0, MFPS introduces a new editor tool, streamlining the execution of common tasks such as previewing and modifying first-person (FP) or third-person (TP) weapons, or loading map scenes. This enhancement is particularly beneficial in MFPS 1.10.0 and later versions, where FP and TP weapons are no longer part of the player prefabs, making the tool even more essential.\n \nUpon importing MFPS, the overlay tool automatically appears in the scene editor view for immediate access. The interface is displayed as follows:");
            DrawServerImage("img-60.png");
            DrawText("If the overlay window doesn't appear or if you've closed it and wish to reopen it, you can easily access it again. Simply navigate to the scene view window, click the options button in the top right corner, and select <b>Overlays > MFPS Toolbar</b>.");
            DrawServerImage("img-61.png");
            DrawText("You can attach the overlay anywhere in the editor. The buttons in the toolbar perform the following actions:");
            DrawServerImage("img-62.png");
            DrawHorizontalColumn("1 (Preview FP Weapons)", "Instantiate a player first person view placeholder in the active scene with all the FP weapon containers so you can preview the FP weapons as they look in-game to make any necesary change or modification.");
            DrawHorizontalColumn("2 (Preview TP Weapons)", "Instantiate a selected player prefab in the active scene with all the TP weapon containers so you can preview the TP weapons as they look in-game in the selected player model to make any necesary change or modification.");
            DrawHorizontalColumn("3 (Open MFPS Manager)", "Opens the MFPS Manager editor window.");
            DrawHorizontalColumn("4 (Load Map Scene)", "Show a list with the MFPS integrated map scenes, by selecting one of the maps in the scene you can open it in the editor.");
        }
    }

    void DrawGameTexts()
    {
        if (subStep == 0)
        {
            DrawText("<b>If you want to modify some text in the game or fix its grammar, most of the text is directly assigned through UI Text components located in scene-specific canvas objects. Additionally, there are texts defined or modified at runtime via code for easier customization. To simplify this process, we have compiled all these texts into a single script: <i>bl_GameTexts.cs</i>.</b>\n\nIn this script, you’ll find all the text assigned by code during runtime. You can modify these texts directly within the script, which makes it simpler to implement your own localization system as a reference point.");
            DrawServerImage("img-22.png");
        }
        else if (subStep == 1)
        {
            DrawText("Since version 1.10.0, MFPS include a profanity word filter, which allow to filter user generated content like in chat or nick name.\n \nBy default this feature is enabled but you can disable it in <b>GameData > Filter Profanity Words</b>.");

            DrawSuperText("<b><size=16>Modify profanity words</size></b>\n \nBy default, the profanity word database included in MFPS contains more than 2800 common profanity words, you can remove or add more words as please.\n \nFor this, you simply have to open and edit the <?link=asset:Assets/MFPS/Scripts/Internal/Data/Text/bad-words.txt>bad-words.txt</link> file located in: <i>Assets ➔ MFPS ➔ Scripts ➔ Internal ➔ Data ➔ Text ➔ <?link=asset:Assets/MFPS/Scripts/Internal/Data/Text/bad-words.txt>bad-words.txt</link></i>, to add new words, follow the same Json syntax.");
        }
    }

    void GameInputDoc()
    {
        if (subStep == 0)
        {
            DrawText("A critical aspect of shooter games <b><size=8>(or all games in general)</size></b> is the user control, but this is kinda special in action games since players usually have their own way to set up the inputs with which they feel comfortable playing, MFPS by default have the \"standard\" input set up which is set in the most fps games.\n \nSince version 1.9, MFPS comes with a custom Input Manager that allows rebinding the input keys in runtime through a menu in the settings window, you can define the default inputs in the \"Input Mapped\" and let the players decide if they wanna change in-game.\n \nAlternatively, if you want to use a third-party Input Manager system, MFPS also facilitates this, all the in-game used inputs are defined in the script bl_GameInput.cs, you can change the code of the functions to point to your custom input system.\n \nYou can identify which input is for what game action by the function name <i>(Fire, Reload, Jump, etc...)</i>");
        }
        else if (subStep == 1)
        {
            DrawText("If you want change the default input mapped of the keyboard or gamepad you simple have to modify the mapped object.\n\nSelected the input mapped scriptable object, if you are using one of the default ones, they are located at: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>, select it and in the inspector you will have the list of all the setup inputs, fold out the input that you want to modify and edit the info <i>(keycode, axis name, description, etc..)</i>\n");
            DrawServerImage("img-40.png");
            DownArrow();
            DrawText("Also you can reorder the input order, the same order in this list is the order that will be displayed in game.");
        }
        else if (subStep == 2)
        {
            DrawText("Add a new input is really simple, basically you only have to add a new field in the input mapped and set the keycode of the input.\n\n1 - Select the Input Mapped in which you want to add the input, by default there're only 2: Keyboard and Xbox Controller, so select the one that you want edit, these mappeds are located at: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Presents ➔ Input Mappeds➔*</i>\n\n2 - In the inspector of the mapped you will see the list called<b> Button Map</b> with all the current inputs, add a new field in this list and fill the info of it:");

            DrawPropertieInfo("KeyName", "string", "The custom key name of this input");
            DrawPropertieInfo("PrimaryKey", "KeyCode", "The Unity Keycode of this input");
            DrawPropertieInfo("PrimaryAxis", "string", "if this input is not a key but a axis, set the name here.");

            DownArrow();

            DrawText("Once you have configured the key, now you can use it in your code, the usage is pretty similar to the default Unity Input, instead of:\n");
            DrawCodeText("Input.GetKeyDown('keyName'){...}");
            DrawText("You have to use:");
            DrawCodeText("bl_Input.isButtonDown('keyName'){...}");
            DrawText("or");
            DrawCodeText("bl_Input.isButton('keyName'){...}\nbl_Input.isButtonUp('keyName'){...}");
            DrawText("Where the <i>'keyName'</i> value is the <b>KeyName</b> of your setup input in the Input Mapped.\n");
        }
        else if (subStep == 3)
        {
            DrawText("In order to use a GamePad/Controler with MFPS and Input Manager you have to do some extra steps.\n \n- First, if you did not override the Input Settings when install MFPS <i>(using the installer window)</i> you have to modify the Unity Input Settings to add the required control axis, Input Manager comes with a prepared <b>Input Settings.asset</b> with all this already set up, so you only have to click the button below.");

            DrawHTMLPlaceholder();
            if (!File.Exists("ProjectSettings/InputManager-backup.asset"))
            {
                if (Buttons.FlowButton("Setup Unity Input Manager"))
                {
                    string sourcePath = "Assets/MFPS/Content/Prefabs/Presets/Input Mappeds/InputManager.txt";
                    if (!File.Exists(sourcePath))
                    {
                        Debug.LogWarning("The MFPS InputSettings data couldn't be found.");
                        return;
                    }
                    string imFile = "ProjectSettings/InputManager.asset";
                    if (!File.Exists(imFile))
                    {
                        Debug.LogWarning("The InputManager data couldn't be found.");
                        return;
                    }
                    File.Move(imFile, imFile.Replace("InputManager", "InputManager-backup"));
                    File.Copy(sourcePath, "ProjectSettings/InputManager.asset");
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                GUILayout.Label("MFPS input settings has been integrated already.");
            }

            DownArrow();

            DrawSuperText("Now in the <?link=asset:Assets/MFPS/Resources/InputManager.asset>InputManager</link> in the field <b>Mapped</b> set the input mapped for your controller, by default the addon comes with the InputManager mapped for xbox controller, drag this or your created one in the <b>Mapped</b> field");
            DrawServerImage("img-41.png");
        }
        else if (subStep == 4)
        {
            DrawText("To create a new input Mapped simply go to the folder where you want to create it <i>(in the Project View window)</i> -> Right Mouse Click -> Create -> MFPS -> Input -> Input Mapped, now you will see the new created object, select it and setup all the inputs of your controller keyboard.");
            DrawServerImage("img-42.png");
            DrawText("Optionally you can just duplicate one of default mappeds and edit the inputs.");
        }
        else if (subStep == 5)
        {

        }
    }

    private AssetStoreAffiliate uiAssets;
    void GameUIDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("An important modification and most of the time skipped or not much time invested in is the redesign of the user interface, it's extremely important that you modify the default UI and not just the color but the actual layout and if possible the sprites and the overall design since it not only will make your game seem more unique but also will make your game not just another quick copycat game and avoid bad reviews of players because of this.\n \nThere are no special steps in MFPS to modify the UI, you can modify it as you would do in any other Unity project that uses UGUI, all the UI is structured and designed inside the canvas in each scene, there you can change the images, text, font, sprites, layout, etc... if you are not familiar with the Unity UI system, check this tutorial first:\n<?link=https://learn.unity.com/tutorial/ui-components#>https://learn.unity.com/tutorial/ui-components#</link>");

            DrawText("If you are already familiar with Unity's UI system then you can modify pretty much anything of the default UI, you simply have to <b>make sure to not delete components that are referenced/required by a script</b>, if you aren't sure if you can delete a UI object and you want to hide it then you can simply disable it instead.");
            DrawText("<b><size=18>Lobby UI</size></b>\n \nAll the lobby UI can be found in the <b>MainMenu</b> scene hierarchy <b>Canvas</b>, a common question I receive is:\n \n<b><size=14>Where to change the background image?</size></b>\n \nYou can change the lobby background image in the inspector of this image component attached to this object in the hierarchy:");
            DrawServerImage("img-48.png");
            DrawSuperText("<size=18><b>Map UI</b></size>\n\nStarting from <b>MFPS 1.10.0</b>, all map/room-related UI elements are placed in a separate scene named <b>`<b><?link=asset:Assets/MFPS/Scenes/RoomUI.unity>RoomUI</link></b>`</b>, which is loaded <i>additively</i> at runtime. This approach ensures that UI changes only need to be made once, eliminating issues with unlinked prefabs or inconsistent updates across scenes.\n\nBy default, when you open a map scene in the Unity Editor, the <b><?link=asset:Assets/MFPS/Scenes/RoomUI.unity>RoomUI</link></b> scene is automatically loaded alongside it. However, you can also open the <b><?link=asset:Assets/MFPS/Scenes/RoomUI.unity>RoomUI</link></b> scene manually to make changes directly to the UI. Once edited and saved, the changes will apply across all maps that include this scene.\n\nYou can find the <b><?link=asset:Assets/MFPS/Scenes/RoomUI.unity>RoomUI</link></b> scene in the following path:\n<b>`<b>MFPS/Scenes/<?link=asset:Assets/MFPS/Scenes/RoomUI.unity>RoomUI</link></b>`</b>");
        }
        else if (subStep == 1)
        {
            DrawText("In case you are looking for UI kits to replace the default UI of the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (uiAssets == null)
            {
                uiAssets = new AssetStoreAffiliate
                {
                    randomize = true
                };
                uiAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555480555/widget-medium");
                uiAssets.FixedHeight = 400;
            }
            else
            {
                uiAssets.OnGUI();
            }
            DrawAffiliateList("https://assetstore.unity.com/linkmaker/embed/list/4673555480555/widget-medium");
        }
    }

    void DrawFriendListDoc()
    {
        DrawText("Photon have a <b>Friend List</b> feature which allow players to check the status of other users connected in the same server, you only have to send the UserIDs of the players that you wanna check the status, the system is pretty limited since you only can know when a player is \"Online\" or not, and only allows you join to the player room if this is in one.\n\nMFPS comes with this Photon feature added, in the Main Menu scene you can add friends and save it locally <i>(or in database if you are using ULogin Pro)</i>, once you added you'll be able to see when this player is online and also you will be able to join to the same room when the player is in one room.\n\nAdded a friend doesn't require confirmation of the other player, you simple set the exact player name and ready.\n\nIn order to add a friend simply click in the top right button (in the lobby) with the person icon:\n\n");
        DrawServerImage(10);
        DrawText("The friend list have a limit number of friends that can be added (by default is 25) this only make sense if you are saving the friends in a database (using ULogin Pro for example) since the more friends they add per player more will be the size of the player data in the database.\n\nYou can change this limit in GameData -> MaxFriendsAdded.\n");
    }

    void CrosshairDoc()
    {
        DrawText("The Crosshair or reticle is a basic feature in most shooter games, in MFPS you can easily change the shape, color, and size of the crosshair, also you can use different crosshair for each type of weapon.");
        DownArrow();
        DrawTitleText("Modify crosshair");
        DrawText("Open one of your map scenes ➔ go to <i>(in the Hierarchy window)</i> <b>UI ➔ No Interactable ➔ Crosshair ➔ Crosshairs ➔ *</b>, there you will see all the available crosshairs setups, by default each one of them is used for different kind of weapons <i>(machineguns, shotguns, knife, etc...)</i>.\n\nSo what you have to do here is Open one of the crosshair setup/styles and apply any kind of modification that you want, you can remove or add any UI component that you want, just make sure everything is under the crosshair style root object.");
        DrawServerImage(32);
        DownArrow();
        DrawTitleText("Hit Marker");
        DrawText("The <b>hit marker</b> is a small cross that shows up when the local player hit an enemy.\n\nYou can customize the design of this on <b>UI ➔ PlayerUI ➔ Crosshair ➔ Crosshairs ➔ Hitmarker</b>.\n\nThe hit marker appears with simple scale-up animation, you can define the final size of the animation in <b>bl_UCrosshair ➔ Increase Amount</b>.");
    }

    void FootStepsDoc()
    {
        DrawText("The footstep sound in MFPS is driven by the surface Tag, there're some predefined tags like <i>Metal, Concrete, Dirt, Wood, and Water</i>, when a player moves a footstep sound will play depending on the surface where the player is over, of course, you can change these sounds or add more surface tags, also you can change the sounds per player prefab so you can have different sounds for different player models.");
        DrawTitleText("Change Sounds");
        DrawHyperlinkText("In order to change the default, MFPS footstep sounds you can simply replace the AudioClips in the default <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, unfold the <b>Groups</b> list ➔ unfold the tag group field ➔ replace the AudioClips in the list.");
        DrawServerImage(29);
        DownArrow();
        DrawTitleText("Add Surfaces");
        DrawHyperlinkText("If you wanna add a new surface <i>(identified by a different tag)</i> and you wanna play a specific footstep sound for it, you can do it by simply add a new field in the Groups list of the <link=asset:Assets/MFPS/Content/Prefabs/Presents/Audio/FootStepsLibrary.asset>FootStepLibrary</link>, in that new field, in the propertie <b>Tag</b> set the tag identifier for that surface.");
        DownArrow();
        DrawTitleText("Terrain Surfaces");
        DrawText("Since the footstep system is driven by <b>Tags</b> you can only set a tag per object/mesh, this comes with a problem for the <b>Unity Terrain system</b>, since the terrain is a single object/mesh, you can set only one tag for it, that is inconvenient because terrains usually have various layers with different textures that simulate different surfaces, so in order to make the footstep system work correctly with the Unity Terrain system <i>(play different sounds depending on the terrain layer)</i> you need to set up your terrain layers as follow.\n\n•  First, select the Terrain object in your scene hierarchy and add the script <color=#00E9FFFF><b>bl_TerrainSurfaces.cs</b></color>\n\n•  Once you add the script you will notice in the inspector of the script that the list <b>TerrainSurfaces</b> have various fields, each of these fields represent one of the layers <i>(textures)</i> in the <b>Terrain</b>, what you have to do is set the <b>Tag</b> name to each layer, if you fold out one of the fields you will see the texture of the layer, so depending on the texture you can set which <b>tag</b> should be assigned in the property <b>Tag</b>.");
        DrawServerImage(30);
    }

    void DoorsDoc()
    {
        DrawSuperText("MFPS includes an optimized door system that can be used to add dynamism to your maps.\n \nThe system is designed to perform well with a high amount of doors per map, of course, synchronized over the network, and really simple to use.\n \n<?title=18>HOW TO ADD DOORS?</title>\n \n- In order to add a door to your map, you only need 2 steps:\n \n<b>1. Place the door on your map:</b> Drag and drop the door prefab in your scene hierarchy and placed it in your map design, you can find the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default door prefab</link> located in: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ Door</i>\n \n<b>2. Register the new door:</b> In the map scene hierarchy, go to ItemManager ➔ bl_DoorManager ➔ click on the button '<b>Collect all active doors in scene</b>' ➔ that's.");
        DrawServerImage("img-32.png");
        Space(20);
        DrawSuperText("<?title=18>HOW TO CREATE A NEW DOOR?</title>\n \n- If you want to create a door with your custom door model, you simply have to use the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default MFPS door</link> prefab and replace its model.\n \n1. Instance the <?link=asset:Assets/MFPS/Content/Prefabs/Level/Items/Door.prefab>default MFPS door</link> prefab in any scene and Unpack the prefab instance <i>(right-click on the door instance ➔ Unpack Prefab Completely)</i>\n \n2. Drag your custom door model on the door instance under Door ➔ Door Model ➔ *, and manually place the new door model in the same position, rotation, and scale as the default model.\n \n3. Select the root of the door instance ➔ <b>bl_BasicDoor</b> ➔ in the <b>Door Pivot</b> field, drag the transform of your new door model that will be the point where the door will rotate around (pivot point).\n \n4. Delete the default model <i>(Door ➔ Door Model ➔ <b>Default Door Model</b>)</i> and save the door as a new prefab by dragging it in any folder of the project view.\n \nIf your door model rotation is not right, simply create a new Door Settings <i>(Project View ➔ Right Click ➔ MFPS ➔ Level ➔ Door Settings)</i> ➔ assign it to the door prefab ➔ bl_BasicDoor ➔ <b>Door Settings</b> and assign your new door model pivot rotation values.");
    }

    void GameStaffDoc()
    {
        DrawText("You may want to highlight working game development members with a badge on their behalf for example<b> Lovatto <color=#FF0000FF>[Admin]</color></b> with a different color that normal players, so other users can see that is a staff member on the game, on MFPS 2.0 there are a simple way to do this and you can set up right on the inspector.\n\nGo to <b>Game Data</b> and find the \"Game Team\" section at the bottom of the inspector:\n");
        DrawServerImage(23);
        DrawText("in this list you can add much member as you want, with a simple settings to set up:\n");
        DrawHorizontalColumn("UserName", "The name that the staff member need write to access to this account.");
        DrawHorizontalColumn("Role", "The staff rank / role on the team.");
        DrawHorizontalColumn("Password", "When try to sing in with this account name a password window will appear to write the password (with other names will not appear), so normal player can't fake identity.");
        DrawHorizontalColumn("Color", "The color with the name text will appear in the game.");
    }

    void DrawMobileDoc()
    {
        DrawText("While MFPS is compatible with Android and iOS, there are critical adjustments required before you build your project. By default, the package is configured with high-end graphics settings for demonstration purposes, which are not optimized for mobile hardware. Additionally, the core package does not include built-in mobile input systems.");

        DrawHyperlinkText("So first thing you need is to integrate a mobile input control, for this, there a in-house addon solution specifically for MFPS 2.0 which is: <link=https://www.lovattostudio.com/en/shop/addons/mfps-mobile-control/>Mobile Control</link>, which contains all the necessary buttons/inputs to work in mobile/touch devices and the integration is automatically, but alternatively you can integrate your preferred third-party system if you want.");

        DrawText("Secondly, you have to made some manual optimization work, the same that you would do in any other mobile project, starting by removing the Post-Process effects <b><size=8><i>(you can do this by removing the Post Processing Stack package from the Unity Package Manager)</i></size></b>, Change the Standard Shaders to mobile friendly ones, reducing the textures quality and resolution, and of course your new levels/maps/models have to be mobile friendly, etc... <b>You don't have to do any code change</b>, MFPS code is mobile ready and optimized for low-end platforms.");
        DrawText("So basically, to build for mobile what you have to do is some graphic optimization, below I will leave you some useful links to tutorials that will help you with graphic optimization and things to keep in consideration:");
        DrawLinkText("https://docs.unity3d.com/2020.1/Documentation/Manual/MobileOptimizationPracticalGuide.html", true);
        DrawLinkText("https://learn.unity.com/search/?k=%5B%22tag%3A5816095d0909150016dc7b17%22%2C%22lang%3Aen%22%2C%22q%3Aoptimization%22%5D", true);
        DrawLinkText("https://cgcookie.com/articles/maximizing-your-unity-games-performance", true);
        DrawLinkText("http://www.theappguruz.com/blog/graphics-optimization-in-unity", true);

        DrawSuperText("If you are just starting with mobile development and want to save some time, there's also an <b>MFPS Mobile</b> asset which is ready to use for mobile platforms, it comes with the Mobile Input Controller already integrated and the game optimization is already done, if you are interested, check it out here: <?link=https://www.lovattostudio.com/en/shop/mobile/mfps-mobile/>MFPS Mobile</link>");
    }

    void PlayerIKDoc()
    {
        DrawText("The Third-person/Remote player upper-body poses are defined by the animations clips in the Animator Controller, but some bones of the model rig are override controlled by Inverse Kinematic aka IK, this to achieve multiple things, one of them is so that <b>you don't have to have a custom player animation for each one of the weapons</b> since normally the left arm goes in different parts depending on the weapon.\n \nSo MFPS solves this using IK in these specific bones that you can easily modify per weapon.\n \nIn order to edit the left arm position/pose of one of the TPWeapons do the following:\n \n1. In the Editor <i>(in edit mode)</i> drag one of the player prefabs in a scene <i>(preferable a clean scene so you can focus)</i>.\n \n2. Go to the Remote Weapons which are located under the right hand of your player model in the hierarchy -> select the TPWeapon to edit the pose and active it.\n \n3. With the weapon selected -> In the inspector window you will see a button called \"<b>Edit Positions</b>\" -> click it and move/rotate the IK target in the scene view until you get the desired position.");
        DrawAnimatedImage(5);
    }

    void PostProcessingDoc()
    {
        if (subStep == 0)
        {
            DrawText("By default MFPS use the Unity <b>Post-Processing stack v2</b>, which is a collection of effects and image filters that apply to the cameras to improve the visual of the game.\n\nAlthough the image effects really improve the visual aspect of the game, that also carries a cost in the performance of the game, that is why you should use this only for high-end platforms like PC or Consoles, you definitely <b>should NOT use for Mobile Platforms</b>, if you are targeting for a mobile platform you should delete this package.\n\nThis system is automatically imported from the Unity Package Manager (UPM) when you import MFPS for the first time in the project, if you want to <b>DELETE</b> it, you can do it clicking on this context menu:");
            DrawServerImage(11);
            DrawText("You can find the Post-Processing Stack official documentation here:");
            DrawLinkText("https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/index.html", true);
        }
        else if (subStep == 1)
        {
            DrawSuperText("Since version 1.9, you can easily define custom <b>Post Processing Profiles per map scene</b>,\nPost Process Profile is the configuration file where you define the image effects that will be rendering in the Post Process Volume, if you need more details or you don't know how to create or use them, check the official documentation for it here: <?link=https://docs.unity3d.com/Packages/com.unity.postprocessing@3.1/manual/Quick-start.html>Post-Process Quick Start</link>\n\n<?title=18>DEFINE PROFILE IN MAP SCENE</title>\n\nTo use a custom PP Profile in a specific map scene: open the map scene in the Editor ➔ in the Hierarchy window, go to: GameManager ➔ Post Process Volume ➔ in the inspector window of bl_PostProcessEffects ➔ Assign your custom Post Process Profile in the \"Process Profile\" field and you are all set.");
        }
        else
        {
            DrawSuperText("There are some common problems related to the Post Processing package, that cause errors in the console due to the Post Processing package missing, here is how to handle them:\n \n<?title=18>DON'T WANT TO USE:</title>\n \nIf you don't intend to use the Post Processing package but you still receive errors due to missing references to the package, you have to remove the package script definition symbols, for it, go to Unity Player Settings ➔ Other Settings ➔ Script Define Symbols ➔ find and remove this part from the input field <?underline=><b>UNITY_POST_PROCESSING_STACK_V2;</b></underline> ➔ hit enter and wait for the compilation.\n \n \n<?title=18>WANT TO USE:</title>\n \nIf you want to use the Post Processing features, but receive errors due to missing references to the package, it means that the package hasn't been imported yet.\n \nTo import the package, go to Window ➔ Package Manager ➔ in the left panel find the <?underline=>Post Processing</underline> package ➔ Click in the Import button on the bottom right corner of the window.");
        }
    }

    private AssetStoreAffiliate fxAssets;
    void ParticlesDecalsDoc()
    {
        if (subStep == 0)
        {
            DrawHyperlinkText("<b><size=22>PARTICLES</size></b>\n\nOne important part of making a good looking game are the particle effects, although MFPS use them just in few events, they show frequently, so use good looking particles is not enough, they have to be optimized, the default particles in MFPS are very much just placeholders, and even tho its not necessary is recommended to replace with better ones.\n \nIn order to replace/modify a specific particle you simply have to modify their prefab, MFPS use <b>Particle System</b> for the following game effects:\n \n•  Muzzleflash\n•  Explosions\n•  Bullet hit\n•  <link=asset:Assets/MFPS/Content/Prefabs/Level/Particles/Prefabs/Impacts/BloodFast.prefab>Blood</link>\n•  Fire\n \n \nAside from the <b>Muzzleflash</b> particle which is located inside each FPWeapon, all the other particles can be found in or under the MFPS folder at <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles->*</i>");
            DrawNote("When you modify a particle prefab, make sure to use the default prefab as a reference since these have attached custom scripts attached aside from the Particle System components which are necessary to the game work correctly.");
            DrawHorizontalSeparator();
            DrawText("<b><size=22>DECALS</size></b>\n \nMFPS doesn't use a custom decal system, since they are only used for the bullets marks, and since these are instanced too frequently it just adds a non-needed performance cost to the game, due to that, MFPS uses a simple Quad mesh with a simple transparent shader as decals.\n \nThe only decals that MFPS uses are the <b>Bullet Marks</b>, these are instanced when a bullet hit a collider, the mark that will appear depending on the hit collider tag, by default MFPS use 5 different bullet marks that simulate the impact in different surfaces (metal, wood, sand, concrete and a generic one), you can change these marks textures by modifying the bullet impacts prefab material, these prefabs are located in <i>Assets -> MFPS -> Content -> Prefabs -> Level -> Particles -> WeaponEffects -> Prefabs->*</i>");

            DrawSuperText("<b><size=16>Add new surface decal / particle hit</size></b>\n\nIf you want to add a new surface tag to show custom decals and particle hit effect when the bullets hit colliders with that tag:\n\n- Select the <?link=asset:Assets/MFPS/Content/Prefabs/Weapons/Decal/Bullet Decal List.asset>Bullet Decal List</link> container object.\n\n- With the object select > go to the inspector window > you will see the <b>Surface Decals</b> list > add a new field to the list and fill the required properties.");

            DrawHorizontalColumn("Surface Tag", "The name of tag that you want to setup, it have to be the exact same one as the defined in the Unity Tags list.");
            DrawHorizontalColumn("Decal Materials", "Here you assign all the bullet marks material variations for this surface, you can copy / duplicate the materials from existing surfaces and simply change they texture.");
            DrawHorizontalColumn("Hit Particle Name", "Here you assign the name of the pooled particle prefab, which is the Key assigned in the Pooled prefab container list.");
            DrawHorizontalColumn("Size Range", "The scale range to be applied to the bullet mark instance (to add size variation)");
        }
        else if (subStep == 1)
        {
            DrawText("In case you are looking for particles or decals to replace the default ones in the game, below you will find a hand-picked collection of assets that you can acquire from the Asset Store");
            Space(20);
            if (fxAssets == null)
            {
                fxAssets = new AssetStoreAffiliate
                {
                    randomize = true
                };
                fxAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555492698/widget-medium");
                fxAssets.FixedHeight = 400;
            }
            else
            {
                fxAssets.OnGUI();
            }
            DrawAffiliateList("https://assetstore.unity.com/linkmaker/embed/list/4673555492698/widget-medium");
        }
    }

    void InGameChatDoc()
    {
        DrawText("A way of user-to-user communication in-game of course is the text chat, for sure MFPS implements that.\n \nIn-game, when players are on a map, they can open the text chat input field to start writing with the <b>Enter/Submit key input</b>, and with the same key or the button next to the input field can send the message which will appear <i>(by default)</i> on the bottom left corner of the screen, pretty basic, but that isn't all, the chat support team only chat so players can communicate only with teammates, for that they simply have to open the chate with a custom key by default <b>T key</b> and that is it.\n \nFor some or other reason you may <b>not want to allow user-to-user communication</b> in-game, if that the case you can simply disable the text chat by deleting or just disabling the Chat UI which is located in the Map scenes UI canvas at UI -> Interactable -> <b>Chat</b>.");
        DrawHorizontalSeparator();
        Space(10);
        DrawText("If you want to show a custom text in the chat panel for all the players in the room by code,  you can do so like this:");
        DrawCodeText("bl_ChatRoom.Instance.SetChat('YOU TEXT HERE');");
    }

    void WeaponRecoilDoc()
    {
        DrawText("The <b>recoil</b> is a mechanic used to simulate the backward momentum a firearm has when it is discharged. When you fire multiple shots in quick succession, the kickback of the weapon cause the camera view to move upwards and sideways, simulating the increased difficulty of controlling the gun due to recoil.\n \nIn MFPS you can define the weapon recoil per weapon, and you have 3 different weapon recoil system that you can define:");
        Space(10);
        DrawHorizontalColumn("Simple Vertical", "This method moves the camera upward only.");
        DrawHorizontalColumn("Pattern", "This method makes the camera movement always follow a predefined pattern, so players can learn this pattern to control the weapon, this method is used in games like Counter Strike and Valorant.");
        DrawHorizontalColumn("Random 2D", "This method moves the camera upward and sideway randomly.");
        Space(10);
        DrawText("<b><size=16>Setup weapon recoil</size></b>\n \nYou can setup a different recoil setup for each weapon, for it, you have to create a preset recoil setting object and then assign it in the inspector of <b>bl_Gun > Recoil Setting</b>:");
        DrawText("1. Create a weapon recoil preset, for this you can simply duplicate one of the existing presets located in the folder: <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Weapons ➔ Recoils➔*</i>, > rename it > modify the values as required.\n \n2. In the FPWeapon you want to add this recoil setup > <b>bl_Gun</b> inspector > assign the recoil preset in the property <b>Recoil Settings</b>.");
        DrawNote("To duplicate a recoil preset, select one of the presets in the folder view > press Ctrl + M");
        DrawServerImage("img-52.png");
        Space(10);
        DrawText("<b><size=16>Recoil Properties</size></b>\n\nThe recoil properties are pretty self explanatory by its name, however, one aspect that might cause some confusion is the <b>Recoil Pattern</b>.\n \nWhen you activate the <b>Recoil Pattern Mode</b> in the recoil preset, an AnimationCurve property will become visible. This is the tool you use to shape your custom recoil pattern or path.\n \nThe AnimationCurve editor allows you to easily design a unique recoil trajectory. To better visualize this, imagine the recoil pattern you want for your weapon, but then rotate that pattern 90 degrees to the right. In this scenario, what you perceive as vertical recoil in the game <i>(upward kick of the weapon)</i> is represented horizontally in the editor, and what is the horizontal recoil <i>(sideways movement)</i> in the game is plotted vertically in the editor.");
        DrawServerImage("img-53.png");
    }

    void LadderDoc()
    {
        DrawText("The package includes a drag and drops ladder system that you can use to add more dynamic navigation to your maps.\n \nAll you have to do to add a new ladder to your map is drag the ladder prefab by default located in <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> > drop in your map scene hierarchy > positioned it where you want and that's.");
        DrawHorizontalSeparator();
        DrawText("<b><size=16>Replace the Ladder model</size></b>\n \nIf you want to replace or add a new ladder model, all you have to do is use the default ladder prefab > delete the default model and replace it with your own.\n \n1. Drag the default ladder prefab locate at <i>Assets ➔ MFPS ➔ Content ➔ Prefabs ➔ Level ➔ Items ➔ <b>Ladder</b></i> in any scene hierarchy.\n2. Drag your new ladder model inside the prefab instance and positioned it in the same position as the default ladder model in Ladder > Model > *.\n3. Delete the model inside the prefab instanced.\n4. Save the new prefab so you can use it in your map scenes.");
        DrawServerImage("img-46.png");
    }

    void MFPSEventsDoc()
    {
        DrawText("There're some special events that you can use if you wanna implement a custom feature or modification in your own scripts, e.g: when the local player spawn, when the local player dies, when receiving damage, etc...\n\nUse these events are really simple, all you have to do is subscribe a function from your script that will listen to the callback of these events when they are dispatched in runtime.\n\nYou subscribe to these events on <b>OnEnable()</b> and unsubscribe on <b>OnDisable()</b> functions:");
        DrawCodeText("void OnEnable()\n{\nbl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;\n}\n\nprivate void OnDisable()\n{\nbl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;\n}\n\nvoid OnLocalPlayerSpawn()\n{\n//execute your code\n}");
        DownArrow();
        DrawText("Below you will have the list of all the available events with a short description\n");
        DownArrow();

        DrawCodeText("bl_EventHandler.onLocalPlayerDeath");
        DrawText("Event called when the LOCAL player die in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalPlayerSpawn");
        DrawText("Event called when the LOCAL player spawn");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpGun");
        DrawText("Event called when the local player pick up a weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onChangeWeapon");
        DrawText("Event Called when the LOCAL player change of weapon");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalAimChanged");
        DrawText("Event Called when the local player change their Aim state");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onMatchStart");
        DrawText("Event Called when the room match start");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onFall");
        DrawText("Event Called when the LOCAL player fall/land in a surface");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPickUpHealth");
        DrawText("Event called when the LOCAL player pick up a health in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAirKit");
        DrawText("Event called when the LOCAL player call an air drop");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onAmmoPickUp");
        DrawText("Event called when the LOCAL player pick up ammo in game");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onLocalKill");
        DrawText("Event called when the Local player get a kill or get killed in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.OnRoundEnd");
        DrawText("Event called when a game round finish");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onPlayerLand");
        DrawText("Event called when the LOCAL player land a surface after falling");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onRemoteActorChange");
        DrawText("Event called when a player that is not the local player spawn or die");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the local player change an in-game setting/option");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onEffectChange");
        DrawText("Event called when the local player change one or more post-process effect option in game.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGameSettingsChange");
        DrawText("Event called when the LOCAL player change their game settings from the settings in-game menu.");
        DrawHorizontalSeparator();

        DrawCodeText("bl_EventHandler.onGamePause");
        DrawText("Event called when the LOCAL player pause or resume the game.");
        DrawHorizontalSeparator();
    }

    void HookDoc()
    {
        if (subStep == 0)
        {
            DrawText("\n<size=20><b>Overview</b></size>\n\n\nThis hook system lets customize MFPS without modifying core code. It supports two main types of hooks:\n\n• <b>Actions:</b> Trigger custom code when an event occurs.\n\n• <b>Filters:</b> Process and modify data before it’s used.\n\n\nBoth hooks support a <i>priority</i> (default is 10), meaning lower numbers run first. You can also remove hooks or check if any are registered. Additionally, a multi-parameter filter lets you process multiple values at once.");
            DrawHorizontalSeparator();
            DrawText("<size=20><b>Actions</b></size>\n\n\n<b>Purpose:</b>Use actions to notify your system when something happens (for example, a player dying). Actions execute callbacks without returning a value.\n\n<b>How to Use:</b>\n\n1. <b>Register an Action:</b>");
            DrawCodeText("bl_Hook.AddAction(\"OnPlayerPickAmmo\", args => {\n    int ammoCount = (int)args[0];\n    Debug.Log($\"{ammoCount} ammo pick up.\");\n});");
            DrawText("This registers an action for the `<b>\"OnPlayerPickAmmo\"</b>` event.\n\n<b>Trigger the Action:</b>");
            DrawCodeText("bl_Hook.DoAction(\"OnPlayerPickAmmo\", 30);");
            DrawHorizontalSeparator();
            DrawText("\n<size=20><b>Filters</b></size>\n\n\n<b>Purpose:</b>\nFilters modify a value by passing it through one or more callbacks. Each filter receives a value, can change it, and passes it along. If no filters are defined, the original value is returned.\n\n<b>How to Use:</b>\n\n1. <b>Register a Filter:</b>");
            DrawCodeText("bl_Hook.AddFilter(\"ModifyScore\", input => {\n    int score = (int)input;\n    return score * 2; // Double the score.\n});");
            DrawText("This filter is registered to <i>\"ModifyScore\"</i>.\n\n<b>Apply the Filter:</b>");
            DrawCodeText("int originalScore = 10;\nint finalScore = (int)bl_Hook.ApplyFilters(\"ModifyScore\", originalScore);\n// finalScore becomes 20.");
            DrawText("1. The original score (10) is passed through the filter, ending up doubled.\n\n\n<b>Tip:</b>\nIf you need the filter chain to stop as soon as one filter returns a non-null value, use:");
            DrawCodeText("object result = bl_Hook.ApplyFiltersUntilNonNull(\"ModifyScore\", 10);");
            DrawHorizontalSeparator();
        }
        else if (subStep == 1)
        {
            DrawText("Here are the MFPS Core default defined hook actions and filters that you can listen to");
            DrawHorizontalSeparator();
            // Action Action_RespawnLocalPlayer
            DrawHookBox("Action_RespawnLocalPlayer", "Action called when the local player respawn in game.\nYou can override the default respawn logic with this by adding an action and set it's priority below 10", 0);
            // Filter_PlayerPrefabToInstance
            DrawHookBox("Filter_PlayerPrefabToInstance", "Filter the player prefab that will be instantiated for the local player.", 1, "object[3] 1:prefab 2:team 3:skinId");
            // Filter_BotNetInstanceData
            DrawHookBox("Filter_BotNetInstanceData", "Filter the bot network instance data (object[]), you can use to modify the bot data before instantiate it, this data is stored and sync over network in the bot network view.", 1, "object[] data");
            // Filter_BotSpawnPrefab
            DrawHookBox("Filter_BotSpawnPrefab", "Filter the bot prefab that will be instantiated for the bot player.", 1, "object[] 1:string prefabName 2:Team");
            // Filter_BotSpawnPosition
            DrawHookBox("Filter_BotSpawnPosition", "Filter the bot spawn position", 1, "Vector3 position");
            DrawHookBox("SpawnProtectedTime", "Filter the local player spawm protection time, the default value is the one defined in the inspector of the player health script, if return 0 = disable spawn protection.", 1, "int protectionTime");
            DrawHookBox("Filter_LocalRespawnTime", "Filter the local player re-spawn time, when the player is eliminated and can respawn after a delay, the returned value determine that delay duration.", 1);
            // CountDownTime
            DrawHookBox("CountDownTime", "Filter the time that the game will start after the countdown, the default value is the one defined in the game data, if return 0 = start the game immediately.", 1, "float countdownTime");
            // UseCountdown
            DrawHookBox("UseCountdown", "Filter if the countdown will be used or bypass it, the default value is always true, but if the countdown option is disable in game data of game mode this hook value does nothing.", 1, "bool useCountdown");
            // TimeToRegenerateHealth
            DrawHookBox("TimeToRegenerateHealth", "Filter the time/delay to regenerate health after not receiving damage, the default value is the one defined in the player health script.", 1, "float time");
            //LocalJumpForce
            DrawHookBox("LocalJumpForce", "Filter the local player jump force, the default value is the one defined in the player movement script.", 1, "float jumpForce");
            // LocalMaxStamina
            DrawHookBox("LocalMaxStamina", "Filter the local player max stamina, the default value (float) is the one defined in the player controller script.", 1, "float maxStamina");
            // PreChatMessage
            DrawHookBox("PreChatMessage", "Filter the chat message before it's displayed in the UI, the default value is the message that the player wrote.", 1);
            // OnChatMessage
            DrawHookBox("OnChatMessage", "Action called when a chat message is received.", 0, "string message", "string senderName");
            // GameModeLogicActive
            DrawHookBox("Filter_ActiveGameMode", "Filter the game mode logic right before active it, you can use to modify the logic script", 1);
            // PlayerNetInstanceData
            DrawHookBox("PlayerNetInstanceData", "Filter the player network instance data (object[]), you can use to modify the player data before instantiate it, this data is stored and sync over network in the player network view.", 1, "object[] data");
            // RoundTimeDuration
            DrawHookBox("RoundTimeDuration", "Filter the round time duration, the default value is the one from the room properties.", 1, "int timeInSeconds");
            // RoomPropertiesReset
            DrawHookBox("Filter_RoomPropertiesReset", "Action called when the room properties are reset, you can use to modify the room properties before they are set.", 1);
            // MatchTotalScoreSave
            DrawHookBox("Filter_MatchTotalScore", "Filter the match total score, the default value is the final result after calculating the player stats in the match.", 1, "int scoreToAdd");
            // MatchTotalCoinsSave
            DrawHookBox("Filter_MatchTotalCoins", "Filter the match total coins, the default value is the final result after calculating the player stats in the match.", 1, "int coinsToAdd");
            // Filter_KillScore
            DrawHookBox("Filter_KillScore", "Filter the score that the player will receive when kill another player, the default value is the one defined in the game data.", 1, "int scoreToAdd");
            // Filter_HeadShotScore
            DrawHookBox("Filter_HeadShotScore", "Filter the score that the player will receive when kill another player with a headshot, the default value is the one defined in the game data.", 1, "int scoreToAdd");
            // Filter_LocalScoreAdd
            DrawHookBox("Filter_LocalScoreAdd", "Filter the score that will be added to the local player score prop, at this point the score is hold in the match stats only, not commited to the database yet.", 1, "int score");
            // Filter_RoomList
            DrawHookBox("Filter_RoomList", "Filter the room list that will be displayed in the room list UI, the default value is the list of available rooms in the server.", 1, "List<RoomInfo> list");
            // Filter_AutoCreatedRoomProps
            DrawHookBox("Filter_AutoCreatedRoomProps", "Filter the room properties that will be used when the room is auto-created, the default value (MFPSRoomInfo) are the random ones selected.", 1);
            // Filter_KillCamInfo
            DrawHookBox("Filter_KillCamInfo", "Filter the kill cam info that will be send to the kill camera handler script when the local player dies.", 1, "bl_KillCamBase.KillCamInfo");
            // Filter_ThrowWeaponSendData
            DrawHookBox("Filter_ThrowWeaponSendData", "Filter the data that will be send to the network when the local player throw a weapon.", 1, "HashTable");
            // Action_OnWorldViewWeaponInstance
            DrawHookBox("Action_OnWorldViewWeaponInstance", "Action called when a weapon is instantiated in the world view, you can use to modify the weapon instance, at this point the weapons have already been instantiated.", 0, "GameObject weapon", "HashTable data");
            // Filter_PickUpWeaponSendData
            DrawHookBox("Filter_PickUpWeaponSendData", "Filter the data that will be send to the network when the local player pick up a weapon.", 1, "HashTable props");
            // Action_OnPickUpWeapon
            DrawHookBox("Action_OnPickUpWeapon", "Action called when the local player pick up a weapon.", 0, "GameObject weapon", "HashTable data");
            // Filter_AutoSelectedTeam
            DrawHookBox("Filter_AutoSelectedTeam", "Filter the team that was auto-selected when the player join the room, the default value is the team selected based on the default selection logic.", 1, "Team selectedTeam");
            // Filter_OfflineRoomProps
            DrawHookBox("Filter_OfflineRoomProps", "Filter the room properties that will be used when the room is offline created.", 1, "Hashtable props");
            // Filter_LocalPlayerLoadout
            DrawHookBox("Filter_LocalPlayerLoadout", "Filter the local player loadout before assign it.", 1, "(ScriptableObject) bl_PlayerClassLoadout");
            // Action_LocalBlockAllWeapons
            DrawHookBox("Action_LocalBlockAllWeapons", "Action called when the local player block all weapons", 0, "bl_GunManager");
            // Action_LocalReleaseWeapons
            DrawHookBox("Action_LocalReleaseWeapons", "Action called when the local player release all weapons", 0, "bl_GunManager");
            // Filter_HealthPickUpAmount
            DrawHookBox("Filter_HealthPickUpAmount", "Filter the amount of health that the player will receive when pick up a health item.", 1, "float health");
        }
    }

    private void DrawHookBox(string name, string description, int type, params string[] parameters)
    {
        string typeStr = type == 0 ? "Action" : "Filter";
        GetTutorialHTML()?.SetHookBlock(name, description, type, parameters);
        var r = EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.BeginHorizontal();
            Space(10);
            EditorGUILayout.LabelField(typeStr, EditorStyles.boldLabel, GUILayout.Width(50), GUILayout.Height(20));
            EditorGUILayout.TextField(name);
            if (parameters != null)
            {
                string p = "";
                for (int i = 0; i < parameters.Length; i++)
                {
                    p += parameters[i];
                    if (i < parameters.Length - 1) p += " | ";
                }
                GUILayout.Label($"<size={10}>{p}</size>", Style.TextStyle);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            DrawHorizontalSeparator(1, 0);
            EditorGUILayout.LabelField(description, Style.TextStyle);
        }
        EditorGUILayout.EndVertical();
        Space(5);
    }

    void UpdateMFPSDoc()
    {
        if (subStep == 0)
        {
            DrawText("MFPS typically receives major version updates every 3-4 months. These updates bring new features, fixes, and enhancements. If you're already using MFPS, you'll likely want to incorporate these improvements into your ongoing project. However, due to the nature of the asset, you can't simply import the new update package over your existing project with the older version. Doing so would overwrite all your modifications, resulting in the loss of your work and progress up to that point.\n \nThe effort required to update an older MFPS project to a new MFPS version depends on several factors. These include the extent of your modifications, the version gap between your project and the new MFPS version, and the number of changes in the new update, among others.\n \nIn the following sections, we'll provide methods for updating your MFPS version under various scenarios:");
            Space(10);
            DrawNote("<b><color=#FF0002FF>IMPORTANT:</color> BEFORE TRY ANY OF THE BELOW METHODS OR TRY UPDATE YOUR PROJECT IN GENERAL MAKE SURE TO CREATE A BACKUP COPY OF YOUR PROJECT</b>");
            Space(10);
            DrawTitleText("Front-end changes");
            DrawText("If your modifications to MFPS have been limited to frontend changes such as replacing player models, tweaking properties, adding maps, weapons, or altering the UI, and you haven't made any backend <i>(code)</i> changes, then updating is relatively straightforward. You can import the new MFPS package into your project, but you'll need to deselect certain assets to avoid overwriting your changes.\n \nHere's how to proceed:\n \n<b>Import the MFPS Update Package:</b> You can do this from the Asset Store or from your local disk.\n \n<b>Select Assets in the Unity Package Import Window:</b> In this window, you can choose which assets to import. By default, all assets are selected. To preserve your changes, you need to deselect the following:\n \nThe <i>Resources</i> folder (which includes all the prefabs and GameData)\nThe MainMenu scene (only if you've made changes to the UI)\nAny prefab that you've modified.\n\n<b>Import the ExampleLevel Scene:</b> It's recommended to import the ExampleLevel scene as it likely contains updates. You can use this as a reference for your other map scenes.");
            DrawServerImage(12);
            DownArrow();
            DrawTitleText("Back-end changes");
            DrawText("If you have made <b>code changes</b> to the MFPS to core scripts or modifications to the main prefabs like <b>GameManager, GameModes, Lobby, etc...</b> the update process is more complicated.\n\nFor start, it is not possible to import the new version on your current project since this would revert all your changes losing all the work done by you, to apply the changes of the new version you must merge the changes manually, checking the scripts and prefabricated modifications of the new version and comparing them with those of your project.\n\nThis process may take some time to be done and there're high chances that a wrong merger cause errors in your game, so I will give you a method that will facilitate and reduce the work to do in this case:\n\n•  First, create a new clean Unity project, in it, import the new MFPS version package as you will normally do <i>(importing Photon PUN as well)</i>, you will use this project as reference to see the frontend changes in scenes, prefabs, properties, etc...\n\n•  Then, you will <b>simulate</b> import the new MFPS version package in your started project <i>(the project with the old version of MFPS that you have modified)</i>, note that I said <b>simulate</b> because <b>you won't import it</b>, you only need that the Unity Package Import Windows show up so you can check the changed files, this window shows all the assets that the package contains and will be imported but also if one of the files already exist in the project, it shows if the file from the package is different with respect at the file in the project, you can differentiate hthose modify files with a \"Refresh\" icon at the right side of the file name:");
            DrawServerImage(26);
            DrawText("Now what you have to do is take note of the modified files, write their name in a simple text file so you can find later by their name as explained below.\n\nOnce you have all the changed file names collected, is time merge those files, unfortunatelly there's not a automated way to do this, you have to manually do this, check the changes on scripts, prefabs, etc...  but don't worry you don't have to check line by line, you can use some tools to automatically detect the changes in two files.\n\nFor check changes on scripts you can use these tools:\n");
            DrawHyperlinkText("Standalone program: <link=https://sourcegear.com/diffmerge/>Diffmerge</link>\nOnline System: <link=https://www.diffchecker.com/>Diffchecker</link>\n");
            DrawText("Both systems are really straight forward to use, you have two boxes, in one you assign the script and or code of the old file/script and in the other you assing the new file, then the program will analyze and highlight you the changed lines:\n");
            DrawServerImage(27);
            DrawText("You will assign the original file <i>(the one from your project)</i> in the left side and the file from the new MFPS version <i>(from the project that you create before)</i> in the right side.\n\nNow with the changes highlighted in the program you simple have to merge the changes in your project file <i>(the file of the old MFPS version)</i>\n\nWith the standalone program <b>Diffmerge</b> you can compare the whole MFPS folder and analyze all the files, by simple selecting the option: File -> Open Folder Diff... -> Set the path to the old version MFPS folder in the first box and set the path to the new version MFPS folder in the second box.");
            DownArrow();
            DrawText("Now that method works efficiently for text files like scripts, but for check the differences between prefabs and scenes you will have to do it differently, for check the difference between prefabs and scenes you will have to compare both projects, Fortunately, Unity allows you to open more than one instance of the editor, so you can open the old version project and the new MFPS version project in different editor windows and inspect the differences between the prefabs and or scenes in both projects.");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=16>Patch Update</size></b>\n \nStarting with version 1.10.0, MFPS includes a Patch Updater tool within the editor. This tool allows you to download and apply quick fixes or modifications to your current version of MFPS.\n \nTo access the Patch Updater, go to the editor’s top navigation menu, select <b>MFPS > Updates</b>. In the window that appears, you’ll find a list of available patch updates. Before you can download and apply these patches, you must verify your MFPS purchase. This is a one-time process achieved by clicking the <b>Verify</b> button in the same window.");
            DrawServerImage("img-63.png");
            DrawText("Once your purchase is verified, you can download and apply any available patches. However, if you have made custom modifications to the MFPS core scripts, be cautious. Applying a patch could overwrite your changes. Always check the patch description to see what files are affected and, during the import process, review the files included in the patch to avoid conflicts.\n \nIf you need to apply a patch but have custom modifications in the files it affects, you should manually merge these changes. To do this, import the patch into a temporary MFPS project and compare the modified files using the same method in the previous section <i>(Upgrade Version).</i>");
        }
    }

    void ServerRegionDoc()
    {
        DrawText("Photon PUN offers <b>multiple server regions to connect</b>, MFPS implements all the available servers, and let decide the players if they what to connect to a specific region.\n \nBy default, Photon PUN selects the <b><i>Best Region</i></b> to connect from the player connection, which drives to a problem for games with not a lot of concurrent players that is that players are divided into different regions making it even harder to find games, so MFPS define a <b>Fixed Region</b>, which is the region where all players will connect by default and then if they want to, they can connect to a specific server manually in-game.\n \n<b><b><size=18>Define the Fixed Region</size></b></b>\n \nGo to the MainMenu scene -> Lobby -> Lobby -> <i>(Inspector window)</i> <b>Default Server</b> -> select the region code that will be the fixed/default region to connect.\n \n<b><b><size=18>Change Server Region in Runtime</size></b></b>\n \nPlayers can change the server region in-game with the bottom right dropdown in the lobby menu.");
        DrawServerImage("img-23.png");
        DrawText("<b><size=18>Define the available regions</size></b>\n\nIf you use PUN and for some reason you don't want to allow to connect to some of the PUN server regions, you can define which server regions will be available in your game easily.\n\nFor it go to <b>GameData</b> > <b>Pun Regions</b> > Remove the regions that you don't want from the list.");
    }

    void LocalNotificationsDoc()
    {
        DrawText("There are some in-game local notification in MFPS that appear after a certain event happens in game e.g: <i>after a kill, pick up an item, etc...</i> these notifications are the <b>Who-Kill-Who</b> on the top right corner, the <b>local kills notification</b> on the center of the screen and the left side notifications for \"<i>non-important</i>\" notifications.\n \nFor the Who-Kill-Who or Killfeed notifications, you can check their respective section in this tutorial for more details about it.\n \n<b><size=16>LOCAL KILL NOTIFICATIONS</size></b>\n \n- Is the notification that appears in the center of the screen after the local player terminates an enemy in-game, MFPS comes with two options for how to display these notifications:\n \n1. <b>QUEQE:</b> show one kill at the time, in case multiple kills happens in a row, one will show ➔ wait for the animation and a delay time ➔ hide ➔ show the next kill until no more kills to show.\n \n2. <b>LIST:</b> Show all the kills on demand, showing in a list, a new kill will be added and showed in the screen as it happens.\n \nBy default the <i>QUEQE</i> mode is used but you can change this in the <b>GameData ➔ Local Kills Show Mode</b>.\n\nThis system is modular and event based hence you can use your own system and stop using this by simply remove/disactive the Center Local Notifier object from the <b>UI ➔ Interactable ➔ Local Notifications ➔ Center Local Notifier.</b>");
        Space(10);
        DrawText("<b><size=16>LEFT SIDE NOTIFICATIONS</size></b>\n \n- Normally used for non-important events e.g after pick up ammo, pick up a weapon, change weapon fire type, etc...\n\nThis notification system is also modular so you can remove or replace with you own system by simpling removing the Left Local Notifier from <i>UI ➔ MenuUI ➔ Local Notifications ➔ <b>Left Local Notifier</b></i>\n\nYou can invoke your own notifications with this code:");
        DrawCodeText("using MFPS.Runtime.UI;\n            ...\n            void ShowNotificationSample()\n            {\n                new MFPSLocalNotification(\"MY NOTIFICATION TEXT HERE\");\n            }");
    }

    void NetworkStats()
    {
        DrawText("MFPS network framework can broadcast application and lobby network statistics to clients. You can make use of this data to debug your game network. You can also brag about these statistics in your game to show how popular it is. :]\n \nThere are two types of network data that you can see on MFPS:\n \n<b><size=22>NETWORK TRANSPORT STATS</size></b>\n \nThis can be used to display some basic but useful information about the current network outgoing and incoming network packages along with the local player ping, the information is displayed in the top left corner of the screen.\n \nThis feature is turned off by default but you can easily enable it from the GameData -> Show Network Stats");
        DrawServerImage("img-25.png");
        DrawText("<b><size=22>LOBBY STATS</size></b>\n \nLobby statistics can be useful if you want to show the activity of your game. Lobby statistics are per region so you will only see the stats from the server region where you are connected.\n \nYou can get information about:\n \nNumber of live rooms\nTotal number of players joined to the lobby or joined to the lobby's rooms\n \nThis feature is enabled by default but you can turn it off in the MainMenu scene -> Lobby -> bl_Lobby -> Show Photon Statistics.");
        DrawServerImage("img-24.png");
    }

    void PlayerHitboxDoc()
    {
        if (subStep == 0)
        {
            DrawText("A hitbox is a basic shape collider that detects when something collides with the player, those colliders essentially are simple shapes like boxes, spheres, or capsules, in MFPS they can be found in each of the standard humanoids bones of the player models in player prefabs.\n \nThese hitboxes are automatically set up when you create a new player with '<b>Add Player Tutorial</b>' but there may be the case that these colliders don't shape well to the player model bones, the collider may be too big, too small, or off position, for these cases, a manually retouch is needed, you will have to manually adjust the collider bounds to the bone, you can do this by simply selecting the bone transform that contains the Collider component and adjusts the center and size properties from the inspector.\n \n<b>The goal is that each hitbox collider fits as much as possible to the model.</b>");
            DrawServerImage("img-33.png");
        }
        else if (subStep == 1)
        {
            DrawText("The main purpose of the player hitboxes is to <b>detect when a bullet hit the player</b>, in MFPS you can have a different damage base for each hitbox in order to cause more or less damage depending on which part of the body the bullet hit.\n \nFor this, you can use the <b>bl_HitboxManager</b> script which is attached in each player prefab and bots prefabs, in the player prefabs you can find attached the '<b>Remote</b>' child, when you select this child, in the inspector window of the <i>bl_HitboxManager</i> script, you will have different damage multipliers which you can modify to cause more or less damage in certain body parts, by default these multiplier values are applied per segment <i>(Head, Chest, Arms, and Legs)</i>:");
            DrawServerImage("img-34.png");
            DownArrow();
            DrawText("Optionally, if you want to assign a damage multiplier per hitbox instead of per segment, simply turn off the '<b>Multiply value per segment?</b>' toggle in the inspector ➔ foldout the hitbox by clicking in its name ➔ set the damage multiplier.");
            DrawServerImage("img-35.png");
        }
    }

    void PlayerDamageDoc()
    {
        if (subStep == 0)
        {
            DrawSuperText("Players by default can receive damage from weapons, fall damage, and vehicle collisions, this damage is received by the player hitboxes or in the bl_PlayerHealthManagerBase script.\n \nIf you want to deal damage to a player here is how you can do it:\n \n<?background=#D1D1D1FF><b><size=16>Deal damage to a local player</size></b></background>\n \n- Dealing damage to a local player can be done by calling the <b>DoDamage(...)</b> function of the <b>bl_PlayerHealthManagerBase</b> script which is attached at the root of the player instance\n \nThat function expects <b>DamageData</b> parameter which contains all the information regarding the damage that is given.\n \nWhat you need is a reference to the player that you want to apply the damage, how you get that player reference depends on how your damage is executed, e.g let's say you do it with a Raycast, then if your Raycast detects a local player > you get the <i>bl_PlayerHealthManagerBase</i> reference from that player > call the <b>DoDamage(...)</b>and pass the <b>DamageData</b>, e.g:");
            DrawCodeText("void DealDamageFunction()\n    {\n        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit raycast, 10))\n        {\n            if (raycast.collider.isLocalPlayerCollider())\n            {\n                var playerReferences = raycast.transform.GetComponent<bl_PlayerReferences>();\n \n                DamageData damageData = new DamageData()\n                {\n                    Damage = 20, // base damage to apply\n                    Direction = transform.position,\n                    Cause = DamageCause.Player,\n                    MFPSActor = bl_MFPS.LocalPlayer.MFPSActor, // MFPS actor that cause this damage\n                    // Check the other DamageData properties\n                };\n \n                playerReferences.playerHealthManager.DoDamage(damageData);\n            }\n        }\n    }");
            DrawText("And that will do it, the example above will deal 20 damage to the target player.\n \nIf you just want to instantly kill the local player then you can just call the <b>Suicide()</b> function in <b>bl_MFPS.LocalPlayer</b> like this:");
            DrawCodeText("bl_MFPS.LocalPlayer.Suicide();");
            DrawHorizontalSeparator();
            DrawSuperText("Now, the above will only work for the local player, if you want to deal damage to a remote player <i>(a player not controlled by the local client)</i> this is how you can do it\n\n<?background=#D1D1D1FF><b><size=16>Deal damage to a remote player</size></b></background>");
            DrawSuperText("You can only deal damage to a remote player through a hitbox reference of the player, these hitboxes are the colliders that wrap the player model, these have attached the <b>bl_HitBoxBase</b> inherited script which contains the function <b>ReceiveDamage(...)</b> which is what you have to call to give damage.\n \nWhat you need is a reference to the hitbox of the remote player that you want to apply the damage, how you get that reference depends on how you want to apply the damage, let's say you what to apply the damage by an explosion for which you detect the colliders in a certain radius of the explosion origin, this is how you can implement the damage function:");
            DrawCodeText("void DealDamageFunction()\n    {\n        Collider[] hittedColliders = Physics.OverlapSphere(transform.position, 10);\n        foreach (Collider collider in hittedColliders)\n        {\n            // if you want to apply the damage only to players\n            // if (!collider.CompareTag(bl_MFPS.HITBOX_TAG)) continue;\n \n            var damageable = collider.GetComponent<IMFPSDamageable>();\n            if (damageable == null) continue;\n \n            DamageData damageData = new DamageData()\n            {\n                Damage = 50,\n                Direction = transform.position,\n                MFPSActor = bl_MFPS.LocalPlayer.MFPSActor,\n                Cause = DamageCause.Explosion\n                // see the other DamageData properties that you can use.\n            };\n \n            // send the damage to the hit box.\n            damageable.ReceiveDamage(damageData);\n        }\n    }");
        }
        else if (subStep == 1)
        {
            DrawText("<b><size=16>Applying Damage to Objects</size></b>\n\nTo apply damage to a game object that isn't a player or bot, you simply need to implement the `<b>IMFPSDamageable</b>` interface in your custom script.\n\n<b>Example Scenario: Damaging a Barrel</b>\n\nSuppose you have a barrel that should take damage when hit by a bullet. You don't need to modify any of the MFPS Core scripts. Instead, follow these steps:\n\n1. <b>Create a Custom Script:</b>  • Implement the `<b>IMFPSDamageable</b>` interface in your new script.\n\n2. <b>Override the `<b>ReceiveDamage</b>` Function:</b>  • Within your custom script, override the `<b>ReceiveDamage(...)</b>` method to define how the barrel should handle damage.\n\n<b>Sample Script: (Not interface implemented yet)</b>");
            DrawCodeText("public class bl_Test : MonoBehaviour\n{\n    public int Health = 100;\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("You have to implement the <b>IMFPSDamageable</b> interface and override the interface function <b>ReceiveDamage()</b> like this:");
            DrawCodeText("public class bl_Test : MonoBehaviour, IMFPSDamageable\n{\n\n    public int Health = 100;\n \n    void IMFPSDamageable.ReceiveDamage(DamageData damageData)\n    {\n        ReduceHealth(damageData.Damage);\n    }\n \n    public void ReduceHealth(int damage)\n    {\n        Health -= damage;\n \n        if(Health <= 0)\n        {\n            // Destroy or wherever happens when run out of health\n        }\n    }\n}");
            DrawText("And that will do it, keep in mind that your object must have a collider in order to be hit by the bullets.");
        }
    }

    void CommonQADoc()
    {
        DrawSpoilerBox("Addons Manager, Store, and Updates windows are disabled?", "If you download MFPS from the Asset Store, you'll notice that some editor features and windows, such as the Addons Manager window, the Addons Store window, and the MFPS Update feature, are disabled. This is due to Asset Store guidelines. To comply with these guidelines, we have to disable these features. However, if you want to enable them <i>(though we do not encourage this)</i>, you can do so by deleting the <color=#FFCF92FF><b>ASCOMPLIANCE</b></color> object from the folder: Assets ➔ MFPS ➔ Scripts ➔ Internal ➔ Editor ➔ MFPS ➔ Resources ➔.");

        DrawSpoilerBox("Bots walk trough walls and objects", "You have to bake the <b>Navmesh</b> in your map scenes in order to let the bots know where they can navigate in your map.\n\nIf you don't know what Navmesh is or how to bake it, check this: <link=https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html>https://docs.unity3d.com/Manual/nav-BuildingNavMesh.html</link>");

        DrawSpoilerBox("Map objects randomly disappear in certain areas.", "This is caused you are using <b>Occlusion Culling</b> and you haven't Bake it in your map scene, for more info about Occlusion Culling, check this: <link=https://docs.unity3d.com/Manual/OcclusionCulling.html>https://docs.unity3d.com/Manual/OcclusionCulling.html</link>\n\nHow to bake it, check this:\n<link=https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html>https://docs.unity3d.com/Manual/occlusion-culling-getting-started.html</link>");

        DrawSpoilerBox("Is there a max limit of players per room?", "There's not a fixed number of players that can join in the same room at the same time, but since each player add an extra stress to the server and consume resources for the local client device after certain amount of players the game will start to feel 'Laggy' both in refresh rate (FPS) as in the network latency (Ping).\n\nThe number of players before the game start experimenting this performance issue depend on various factors like the runtime platform, device specs, network connection, etc...\n\nWe have done our own tests with the default MFPS (1.5) to have some benchmarks, these are the result:\n\nFor <b>PC</b> with these specs:\n<i>Intel i7 3.70GHz\n16Gb Ram\nNvidia GTX 1070</i>\n\n18 players in the same room\nMedium graphic quality\nRun with a average of 60-80 FPS\n\nFor <b>Mobile</b>:\nusing a Samsung S8 Plus\n\n12 Players in the same room\nWith MFPS optimized for mobile <i>(not the default scenes)</i>\nRun with a average of 60-75 FPS\n\nYou can use these statistics as a reference but keep in mind that there are many factors which can influence the result, so it is recommended that you do your own tests.");

        DrawSpoilerBox("Alternative network solution than Photon?", "Since MFPS comes with full source code you can make any change that you want, that includes integrate other network solution.\n\nBut by default this is not possible from a front-end option <i>(like a toggle to switch between libraries)</i>,\nintegration other network library will require a lot of code changes to switch from the Photon syntax to your network sdk syntax, and there is the possibility that some methods or features of Photon do not exist or are done differently in your network library.\n");

        DrawSpoilerBox("Can I host my own dedicated server?", "Yes, Photon offers a solution for host your own server using their <b>Photon OnPremise</b> <i>(a.k.a Photon Server)</i> switch to this from the default Photon PUN (Cloud) doesn't require code changes, all that you have to do is setup the Photon Server SDK and set the IP in your PhotonServerSettings.\n\nInfo: <link=https://doc.photonengine.com/en-us/server/current/getting-started/photon-server-in-5min>Photon Server Information</link>\n");

        DrawSpoilerBox("Rooms doesn't show up for other players?", "Sometimes when you testing your game with other players you may find out that the rooms that you or one of the testers created doesn't appear in the Room/Server list, the most common cause of this is that you and your tester <b>are not in the same server region</b>.\n\nIn the MainMenu/Lobby scene, in the bottom right corner you will see a drop-down with a region name, you can use it to select and change of server, so make sure you and your testers are connected to the same server region.");
    }

    void KnownIssuesDoc()
    {
        DrawText("On this page, we will list known issues with MFPS. The focus here is on issues that we can't fix or workaround at the moment but not just bugs or errors, but also <b>problems that affect the development workflow with MFPS</b>.");
        DownArrow();
        DrawText("<b><size=20>OTHER NETWORK SOLUTIONS</size></b>\n\nCurrently, MFPS only supports Photon PUN 2 as the network solution, and it's deeply integrated into the game code. Supporting other network solutions is not feasible at the moment because it would require rewriting the entire game code. Additionally, PUN has a unique implementation, so using a different network solution would involve a completely different approach.\n\nThere are no plans to support other network solutions in MFPS 2.0. However, MFPS 3.0 is planned to be a major upgrade, not just an update. The goal for MFPS 3.0 is to be more network-agnostic or, at the very least, make it easier to integrate different network solutions. The new version will likely use a more open-source network solution rather than relying on PUN.");
        Space(10);
        DrawText("\n<b><b><size=22>SUPPORT FOR BOTS IN ALL GAME MODES</size></b></b>\n\nA common question from MFPS users is: <b>Why aren't bots supported in all game modes?</b>\n\nWhile this might not be the answer you're hoping for, the reason is simple: it's a huge amount of work. Developing AI, even for basic bots like the ones in MFPS, is a challenging task—especially when integrated into a multiplayer environment. However, MFPS does include a multiplayer AI shooter system that supports two of the most popular game modes.\n\nThe challenge lies in the fact that each game mode requires custom bot behaviors to fit its specific rules. This makes things much more complex and time-consuming. As much as I'd love to add bot support to all modes, balancing development time and income is important for the continued progress of MFPS. Since AI development is one of the more time-intensive areas, additional bot support for other game modes will likely be available as a paid addon in the future.\n\nDoes this mean bots will never be supported in other game modes? Not at all—it’s just not planned at the moment. My focus right now is on features that are more impactful for the community, but additional bot support is something I’m keeping in mind for later.");
        Space(10);
        DrawText("\n<b><size=20>UPDATING MFPS</size></b>\n\nDue to the nature of this asset, updating MFPS when a new version is released can be a bit tricky. As a game template, it's meant to serve as the foundation for your own game, which means you'll likely need to make modifications, integrations, or removals to suit your specific game plan. These changes often involve editing the core code or content of the base template, which complicates the update process.\n\nOnce you've made core modifications, automatic updates are no longer possible. Instead, you'll need to manually merge the new changes into your project, which can be time-consuming and, if done incorrectly, may cause issues in your game. For more information and detailed instructions on how to manually merge updates, check the \"Update MFPS\" section.");
        Space(10);
        DrawText("\n<b><b><size=22>GRAMMAR</size></b></b>\n\nI hope you can bear with me! D:\nIt's pretty clear from the documentation, code, and comments that English grammar isn't my strongest skill. The reason is simple: I'm not a native English speaker. But that's not an excuse—many people aren't, and they do just fine! I know there are mistakes, but just for context: I’m from El Salvador, a small country in Central America that you probably won’t notice unless you zoom in on Google Maps. Unfortunately, there aren’t many opportunities here to refine language skills, but I do my best—so please don’t be too hard on me! :[\n\nIn the meantime, if you spot any grammar errors, feel free to let me know! Your feedback is always appreciated.");
    }

    [MenuItem("MFPS/Tutorials/Documentation", false, 111)]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(MFPSGeneralDoc));
    }

    [MenuItem("MFPS/Tools/Cache Methods")]
    private static void DeletePP()
    {
        bl_MFPS.GameData.CacheGameItemCollectors();
    }
}