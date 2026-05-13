using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;

public static class bl_MathUtility
{
    public static Vector3 Gravity => Physics.gravity;

    /// <summary>
    /// Return the normalized intersection value between two vectors
    /// </summary>
    /// <returns></returns>
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Quaternion ClampRotationAroundAxis(Quaternion q, float minimun, float maximun, Axis axis)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angle = 0;
        if (axis == Axis.X)
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angle = Mathf.Clamp(angle, minimun, maximun);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);
        }
        else
        {
            angle = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.y);
            angle = Mathf.Clamp(angle, minimun, maximun);
            q.y = Mathf.Tan(0.5f * Mathf.Deg2Rad * angle);
        }

        return q;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector3 from, Vector3 to)
    {
        Vector3 v = new Vector3(from.x - to.x, from.y - to.y, from.z - to.z);
        return Mathf.Sqrt((v.x * v.x + v.y * v.y + v.z * v.z));
    }

    /// <summary>
    /// Clamp a value in 0-360 where the min can be > to the max angle
    /// </summary>
    /// <returns></returns>
    public static float Clamp360Angle(float angle, float min, float max)
    {
        if (angle <= min)
        {
            if (min > max)
            {
                if (angle > max)
                {
                    angle = GetNearestBetweenTwo(angle, min, max);
                }
            }
            else
            {
                angle = min;
            }
        }
        if (angle >= max)
        {
            if (min > max)
            {
                if (angle < min)
                {
                    angle = GetNearestBetweenTwo(angle, min, max);
                }
            }
            else
            {
                angle = max;
            }
        }
        return angle;
    }

    /// <summary>
    /// Returns the value from where the val is nearest two between the min max
    /// </summary>
    /// <returns></returns>
    public static float GetNearestBetweenTwo(float val, float min, float max)
    {
        float v1 = min - val;
        float v2 = max - val;
        if (Mathf.Abs(v1) < Mathf.Abs(v2)) return min;
        return max;
    }

    /// <summary>
    /// Get ClampAngle
    /// </summary>
    /// <returns></returns>
    public static float ClampAngle(float ang, float min, float max)
    {
        if (ang < -360f)
        {
            ang += 360f;
        }
        if (ang > 360f)
        {
            ang -= 360f;
        }
        return Mathf.Clamp(ang, min, max);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }


    /// <summary>
    /// The angle between dirA and dirB around axis
    /// </summary>
    /// <returns></returns>
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        // Project A and B onto the plane orthogonal target axis
        dirA = dirA - Vector3.Project(dirA, axis);
        dirB = dirB - Vector3.Project(dirB, axis);

        // Find (positive) angle between A and B
        float angle = Vector3.Angle(dirA, dirB);

        // Return angle multiplied with 1 or -1
        return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static float AngleSigned(Vector3 vector1, Vector3 vector2, Vector3 normal)
    {
        return Mathf.Atan2(Vector3.Dot(normal, Vector3.Cross(vector1, vector2)), Vector3.Dot(vector1, vector2)) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool Approximately(float a, float b, float tolerance = 1E-06f)
    {
        return Mathf.Abs(a - b) < tolerance;
    }

    /// <summary>
	/// returns the angle between a look vector and a target position.
	/// can be used for various aiming logic
	/// </summary>
	public static float LookAtAngle(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
    {
        return (Vector3.Cross(fromForward, (toPosition - fromPosition).normalized).y < 0.0f) ?
                -Vector3.Angle(fromForward, (toPosition - fromPosition).normalized) :
                Vector3.Angle(fromForward, (toPosition - fromPosition).normalized);
    }


    /// <summary>
    /// returns the angle between a look vector and a target position as
    /// seen top-down in the cardinal directions. useful for gui pointers
    /// </summary>
    public static float LookAtAngleHorizontal(Vector3 fromPosition, Vector3 fromForward, Vector3 toPosition)
    {
        return LookAtAngle(
            HorizontalVector(fromPosition),
            HorizontalVector(fromForward),
            HorizontalVector(toPosition));
    }

    /// <summary>
	/// Zeroes the y property of a Vector3, for some cases where you want to
	/// make 2D physics calculations.
	/// </summary>
	public static Vector3 HorizontalVector(Vector3 value)
    {
        value.y = 0.0f;
        return value;
    }

    /// <summary>
    /// Calculate the bullet's drag's acceleration
    /// </summary>
    /// <returns></returns>
    public static Vector3 CalculateDrag(Vector3 velocityVec)
    {
        //F_drag = k * v^2 = m * a
        //k = 0.5 * C_d * rho * A 

        float m = 0.2f; // kg
        float C_d = 0.5f;
        float A = Mathf.PI * 0.05f * 0.05f; // m^2
        float rho = 1.225f; // kg/m3

        float k = 0.5f * C_d * rho * A;

        float vSqr = velocityVec.sqrMagnitude;

        float aDrag = (k * vSqr) / m;

        //Has to be in a direction opposite of the bullet's velocity vector
        Vector3 dragVec = aDrag * velocityVec.normalized * -1f;

        return dragVec;
    }

    /// <summary>
    /// Interpolate two values using spring/jiggle effect
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static float Spring(float from, float to, float time)
    {
        time = Mathf.Clamp01(time);
        time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
        return from + (to - from) * time;
    }

    /// <summary>
    /// Interpolate two vectors using spring/jiggle effect
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 Spring(Vector3 from, Vector3 to, float time)
    {
        return new Vector3(Spring(from.x, to.x, time), Spring(from.y, to.y, time), Spring(from.z, to.z, time));
    }

    /// <summary>
    /// Return K/D ratio
    /// </summary>
    /// <param name="kills"></param>
    /// <param name="deaths"></param>
    /// <returns></returns>
    public static float GetKDRatio(int kills, int deaths)
    {
        if (deaths == 0) return kills;
        else return (float)kills / (float)deaths;
    }

    /// <summary>
    /// Rotate a point around the origin
    /// </summary>
    /// <param name="point"></param>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    public static Vector3 RotatePointAroundOrigin(Vector3 point, float angleInDegrees)
    {
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        float cosTheta = Mathf.Cos(angleInRadians);
        float sinTheta = Mathf.Sin(angleInRadians);
        float newX = point.x * cosTheta - point.z * sinTheta;
        float newZ = point.x * sinTheta + point.z * cosTheta;

        return new Vector3(newX, point.y, newZ);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static float DeltaAngle(float current, float target)
    {
        float delta = target - current;
        while (delta > 180) delta -= 360;
        while (delta < -180) delta += 360;
        return delta;
    }

    /// <summary>
    /// Snap a small value to zero
    /// </summary>
    /// <param name="value"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public static float SnapSmallValueToZero(float value, float threshold)
    {
        if (value > 0 && value < threshold)
        {
            return 0f;
        }
        else if (value < 0 && value > -threshold)
        {
            return 0f;
        }
        return value;
    }

    /// <summary>
    /// Normalized influence (0..1) from an explosion at <paramref name="origin"/> to <paramref name="target"/>.
    /// 1 inside <paramref name="innerRadius"/> (full-effect core), smoothly falling to 0 at <paramref name="outerRadius"/>.
    /// Uses squared-distance math (no sqrt). Set falloff=0.5 for ~linear-in-distance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ExplosionInfluence(Vector3 origin, Vector3 target, float innerRadius, float outerRadius, float falloff = 0.5f)
    {
        if (outerRadius <= innerRadius)
            return (target - origin).sqrMagnitude <= innerRadius * innerRadius ? 1f : 0f;

        float r0 = Mathf.Max(0f, innerRadius);
        float r1 = Mathf.Max(r0 + 1e-4f, outerRadius);
        float dsq = (target - origin).sqrMagnitude;

        // Map dsq in [r1^2..r0^2] → t in [0..1], clamped; avoids sqrt.
        float t = Mathf.InverseLerp(r1 * r1, r0 * r0, dsq);

        // Curve shaping: 0.5 ≈ linear-in-distance, 1 = linear-in-r^2, >1 = steeper drop.
        return Mathf.Pow(t, Mathf.Max(0.0001f, falloff));
    }
}