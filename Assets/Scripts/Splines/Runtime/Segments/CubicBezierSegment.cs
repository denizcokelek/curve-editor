using System;
using UnityEngine;

[Serializable]
public class CubicBezierSegment : ISplineSegment
{
    [SerializeField] private Vector3 _p0;
    [SerializeField] private Vector3 _p1;
    [SerializeField] private Vector3 _p2;
    [SerializeField] private Vector3 _p3;

    public int ControlPointCount => 4;

    public Vector3 P0 => _p0;
    public Vector3 P1 => _p1;
    public Vector3 P2 => _p2;
    public Vector3 P3 => _p3;

    public CubicBezierSegment()
    {
        _p0 = Vector3.zero;
        _p1 = new Vector3(0f, 0f, 0.33f);
        _p2 = new Vector3(0f, 0f, 0.66f);
        _p3 = Vector3.forward;
    }

    public CubicBezierSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
        _p3 = p3;
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCubic(_p0, _p1, _p2, _p3, t);
    }

    public Vector3 EvaluateDerivative(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCubicDerivative(_p0, _p1, _p2, _p3, t);
    }

    public Vector3 EvaluateSecondDerivative(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateCubicSecondDerivative(_p0, _p1, _p2, _p3, t);
    }

    public Vector3[] GetControlPoints()
    {
        return new Vector3[] { _p0, _p1, _p2, _p3 };
    }

    public void SetControlPoints(Vector3[] points)
    {
        if (points == null || points.Length < 4)
            throw new ArgumentException("Cubic Bezier segment requires 4 control points");

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

    public void Split(float t, out CubicBezierSegment left, out CubicBezierSegment right)
    {
        t = Mathf.Clamp01(t);

        Vector3 q0 = Vector3.Lerp(_p0, _p1, t);
        Vector3 q1 = Vector3.Lerp(_p1, _p2, t);
        Vector3 q2 = Vector3.Lerp(_p2, _p3, t);

        Vector3 r0 = Vector3.Lerp(q0, q1, t);
        Vector3 r1 = Vector3.Lerp(q1, q2, t);

        Vector3 s = Vector3.Lerp(r0, r1, t);

        left = new CubicBezierSegment(_p0, q0, r0, s);
        right = new CubicBezierSegment(s, r1, q2, _p3);
    }
}
