using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Runtime.Motion
{
    [Serializable]
    public class bl_SpringVector3
    {
        /// <summary>
        /// 
        /// </summary>
        public class TargetLayer
        {
            public Vector3 Target { get; set; }

            private bool autoReset = false;
            private float resetProgress = 0;

            /// <summary>
            /// 
            /// </summary>
            public void Update()
            {
                if (!autoReset) return;

                resetProgress += Time.deltaTime;
                Target = Vector3.Lerp(Target, Vector3.zero, resetProgress);

                if (resetProgress >= 1)
                {
                    autoReset = false;
                    Target = Vector3.zero;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void AutoReset()
            {
                autoReset = true;
                resetProgress = 0;
            }
        }

        /// <summary>
        /// Target value to reach
        /// </summary>
        public Vector3 Target { get; set; }

        /// <summary>
        /// Current value
        /// </summary>
        public Vector3 Current { get; set; }

        /// <summary>
        /// Current velocity
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public float TimeScale { get; set; } = 1;

        public bool Enable;
        public float Stiffness = 10;  // Spring stiffness constant
        public float Damping = 1;  // Damping constant
        public float Mass = 1;

        private bool isPlayingSineWave = false;
        private float sineWaveTime = 0f;
        private float sineWaveDecayConstant = 1.0f;
        private float sineWaveFrequency = 1.0f;
        private float sineWaveAmplitude = 1.0f;
        private float decayTrackingValue = 1f;
        private Vector3 sineWaveVector;
        private List<TargetLayer> layers;
        private bool useDeltaAngles = false;
        private float defaultStiffness = -1;
        private float defaultDamping = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="stiffness"></param>
        /// <param name="damping"></param>
        /// <param name="mass"></param>
        /// <param name="initialValue"></param>
        /// <param name="initialVelocity"></param>
        public bl_SpringVector3(Vector3 target, float stiffness, float damping, float mass, Vector3 initialValue = new Vector3(), Vector3 initialVelocity = new Vector3())
        {
            Target = target;
            Current = initialValue;
            Velocity = initialVelocity;
            Stiffness = stiffness;
            Damping = damping;
            Mass = mass;
            defaultStiffness = -1;
            TimeScale = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="initialValue"></param>
        /// <param name="initialVelocity"></param>
        public void Init(Vector3 target, Vector3 initialValue = new Vector3(), Vector3 initialVelocity = new Vector3())
        {
            Target = target;
            Current = initialValue;
            Velocity = initialVelocity;
            defaultStiffness = -1;
            TimeScale = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (!Enable) return;

            const float maxDeltaTime = 0.0333f; // This corresponds to ~30 FPS
            deltaTime = Mathf.Min(deltaTime, maxDeltaTime);

            UpdateSineWave();
            UpdateLayers();

            Vector3 vector;
            // Hooke's Law (for each axis)
            if (useDeltaAngles)
            {
                vector = (Target + GetLayersTarget());
                vector = new Vector3(DeltaAngle(Current.x, vector.x), DeltaAngle(Current.y, vector.y), DeltaAngle(Current.z, vector.z));
            }
            else
            {
                vector = Current - (Target + GetLayersTarget());
            }
            Vector3 springForce = -Stiffness * vector;
            Vector3 dampingForce = -Damping * Velocity;
            Vector3 netForce = springForce + dampingForce;
            Vector3 acceleration = netForce / Mass;
            Velocity += acceleration * deltaTime;

            // Update the current value
            Current += deltaTime * TimeScale * Velocity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateLayers()
        {
            if (layers == null || layers.Count == 0) return;

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                layers[i].Update();
            }
        }

        /// <summary>
        /// Add a additive target
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="target"></param>
        public TargetLayer SetTargetLayer(int layer, Vector3 target, bool autoReset = false)
        {
            if (layer == -1)
            {
                Target = target;
                return null;
            }

            layers ??= new List<TargetLayer>();

            // make sure the layer is assigned
            while ((layers.Count - 1) <= layer)
            {
                layers.Add(new TargetLayer());
            }

            layers[layer].Target = target;
            if (autoReset) layers[layer].AutoReset();
            return layers[layer];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Stiffness"></param>
        /// <param name="dampingMul"></param>
        public void SetMultipliers(float stiffnessMul, float dampingMul)
        {
            if (defaultStiffness <= 0)
            {
                defaultStiffness = Stiffness;
                defaultDamping = Damping;
            }

            Stiffness = defaultStiffness * stiffnessMul;
            Damping = defaultDamping * dampingMul;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="frequency"></param>
        /// <param name="decayConstant"></param>
        public void StartDecayingSineWave(Vector3 axis, float amplitude, float frequency, float decayConstant)
        {
            sineWaveVector = axis;
            sineWaveAmplitude = amplitude;
            sineWaveFrequency = frequency;
            sineWaveDecayConstant = decayConstant;
            sineWaveTime = 0f;
            isPlayingSineWave = true;
            decayTrackingValue = 1;
        }

        /// <summary>
        /// 
        /// </summary>
        void UpdateSineWave()
        {
            if (!isPlayingSineWave) return;

            float sineValue = sineWaveAmplitude * Mathf.Exp(-sineWaveDecayConstant * sineWaveTime) * Mathf.Sin(2 * Mathf.PI * sineWaveFrequency * sineWaveTime);
            SetTargetLayer(0, sineWaveVector * sineValue);
            sineWaveTime += Time.deltaTime;
            decayTrackingValue *= Mathf.Exp(-sineWaveDecayConstant * Time.deltaTime);

            if (Mathf.Abs(decayTrackingValue) < 0.03f)
            {
                isPlayingSineWave = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopSineWave()
        {
            isPlayingSineWave = false;
            SetTargetLayer(0, Vector3.zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="use"></param>
        public void UseDeltaAngle(bool use) => useDeltaAngles = use;

        /// <summary>
        /// Is the current target complete (current = target)
        /// </summary>
        /// <returns></returns>
        public bool IsComplete() => Vector3.Distance(Current, Target) < float.Epsilon;

        /// <summary>
        /// Is the current position near the target?
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNear(float value) => Vector3.Distance(Current, Target) <= value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Vector3 GetLayersTarget()
        {
            if (layers == null || layers.Count == 0) return Vector3.zero;

            Vector3 accumulate = Vector3.zero;
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                accumulate += layers[i].Target;
            }
            return accumulate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public float DeltaAngle(float current, float target)
        {
            float delta = current - target;
            while (delta > 180) delta -= 360;
            while (delta < -180) delta += 360;
            return delta;
        }
    }
}