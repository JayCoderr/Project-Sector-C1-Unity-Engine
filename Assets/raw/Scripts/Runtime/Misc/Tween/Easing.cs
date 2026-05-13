using System;
using UnityEngine;

namespace MFPS.Tween
{
    public static class Easing
    {
        public static float Do(float t, EasingType type = EasingType.Quartic, EasingMode mode = EasingMode.InOut)
        {
            switch (type)
            {
                //-----------------------------------------------------------
                case EasingType.Back:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Back.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Back.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Back.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Back.OutIn(t);
                        default: return EasingFunctions.Back.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Bounce:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Bounce.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Bounce.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Bounce.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Bounce.OutIn(t);
                        default: return EasingFunctions.Bounce.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Circular:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Circular.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Circular.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Circular.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Circular.OutIn(t);
                        default: return EasingFunctions.Circular.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Cubic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Cubic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Cubic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Cubic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Cubic.OutIn(t);
                        default: return EasingFunctions.Cubic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Elastic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Elastic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Elastic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Elastic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Elastic.OutIn(t);
                        default: return EasingFunctions.Elastic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Exponential:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Exponential.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Exponential.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Exponential.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Exponential.OutIn(t);
                        default: return EasingFunctions.Exponential.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Linear:
                    switch (mode)
                    {
                        case EasingMode.In:
                        case EasingMode.Out:
                        case EasingMode.InOut:
                        case EasingMode.OutIn:
                        default: return EasingFunctions.Linear.Identity(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Quadratic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quadratic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quadratic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quadratic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quadratic.OutIn(t);
                        default: return EasingFunctions.Quadratic.InOut(t);
                    }
                //-----------------------------------------------------------
                default:
                case EasingType.Quartic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quartic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quartic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quartic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quartic.OutIn(t);
                        default: return EasingFunctions.Quartic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Quintic:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Quintic.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Quintic.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Quintic.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Quintic.OutIn(t);
                        default: return EasingFunctions.Quintic.InOut(t);
                    }
                //-----------------------------------------------------------
                case EasingType.Sinusoidal:
                    switch (mode)
                    {
                        case EasingMode.In:
                            return EasingFunctions.Sinusoidal.In(t);
                        case EasingMode.Out:
                            return EasingFunctions.Sinusoidal.Out(t);
                        case EasingMode.InOut:
                            return EasingFunctions.Sinusoidal.InOut(t);
                        case EasingMode.OutIn:
                            return EasingFunctions.Sinusoidal.OutIn(t);
                        default: return EasingFunctions.Sinusoidal.InOut(t);
                    }
            }
        }
    }

    internal static class EasingFunctions
    {
        public static class Back
        {
            public static float In(float t)
            {
                return t * t * t - t * Mathf.Sin(t * Mathf.PI);
            }

            public static float Out(float t)
            {
                var x = 1 - t;

                return 1 - (x * x * x - x * Mathf.Sin(x * Mathf.PI));
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 4 * t * t * t - t * Mathf.Sin(2 * t * Mathf.PI)
                    : 1 - (4 * (1 - t) * (1 - t) * (1 - t) - (1 - t) * Mathf.Sin(2 * (1 - t) * Mathf.PI));
            }

            public static float OutIn(float t)
            {
                var x = 2 * t - 1;
                var y = Mathf.Sin(x * Mathf.PI);

                return (t < 0.5f)
                    ? 0.5f * (1 + (x * x * x + x * y))
                    : 0.5f * (x * x * x - x * y + 1);
            }
        }
        public static class Bounce
        {
            public static float In(float t)
            {
                return 1 - Out(1 - t);
            }

            public static float Out(float t)
            {
                const float
                    A = 4.0f / 11.0f,
                    B = 8.0f / 11.0f,
                    C = 9.0f / 10.0f,
                    D = 363.0f / 40.0f,
                    E = 99.0f / 10.0f,
                    F = 17.0f / 5.0f,
                    G = 4356.0f / 361.0f,
                    H = 35442.0f / 1805.0f,
                    I = 16061.0f / 1805.0f,
                    J = 54.0f / 5.0f,
                    K = 513.0f / 25.0f,
                    L = 268.0f / 25.0f;

                if (t < A) return (121.0f * t * t) / 16.0f;

                if (t < B) return (D * t * t) - (E * t) + F;

                if (t < C) return (G * t * t) - (H * t) + I;

                return (J * t * t) - (K * t) + L;
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * In(2 * t)
                    : 0.5f * Out(2 * t - 1) + 0.5f;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * Out(2 * t)
                    : 0.5f * In(2 * t - 1) + 0.5f;
            }
        }
        public static class Circular
        {
            public static float In(float t)
            {
                return -Mathf.Sqrt(1 - (t * t)) + 1;
            }

            public static float Out(float t)
            {
                var x = t - 1;

                return Mathf.Sqrt(1 - (x * x));
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * (-Mathf.Sqrt(1 - (4 * t * t)) + 1)
                    : 0.5f * (Mathf.Sqrt(1 - (4 * (t - 1) * (t - 1))) + 1);
            }

            public static float OutIn(float t)
            {
                var x = (2 * t - 1);
                var y = 1 - (x * x);

                return (t < 0.5f)
                    ? 0.5f * (Mathf.Sqrt(y))
                    : 0.5f * -Mathf.Sqrt(y) + 1;
            }
        }
        public static class Cubic
        {
            public static float In(float t)
            {
                return Mathf.Pow(t, 3);
            }

            public static float Out(float t)
            {
                return Mathf.Pow(t - 1, 3) + 1;
            }

            public static float InOut(float t)
            {
                var x = 2 * t;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Pow(x, 3)
                    : 0.5f * Mathf.Pow(x - 2, 3) + 1;
            }

            public static float OutIn(float t)
            {
                var x = 2 * t - 1;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Pow(x, 3) + 0.5f
                    : 0.5f * (Mathf.Pow(x, 3) + 1);
            }
        }
        public static class Elastic
        {
            public static float In(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                const float A = 13 * HALF_PI;

                return Mathf.Sin(A * t) * Mathf.Pow(2, 10 * (t - 1));
            }

            public static float Out(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                const float A = -13 * HALF_PI;

                return Mathf.Sin(A * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
            }

            public static float InOut(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                const float A = 13 * HALF_PI;

                var x = A * 2 * t;
                var y = 10 * (2 * t - 1);
                var z = Mathf.Sin(x);

                return (t < 0.5f)
                    ? 0.5f * (z * Mathf.Pow(2, y))
                    : 0.5f * (-z * Mathf.Pow(2, -y) + 2);
            }

            public static float OutIn(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                const float A = 13 * HALF_PI;

                var x = A * 2 * t;
                var y = 20 * t;

                return (t < 0.5f)
                    ? 0.5f * (Mathf.Sin(-x - A) * Mathf.Pow(2, -y) + 1)
                    : 0.5f * (Mathf.Sin(x - A) * Mathf.Pow(2, y - 20) + 1);
            }
        }
        public static class Exponential
        {
            public static float In(float t)
            {
                return Mathf.Pow(2, 10 * (t - 1));
            }

            public static float Out(float t)
            {
                return -Mathf.Pow(2, -10 * t) + 1;
            }

            public static float InOut(float t)
            {
                var x = 2 * t - 1;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Pow(2, 10 * x)
                    : 0.5f * -Mathf.Pow(2, -10 * x) + 1;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? 0.5f * (-Mathf.Pow(2, -20 * t) + 1)
                    : 0.5f * (Mathf.Pow(2, 20 * (t - 1)) + 1);
            }
        }
        public static class Linear
        {
            public static float Identity(float t)
            {
                return t;
            }
        }
        public static class Quadratic
        {
            public static float In(float t)
            {
                return t * t;
            }

            public static float Out(float t)
            {
                return -t * (t - 2);
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 2 * t * t
                    : (-2 * t * t) + (4 * t) - 1;
            }

            public static float OutIn(float t)
            {
                return (t < 0.5f)
                    ? (-2 * t * t) + (2 * t)
                    : (2 * t * t) - (2 * t) + 1;
            }
        }
        public static class Quartic
        {
            public static float In(float t)
            {
                return t * t * t * t;
            }

            public static float Out(float t)
            {
                float x = t - 1;

                return x * x * x * (1 - t) + 1;
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 8 * t * t * t * t
                    : -8 * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;
            }

            public static float OutIn(float t)
            {
                float x = 2 * t - 1;

                return (t < 0.5f)
                    ? 0.5f * (-x * x * x * x + 1)
                    : 0.5f * (x * x * x * x + 1);
            }
        }
        public static class Quintic
        {
            public static float In(float t)
            {
                return t * t * t * t * t;
            }

            public static float Out(float t)
            {
                float x = t - 1;

                return x * x * x * x * x + 1;
            }

            public static float InOut(float t)
            {
                return (t < 0.5f)
                    ? 16 * t * t * t * t * t
                    : 16 * (t - 1) * (t - 1) * (t - 1) * (t - 1) * (t - 1) + 1;
            }

            public static float OutIn(float t)
            {
                float x = 2 * t - 1;

                return 0.5f * (x * x * x * x * x + 1);
            }
        }
        public static class Sinusoidal
        {
            public static float In(float t)
            {
                const float HALF_PI = Mathf.PI / 2;

                return 1 - Mathf.Cos(t * HALF_PI);
            }

            public static float Out(float t)
            {
                const float HALF_PI = Mathf.PI / 2;

                return Mathf.Sin(t * HALF_PI);
            }

            public static float InOut(float t)
            {
                return (1 - Mathf.Cos(t * Mathf.PI)) / 2;
            }

            public static float OutIn(float t)
            {
                const float HALF_PI = Mathf.PI / 2;
                var x = 2 * t * HALF_PI;

                return (t < 0.5f)
                    ? 0.5f * Mathf.Sin(x)
                    : 0.5f * (-Mathf.Cos(x - HALF_PI) + 2);
            }
        }
    }

    [Serializable]
    public enum EasingType
    {
        Exponential,
        Linear,
        Quintic,
        Quadratic,
        Sinusoidal,
        Bounce,
        Back,
        Elastic,
        Circular,
        Cubic,
        Quartic,
    }

    [Serializable]
    public enum EasingMode
    {
        In,
        Out,
        InOut,
        OutIn,
    }
}