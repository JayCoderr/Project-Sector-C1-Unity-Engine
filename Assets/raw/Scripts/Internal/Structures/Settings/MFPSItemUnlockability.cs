using System;
using System.Collections.Generic;
using MFPS.Internal.Scriptables;
using UnityEngine;
using MFPSEditor;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Internal.Structures
{
    [Serializable]
    public class MFPSItemUnlockability
    {
        [Tooltip("How this item can be unlocked.")]
        public UnlockabilityMethod UnlockMethod = UnlockabilityMethod.UnlockedByDefault;
        public ItemTypeEnum ItemType = ItemTypeEnum.Weapon;
        public int Price = 0;
        [Tooltip("The level at which this item can be unlocked. If 0, the item is not unlockable by level up.\nThis require the Level Manager addon to work.")]
        public int UnlockAtLevel = 0;
        [Tooltip("Coins with which can't purchase this item; leave empty if all coins are available.")]
        [MFPSCoinID] public int[] NoAllowedCoins;

        /// <summary>
        /// Is this item unlocked for the local player?
        /// </summary>
        public bool IsUnlocked(int itemID)
        {
            return UnlockMethod == UnlockabilityMethod.UnlockedByDefault || GetLockReason(itemID) == LockReason.None;
        }

        /// <summary>
        /// If locked return the reason, otherwise return: LockReason.None
        /// </summary>
        public LockReason GetLockReason(int itemID)
        {
            var reason = LockReason.None;
            if (UnlockMethod == UnlockabilityMethod.UnlockedByDefault) return reason;
            if (UnlockMethod == UnlockabilityMethod.Hidden) return LockReason.Hidden;

            if (UnlockMethod == UnlockabilityMethod.VIPOnly)
            {
#if MFPS_VIP
                return bl_VIP.IsVIP ? LockReason.None : LockReason.Hidden;
#else
                return LockReason.Hidden;
#endif
            }

            bool isPurchased = false;

            if (CanBePurchased())
            {
                isPurchased = bl_MFPSDatabase.User.IsItemPurchased((int)ItemType, itemID);
                if (!isPurchased)
                {
                    reason = LockReason.NoPurchased;
                }
            }

#if LM
            if (CanBeUnlockedByLevel())
            {
                // if the local player level doesn't meets the required level to unlock.
                if ((bl_LevelManager.Instance.GetLevelID() + 1) < UnlockAtLevel)
                {
                    if (reason == LockReason.NoPurchased) reason = LockReason.NoPurchasedAndLevel;
                    else
                    {
                        // if this item is purchased and both ways are allowed to unlock the item (purchase or level up) then the item is unlock no matter the player level.
                        reason = isPurchased && UnlockMethod == UnlockabilityMethod.PurchasedOrLevelUp ? LockReason.None : LockReason.Level;
                    }
                }// if the player have the required level to unlock.
                else
                {
                    // if the item is not purchased, but the player meets the required level to unlock the item.
                    if (reason == LockReason.NoPurchased && UnlockMethod == UnlockabilityMethod.PurchasedOrLevelUp)
                    {
                        reason = LockReason.None;
                    }
                }
            }
#endif
            if (isPurchased) { }
            return reason;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsFree() => Price <= 0;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanBePurchased()
        {
            return UnlockMethod != UnlockabilityMethod.UnlockedByDefault && UnlockMethod != UnlockabilityMethod.LevelUpOnly && UnlockMethod != UnlockabilityMethod.VIPOnly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool CanBeUnlockedByLevel()
        {
            return UnlockMethod != UnlockabilityMethod.UnlockedByDefault && UnlockMethod != UnlockabilityMethod.PurchasedOnly && UnlockMethod != UnlockabilityMethod.VIPOnly;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool AllowUnlockByLevel()
        {
            return CanBeUnlockedByLevel() && UnlockAtLevel > 0;
        }

        /// <summary>
        /// Determines whether this item is VIP only.
        /// </summary>
        /// <returns>True if the item is VIP only, false otherwise.</returns>
        public bool IsVIPOnly()
        {
            return UnlockMethod == UnlockabilityMethod.VIPOnly;
        }

        /// <summary>
        /// Get the coins that can be used with this item
        /// Compare from all the available coins in the game (assigned in GameData -> Game Coins)
        /// </summary>
        /// <param name="originalCoins"></param>
        /// <returns></returns>
        public List<MFPSCoin> GetAllowedCoins()
        {
            var all = bl_MFPS.Coins.GetAllCoins();
            var copy = new List<MFPSCoin>();
            foreach (var c in all)
            {
                copy.Add(c);
            }

            return NoAllowedCoins == null || NoAllowedCoins.Length <= 0 ? copy : copy.Except(GetNoAllowedCoins()).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<MFPSCoin> GetNoAllowedCoins()
        {
            var copy = new List<MFPSCoin>();
            for (int i = 0; i < NoAllowedCoins.Length; i++)
            {
                copy.Add((MFPSCoin)NoAllowedCoins[i]);
            }
            return copy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public bool IsCoinAllowed(MFPSCoin coin)
        {
            return NoAllowedCoins == null || NoAllowedCoins.Length <= 0 || !NoAllowedCoins.Contains((int)coin);
        }

        /// <summary>
        /// Get the item price converted to an specific coin value.
        /// </summary>
        /// <returns></returns>
        public int GetPriceForCoins(MFPSCoin coin)
        {
            return Price <= 0 ? 0 : coin.DoConversion(Price);
        }

        [Serializable]
        public enum UnlockabilityMethod
        {
            UnlockedByDefault = 0,
            PurchasedOnly = 1,
            LevelUpOnly,
            PurchasedOrLevelUp,
            Hidden, // for items that are unlocked only by special events
            VIPOnly,
        }

        [Serializable]
        public enum ItemTypeEnum
        {
            Weapon = 0,
            WeaponCamo = 1,
            PlayerSkin = 2,
            PlayerAccesory = 3,
            Emblem = 4,
            CallingCard = 5,
            Emote = 6,
            SeasonPass = 7,
            LootBox = 8,
            Bundle = 9,
            CoinPack = 10,
            NoAds = 11,
            Coins = 12,
            VIP = 13,
            Spray = 14,
            RadioCommand = 15,
            None = 99,
        }

        public enum LockReason
        {
            None,
            NoPurchased,
            Level,
            NoPurchasedAndLevel,
            Hidden,
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MFPSItemUnlockability))]
    public class MFPSItemUnlockabilityDrawer : PropertyDrawer
    {
        public static Texture2D s_Lock_Icon;
        public static Texture2D s_Unlock_Icon;
        public static Texture2D s_Hidden_Icon;
        public static Texture2D s_VIP_Icon;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            var r = position;
            r.height = EditorGUIUtility.singleLineHeight;

            GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
            GUI.Box(r, GUIContent.none, "PreviewPackageInUse");
            GUI.backgroundColor = Color.white;

            if (s_Lock_Icon == null) s_Lock_Icon = EditorGUIUtility.IconContent("IN LockButton on act@2x").image as Texture2D;
            if (s_Unlock_Icon == null) s_Unlock_Icon = EditorGUIUtility.IconContent("IN LockButton act@2x").image as Texture2D;
            if (s_Hidden_Icon == null) s_Hidden_Icon = EditorGUIUtility.IconContent("d_SceneViewVisibility").image as Texture2D;
            if (s_VIP_Icon == null) s_VIP_Icon = EditorGUIUtility.IconContent("d_winbtn_mac_min").image as Texture2D;

            var unlockMethod = property.FindPropertyRelative("UnlockMethod");
            var um = (MFPSItemUnlockability.UnlockabilityMethod)unlockMethod.enumValueIndex;

            property.isExpanded = EditorGUI.Foldout(r, property.isExpanded, label, true);
            GUI.DrawTexture(r, GetStateIcon(um), ScaleMode.ScaleToFit);

            if (property.isExpanded)
            {
                r.y += EditorGUIUtility.singleLineHeight + 6;

                float rect = GetPropertyHeight(property, label) - (EditorGUIUtility.singleLineHeight + 4);
                var rbg = r;
                rbg.height = rect;
                EditorGUI.DrawRect(rbg, new Color(0, 0, 0, 0.2f));

                EditorGUI.PropertyField(r, unlockMethod);

                r.y += EditorGUIUtility.singleLineHeight;
                var itemType = property.FindPropertyRelative("ItemType");
                EditorGUI.PropertyField(r, itemType);

                GUI.enabled = um != MFPSItemUnlockability.UnlockabilityMethod.UnlockedByDefault;
                if (um != MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly && um != MFPSItemUnlockability.UnlockabilityMethod.VIPOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    var priceProperty = property.FindPropertyRelative("Price");
                    EditorGUI.PropertyField(r, priceProperty);
                    if (um != MFPSItemUnlockability.UnlockabilityMethod.UnlockedByDefault && priceProperty.intValue <= 0) ShowWarningIconInRect(r);
                }

                if (um != MFPSItemUnlockability.UnlockabilityMethod.PurchasedOnly && um != MFPSItemUnlockability.UnlockabilityMethod.VIPOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("UnlockAtLevel"));
                }

                if (um != MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly && um != MFPSItemUnlockability.UnlockabilityMethod.VIPOnly)
                {
                    r.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(r, property.FindPropertyRelative("NoAllowedCoins"), true);
                }
                GUI.enabled = true;
            }
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return base.GetPropertyHeight(property, label) + 4;
            else
            {
                float less = 0;
                var unlockMethod = property.FindPropertyRelative("UnlockMethod").enumValueIndex;
                if (unlockMethod == 2) less = EditorGUIUtility.singleLineHeight * 2;
                else if (unlockMethod == 1) less = EditorGUIUtility.singleLineHeight;
                else if (unlockMethod == 5) less = EditorGUIUtility.singleLineHeight * 3;

                return EditorGUI.GetPropertyHeight(property, label, true) - less;
            }
        }

        private void ShowWarningIconInRect(Rect rect)
        {
            var icon = EditorGUIUtility.IconContent("console.warnicon.sml");
            rect.x += rect.width - icon.image.width;
            rect.width = icon.image.width;
            rect.height = icon.image.height;
            GUI.DrawTexture(rect, icon.image);
        }

        private Texture2D GetStateIcon(MFPSItemUnlockability.UnlockabilityMethod unlockMethod)
        {
            switch (unlockMethod)
            {
                case MFPSItemUnlockability.UnlockabilityMethod.UnlockedByDefault:
                    return s_Unlock_Icon;
                case MFPSItemUnlockability.UnlockabilityMethod.Hidden:
                    return s_Hidden_Icon;
                case MFPSItemUnlockability.UnlockabilityMethod.VIPOnly:
                    return s_VIP_Icon;
                default:
                    return s_Lock_Icon;
            }
        }
    }
#endif
}