using MFPS.GameModes.TeamDeathMatch;

public class bl_TeamDeathMatch : bl_GameModeBase
{
    #region Interface
    /// <summary>
    /// 
    /// </summary>
    public override void Initialize()
    {
        //check if this is the game mode of this room
        if (IsThisModeActive(GameMode.TDM))
        {
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
            bl_UIReferences.SafeUIInvoke(() => { bl_TeamDeathMatchUI.Instance.ShowUp(); });
        }
        else
        {
            bl_UIReferences.SafeUIInvoke(() => { bl_TeamDeathMatchUI.Instance.Hide(); });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnLocalPlayerKill()
    {
        // In TDM, the team score is updated when a enemy player is killed
        bl_PhotonNetwork.CurrentRoom.SetTeamScore(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// This is called if the game mode implements bl_MFPS.RoomGameMode.SetPointToGameMode()
    /// to add points to the game mode
    /// </summary>
    /// <param name="points"></param>
    /// <param name="team"></param>
    public override void OnLocalPoint(int points, Team team)
    {
        bl_PhotonNetwork.CurrentRoom.SetTeamScore(team);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertiesThatChanged"></param>
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score) || propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
            int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
            bl_TeamDeathMatchUI.Instance.SetScores(team1, team2);

            // check if any team reached the score limit
            if (CheckIfTeamReachedGoal(out Team _))
            {
                FinishRound(FinishRoundCause.ScoreReached);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsLocalPlayerWinner()
    {
        return GetWinningTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override MatchResult GetMatchResult()
    {
        var winningTeam = GetWinningTeam();
        return winningTeam == Team.None ? MatchResult.Draw : winningTeam == PlayerTeam ? MatchResult.Victory : MatchResult.Defeat;
    }
    #endregion
}