using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace MFPS.GameModes.CaptureOfFlag
{
    /// <summary>
    /// Represents a flag point in the Capture the Flag game mode.
    /// </summary>
    public class bl_FlagPoint : bl_PhotonHelper
    {
        /// <summary>
        /// The team that owns the flag point.
        /// </summary>
        public Team flagTeam;

        /// <summary>
        /// The current state of the flag point.
        /// </summary>
        public FlagState State = FlagState.InHome;

        /// <summary>
        /// The icon representing the flag.
        /// </summary>
        public Texture2D FlagIcon;

        /// <summary>
        /// The target transform for displaying the flag icon.
        /// </summary>
        public Transform IconTarget;

        [SerializeField] private Transform homeMark = null;

        /// <summary>
        /// The size of the flag icon.
        /// </summary>
        public Vector2 IconSize = new Vector2(7, 7);

        /// <summary>
        /// The time it takes for the flag to return to its home position after being dropped.
        /// </summary>
        public float ReturnTime;

        private Vector3 originalPos, originalRot, originalScale;
        private bl_FlagPoint oppositeFlag;
        public bl_PlayerSettings carriyingPlayer/* { get; set; } = null*/;
        private Color IconColor;

        /// <summary>
        /// Initializes the flag point.
        /// </summary>
        void Awake()
        {
            originalPos = transform.position;
            originalRot = transform.eulerAngles;
            originalScale = transform.localScale;
            oppositeFlag = bl_CaptureOfFlag.Instance.GetFlag(bl_CaptureOfFlag.GetOppositeTeam(flagTeam));
            IconColor = flagTeam.GetTeamColor();
            if (homeMark != null) homeMark.parent = transform.parent;
            bl_EventHandler.onLocalPlayerDeath += this.OnLocalPlayerDeath;
        }

        /// <summary>
        /// Cleans up the flag point.
        /// </summary>
        private void OnDestroy()
        {
            bl_EventHandler.onLocalPlayerDeath -= this.OnLocalPlayerDeath;
        }

        /// <summary>
        /// Changes the state of the flag point.
        /// </summary>
        /// <param name="newState">The new state of the flag point.</param>
        public void ChangeFlagState(FlagState newState)
        {
            if (State == newState) return;

            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 0);
            data.Add("state", newState);
            data.Add("team", flagTeam);
            data.Add("player", bl_PhotonNetwork.LocalPlayer);
            data.Add("viewID", bl_MFPS.LocalPlayer.ViewID);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        /// <summary>
        /// Handles the logic when a collider enters the flag point.
        /// </summary>
        /// <param name="collider">The collider that entered the flag point.</param>
        void OnTriggerEnter(Collider collider)
        {
            if (collider.isLocalPlayerCollider())
            {
                var player = collider.gameObject.GetComponent<bl_PlayerSettings>();
                if (CanBePickedUpBy(player) == true)
                {
                    ChangeFlagState(FlagState.PickUp);
                }
            }
        }

        /// <summary>
        /// Handles the logic when the local player dies.
        /// </summary>
        void OnLocalPlayerDeath()
        {
            if (carriyingPlayer == null)
            {
                return;
            }

            var local = bl_MFPS.LocalPlayerReferences;
            if (local == null) return;

            if (carriyingPlayer.View.ViewID == local.ViewID)
            {
                // Drop the flag
                var data = bl_UtilityHelper.CreatePhotonHashTable();
                data.Add("cmd", 1);
                data.Add("team", flagTeam);
                data.Add("pos", transform.position);
                bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
            }
        }

        /// <summary>
        /// Handles the logic for flag capture.
        /// </summary>
        public void HandleFlagCapture()
        {
            if (carriyingPlayer == null)
                return;

            if (carriyingPlayer.View.ViewID != bl_MFPS.LocalPlayer.ViewID)
            {
                return;
            }

            if (oppositeFlag.IsHome() == true && bl_UtilityHelper.Distance(transform.position, oppositeFlag.transform.position) <= bl_CaptureOfFlag.Instance.captureAreaRange)
            {
                ChangeFlagState(FlagState.Captured);
                // change the state locally instantly to prevent detect the capture multiple times.
                State = FlagState.Captured;
            }
        }

        /// <summary>
        /// Determines whether the flag point is at its home base.
        /// </summary>
        /// <returns><c>true</c> if the flag point is at its home base; otherwise, <c>false</c>.</returns>
        public bool IsHome()
        {
            return transform.position == originalPos;
        }

        /// <summary>
        /// Called on all clients when a player drops the flag.
        /// </summary>
        /// <param name="data">The data associated with the drop flag event.</param>
        public void DropFlag(Hashtable data)
        {
            var position = (Vector3)data["pos"];

            State = FlagState.Dropped;
            carriyingPlayer = null;
            transform.parent = null;
            transform.position = position;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;

            if (bl_PhotonNetwork.IsMasterClient)
            {
                Invoke(nameof(ReturnInvoke), ReturnTime);
            }
        }

        /// <summary>
        /// Called on all clients when a player successfully captures the flag.
        /// </summary>
        /// <param name="actor">The player who captured the flag.</param>
        public void OnCapture(Player actor)
        {
            carriyingPlayer = null;
            SetFlagToOrigin();

            //Only the player who captures the flag, updates the properties
            if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.LocalPlayer.NickName, bl_GameTexts.CaptureTheFlag, actor.GetPlayerTeam());
                bl_MFPS.RoomGameMode.SetPointToGameMode(1, GameMode.CTF);
                //Add Point for personal score
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_GameData.ScoreSettings.ScoreForFlagCapture);
                bl_CaptureOfFlag.Instance.onCapture?.Invoke();
                bl_EventHandler.DispatchGameplayPlayerEvent("flag-captured");
            }
        }

        /// <summary>
        /// Called on all clients when a player recovers the flag.
        /// </summary>
        /// <param name="actor">The player who recovered the flag.</param>
        public void Recover(Player actor)
        {
            SetFlagToOrigin();

            if (actor.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber)
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_GameData.ScoreSettings.ScoreForFlagRecover);
                bl_CaptureOfFlag.Instance.onRecover?.Invoke();
                bl_EventHandler.DispatchGameplayPlayerEvent("flag-reovered");
            }
        }

        /// <summary>
        /// Called on all clients when a player picks up this flag.
        /// </summary>
        /// <param name="actor">The player who picked up the flag.</param>
        /// <param name="viewID">The view ID of the player.</param>
        public void OnPickup(Player actor, int viewID)
        {
            Transform actorTransform = bl_GameManager.Instance.FindActor(viewID);
            if (actorTransform == null)
            {
                return;
            }

            var logic = actorTransform.GetComponent<bl_PlayerSettings>();
            if (!CanBePickedUpBy(logic))
            {
                return;
            }

            if (logic.PlayerTeam == flagTeam)
            {
                if (IsHome() == false)
                {
                    // the flag is dropped and taken by a team member = recovering the flag.
                    if (bl_PhotonNetwork.IsMasterClient) ChangeFlagState(FlagState.InHome);
                }
            }
            else
            {
                SetFlagToCarrier(logic);
            }

            //show capture notification
            if (bl_PhotonNetwork.LocalPlayer.ActorNumber == actor.ActorNumber)
            {
                Team enemyTeam = bl_CaptureOfFlag.GetOppositeTeam(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                string obtainedText = string.Format(bl_GameTexts.ObtainedFlag, enemyTeam.GetTeamName());
                bl_KillFeedBase.Instance.SendTeamHighlightMessage(bl_PhotonNetwork.LocalPlayer.NickName, obtainedText, bl_PhotonNetwork.LocalPlayer.GetPlayerTeam());
                bl_CaptureOfFlag.Instance.onPickUp?.Invoke();
            }
        }

        /// <summary>
        /// Sets the flag to the carrier.
        /// </summary>
        /// <param name="carrier">The player who carries the flag.</param>
        public void SetFlagToCarrier(bl_PlayerSettings carrier)
        {
            carriyingPlayer = carrier;
            transform.parent = carrier.FlagPosition;
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
            if (bl_CaptureOfFlag.Instance.moveFlagWithCarrierMotion && carrier.carrierPoint != null)
                transform.parent = carrier.carrierPoint;
        }

        /// <summary>
        /// Invokes the return of the flag to its home position.
        /// </summary>
        void ReturnInvoke()
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", 2);
            data.Add("team", flagTeam);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.CaptureOfFlagMode, data);
        }

        /// <summary>
        /// Determines whether the flag can be picked up by the player.
        /// </summary>
        /// <param name="logic">The player settings of the player.</param>
        /// <returns><c>true</c> if the flag can be picked up by the player; otherwise, <c>false</c>.</returns>
        public bool CanBePickedUpBy(bl_PlayerSettings logic)
        {
            //If the flag is at its home position, only the enemy team can grab it
            if (IsHome() == true)
            {
                return logic.PlayerTeam != flagTeam;
            }

            //If another player is already carrying the flag, no one else can grab it
            return carriyingPlayer == null;
        }

        /// <summary>
        /// Sets the flag to its original position.
        /// </summary>
        public void SetFlagToOrigin()
        {
            transform.parent = null;
            transform.position = originalPos;
            transform.eulerAngles = originalRot;
            transform.localScale = originalScale;
            State = FlagState.InHome;
        }

        #region GUI
        void OnGUI()
        {
            if (carriyingPlayer != null && carriyingPlayer.View.ViewID == bl_MFPS.LocalPlayer.ViewID) return;

            GUI.color = IconColor;
            if (bl_CameraIdentity.CurrentCamera)
            {
                Vector3 vector = bl_CameraIdentity.CurrentCamera.WorldToScreenPoint(this.IconTarget.position);
                if (vector.z > 0)
                {
                    GUI.DrawTexture(new Rect(vector.x - 5, Screen.height - vector.y - 7, 13 + IconSize.x, 13 + IconSize.y), this.FlagIcon);
                }
            }
            GUI.color = Color.white;
        }

        private SphereCollider SpheCollider;
        private void OnDrawGizmos()
        {
            if (SpheCollider != null)
            {
                Vector3 v = SpheCollider.bounds.center;
                v.y = transform.position.y;
                bl_GizmosUtility.DrawWireArc(v, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
            }
            else
            {
                SpheCollider = GetComponent<SphereCollider>();
            }
        }
        #endregion

        [System.Serializable]
        public enum FlagState
        {
            InHome = 0,
            PickUp = 1,
            Captured = 2,
            Dropped = 3,
        }
    }
}