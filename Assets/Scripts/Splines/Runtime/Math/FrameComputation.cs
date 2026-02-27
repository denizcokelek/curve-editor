using UnityEngine;

public static class FrameComputation
{
    private const float Epsilon = 1e-6f;

    #region Frenet Frame

    public static SplineFrame ComputeFrenetFrame(Vector3 tangent, Vector3 secondDerivative)
    {
        tangent = tangent.normalized;

        if (tangent.sqrMagnitude < Epsilon)
            return SplineFrame.Identity;

        Vector3 binormal = Vector3.Cross(tangent, secondDerivative);

        if (binormal.sqrMagnitude < Epsilon)
        {
            return ComputeFrameWithReferenceUp(tangent, Vector3.up);
        }

        binormal = binormal.normalized;
        Vector3 normal = Vector3.Cross(binormal, tangent).normalized;

        return new SplineFrame(tangent, normal, binormal);
    }

    public static SplineFrame ComputeFrenetFrame(ISplineSegment segment, float t)
    {
        Vector3 tangent = segment.EvaluateDerivative(t);
        Vector3 secondDerivative = segment.EvaluateSecondDerivative(t);
        return ComputeFrenetFrame(tangent, secondDerivative);
    }

    #endregion

    #region Reference Up Frame

    public static SplineFrame ComputeFrameWithReferenceUp(Vector3 tangent, Vector3 referenceUp)
    {
        tangent = tangent.normalized;

        if (tangent.sqrMagnitude < Epsilon)
            return SplineFrame.Identity;

        Vector3 binormal = Vector3.Cross(referenceUp, tangent);

        if (binormal.sqrMagnitude < Epsilon)
        {
            referenceUp = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.99f
                ? Vector3.forward
                : Vector3.up;
            binormal = Vector3.Cross(referenceUp, tangent);
        }

        binormal = binormal.normalized;
        Vector3 normal = Vector3.Cross(tangent, binormal).normalized;

        return new SplineFrame(tangent, normal, binormal);
    }

    #endregion

    #region Rotation Minimizing Frame (RMF)

    public static SplineFrame[] ComputeRotationMinimizingFrames(ISplineSegment segment, int sampleCount, Vector3 initialUp)
    {
        sampleCount = Mathf.Max(2, sampleCount);
        SplineFrame[] frames = new SplineFrame[sampleCount + 1];

        Vector3 tangent0 = segment.EvaluateDerivative(0f).normalized;
        frames[0] = ComputeFrameWithReferenceUp(tangent0, initialUp);

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            frames[i] = PropagateFrame(segment, frames[i - 1], (float)(i - 1) / sampleCount, t);
        }

        return frames;
    }

    public static SplineFrame PropagateFrame(ISplineSegment segment, SplineFrame previousFrame, float previousT, float currentT)
    {
        Vector3 tangent1 = segment.EvaluateDerivative(previousT).normalized;
        Vector3 tangent2 = segment.EvaluateDerivative(currentT).normalized;

        return PropagateFrameDoubleReflection(previousFrame, tangent1, tangent2,
            segment.Evaluate(previousT), segment.Evaluate(currentT));
    }

    public static SplineFrame PropagateFrameDoubleReflection(SplineFrame frame, Vector3 tangent1, Vector3 tangent2,
        Vector3 position1, Vector3 position2)
    {
        Vector3 v1 = position2 - position1;

        if (v1.sqrMagnitude < Epsilon)
            return new SplineFrame(tangent2.normalized, frame.Normal, frame.Binormal);

        float c1 = Vector3.Dot(v1, v1);
        Vector3 riL = frame.Normal - (2f / c1) * Vector3.Dot(v1, frame.Normal) * v1;
        Vector3 tiL = tangent1 - (2f / c1) * Vector3.Dot(v1, tangent1) * v1;

        Vector3 v2 = tangent2 - tiL;
        float c2 = Vector3.Dot(v2, v2);

        Vector3 normal;
        if (c2 < Epsilon)
        {
            normal = riL;
        }
        else
        {
            normal = riL - (2f / c2) * Vector3.Dot(v2, riL) * v2;
        }

        normal = normal.normalized;
        Vector3 binormal = Vector3.Cross(tangent2.normalized, normal).normalized;

        return new SplineFrame(tangent2.normalized, normal, binormal);
    }

    #endregion

    #region Frame Interpolation

    public static SplineFrame InterpolateFrames(SplineFrame a, SplineFrame b, float t)
    {
        return new SplineFrame(
            Vector3.Slerp(a.Tangent, b.Tangent, t).normalized,
            Vector3.Slerp(a.Normal, b.Normal, t).normalized,
            Vector3.Slerp(a.Binormal, b.Binormal, t).normalized
        );
    }

    public static SplineFrame GetFrameFromCache(SplineFrame[] cachedFrames, float t)
    {
        if (cachedFrames == null || cachedFrames.Length < 2)
            return SplineFrame.Identity;

        t = Mathf.Clamp01(t);
        float scaledT = t * (cachedFrames.Length - 1);
        int lowIndex = Mathf.FloorToInt(scaledT);
        int highIndex = Mathf.Min(lowIndex + 1, cachedFrames.Length - 1);
        float ratio = scaledT - lowIndex;

        return InterpolateFrames(cachedFrames[lowIndex], cachedFrames[highIndex], ratio);
    }

    #endregion

    #region Utility

    public static Vector3 GetPerpendicular(Vector3 v)
    {
        v = v.normalized;
        Vector3 axis = Mathf.Abs(v.x) < 0.9f ? Vector3.right : Vector3.up;
        return Vector3.Cross(v, axis).normalized;
    }

    public static Quaternion FrameToQuaternion(SplineFrame frame)
    {
        if (frame.Tangent.sqrMagnitude < Epsilon)
            return Quaternion.identity;

        return Quaternion.LookRotation(frame.Tangent, frame.Normal);
    }

    public static SplineFrame QuaternionToFrame(Quaternion rotation)
    {
        return new SplineFrame(
            rotation * Vector3.forward,
            rotation * Vector3.up,
            rotation * Vector3.right
        );
    }

    #endregion
}
