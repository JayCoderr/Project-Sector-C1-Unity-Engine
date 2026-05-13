using MFPS.Internal.Structures;
using MFPSEditor;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_MFPSCoinPriceUI : MonoBehaviour
    {
        public CoinUI[] coins;
        public LayoutGroup layoutGroup;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            ForceLayout();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="realPrice"></param>
        public bl_MFPSCoinPriceUI SetPrice(int realPrice)
        {
            foreach (var item in coins)
            {
                item.ParsePrice(realPrice);
            }
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="realPrice"></param>
        public bl_MFPSCoinPriceUI SetPrice(MFPSItemUnlockability unlockability)
        {
            var noallowedCoins = unlockability.NoAllowedCoins.ToList();
            foreach (var item in coins)
            {
                item.ParsePrice(unlockability.Price);
                if (item.PriceText != null && noallowedCoins.Contains(bl_MFPS.Coins.GetCoinData(item.CoinID)))
                {
                    item.PriceText.gameObject.SetActive(false);
                }
            }
            ForceLayout();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ForceLayout()
        {
            if (layoutGroup != null)
            {
                layoutGroup.enabled = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                layoutGroup.enabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        [Serializable]
        public class CoinUI
        {
            [MFPSCoinID] public int CoinID;
            public TextMeshProUGUI PriceText;
            public Image CoinIcon;

            public void ParsePrice(int realPrice)
            {
                var coin = bl_MFPS.Coins.GetCoinData(CoinID);
                if (coin == null) return;

                if (CoinIcon != null) CoinIcon.sprite = coin.CoinIcon;
                if (PriceText != null) PriceText.text = coin.DoConversion(realPrice).ToString();
            }
        }
    }
}