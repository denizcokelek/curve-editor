using System;
using UnityEngine;

[Serializable]
public struct SplineSample
{
    public Vector3 Position;
    public SplineFrame Frame;
    public float T;
    public float Distance;

    public SplineSample(Vector3 position, SplineFrame frame, float t, float distance)
    {
        Position = position;
        Frame = frame;
        T = t;
        Distance = distance;
    }

    public Vector3 Tangent => Frame.Tangent;
    public Vector3 Normal => Frame.Normal;
    public Vector3 Binormal => Frame.Binormal;
    public Quaternion Rotation => Frame.Rotation;

    public static SplineSample Lerp(SplineSample a, SplineSample b, float t)
    {
        return new SplineSample(
            Vector3.Lerp(a.Position, b.Position, t),
            new SplineFrame(
                Vector3.Slerp(a.Frame.Tangent, b.Frame.Tangent, t).normalized,
                Vector3.Slerp(a.Frame.Normal, b.Frame.Normal, t).normalized,
                Vector3.Slerp(a.Frame.Binormal, b.Frame.Binormal, t).normalized
            ),
            Mathf.Lerp(a.T, b.T, t),
            Mathf.Lerp(a.Distance, b.Distance, t)
        );
    }
}
