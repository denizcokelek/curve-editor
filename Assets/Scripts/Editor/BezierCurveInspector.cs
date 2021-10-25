using System;
using UnityEditor;
using UnityEngine;

namespace Project
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveInspector : UnityEditor.Editor
    {
        private BezierCurve _bezierCurve;
        private Transform _bezierTransform;
        private const int LINESTEPS = 10;

        private void OnSceneGUI()
        {
            _bezierCurve = target as BezierCurve;

            _bezierTransform = _bezierCurve.transform;

            Vector3 point0 = ShowPoint(0);
            Vector3 point1 = ShowPoint(1);
            Vector3 point2 = ShowPoint(2);

            Handles.color = Color.gray;
            Handles.DrawLine(point0, point1);
            Handles.DrawLine(point1, point2);

            Handles.color = Color.white;
            Vector3 lineStart = _bezierCurve.GetCurvePoint(0f);
            for (int i = 1; i <= LINESTEPS; i++)
            {
                Vector3 lineEnd = _bezierCurve.GetCurvePoint(i / (float)LINESTEPS);
                Handles.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _bezierTransform.transform.TransformPoint(_bezierCurve.Points[index]);
            Handles.DoPositionHandle(point, _bezierTransform.transform.rotation);

            _bezierCurve.Points[index] = _bezierTransform.transform.InverseTransformPoint(point);

            return point;
        }
    }
}