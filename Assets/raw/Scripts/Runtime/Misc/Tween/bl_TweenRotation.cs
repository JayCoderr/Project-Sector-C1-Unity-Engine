using System.Collections;
using UnityEngine;

namespace MFPS.Tween
{
    public class bl_TweenRotation : bl_TweenBase, ITween
    {
        [Header("Settings")]
        [LovattoToogle] public bool onEnable = true;
        [Range(0, 10)] public float Delay = 0;
        [Range(0.1f, 7)] public float Duration = 1;
        [LovattoToogle] public bool relativeRotation = false;
        public TweenOrigin tweenTarget = TweenOrigin.To;
        public Vector3 rotation;

        public EasingType m_EasingInType = EasingType.Quintic;
        public EasingMode m_EasingMode = EasingMode.Out;

        [Header("Event")]
        public UEvent onFinish;

        private Transform m_Transform;
        private float duration = 0;
        private Vector3 defaultRotation;
        private bool defaultsCached = false;

        void Awake()
        {
            m_Transform = transform;
        }

        void OnEnable()
        {
            if (onEnable)
            {
                if (tweenTarget == TweenOrigin.From)
                {
                    CacheDefaults();
                    m_Transform.localEulerAngles = relativeRotation ? rotation + defaultRotation : rotation;
                }
                StartTween();
            }
        }

        public void StartTween()
        {
            duration = 0;
            CacheDefaults();
            StopAllCoroutines();
            StartCoroutine(DoTween());
        }

        public void StartReverseTween(bool deactivate = false)
        {
            CacheDefaults();
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine(DoTweenReverse(deactivate));
        }

        void OnDisable()
        {
            duration = 0;
            StopAllCoroutines();
        }

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
            Vector3 origin = tweenTarget == TweenOrigin.From ? rotation : defaultRotation;
            Vector3 target = tweenTarget == TweenOrigin.From ? defaultRotation : rotation;
            if (relativeRotation)
            {
                origin = tweenTarget == TweenOrigin.From ? rotation + defaultRotation : defaultRotation;
                target = tweenTarget == TweenOrigin.From ? defaultRotation : rotation + defaultRotation;
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
                m_Transform.localEulerAngles = Vector3.Lerp(origin, target, Easing.Do(duration, m_EasingInType, m_EasingMode));
                yield return null;
            }
            m_Transform.localEulerAngles = target;
            onFinish?.Invoke();
        }

        IEnumerator DoTweenReverse(bool deactivate)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            duration = (duration > 0) ? duration : 1;
            duration = Mathf.Clamp(duration, 0, 1);
            Vector3 origin = m_Transform.localEulerAngles;
            Vector3 target = tweenTarget == TweenOrigin.From ? rotation : defaultRotation;
            if (relativeRotation)
            {
                target = tweenTarget == TweenOrigin.From ? defaultRotation : rotation + defaultRotation;
            }

            while (duration > 0)
            {
                duration -= Time.deltaTime / Duration;
                m_Transform.localEulerAngles = Vector3.Lerp(target, origin, Easing.Do(duration, m_EasingInType, m_EasingMode));
                yield return null;
            }
            m_Transform.localEulerAngles = target;
            onFinish?.Invoke();
            if (deactivate) { gameObject.SetActive(false); }
        }

        private void CacheDefaults()
        {
            if (defaultsCached) return;

            defaultRotation = m_Transform.localEulerAngles;
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
            m_Transform = transform;
            duration = 0;
            defaultRotation = m_Transform.localEulerAngles;
        }

        public override void ResetDefault()
        {
            transform.localEulerAngles = defaultRotation;
        }

        [ContextMenu("Get Rotation")]
        void GetRotation()
        {
            rotation = transform.localEulerAngles;
        }
#endif
    }
}