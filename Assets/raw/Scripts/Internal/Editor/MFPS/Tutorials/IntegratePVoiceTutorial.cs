using MFPSEditor;
using UnityEditor;
using UnityEngine;

public class IntegratePVoiceTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfps2/editor/voice/";
    private readonly NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new() {Name = "https://assetstorev1-prd-cdn.unity3d.com/key-image/2b5edb30-8595-48e4-9dd9-1913f423b7bd.png", Type = NetworkImages.ImageType.Custom},
        new() {Name = "img-1.jpg", Image = null},
        new() {Name = "img-2.jpg", Image = null},

    };
    private readonly Steps[] AllSteps = new Steps[] {
     new() { Name = "Photon Voice", StepsLenght = 3 },
    };
    //final required////////////////////////////////////////////////

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawTutorial();
        }
    }

    void DrawTutorial()
    {
        if (subStep == 0)
        {
            DrawImage(GetServerImage(0), TextAlignment.Center);
            DrawText("MFPS comes with support for the Photon Voice plugin, which is another Photon Cloud service specifically for Voice chat in multiplayer games, allowing the players to talk with teammates in-realtime inside the game.\n \nThis feature is NOT supported on all platforms, these are the platforms where you can use Photon Voice:\n\n■ Windows\n■ UWP\n■ macOS\n■ Linux\n■ Android (for 64-bit support read this)\n■ iOS\n■ PlayStation 4 (requires a special add-on)\n■ PlayStation 5 (requires a special add-on)\n■ Nintendo Switch (requires a special add-on)\n■ Xbox One (requires a special add-on)\n■ Xbox Series X and Xbox Series S (requires a special add-on)");
            DrawText("In order to use this feature, you need to import the Photon Voice 2 package, you can get it for free on the Asset Store, click on the button below to redirect to the package page:");
            GUILayout.Space(5);
            if (DrawButton("Open Photon Voice 2 On Browser"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518");
                NextStep();
            }
        }
        else if (subStep == 1)
        {
            DrawText("Now download and import the package from the asset store page and wait until process finish.");
            DownArrow();
            DrawText("Then, you need enable the integrated code, for it Go to (Toolbar) MFPS -> Addons -> Voice -> <b>Enable</b> and wait until script compilation finish.");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("After compilation finish do the same but click on the 'Integrate' button MFPS -> Addons -> Voice -> <b>Integrate</b>");
            DownArrow();
            DrawText("Ok, it's all, now Photon Voice is integrated");
        }
        else if (subStep == 2)
        {
            DrawText("By default, voice chat is set to <b>Push-to-Talk</b>. The default key is <b>`P`</b> — hold it down to transmit your voice.\n \nYou can change this key at any time in the <b>Keybindings</b> section of the game settings menu. If you prefer a different default key mapping, you can also adjust it directly in the <b>Input Manager</b>.\n \nIf you don't want to use Push-to-Talk, you can switch the voice chat mode to <b>Voice Detection</b> in the game settings menu.");
        }
    }

    [MenuItem("MFPS/Tutorials/Photon Voice")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(IntegratePVoiceTutorial));
    }
}