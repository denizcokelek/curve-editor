using System;
using UnityEngine;

[Serializable]
public class QuadraticBezierSegment : ISplineSegment
{
    [SerializeField] private Vector3 _p0;
    [SerializeField] private Vector3 _p1;
    [SerializeField] private Vector3 _p2;

    public int ControlPointCount => 3;

    public QuadraticBezierSegment()
    {
        _p0 = Vector3.zero;
        _p1 = new Vector3(0.5f, 0.5f, 0.5f);
        _p2 = Vector3.forward;
    }

    public QuadraticBezierSegment(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        _p0 = p0;
        _p1 = p1;
        _p2 = p2;
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateQuadratic(_p0, _p1, _p2, t);
    }

    public Vector3 EvaluateDerivative(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateQuadraticDerivative(_p0, _p1, _p2, t);
    }

    public Vector3 EvaluateSecondDerivative(float t)
    {
        return BezierMath.EvaluateQuadraticSecondDerivative(_p0, _p1, _p2);
    }

    public Vector3[] GetControlPoints()
    {
        return new Vector3[] { _p0, _p1, _p2 };
    }

    public void SetControlPoints(Vector3[] points)
    {
        if (points == null || points.Length < 3)
            throw new ArgumentException("Quadratic Bezier segment requires 3 control points");

        _p0 = points[0];
        _p1 = points[1];
        _p2 = points[2];
    }

    public void SetControlPoint(int index, Vector3 position)
    {
        switch (index)
        {
            case 0: _p0 = position; break;
            case 1: _p1 = position; break;
            case 2: _p2 = position; break;
            default: throw new IndexOutOfRangeException();
        }
    }
}
