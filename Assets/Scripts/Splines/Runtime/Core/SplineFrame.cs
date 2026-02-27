using System;
using UnityEngine;

[Serializable]
public struct SplineFrame
{
    public Vector3 Tangent;
    public Vector3 Normal;
    public Vector3 Binormal;

    public SplineFrame(Vector3 tangent, Vector3 normal, Vector3 binormal)
    {
        Tangent = tangent;
        Normal = normal;
        Binormal = binormal;
    }

    public Quaternion Rotation
    {
        get
        {
            if (Tangent == Vector3.zero)
                return Quaternion.identity;
            return Quaternion.LookRotation(Tangent, Normal);
        }
    }

    public static SplineFrame Identity => new SplineFrame(Vector3.forward, Vector3.up, Vector3.right);
}
