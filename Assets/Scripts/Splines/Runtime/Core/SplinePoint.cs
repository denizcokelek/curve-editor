using System;
using UnityEngine;

[Serializable]
public struct SplinePoint
{
    public Vector3 Position;
    public Vector3 TangentIn;
    public Vector3 TangentOut;
    public bool AutoTangent;

    public SplinePoint(Vector3 position)
    {
        Position = position;
        TangentIn = Vector3.zero;
        TangentOut = Vector3.zero;
        AutoTangent = true;
    }

    public SplinePoint(Vector3 position, Vector3 tangentIn, Vector3 tangentOut)
    {
        Position = position;
        TangentIn = tangentIn;
        TangentOut = tangentOut;
        AutoTangent = false;
    }
}
