using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif

namespace MFPS.Tween
{
    public class bl_TweenBase : MonoBehaviour
    {

        [Serializable] public class UEvent : UnityEvent { }

        public float Delta
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    return Time.deltaTime;
                }
                else { return Time.deltaTime; }
#else
             return Time.unscaledDeltaTime;
#endif
            }
        }

        [Serializable]
        public enum TweenOrigin
        {
            From,
            To
        }

        public enum ApplyMode
        {
            Override,
            Additive
        }

#if UNITY_EDITOR
        public virtual void CacheDefaultValues() { }
        public virtual void ResetDefault() { }
        public virtual void InitInEditor() { }
        public virtual void PlayEditor() { }
        public virtual void PlayReverseEditor() { }
#endif
    }
}