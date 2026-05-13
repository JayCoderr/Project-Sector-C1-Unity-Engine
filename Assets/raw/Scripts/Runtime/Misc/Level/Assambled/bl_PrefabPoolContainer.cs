using System;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Prefab Pool Container", menuName = "MFPS/Level/Prefab Pool")]
    public class bl_PrefabPoolContainer : ScriptableObject
    {
        [Serializable]
        public class PrefabData
        {
            public string Key;
            public GameObject Prefab;
            public int PoolCount;
            public InstanceMethod InstanceMethod;
        }

        public PrefabData[] Prefabs;

        /// <summary>
        /// 
        /// </summary>
        public int Length => Prefabs.Length;

        public enum InstanceMethod
        {
            OnStart,
            OnCall
        }
    }
}