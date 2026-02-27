using System;
using UnityEngine;

[Serializable]
public class LinearSegment : ISplineSegment
{
    [SerializeField] private Vector3 _p0;
    [SerializeField] private Vector3 _p1;

    public int ControlPointCount => 2;

    public LinearSegment()
    {
        _p0 = Vector3.zero;
        _p1 = Vector3.forward;
    }

    public LinearSegment(Vector3 p0, Vector3 p1)
    {
        _p0 = p0;
        _p1 = p1;
    }

    public Vector3 Evaluate(float t)
    {
        t = Mathf.Clamp01(t);
        return BezierMath.EvaluateLinear(_p0, _p1, t);
    }

    public Vector3 EvaluateDerivative(float t)
    {
        return BezierMath.EvaluateLinearDerivative(_p0, _p1);
    }

    public Vector3 EvaluateSecondDerivative(float t)
    {
        return Vector3.zero;
    }

    public Vector3[] GetControlPoints()
    {
        return new Vector3[] { _p0, _p1 };
    }

    public void SetControlPoints(Vector3[] points)
    {
        if (points == null || points.Length < 2)
            throw new ArgumentException("Linear segment requires 2 control points");

        _p0 = points[0];
        _p1 = points[1];
    }

    public void SetControlPoint(int index, Vector3 position)
    {
        switch (index)
        {
            case 0: _p0 = position; break;
            case 1: _p1 = position; break;
            default: throw new IndexOutOfRangeException();
        }
    }

    public float GetLength()
    {
        return Vector3.Distance(_p0, _p1);
    }
}
