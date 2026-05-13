using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MFPS/Motion/Spring", fileName = "Spring")]
public class bl_Spring : ScriptableObject
{
    public float stiffness = 169f;
    public float damping = 26f;
    public float mass = 1f;

    [Tooltip("Maximum simulation step size in seconds. Lower values improve stability at low FPS, at a small CPU cost.")]
    [Min(0.001f)]
    public float maxStep = 1f / 120f;

    public float Target { get; set; }
    public float Current { get; set; }

    public float Velocity { get; private set; }
    private List<TargetLayer> layers;
    private float defaultStiffness = -1;
    private float defaultDamping = -1;

    private float layersTargetCache = 0;
    private bool layersTargetDirty = true;

    /// <summary>
    /// 
    /// </summary>
    public class TargetLayer
    {
        public float Target { get; set; }

        private bool autoReset = false;
        private float resetProgress = 0;

        /// <summary>
        /// 
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!autoReset) return;

            resetProgress += deltaTime;
            Target = Mathf.Lerp(Target, 0, resetProgress);

            if (resetProgress >= 1)
            {
                autoReset = false;
                Target = 0;
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
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Spring InitAndGetInstance()
    {
        var copy = Instantiate(this);
        copy.defaultStiffness = -1;
        copy.defaultDamping = -1;

        return copy;
    }

    /// <summary>
    /// Advance a step by deltaTime(seconds).
    /// </summary>
    /// <param name="deltaTime">Delta time since previous frame</param>
    /// <returns>Evaluated value</returns>
    public void UpdateSpring(float deltaTime)
    {
        if (deltaTime <= 0) return;

        float m = Mathf.Max(0.0001f, mass);
        float k = Mathf.Max(0f, stiffness);
        float c = Mathf.Max(0f, damping);

        // Update layers once per frame. If they change the target, we also refresh the cached sum.
        UpdateLayers(deltaTime);
        float target = Target + GetLayersTarget();

        // Sub-step integration for stability at low FPS.
        float step = Mathf.Clamp(maxStep, 0.001f, 0.05f);
        int iterations = Mathf.Clamp(Mathf.CeilToInt(deltaTime / step), 1, 16);
        float h = deltaTime / iterations;

        for (int i = 0; i < iterations; i++)
        {
            // Semi-implicit Euler (symplectic) for good energy behavior.
            float x = Current;
            float v = Velocity;
            float a = (-k * (x - target) - c * v) / m;
            v += a * h;
            x += v * h;
            Velocity = v;
            Current = x;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateLayers(float deltaTime)
    {
        if (layers == null || layers.Count == 0) return;

        bool changed = false;
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            float before = layers[i].Target;
            layers[i].Update(deltaTime);
            if (!Mathf.Approximately(before, layers[i].Target)) changed = true;
        }

        if (changed) layersTargetDirty = true;
    }

    /// <summary>
    /// Add a additive target
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="target"></param>
    public TargetLayer SetTargetLayer(int layer, float target, bool autoReset = false)
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
        layersTargetDirty = true;
        if (autoReset) layers[layer].AutoReset();
        return layers[layer];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private float GetLayersTarget()
    {
        if (layers == null || layers.Count == 0) return 0;

        if (!layersTargetDirty) return layersTargetCache;

        float accumulate = 0;
        for (int i = layers.Count - 1; i >= 0; i--)
        {
            accumulate += layers[i].Target;
        }
        layersTargetCache = accumulate;
        layersTargetDirty = false;
        return layersTargetCache;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetMultiplier(float multiplier)
    {
        if (defaultStiffness <= 0)
        {
            defaultStiffness = stiffness;
            defaultDamping = damping;
        }

        stiffness = defaultStiffness * multiplier;
        damping = defaultDamping * multiplier;
    }
}