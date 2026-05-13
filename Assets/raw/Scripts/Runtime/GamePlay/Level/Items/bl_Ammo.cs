using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_Ammo : bl_NetworkItem
    {
        public WeaponSupplyingMode supplyMode = WeaponSupplyingMode.ForAllWeapons;
        [LovattoToogle] public bool autoRespawn = false;
        [GunID] public int ForGun = 0;

        public AmmoType forAmmoType = AmmoType.HeavyAmmo;
        public int Bullets = 30;
        public int Projectiles = 2;
        public AudioClip PickSound;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_other"></param>
        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.isLocalPlayerCollider()) return;

            if (bl_EventHandler.Gameplay.onAmmoPickUp(new bl_EventHandler.Gameplay.AmmoPickUpData()
            {
                SupplyMode = supplyMode,
                AmmoType = forAmmoType,
                Bullets = Bullets,
                Projectiles = Projectiles,
                GunID = ForGun
            }))
            {
                if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);
                //should this ammo reaper after certain time?
                if (autoRespawn)
                {
                    bl_ItemManagerBase.Instance.RespawnAfter(this);
                }
                else
                {
                    DestroySync();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnBeforeDestroy(int byActorId, Team byTeam)
        {
            // if the local player did not throw this ammo or the local player was who picked it up, return
            if (OwnerActorID != bl_PhotonNetwork.LocalPlayer.ActorNumber || byActorId == bl_PhotonNetwork.LocalPlayer.ActorNumber) return;

            if (byTeam == bl_MFPS.LocalPlayer.Team)
            {
                // if the player who picked up the ammo is in the same team, give experience points to the player who threw it (the local player)
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_GameData.ScoreSettings.ScoreForAmmoSupplying);
                bl_EventHandler.Player.onLocalSupplyingTeammate?.Invoke("ammo");
            }
        }
    }
}