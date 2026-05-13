using MFPS.Core.Motion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class bl_CameraShaker : bl_CameraShakerBase
{
    public SubShakeTransform[] subShakeTransforms;

    #region Private members
    private Vector3 OrigiPosition;
    private readonly Dictionary<string, ShakerPresent> shakersRunning = new();
    private Transform m_Transform;
    private Vector3 tempVector = Vector3.zero;
    #endregion

    [System.Serializable]
    public class SubShakeTransform
    {
        public Transform Transform;
        [Range(-3, 3)] public float Influence = 1;

        private Vector3 OriginRotation;

        internal void CollectOrigin()
        {
            if (Transform != null)
                OriginRotation = Transform.localEulerAngles;
        }

        internal void SetShake(Vector3 shake)
        {
            if (Transform != null)
                Transform.localRotation = Quaternion.Euler(OriginRotation + (shake * Influence));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        SetCurrentAsOriginPosition();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetCurrentAsOriginPosition()
    {
        if (m_Transform == null) m_Transform = transform;
        OrigiPosition = m_Transform.localEulerAngles;

        foreach (var item in subShakeTransforms)
        {
            item.CollectOrigin();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerShake += OnShake;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerShake -= OnShake;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="present"></param>
    public override void SetShake(ShakerPresent present, string key)
    {
        AddShake(present, key, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Stop()
    {
        StopAllCoroutines();
        shakersRunning.Clear();
        m_Transform.localRotation = Quaternion.Euler(OrigiPosition);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="present"></param>
    /// <param name="key"></param>
    /// <param name="influence"></param>
    void OnShake(ShakerPresent present, string key, float influence)
    {
        AddShake(present, key, influence);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="present"></param>
    /// <param name="key"></param>
    /// <param name="influenced"></param>
    public void AddShake(ShakerPresent present, string key, float influenced = 1)
    {
        if (present == null) return;
        bool first = shakersRunning.Count <= 0;
        if (shakersRunning.ContainsKey(key))
        {
            shakersRunning.Remove(key);
        }
        present.currentTime = 1;
        present.starting = false;
        present.influence = influenced;
        shakersRunning.Add(key, present);

        if (first)
        {
            StartCoroutine(UpdateShake());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void RemoveShake(string key)
    {
        if (shakersRunning.ContainsKey(key))
        {
            shakersRunning.Remove(key);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateShake()
    {
        Vector3 pos;
        while (true)
        {
            if (shakersRunning.Count <= 0) { yield break; }
            pos = Vector2.zero;
            ShakerPresent p;
            for (int i = 0; i < shakersRunning.Count; i++)
            {
                p = shakersRunning.Values.ElementAt(i);
                if (p.fadeInTime <= 0)
                {
                    p.starting = false;
                    p.currentTime = 1;
                }

                if (p.starting)
                {
                    p.currentTime += Time.deltaTime / (p.Duration * p.fadeInTime);
                    if (p.currentTime >= 1)
                    {
                        p.currentTime = 1;
                        p.starting = false;
                    }
                }

                float amplitude = p.amplitude * p.currentTime;

                if (p.shakeMethod == ShakerPresent.ShakeMethod.PerlinNoise)
                    pos += ShakePerlinNoise(amplitude, p.frequency, p.octaves, p.persistance, p.lacunarity, p.burstFrequency, p.burstContrast, p.influence);
                else if (p.shakeMethod == ShakerPresent.ShakeMethod.RandomSet)
                    pos += ShakeSimple(amplitude, p.influence, p.currentTime);
                else if (p.shakeMethod == ShakerPresent.ShakeMethod.Oscillation)
                    pos += ShakeOscillation(p.currentTime, p);
                else if (p.shakeMethod == ShakerPresent.ShakeMethod.Curve)
                    pos += ShakeCurve(p.currentTime, p);

                if (!p.starting)
                {
                    if (!p.Loop)
                        p.currentTime -= Time.deltaTime / (p.Duration - (p.Duration * p.fadeInTime));
                }

                if (!p.starting && p.currentTime <= 0)
                {
                    shakersRunning.Remove(shakersRunning.ElementAt(i).Key);
                }
            }
            m_Transform.localRotation = Quaternion.Euler(OrigiPosition + pos);
            if (subShakeTransforms != null && subShakeTransforms.Length > 0)
            {
                foreach (var item in subShakeTransforms)
                {
                    item.SetShake(pos);
                }
            }
            yield return null;
        }
    }

    public Vector3 ShakeSimple(float intensity, float influence, float time)
    {
        time = Mathf.Clamp01(time);
        time = ElasticIn(time);
        intensity *= influence;
        intensity *= time;
        tempVector.x = Random.Range(-intensity, intensity) * 0.5f;
        tempVector.y = Random.Range(-intensity, intensity) * 0.5f;
        //tempVector.z = Random.Range(-intensity, intensity) * 0.5f;
        return tempVector;
    }

    public Vector3 ShakePerlinNoise(
    float amplitude,
    float frequency,
    int octaves,
    float persistence,
    float lacunarity,
    float burstFrequency,
    int burstContrast,
    float influence)
    {
        // Time reference
        float t = Time.time;
        float baseFreq = Mathf.Max(0.0001f, frequency);

        // Deterministic seed per instance (so every run is stable)
        int id = GetInstanceID();
        Vector2 seed = new Vector2(
            0.001f * ((id & 0x3FFF) + 1237),
            0.001f * (((id >> 3) & 0x3FFF) + 7919)
        );

        // Fractional Brownian motion accumulation
        Vector2 acc = Vector2.zero;
        float amp = 1f;
        float norm = 0f;
        float f = 1f;

        int oct = Mathf.Max(1, octaves);
        float p = Mathf.Clamp01(persistence);
        float l = Mathf.Max(1.0001f, lacunarity);

        for (int i = 0; i < oct; i++)
        {
            float phaseX = seed.x + t * baseFreq * f;
            float phaseY = seed.y + t * baseFreq * f;

            float nx = Mathf.PerlinNoise(phaseX, phaseY * 0.57f) * 2f - 1f;
            float ny = Mathf.PerlinNoise(phaseX * 0.61f, phaseY) * 2f - 1f;

            acc.x += nx * amp;
            acc.y += ny * amp;
            norm += amp;

            amp *= p;
            f *= l;
        }

        Vector2 v = (norm > 1e-6f) ? (acc / norm) : Vector2.right;

        // Burst amplitude modulation
        float burst = 1f;
        if (burstFrequency > 0f)
        {
            float bt = t * burstFrequency;
            burst = Mathf.PerlinNoise(seed.x + bt, seed.y + bt);
            if (burstContrast > 1) burst = Mathf.Pow(burst, burstContrast);
            burst = Mathf.Lerp(0.85f, 1f, burst); // keep within range
        }

        // Normalize vector to enforce consistent amplitude
        float mag = v.magnitude;
        if (mag > 1e-6f) v /= mag;
        else v = new Vector2(0.7071f, 0.7071f);

        float radius = (amplitude * influence) * burst;

        tempVector.Set(v.x * radius, v.y * radius, 0f);
        return tempVector;
    }


    public Vector3 ShakeOscillation(float progress, ShakerPresent present)
    {
        progress *= -1;
        float easedProgress = ElasticIn(progress);
        float sineValue = Mathf.Sin(easedProgress * Mathf.PI * 2 * present.octaves); // Oscillate between -1 and 1
        float zRotation = sineValue * present.amplitude;

        // Apply the Z-axis rotation shake
        Vector3 v = new(0, 0, zRotation);

        // Apply the FOV kick with elastic easing
        // float easedFovProgress = ElasticIn(progress);
        //float fovKick = Mathf.Lerp(0, present.lacunarity, Mathf.Sin(easedFovProgress * Mathf.PI));
        //m_camera.fieldOfView = initialFOV + fovKick;
        return v;
    }

    public Vector3 ShakeCurve(float time, ShakerPresent present)
    {
        time = Mathf.Clamp01(time);
        time = 1 - time;

        tempVector = Vector3.zero;
        tempVector.z = present.curve.Evaluate(time) * present.amplitude * present.influence;

        return tempVector;
    }

    public static float ElasticIn(float t)
    {
        const float HALF_PI = Mathf.PI / 2;
        const float A = -13 * HALF_PI;

        return (Mathf.Sin(A * (t + 1)) * Mathf.Pow(2, -10 * t)) + 1;
    }
}