using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_MedicalKit : bl_NetworkItem
    {
        [LovattoToogle] public bool autoRespawn = false;
        [Range(0, 100)]
        public int health = 25;

        /// <summary>
        /// 
        /// </summary>
        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            var pdm = m_other.transform.root.GetComponent<bl_PlayerHealthManagerBase>();
            if (pdm == null) { return; }

            //don't pickup if the player has max health
            if (pdm.GetHealth() >= pdm.GetMaxHealth()) return;

            bl_EventHandler.Gameplay.onPickUpHealth?.Invoke(health);

            //should this health kit respawn after certain time?
            if (autoRespawn)
            {
                bl_ItemManagerBase.Instance.RespawnAfter(this);
            }
            else
            {
                DestroySync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnBeforeDestroy(int byActorId, Team byTeam)
        {
            // if the local player did not throw this kit or the local player was who picked it up, return
            if (OwnerActorID != bl_PhotonNetwork.LocalPlayer.ActorNumber || byActorId == bl_PhotonNetwork.LocalPlayer.ActorNumber) return;

            if (byTeam == bl_MFPS.LocalPlayer.Team)
            {
                // if the player who picked up the kit is in the same team, give experience points to the player who threw it (the local player)
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_GameData.ScoreSettings.ScoreForHealthSupplying);
                bl_EventHandler.Player.onLocalSupplyingTeammate?.Invoke("health");
            }
        }
    }
}