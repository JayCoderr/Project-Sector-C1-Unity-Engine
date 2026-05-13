using MFPS.Runtime.AI;
using TMPro;
using UnityEngine;

/// <summary>
/// Default MFPS after match resume screen
/// If you want to use your custom resume screen, simply inherit your custom script from <see cref="bl_MatchFinishResumeBase"/>
/// and handle the inherited functions using this default script as reference.
/// </summary>
public class bl_GameFinish : bl_MatchFinishResumeBase
{

    [SerializeField] private TextMeshProUGUI PlayerNameText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private TextMeshProUGUI KDRText = null;
    [SerializeField] private TextMeshProUGUI TimePlayedText = null;
    [SerializeField] private TextMeshProUGUI WinScoreText = null;
    [SerializeField] private TextMeshProUGUI HeadshotsText = null;
    [SerializeField] private TextMeshProUGUI TotalScoreText = null;
    [SerializeField] private TextMeshProUGUI CoinsText = null;
    [SerializeField] private TextMeshProUGUI assistsText = null;
    [SerializeField] private GameObject Content = null;

    /// <summary>
    /// 
    /// </summary>
    public override void CollectMatchData()
    {
        int kills = bl_PhotonNetwork.LocalPlayer.GetKills();

        // if bots eliminations doesn't count for the player stats
        if (bl_GameData.CoreSettings.howConsiderBotsEliminations != BotKillConsideration.SameAsRealPlayers)
        {
            if (bl_RoomSettings.TryGetMatchPersistData("bot-kills", out var value))
            {
                int bk = value is int ? (int)value : 0;
                kills = Mathf.Min(0, kills - bk);
            }
        }

        int deaths = bl_PhotonNetwork.LocalPlayer.GetDeaths();
        int score = bl_PhotonNetwork.LocalPlayer.GetPlayerScore();
        int assists = bl_PhotonNetwork.LocalPlayer.GetAssists();
        float kd = bl_MathUtility.GetKDRatio(kills, deaths);

        // calculated the time played in minutes
        float secondsPlayed = bl_GameManager.Instance.PlayedTime;
        int minutesPlayed = Mathf.RoundToInt(secondsPlayed / 60);

        int scorePerTime = bl_GameData.ScoreSettings.GetScorePerTimePlayed((int)secondsPlayed);
        int headshotsScore = bl_GameManager.Headshots * bl_GameData.ScoreSettings.ScorePerHeadShot;
        bool winner = bl_GameManager.Instance.IsLocalPlayerWinner();
        int matchResultScore = (winner) ? bl_GameData.ScoreSettings.ScoreForWinMatch : bl_GameData.ScoreSettings.ScoreForLostMatch;

        // The match total score is the sum of the player score, the score for the time played and the score for the win match
        int totalScore = score + matchResultScore + scorePerTime;

        totalScore = bl_Hook.ApplyFilters("Filter_MatchTotalScore", totalScore);

        int coins = 0;
        if (totalScore > 0 && bl_GameData.ScoreSettings.CoinScoreValue > 0 && totalScore > bl_GameData.ScoreSettings.CoinScoreValue)
        {
            coins = totalScore / bl_GameData.ScoreSettings.CoinScoreValue;
        }

        coins = bl_Hook.ApplyFilters("Filter_MatchTotalCoins", coins);

        PlayerNameText.text = bl_PhotonNetwork.NickName;
        KillsText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Kills.Localized(126).ToUpper(), kills);
        DeathsText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Deaths.Localized(58, true).ToUpper(), deaths);
        if (assistsText != null) assistsText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Assists.Localized(249, true).ToUpper(), assists);
        ScoreText.text = string.Format("{0}: <b>{1}</b>", bl_GameTexts.Score.Localized(59).ToUpper(), score);
        WinScoreText.text = string.Format(bl_GameTexts.WinMatch.Localized(61), matchResultScore);
        KDRText.text = string.Format("{0}\n<size=10>KDR</size>", kd);
        TimePlayedText.text = string.Format("{0} <b>{1}</b> +{2}", bl_GameTexts.TimePlayed.Localized(60).ToUpper(), bl_StringUtility.GetTimeFormat(minutesPlayed, secondsPlayed), scorePerTime);
        HeadshotsText.text = string.Format("{0} <b>{1}</b> +{2}", bl_GameTexts.HeadShot.Localized(16, true).ToUpper(), bl_GameManager.Headshots, headshotsScore);
        TotalScoreText.text = string.Format("{0}\n<size=9>{1}</size>", totalScore, bl_GameTexts.TotalScore.Localized(35).ToUpper());
        CoinsText.text = string.Format("+{0}\n<size=9>{1}</size>", coins, bl_GameTexts.Coins.Localized(162));

        // save match data in database
        SaveMatchInDataBase(coins, totalScore);

        bl_WaitingRoom.SetWaitingState(bl_WaitingRoom.WaitingState.Waiting);

        bl_EventHandler.Match.onSaveMatchData?.Invoke(totalScore);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active)
    {
        Content.SetActive(active);
        if (active) Invoke(nameof(GoToLobby), 60);//maximum time out to leave.
    }
}