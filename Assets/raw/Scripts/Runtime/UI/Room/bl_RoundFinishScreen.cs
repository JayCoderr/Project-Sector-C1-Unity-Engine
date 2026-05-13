using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI.Layout
{
    public class bl_RoundFinishScreen : bl_RoundFinishScreenBase
    {
        public GameObject content;
        [SerializeField] private TextMeshProUGUI winnerNameText = null;
        [SerializeField] private TextMeshProUGUI roundResultText = null;
        [SerializeField] private TextMeshProUGUI finishCauseText = null;
        [SerializeField] private TextMeshProUGUI countdownText = null;
        [SerializeField] private GameObject winnerUI = null;
        [SerializeField] private GameObject teamsScoreUI = null;
        [SerializeField] private GameObject soloScoreUI = null;
        [SerializeField] private TextMeshProUGUI[] teamsScoreText = null;
        [SerializeField] private TextMeshProUGUI soloScoreText = null;

        /// <summary>
        /// Show the final round UI
        /// </summary>
        public override void Show(bl_GameModeBase.MatchOverInformation matchOverInformation)
        {
            content.SetActive(true);

            roundResultText.text = matchOverInformation.LocalResultTitle.ToUpper();
            if (string.IsNullOrEmpty(matchOverInformation.FinishReason))
            {

            }
            else
            {
                finishCauseText.text = matchOverInformation.FinishReason.ToUpper();
            }

            if (winnerUI != null)
            {
                winnerUI.SetActive(!string.IsNullOrEmpty(matchOverInformation.WinnerName));
                winnerNameText.text = matchOverInformation.WinnerName;
            }

            if (!matchOverInformation.DisplayScores)
            {
                soloScoreUI.SetActive(false);
                teamsScoreUI.SetActive(false);
                return;
            }

            if (bl_MFPS.CurrentGameModeLogic.isOneTeamMode)
            {
                soloScoreUI.SetActive(true);
                teamsScoreUI.SetActive(false);
                soloScoreText.text = matchOverInformation.LocalPlayerScore.ToString();
            }
            else
            {
                soloScoreUI.SetActive(false);
                teamsScoreUI.SetActive(true);
                teamsScoreText[0].text = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1).ToString();
                teamsScoreText[1].text = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2).ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public override void SetCountdown(int count)
        {
            count = Mathf.Clamp(count, 0, int.MaxValue);
            if (countdownText != null) countdownText.text = count.ToString();
        }
    }
}