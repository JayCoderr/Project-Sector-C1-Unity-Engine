using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the display of match introduction information, such as map name, game mode, team name, and a fake date, in
/// the user interface.
/// </summary>
/// <remarks>This class is responsible for showing introductory details about the current match when the local
/// player spawns. It handles UI updates for elements like the map name, game mode, team name, and a predefined fake
/// date. The display includes fade-in and fade-out animations, with configurable delays and durations. The class also
/// ensures that the introduction is displayed only once per local player spawn.</remarks>
public class bl_MatchIntroText : MonoBehaviour
{
    [Header("Settings")]
    public float Delay = 1.2f;
    public float VisibleTime = 3.5f;
    public float FadeDuration = 1;
    public string fakeDate = "DAY 21 10:25:36";
    [Header("References")]
    public CanvasGroup RootAlpha;
    public TextMeshProUGUI MapNameText;
    public TextMeshProUGUI DateText;
    public TextMeshProUGUI GameModeText;
    public TextMeshProUGUI TeamText;
    private bool isLocalPlayerSpawned = false;

    private void Awake()
    {
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
    }

    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
    }

    private void OnLocalSpawn()
    {
        if (isLocalPlayerSpawned) { return; }

        DisplayInfo();
        isLocalPlayerSpawned = true;
    }

    /// <summary>
    /// Displays the current room and player information in the user interface.
    /// </summary>
    /// <remarks>This method retrieves and displays details such as the map name, game mode, team name,  and a
    /// fake date in the associated UI elements. It also initiates a coroutine to handle  additional display logic
    /// asynchronously.</remarks>
    public void DisplayInfo()
    {
        MFPSRoomInfo props = PhotonNetwork.CurrentRoom.GetRoomInfo();
        MapNameText.text = props.GetMapInfo().ShowName.ToUpper();
        DateText.text = fakeDate;
        GameModeText.text = props.gameMode.GetName().ToUpper();
        TeamText.text = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam().GetTeamName().ToUpper();
        StartCoroutine(DoDisplay());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoDisplay()
    {
        yield return new WaitForSeconds(Delay);
        RootAlpha.gameObject.SetActive(true);
        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime / FadeDuration;
            RootAlpha.alpha = d;
            yield return null;
        }
        yield return new WaitForSeconds(VisibleTime);
        while (d > 0)
        {
            d -= Time.deltaTime / FadeDuration;
            RootAlpha.alpha = d;
            yield return null;
        }
        RootAlpha.gameObject.SetActive(false);
    }
}