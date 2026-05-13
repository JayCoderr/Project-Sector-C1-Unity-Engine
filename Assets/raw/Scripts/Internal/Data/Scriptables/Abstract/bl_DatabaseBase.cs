using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    /// <summary>
    /// The base class for the database
    /// This script contain all the methods that the database should implement to work with the MFPS system
    /// You can inherit from this class to create your own database wrapper system
    /// </summary>
    public abstract class bl_DatabaseBase : ScriptableObject
    {
        public struct OperationResult
        {
            public string Message;
            public bool Success;
        }

        // create a delegate event for operation result
        public delegate void OnOperationResult(OperationResult result);

        private static float startTime = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool IsUserLogged();

        /// <summary>
        /// 
        /// </summary>
        public abstract string NickName { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string GetRolePrefix()
        {
            return string.Empty;
        }

        /// <summary>
        /// Get a string value from the database for the local player
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract string GetString(string key, string defaultValue = "");

        /// <summary>
        /// Get an int value from the database for the local player
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public abstract int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// Set a string value in the database for the local player
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="onFinish"></param>
        public abstract void SetString(string key, string value, Action<bool> onFinish = null);

        /// <summary>
        /// Store the player match stats (kills, deaths, score, etc)
        /// </summary>
        public abstract void StorePlayerMatchStats(int overrideScore = -1, Dictionary<string, object> extraProperties = null, Action<bool> onComplete = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public abstract bool IsItemPurchased(int itemType, int itemID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadoutSlot"></param>
        /// <param name="defaultLoadout"></param>
        /// <returns></returns>
        public abstract bl_PlayerClassLoadout GetLoadout(PlayerClass loadoutSlot, bl_PlayerClassLoadout defaultLoadout = null);

        /// <summary>
        /// Store the weapon loadouts of the user
        /// </summary>
        /// <param name="loadouts"></param>
        public abstract void StoreLoadouts(bl_PlayerClassLoadout[] loadouts, Action onComplete = null);

        /// <summary>
        /// Get the coins of the user
        /// </summary>
        /// <returns></returns>
        public abstract int[] GetCoins(MFPSCoin[] coinsToGet, string endPoint = "", OnOperationResult onFinish = null);

        /// <summary>
        /// Add coins to the user account
        /// </summary>
        /// <param name="coinsToAdd"></param>
        /// <param name="userName"></param>
        public abstract void AddCoins(int coinsToAdd, MFPSCoin coinToAdd, OnOperationResult onFinish = null);

        /// <summary>
        /// Deduct coins from the user account
        /// </summary>
        /// <param name="coinsToRemove"></param>
        public abstract void RemoveCoins(int coinsToRemove, MFPSCoin coinToRemoveFrom, OnOperationResult onFinish = null);

        /// <summary>
        /// Get the friends list of the local player
        /// </summary>
        public abstract List<string> GetFriends();

        /// <summary>
        /// Save the friends list of the local player
        /// </summary>
        public abstract void StoreFriends(List<string> friends);

        /// <summary>
        /// Check if the user exist in the database
        /// </summary>
        /// <param name="where"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public virtual void CheckIfUserExist(string where, string index, Action<bool> callback)
        {
            // determine if the user exist in the database
            callback?.Invoke(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void StartRecordingPlayTime()
        {
            startTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Return the time in seconds since the start of the match
        /// </summary>
        public virtual int StopRecordingPlayTime()
        {
            float playTime = Time.realtimeSinceStartup - startTime;
            return Mathf.FloorToInt(playTime);
        }

        /// <summary>
        /// Sign out the user
        /// </summary>
        public virtual void SignOut()
        {
            bl_PhotonNetwork.NickName = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLocalInClan() { return false; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetClanTag() { return string.Empty; }
    }
}