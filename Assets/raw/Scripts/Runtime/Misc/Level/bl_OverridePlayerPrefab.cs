using UnityEngine;

/// <summary>
/// Use this script to override the default player prefabs set in GameData -> PlayerTeam1 and PlayerTeam2
/// Attach this script in any game object of the map scene where you wanna override the player prefabs
/// </summary>
public class bl_OverridePlayerPrefab : MonoBehaviour
{
    public bl_PlayerNetworkBase Team1Player, Team2Player;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        //  bl_GameManager.Instance.OverridePlayerPrefab = this;
        bl_Hook.AddFilter("Filter_PlayerPrefabToInstance", input =>
        {
            var args = (object[])input;

            Team team = (Team)args[1];
            args[0] = GetPlayerForTeam(team);

            return args;
        });
    }

    /// <summary>
    /// 
    /// </summary>
    public GameObject GetPlayerForTeam(Team team)
    {
        var player = Team1Player.gameObject;
        if (team == Team.Team2)
        {
            player = Team2Player.gameObject;
            if (player == null) player = bl_GameData.Instance.Player2.gameObject;
        }
        else
        {
            if (player == null) player = bl_GameData.Instance.Player1.gameObject;
        }

        return player;
    }
}