using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Item Drop Container", menuName = "MFPS/Loadout/Item Drop Container")]
    public class bl_ItemDropContainer : ScriptableObject
    {
        [Serializable]
        public class ItemData
        {
            public string Key;
            public string DisplayName;
            public Sprite Icon;
            public int Count;
            public GameObject Prefab;
        }

        public List<ItemData> Items;

        /// <summary>
        /// Get a item info by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemData GetItem(int index)
        {
            if (index >= Items.Count) return null;

            return Items[index];
        }

        /// <summary>
        /// Get a item info by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ItemData GetItem(string key)
        {
            return Items.Find(x => x.Key == key);
        }

        /// <summary>
        /// Get a item prefab by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public GameObject GetItemPrefab(string key)
        {
            ItemData item = GetItem(key);
            if (item == null) return null;
            return item.Prefab;
        }
    }
}