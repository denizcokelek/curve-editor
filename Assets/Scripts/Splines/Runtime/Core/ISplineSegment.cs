using UnityEngine;

public interface ISplineSegment
{
    Vector3 Evaluate(float t);
    Vector3 EvaluateDerivative(float t);
    Vector3 EvaluateSecondDerivative(float t);
    Vector3[] GetControlPoints();
    void SetControlPoints(Vector3[] points);
    int ControlPointCount { get; }
}
