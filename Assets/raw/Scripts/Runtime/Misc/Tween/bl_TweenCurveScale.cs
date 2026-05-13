using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MFPS.Tween
{
    public class bl_TweenCurveScale : bl_TweenBase, ITween
    {

        [Header("Settings")]
        [LovattoToogle] public bool OnStart = true;
        [LovattoToogle] public bool ApplyOnStart = true;
        [LovattoToogle] public bool Uniform = false;
        [Range(0, 10)] public float Delay = 0;
        [Range(0.1f, 7)] public float Duration = 1;
        [Range(0.1f, 4)] public float Multiplier = 1;
        public ApplyMode m_ApplyMode = ApplyMode.Override;
        public float delaySiblingIndex = 0;
        [Header("Axes")]
        public AnimationCurve X = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve Y = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve Z = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public EasingType m_EasingInType = EasingType.Quintic;
        public EasingMode m_EasingMode = EasingMode.Out;

        [System.Serializable] public class OnFinish : UnityEvent { }
        [Header("Event")]
        [SerializeField]
        private OnFinish m_OnFinish = new OnFinish();

        private Transform m_Transform;
        private float duration = 0;
        private Vector3 defaultScale;

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            m_Transform = transform;
            defaultScale = m_Transform.localScale;

            if (ApplyOnStart)
            {
                if (Uniform)
                {
                    Vector3 initialScale = new Vector3(X.keys[0].value, X.keys[0].value, X.keys[0].value) * Multiplier;
                    m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + initialScale : initialScale;
                }
                else
                {
                    Vector3 initialScale = new Vector3(X.keys[0].value, Y.keys[0].value, Z.keys[0].value) * Multiplier;
                    m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + initialScale : initialScale;
                }

                if (OnStart)
                {
                    if (delaySiblingIndex > 0) Delay = m_Transform.GetSiblingIndex() * delaySiblingIndex;
                    StartTween();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartTween()
        {
            duration = 0;
            StopAllCoroutines();
            StartCoroutine(DoTween());
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartReverseTween(bool desactive = false)
        {
            StopAllCoroutines();
            if (gameObject.activeInHierarchy)
                StartCoroutine(DoTweenReverse(desactive));
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
        /// <returns></returns>
        IEnumerator DoTween()
        {
            bool isPlaying = Application.isPlaying;
#if UNITY_EDITOR
            if (isPlaying)
            {
                if (Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
            }
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            float time = 0;
            Vector3 s = m_Transform.localScale;
            while (duration < 1)
            {

#if UNITY_EDITOR
                if (isPlaying)
                {
                    duration += Delta / Duration;
                }
                else
                {
                    duration += 0.015f / Duration;
                }
#else
                duration += Delta / Duration;
#endif

                time = Mathf.Lerp(0, 1, Easing.Do(duration, m_EasingInType, m_EasingMode));
                s.x = X.Evaluate(time) * Multiplier;
                if (Uniform)
                {
                    s.y = s.x;
                    s.z = s.x;
                }
                else
                {
                    s.y = Y.Evaluate(time) * Multiplier;
                    s.z = Z.Evaluate(time) * Multiplier;
                }
                m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + s : s;
                yield return null;
            }
            Vector3 v = Uniform ? Vector3.one * X.keys[X.length - 1].value : new(X.keys[X.length - 1].value, Y.keys[Y.length - 1].value, Z.keys[Z.length - 1].value);
            v *= Multiplier;
            m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + v : v;
            m_OnFinish?.Invoke();
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
#else
        if(Delay > 0) { yield return new WaitForSecondsRealtime(Delay); }
#endif
            duration = (duration > 0) ? duration : 1;
            duration = Mathf.Clamp(duration, 0, 1);
            float time = duration;
            Vector3 s = m_Transform.localScale;
            while (duration > 0)
            {
                duration -= Delta / Duration;
                time = Mathf.Lerp(time, duration, Easing.Do(1 - duration, m_EasingInType, m_EasingMode));
                s.x = X.Evaluate(time);
                if (Uniform)
                {
                    s.y = s.x;
                    s.z = s.x;
                }
                else
                {
                    s.y = Y.Evaluate(time);
                    s.z = Z.Evaluate(time);
                }
                m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + s : s;
                yield return null;
            }

            Vector3 v = Uniform ? Vector3.one * X.keys[0].value : new(X.keys[0].value, Y.keys[0].value, Z.keys[0].value);
            v *= Multiplier;
            m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + v : v;
            m_OnFinish?.Invoke();
            if (desactive) { gameObject.SetActive(false); }
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
            defaultScale = m_Transform.localScale;
            if (ApplyOnStart)
            {
                if (Uniform)
                {
                    Vector3 initialScale = new Vector3(X.keys[0].value, X.keys[0].value, X.keys[0].value) * Multiplier;
                    m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + initialScale : initialScale;
                }
                else
                {
                    Vector3 initialScale = new Vector3(X.keys[0].value, Y.keys[0].value, Z.keys[0].value) * Multiplier;
                    m_Transform.localScale = m_ApplyMode == ApplyMode.Additive ? defaultScale + initialScale : initialScale;
                }
            }
        }
#endif
    }
}