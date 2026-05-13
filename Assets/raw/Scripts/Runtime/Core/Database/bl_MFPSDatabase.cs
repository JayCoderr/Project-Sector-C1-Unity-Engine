using MFPS.Internal.Scriptables;
using System;
using System.Collections.Generic;

public class bl_MFPSDatabase
{

    /// <summary>
    /// Return if the local player is logged in an account
    /// </summary>
    public static bool IsUserLogged
    {
        get => DatabaseHandler != null && DatabaseHandler.IsUserLogged();
    }

    /// <summary>
    /// Pull the data from the server and update the local database
    /// </summary>
    public static void SyncDataFromServer(Action onFinish = null)
    {

    }

    /// <summary>
    /// Return an string value from the database for the local player
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetString(string key, string defaultValue = "")
    {
        if (DatabaseHandler == null)
        {
            return defaultValue;
        }

        return DatabaseHandler.GetString(key, defaultValue);
    }

    /// <summary>
    /// Return an int value from the database for the local player
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (DatabaseHandler == null)
        {
            return defaultValue;
        }

        return DatabaseHandler.GetInt(key, defaultValue);
    }

    /// <summary>
    /// Set a string value in the database for the local player
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="onFinish"></param>
    public static void SetString(string key, string value, Action<bool> onFinish = null)
    {
        if (DatabaseHandler == null) return;

        DatabaseHandler.SetString(key, value, onFinish);
    }

    /// <summary>
    /// Operations related to the local player
    /// </summary>
    public class User
    {

        public static string NickName
        {
            get => DatabaseHandler != null ? DatabaseHandler.NickName : bl_PhotonNetwork.NickName;
        }

        /// <summary>
        /// Get the role prefix of the local player (if any)
        /// </summary>
        /// <returns></returns>
        public static string GetRolePrefix()
        {
            if (DatabaseHandler == null)
            {
                return string.Empty;
            }

            return DatabaseHandler.GetRolePrefix();
        }

        /// <summary>
        /// Store the player match stats (kills, deaths, score, etc)
        /// </summary>
        public static void StorePlayerMatchStats(int overrideScore = -1, Dictionary<string, object> extraProperties = null, Action<bool> onComplete = null)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.StorePlayerMatchStats(overrideScore, extraProperties, onComplete);
        }

        /// <summary>
        /// Return if an item has been purchased by the local player
        /// NOTE: This only check if the item has been purchased, it doesn't check if the item is available for the player.
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool IsItemPurchased(int itemType, int itemID)
        {
            if (DatabaseHandler == null) return false;

            return DatabaseHandler.IsItemPurchased(itemType, itemID);
        }

        /// <summary>
        /// Return the loadout of the local player
        /// </summary>
        /// <param name="loadoutSlot"></param>
        /// <param name="defaultLoadout"></param>
        /// <returns></returns>
        public static bl_PlayerClassLoadout GetLoadout(PlayerClass loadoutSlot, bl_PlayerClassLoadout defaultLoadout = null)
        {
            if (DatabaseHandler == null) return defaultLoadout;

            return DatabaseHandler.GetLoadout(loadoutSlot, defaultLoadout);
        }

        /// <summary>
        /// Store the loadouts of the local player
        /// </summary>
        public static void StoreLoadouts(bl_PlayerClassLoadout[] loadouts, Action onComplete = null)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.StoreLoadouts(loadouts, onComplete);
        }

        /// <summary>
        /// Sign out the local player from the database account
        /// </summary>
        public static void SignOut()
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.SignOut();
        }
    }

    public class Users
    {
        /// <summary>
        /// Check if the user exist in the database
        /// </summary>
        /// <param name="where">query</param>
        /// <param name="index">identifier</param>
        /// <param name="callback"></param>
        public static void CheckIfUserExist(string where, string index, Action<bool> callback)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.CheckIfUserExist(where, index, callback);
        }
    }

    public class Coins
    {

        /// <summary>
        /// Return the coins of the local player
        /// </summary>
        /// <returns></returns>
        public static int[] GetCoins(MFPSCoin[] coinsToGet, string endPoint = "", bl_DatabaseBase.OnOperationResult onFinish = null)
        {
            if (DatabaseHandler == null) return null;

            return DatabaseHandler.GetCoins(coinsToGet, endPoint, onFinish);
        }

        /// <summary>
        /// Add coins to the local player account
        /// </summary>
        /// <param name="coins"></param>
        /// <param name="forUser"></param>
        public static void Add(int coinsToAdd, MFPSCoin coinToAdd, bl_DatabaseBase.OnOperationResult onFinish = null)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.AddCoins(coinsToAdd, coinToAdd, onFinish);
        }

        /// <summary>
        /// Deduct coins from the local player account
        /// </summary>
        /// <param name="coinsToRemove"></param>
        /// <param name="coinToRemove"></param>
        public static void Deduct(int coinsToRemove, MFPSCoin coinToRemove, bl_DatabaseBase.OnOperationResult onFinish = null)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.RemoveCoins(coinsToRemove, coinToRemove, onFinish);
        }
    }

    public class Friends
    {
        /// <summary>
        /// Get the friends list of the local player
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFriends()
        {
            if (DatabaseHandler == null) return new List<string>();

            return DatabaseHandler.GetFriends();
        }

        /// <summary>
        /// Store the friends list of the local player account
        /// </summary>
        /// <param name="friends"></param>
        public static void StoreFriends(List<string> friends)
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.StoreFriends(friends);
        }
    }

    public class PlayTime
    {
        /// <summary>
        /// Start recording the play time of the local player
        /// </summary>
        public static void StartRecordingTime()
        {
            if (DatabaseHandler == null) return;

            DatabaseHandler.StartRecordingPlayTime();
        }

        /// <summary>
        /// Return the time in seconds since the start of the match
        /// </summary>
        public static int StopRecordingPlayTime()
        {
            if (DatabaseHandler == null) return 0;

            return DatabaseHandler.StopRecordingPlayTime();
        }
    }

    public class Clan
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsLocalInClan()
        {
            if (DatabaseHandler == null) return false;
            return DatabaseHandler.IsLocalInClan();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetClanTag()
        {
            if (DatabaseHandler == null) return string.Empty;
            return DatabaseHandler.GetClanTag();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bl_DatabaseBase DatabaseHandler
    {
        get => bl_GlobalReferences.DatabaseBase;
    }
}