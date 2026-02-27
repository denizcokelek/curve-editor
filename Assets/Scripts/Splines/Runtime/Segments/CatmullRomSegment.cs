using System;
using UnityEngine;

[Serializable]
public class CatmullRomSegment : ISplineSegment
{
    [SerializeField] private Vector3 _p0;
    [SerializeField] private Vector3 _p1;
    [SerializeField] private Vector3 _p2;
    [SerializeField] private Vector3 _p3;
    [SerializeField] private float _tension;

    public int ControlPointCount => 4;

    public float Tension
    {
        get => _tension;
        set => _tension = Mathf.Clamp01(value);
    }

    public CatmullRomSegment()
    {
        _p0 = new Vector3(0f, 0f, -1f);
        _p1 = Vector3.zero;
        _p2 = Vector3.forward;
        _p3 = new Vector3(0f, 0f, 2f);
        _tension = 0.5f;
    }

    public CatmullRomSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tension = 0.5f)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
        _p3 = p3;
        _tension = tension;
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCatmullRom(_p0, _p1, _p2, _p3, t, _tension);
    }

    public Vector3 EvaluateDerivative(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCatmullRomDerivative(_p0, _p1, _p2, _p3, t, _tension);
    }

    public Vector3 EvaluateSecondDerivative(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCatmullRomSecondDerivative(_p0, _p1, _p2, _p3, t, _tension);
    }

    public Vector3[] GetControlPoints()
    {
        return new Vector3[] { _p0, _p1, _p2, _p3 };
    }

    public void SetControlPoints(Vector3[] points)
    {
        if (points == null || points.Length < 4)
            throw new ArgumentException("Catmull-Rom segment requires 4 control points");

        _p0 = points[0];
        _p1 = points[1];
        _p2 = points[2];
        _p3 = points[3];
    }

    public void SetControlPoint(int index, Vector3 position)
    {
        switch (index)
        {
            case 0: _p0 = position; break;
            case 1: _p1 = position; break;
            case 2: _p2 = position; break;
            case 3: _p3 = position; break;
            default: throw new IndexOutOfRangeException();
        }
    }

    public CubicBezierSegment ToCubicBezier()
    {
        BezierMath.CatmullRomToCubicBezier(_p0, _p1, _p2, _p3, _tension,
            out Vector3 b0, out Vector3 b1, out Vector3 b2, out Vector3 b3);
        return new CubicBezierSegment(b0, b1, b2, b3);
    }
}
