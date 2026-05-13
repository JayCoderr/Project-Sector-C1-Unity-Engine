using UnityEngine;

namespace MFPS.Runtime.UI
{
    public abstract class bl_KillZoneUIBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public virtual void SetText(string text)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public abstract void SetCount(int count);

        private static bl_KillZoneUIBase instance;
        public static bl_KillZoneUIBase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<bl_KillZoneUIBase>();
                }
                return instance;
            }
        }
    }
}