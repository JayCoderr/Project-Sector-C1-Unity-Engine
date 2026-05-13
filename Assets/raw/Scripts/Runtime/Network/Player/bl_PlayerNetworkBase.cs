using System;
using UnityEngine;

public abstract class bl_PlayerNetworkBase : bl_MonoBehaviour
{
    /// <summary>
    /// the current state of the local player in FPV
    /// </summary>
    public PlayerFPState FPState = PlayerFPState.Idle;

    /// <summary>
    /// The current synced stated of this player.
    /// </summary>
    public PlayerState NetworkBodyState
    { get; set; }

    /// <summary>
    /// The current TPWeapon GunID
    /// </summary>
    public int NetworkGunID
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public struct PlayerCommandData
    {
        public int CommandID;
        public string Arg;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        public readonly bool GetBool(int argIndex)
        {
            var args = GetSplitArgs();
            if (args == null || args.Length <= argIndex) return false;

            return args[argIndex] == "1";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        public readonly int GetInt(int argIndex)
        {
            var args = GetSplitArgs();
            if (args == null || args.Length <= argIndex) return -1;

            int.TryParse(args[argIndex], out int value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        public readonly float GetFloat(int argIndex)
        {
            var args = GetSplitArgs();
            if (args == null || args.Length <= argIndex) return -1;

            float.TryParse(args[argIndex], out float value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argIndex"></param>
        /// <returns></returns>
        public readonly string GetString(int argIndex)
        {
            var args = GetSplitArgs();
            if (args == null || args.Length <= argIndex) return string.Empty;

            return args[argIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Set(bool value)
        {
            string sv = value ? "1" : "0";
            Arg += sv + "|";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            Arg += value.ToString() + "|";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private readonly string[] GetSplitArgs()
        {
            if (string.IsNullOrEmpty(Arg)) return null;

            return Arg.Split('|');
        }
    }

    /// <summary>
    /// Event called when the player receive a command
    /// This is invoked on all clients at the same time.
    /// </summary>
    public Action<PlayerCommandData> onPlayerCommand;

    /// <summary>
    /// This function control which TP Weapon should be showing on remote players.
    /// </summary>
    /// <param name="local">Should take the current gun from the local player or from the network data?</param>
    /// <param name="force">Double-check even if the equipped weapon has not changed.</param>
    public abstract void CurrentTPVGun(bool local = false, bool force = false);

    /// <summary>
    /// Equipped the given weapon (world view weapon)
    /// </summary>
    /// <param name="weaponType"></param>
    /// <param name="networkGun"></param>
    public abstract void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun);

    /// <summary>
    /// Replicate command with all other players
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="arg"></param>
    /// <param name="calledFromLocal"></param>
    public abstract void ReplicatePlayerCommand(int commandId, string arg, bool calledFromLocal = true);

    /// <summary>
    /// Send a call to all other clients to sync a bullet
    /// </summary>
    public abstract void ReplicateFire(GunType weaponType, Vector3 hitPosition, Vector3 inacuracity, bool calledFromLocal = true, FireReplicationFlag flag = FireReplicationFlag.None);

    /// <summary>
    /// Sync a player animation command
    /// </summary>
    /// <param name="command"></param>
    public abstract void ReplicatePlayerAnimationCommand(PlayerAnimationCommands command, string arg = "", bool calledFromLocal = true);

    /// <summary>
    /// Sync grenade throws
    /// </summary>
    /// <param name="inacuracity"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="direction"></param>
    public abstract void ReplicateGrenadeThrow(float inacuracity, Vector3 origin, Quaternion rot, Vector3 direction, string id, bool calledFromLocal = true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectileData"></param>
    public abstract void ReplicateCustomProjectile(CustomProjectileData projectileData, bool calledFromLocal = true);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toGunId"></param>
    /// <param name="timeToShow"></param>
    public virtual async void TemporarySwitchTPWeapon(int toGunId, int timeToShow) { await System.Threading.Tasks.Task.CompletedTask; }

    /// <summary>
    /// Block/Unblock all the weapons.
    /// </summary>
    /// <param name="blockState"></param>
    public virtual void SetWeaponBlocked(int blockState, bool calledFromLocal = true) { }

    /// <summary>
    /// Get the current equipped network weapon, which is the world view weapon model.
    /// </summary>
    /// <returns></returns>
    public abstract bl_NetworkGun GetCurrentNetworkWeapon();

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}
