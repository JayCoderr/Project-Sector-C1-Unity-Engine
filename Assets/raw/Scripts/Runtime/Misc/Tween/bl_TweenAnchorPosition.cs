using System.Collections;
using UnityEngine;

namespace MFPS.Tween
{
    public class bl_TweenAnchorPosition : bl_TweenBase, ITween
    {
        [Header("Settings")]
        [LovattoToogle] public bool onEnable = true;
        [Range(0, 10)] public float Delay = 0;
        [Range(0.1f, 7)] public float Duration = 1;
        [LovattoToogle] public bool relativePosition = true;
        public TweenOrigin tweenTarget = TweenOrigin.To;
        public Vector2 vector;

        public EasingType m_EasingInType = EasingType.Quintic;
        public EasingMode m_EasingMode = EasingMode.Out;

        [Header("Event")]
        public UEvent onFinish;

        private RectTransform m_Transform;
        private float duration = 0;
        private Vector2 defaultVector;
        private bool defaultsCached = false;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            m_Transform = (RectTransform)transform;
            CacheDefaults();
        }

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            if (onEnable)
            {
                if (tweenTarget == TweenOrigin.From)
                {
                    CacheDefaults();
                    m_Transform.anchoredPosition = relativePosition ? vector + defaultVector : vector;
                }
                StartTween();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDisable()
        {
            duration = 0;
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartTween()
        {
            duration = 0;
            CacheDefaults();
            StopAllCoroutines();
            StartCoroutine(DoTween());
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartReverseTween(bool desactive = false)
        {
            CacheDefaults();
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine(DoTweenReverse(desactive));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoTween()
        {
#if UNITY_EDITOR
            bool isPlaying = Application.isPlaying;
            if (isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            Vector3 origin = tweenTarget == TweenOrigin.From ? vector : defaultVector;
            Vector3 target = tweenTarget == TweenOrigin.From ? defaultVector : vector;
            if (relativePosition)
            {
                origin = tweenTarget == TweenOrigin.From ? vector + defaultVector : defaultVector;
                target = tweenTarget == TweenOrigin.From ? defaultVector : vector + defaultVector;
            }

            while (duration < 1)
            {

#if UNITY_EDITOR
                if (isPlaying)
                {
                    duration += Time.deltaTime / Duration;
                }
                else
                {
                    duration += 0.015f / Duration;
                }
#else
                    duration += Time.deltaTime / Duration;
#endif
                m_Transform.anchoredPosition = Vector3.Lerp(origin, target, Easing.Do(duration, m_EasingInType, m_EasingMode));
                yield return null;
            }
            m_Transform.anchoredPosition = target;
            onFinish?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator DoTweenReverse(bool desactive)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
            else
            {
                //   if (Delay > 0) { yield return new EditorWaitForSeconds(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            duration = (duration > 0) ? duration : 1;
            duration = Mathf.Clamp(duration, 0, 1);
            Vector3 origin = m_Transform.anchoredPosition;
            Vector3 target = tweenTarget == TweenOrigin.From ? vector : defaultVector;
            if (relativePosition)
            {
                target = tweenTarget == TweenOrigin.From ? defaultVector : vector + defaultVector;
            }

            while (duration > 0)
            {
                duration -= Time.deltaTime / Duration;
                m_Transform.anchoredPosition = Vector3.Lerp(target, origin, Easing.Do(duration, m_EasingInType, m_EasingMode));
                yield return null;
            }
            m_Transform.anchoredPosition = target;
            onFinish?.Invoke();
            if (desactive) { gameObject.SetActive(false); }
        }

        private void CacheDefaults()
        {
            if (defaultsCached) return;

            defaultVector = m_Transform.anchoredPosition;
            defaultsCached = true;
        }

#if UNITY_EDITOR
        public override void PlayEditor()
        {
            InitInEditor();
            MFPSEditor.EditorCoroutines.StartBackgroundTask(DoTween());
        }

        public override void PlayReverseEditor()
        {
            InitInEditor();
            MFPSEditor.EditorCoroutines.StartBackgroundTask(DoTweenReverse(false));
        }

        public override void InitInEditor()
        {
            m_Transform = (RectTransform)transform;
            duration = 0;
            defaultVector = m_Transform.anchoredPosition;
        }

        public override void ResetDefault()
        {
            m_Transform = (RectTransform)transform;
            m_Transform.anchoredPosition = defaultVector;
        }
#endif
    }
}