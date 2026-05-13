using UnityEngine;

namespace MFPS.Runtime.Motion
{
    [CreateAssetMenu(fileName = "Spring Transform", menuName = "MFPS/Motion/Spring Transform")]
    public class bl_SpringTransform : ScriptableObject
    {
        public bl_SpringVector3 PositionSpring;
        public bl_SpringVector3 RotationSpring;

        public Vector3 DefaultPosition { get; set; }
        public Vector3 DefaultRotation { get; set; }

        private float returnTimer = 0;
        private bool hasATimer = false;
        private bool autoReturnPosition = false;
        private bool autoReturnRotation = false;
        private Transform Transform;

        /// <summary>
        /// 
        /// </summary>
        public bl_SpringTransform InitAndGetInstance(Transform transform)
        {
            if (transform == null)
            {
                // log warning
                Debug.LogWarning("No transform was provided for the spring.");
                return null;
            }

            var instance = Instantiate(this);
            instance.Init(transform);
            return instance;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init(Transform transform)
        {
            Transform = transform;
            DefaultPosition = Transform.localPosition;
            DefaultRotation = Transform.localEulerAngles;

            PositionSpring.Init(DefaultPosition, DefaultPosition);
            RotationSpring.Init(DefaultRotation, DefaultRotation);
            RotationSpring.UseDeltaAngle(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            PositionSpring.Update(Time.deltaTime);
            RotationSpring.Update(Time.deltaTime);

            UpdateTimer();
            AutoReturns();

            if (Transform == null) return;

            if (PositionSpring.Enable) Transform.localPosition = PositionSpring.Current;
            if (RotationSpring.Enable) Transform.localEulerAngles = RotationSpring.Current;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateTimer()
        {
            if (returnTimer <= 0 && !hasATimer) return;

            returnTimer -= Time.deltaTime;
            if (returnTimer <= 0)
            {
                SetTargetToDefault();
                hasATimer = false;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void AutoReturns()
        {
            if (autoReturnPosition)
            {
                if (PositionSpring.IsNear(0.012f))
                {
                    PositionSpring.Target = DefaultPosition;
                    autoReturnPosition = false;
                }
            }

            if (autoReturnRotation)
            {
                if (RotationSpring.IsNear(0.012f))
                {
                    RotationSpring.Target = DefaultRotation;
                    autoReturnRotation = false;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bl_SpringTransform SetTargetToDefault()
        {
            PositionSpring.Target = DefaultPosition;
            RotationSpring.Target = DefaultRotation;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        public bl_SpringTransform SetTargetToDefaultAfter(float delay)
        {
            returnTimer = delay;
            hasATimer = true;
            return this;
        }

        /// <summary>
        /// Make the spring return to the default position once complete the next target position.
        /// </summary>
        public bl_SpringTransform AutoReturnPosition()
        {
            autoReturnPosition = true;
            return this;
        }

        /// <summary>
        /// Make the spring return to the default rotation once complete the next target rotation.
        /// </summary>
        public bl_SpringTransform AutoReturnRotation()
        {
            autoReturnRotation = true;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bl_SpringTransform SetPositionTarget(Vector3 target)
        {
            PositionSpring.Target = target;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bl_SpringTransform SetRotationTarget(Vector3 target)
        {
            RotationSpring.Target = target;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scale"></param>
        public void SetTimeScale(float scale)
        {
            PositionSpring.TimeScale = scale;
            RotationSpring.TimeScale = scale;
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector3 RotationCurrent
        {
            get => RotationSpring.Current;
            set => RotationSpring.Current = value;
        }

        /// <summary>
        /// Is the target position the default position?
        /// </summary>
        /// <returns></returns>
        public bool IsRotationTargetDefault() => RotationSpring.Target == DefaultPosition;
    }
}