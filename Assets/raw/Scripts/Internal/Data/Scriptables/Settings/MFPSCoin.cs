using MFPSEditor;
using System;
using System.Text;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(menuName = "MFPS/Shop/Coin", fileName = "Coin")]
    public class MFPSCoin : ScriptableObject
    {
        public string CoinName;
        public string Acronym;
        [Tooltip("The value of this coin with respect to 1, e.g items priced 100 can be purchase with 1000 coins with value of 0.1")]
        public float CoinValue = 1;
        public int InitialCoins = 0;
        [TextArea(2, 3)] public string Description;
        [SpritePreview] public Sprite CoinIcon;

        /// <summary>
        /// Add and save coins
        /// </summary>
        /// <param name="coins">Coins to add</param>
        /// <returns></returns>
        public MFPSCoin Add(int coins, string forUser = "")
        {
            bl_MFPSDatabase.Coins.Add(coins, this);
            return this;
        }

        /// <summary>
        /// Add and save coins
        /// </summary>
        /// <param name="coins">Coins to add</param>
        /// <returns></returns>
        public MFPSCoin Deduct(int coins, string forUser = "")
        {
            bl_MFPSDatabase.Coins.Deduct(coins, this);
            return this;
        }

        /// <summary>
        /// Get the locally saved coins
        /// </summary>
        /// <returns></returns>
        public int GetCoins(string endPoint = "")
        {
            int[] coins = bl_MFPSDatabase.Coins.GetCoins(new MFPSCoin[] { this }, endPoint);
            if (coins != null && coins.Length > 0) return coins[0];

            return 0;
        }

        /// <summary>
        /// Return the conversion of this coin to the reference price (value of 1)
        /// </summary>
        /// <param name="realPrice"></param>
        /// <returns></returns>
        public int DoConversion(int realPrice)
        {
            return Mathf.FloorToInt(realPrice / CoinValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string Key(string endPoint = "")
        {
            var k = $"{Application.productName}.coin.{CoinName}.{endPoint}";
            //Most basic obfuscation possible, is not recommended store coins locally.
            //For serious game, store the coins in a external server.
            k = Convert.ToBase64String(Encoding.UTF8.GetBytes(k));
            return k;
        }

        public static implicit operator int(MFPSCoin coin) => bl_MFPS.Coins.GetIndexOfCoin(coin);
        public static explicit operator MFPSCoin(int coinID) => bl_MFPS.Coins.GetAllCoins()[coinID];
    }
}