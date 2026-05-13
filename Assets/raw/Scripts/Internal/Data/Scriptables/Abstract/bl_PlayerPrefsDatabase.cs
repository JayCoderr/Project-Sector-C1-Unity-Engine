using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.Storage;
#endif

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "PlayerPrefs Database", menuName = "MFPS/Database/PlayerPrefs Handler")]
    public class bl_PlayerPrefsDatabase : bl_DatabaseBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsUserLogged()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string NickName => bl_PhotonNetwork.NickName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="onFinish"></param>
        public override void SetString(string key, string value, Action<bool> onFinish = null)
        {
            PlayerPrefs.SetString(key, value);
            onFinish?.Invoke(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StorePlayerMatchStats(int overrideScore = -1, Dictionary<string, object> extras = null, Action<bool> onComplete = null)
        {
            int allKills = GetInt("kills", 0);
            int allDeaths = GetInt("deaths", 0);
            int allScore = GetInt("score", 0);
            int allAssists = GetInt("assists", 0);

            var player = bl_PhotonNetwork.LocalPlayer;
            int matchKills = player.GetKills();
            int matchDeaths = player.GetDeaths();
            int matchScore = player.GetPlayerScore();
            int matchAssists = player.GetAssists();

            int newKills = allKills + matchKills;
            int newDeaths = allDeaths + matchDeaths;
            int newScore = allScore + matchScore;
            int newAssists = allAssists + matchAssists;

            PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("kills", player.NickName), newKills);
            PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("deaths", player.NickName), newDeaths);
            PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("score", player.NickName), newScore);
            PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("assists", player.NickName), newAssists);

            if (extras != null)
                foreach (var item in extras)
                {
                    if (item.Value is int)
                    {
                        PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer(item.Key, player.NickName), (int)item.Value);
                    }
                    else if (item.Value is string)
                    {
                        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKeyForPlayer(item.Key, player.NickName), (string)item.Value);
                    }
                }

            onComplete?.Invoke(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public override bool IsItemPurchased(int itemType, int itemID)
        {
            // items purchased are not supported in local database
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int[] GetCoins(MFPSCoin[] coinsToGet, string endPoint = "", OnOperationResult onFinish = null)
        {
            int[] amounts = new int[coinsToGet.Length];
            for (int i = 0; i < coinsToGet.Length; i++)
            {
#if !ACTK_IS_HERE
                amounts[i] = PlayerPrefs.GetInt(Key(endPoint), coinsToGet[i].InitialCoins);
#else
                 amounts[i] = ObscuredPrefs.Get<int>(Key(endPoint), coinsToGet[i].InitialCoins);
#endif
            }
            onFinish?.Invoke(new OperationResult()
            {
                Success = true
            });
            return amounts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToAdd"></param>
        public override void AddCoins(int coinsToAdd, MFPSCoin coinToAdd, OnOperationResult onFinish = null)
        {
            string user = bl_PhotonNetwork.NickName;
            int savedCoins = GetCoins(new MFPSCoin[] { coinToAdd }, user)[0];
            savedCoins += coinsToAdd;


#if !ACTK_IS_HERE
            PlayerPrefs.SetInt(Key(user), savedCoins);
#else
            ObscuredPrefs.Set<int>(Key(user), savedCoins);
#endif
            onFinish?.Invoke(new OperationResult()
            {
                Success = true
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinsToRemove"></param>
        public override void RemoveCoins(int coinsToRemove, MFPSCoin coinToRemoveFrom, OnOperationResult onFinish = null)
        {
            string user = bl_PhotonNetwork.NickName;
            int savedCoins = GetCoins(new MFPSCoin[] { coinToRemoveFrom }, user)[0];
            savedCoins -= coinsToRemove;

            if (savedCoins < 0)
            {
                Debug.LogWarning("Something weird happen, funds where not verified before execute a transaction");
                savedCoins = 0;
            }

#if !ACTK_IS_HERE
            PlayerPrefs.SetInt(Key(user), savedCoins);
#else
            ObscuredPrefs.Set<int>(Key(user), savedCoins);
#endif
            onFinish?.Invoke(new OperationResult()
            {
                Success = true
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadoutSlot"></param>
        /// <param name="defaultLoadout"></param>
        /// <returns></returns>
        public override bl_PlayerClassLoadout GetLoadout(PlayerClass loadoutSlot, bl_PlayerClassLoadout defaultLoadout = null)
        {
            string nick = string.IsNullOrEmpty(bl_PhotonNetwork.NickName) ? "Offline Player" : bl_PhotonNetwork.NickName;
            string key = PropertiesKeys.GetUniqueKeyForPlayer("loadouts", nick);
            var instance = Instantiate(defaultLoadout);
            if (!PlayerPrefs.HasKey(key)) return instance;

            string loadoutData = PlayerPrefs.GetString(key);
            instance.FromString(loadoutData, (int)loadoutSlot);
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadouts"></param>
        public override void StoreLoadouts(bl_PlayerClassLoadout[] loadouts, Action onComplete = null)
        {
            string nick = string.IsNullOrEmpty(bl_PhotonNetwork.NickName) ? "Offline Player" : bl_PhotonNetwork.NickName;
            string key = PropertiesKeys.GetUniqueKeyForPlayer("loadouts", nick);
            string compiled = string.Empty;
            for (int i = 0; i < loadouts.Length; i++)
            {
                compiled += $"{loadouts[i].ToString()},";
            }
            // remove the last ,
            compiled = compiled.Remove(compiled.Length - 1);
            PlayerPrefs.SetString(key, compiled);
            onComplete?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int StopRecordingPlayTime()
        {
            int playTime = base.StopRecordingPlayTime();
            int currentPlayTime = GetInt("playtime", 0);
            int newPlayTime = currentPlayTime + playTime;
            PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKeyForPlayer("playtime", bl_PhotonNetwork.NickName), newPlayTime);
            return newPlayTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="where"></param>
        /// <param name="index"></param>
        /// <param name="callback"></param>
        public override void CheckIfUserExist(string where, string index, Action<bool> callback)
        {
            // We can't check if the user exist using PlayerPrefs
            callback?.Invoke(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<string> GetFriends()
        {
            var list = new List<string>();
            string cacheFriend = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKeyForPlayer("mfps.friends", bl_PhotonNetwork.NickName), "Null");
            if (!string.IsNullOrEmpty(cacheFriend))
            {
                string[] splitFriends = cacheFriend.Split('/');
                list.AddRange(splitFriends);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StoreFriends(List<string> friends)
        {
            string friendsString = string.Join("/", friends.ToArray());
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKeyForPlayer("mfps.friends", bl_PhotonNetwork.NickName), friendsString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string Key(string coinName, string endPoint = "")
        {
            var k = $"{Application.productName}.coin.{coinName}.{endPoint}";
            //Most basic obfuscation possible, is not recommended store coins locally.
            //For serious game, store the coins in a external server.
            k = Convert.ToBase64String(Encoding.UTF8.GetBytes(k));
            return k;
        }
    }
}