using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using MFPS.GameModes.FreeForAll;

public class bl_FreeForAll : bl_GameModeBase
{
    [HideInInspector] public List<MFPSPlayer> FFAPlayerSort = new List<MFPSPlayer>();

    /// <summary>
    /// 
    /// </summary>
    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PropertiesKeys.KillsKey))
        {
            CheckScore();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckScore()
    {
        if (!bl_RoomSettings.Instance.RoomInfoFetched) return;

        FFAPlayerSort.Clear();
        FFAPlayerSort.AddRange(bl_GameManager.Instance.OthersActorsInScene);
        FFAPlayerSort.Add(bl_GameManager.Instance.LocalActor);

        MFPSPlayer player = null;
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            FFAPlayerSort.Sort(bl_UtilityHelper.GetSortPlayerByKills);
            player = FFAPlayerSort[0];
        }
        else
        {
            player = bl_GameManager.Instance.LocalActor;
        }
        bl_FreeForAllUI.Instance.SetScores(player);
        //check if the best player reach the max kills
        if ((int)player.GetPlayerPropertie(PropertiesKeys.KillsKey) >= bl_RoomSettings.Instance.GameGoal && !bl_PhotonNetwork.OfflineMode)
        {
            // bl_MatchTimeManagerBase.Instance.FinishRound();
            FinishRound(FinishRoundCause.ScoreReached);
            return;
        }
        //check if bots have not reach max kills
        if (bl_AIMananger.Instance != null && bl_AIMananger.Instance.BotsActive && bl_AIMananger.Instance.BotsStatistics.Count > 0)
        {
            if (bl_AIMananger.Instance.GetBotWithMoreKills().Kills >= bl_RoomSettings.Instance.GameGoal)
            {
                //bl_MatchTimeManagerBase.Instance.FinishRound();
                FinishRound(FinishRoundCause.ScoreReached);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer GetBestPlayer()
    {
        if (FFAPlayerSort.Count > 0 && FFAPlayerSort != null)
        {
            return FFAPlayerSort[0];
        }
        else
        {
            return bl_GameManager.Instance.LocalActor;
        }
    }

    #region Interface
    public override bool IsLocalPlayerWinner()
    {
        string winner = GetBestPlayer().Name;
        if (bl_AIMananger.Instance != null && bl_AIMananger.Instance.GetBotWithMoreKills().Kills >= bl_RoomSettings.Instance.GameGoal)
        {
            winner = bl_AIMananger.Instance.GetBotWithMoreKills().Name;
        }
        return winner == bl_PhotonNetwork.LocalPlayer.NickName;
    }

    public override void Initialize()
    {
        //check if this is the game mode of this room
        if (IsThisModeActive(GameMode.FFA))
        {
            bl_GameManager.Instance.SetGameState(MatchState.Starting);         
            bl_UIReferences.SafeUIInvoke(() => {
                bl_FreeForAllUI.Instance.ShowUp();
            });
        }
        else
        {           
            bl_UIReferences.SafeUIInvoke(() => {
                bl_FreeForAllUI.Instance.Hide();
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override MatchResult GetMatchResult()
    {
        if (IsLocalPlayerWinner()) return MatchResult.Victory;
        return MatchResult.Defeat;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override MatchOverInformation GetMatchOverInformation()
    {
        var info = base.GetMatchOverInformation();
        info.LocalPlayerScore = bl_PhotonNetwork.LocalPlayer.GetKills();
        info.WinnerName = GetBestPlayer().Name;
        return info;
    }

    public override void OnLocalPoint(int points, Team teamToAddPoint)
    {
        CheckScore();
    }
    #endregion
}