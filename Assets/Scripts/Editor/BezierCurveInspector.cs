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
            Vector3 point3 = ShowPoint(3);

            Handles.color = Color.gray;
            Handles.DrawLine(point0, point1);
            Handles.DrawLine(point1, point2);
            Handles.DrawLine(point2, point3);

            Handles.DrawBezier(point0, point3, point1, point2, Color.white, null, 2f);

            ShowDirections();
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _bezierTransform.TransformPoint(_bezierCurve.Points[index]);
            Handles.DoPositionHandle(point, _bezierTransform.rotation);

            return point;
        }

        private void ShowDirections()
        {
            Handles.color = Color.green;
            Vector3 startPoint = _bezierCurve.GetCubicCurvePoint(0f);
            Handles.DrawLine(startPoint, startPoint + _bezierCurve.GetCubicCurveVelocity(0f));
            for (int i = 1; i <= LINESTEPS; i++)
            {
                Vector3 point = _bezierCurve.GetCubicCurvePoint(i / (float) LINESTEPS);
                Handles.DrawLine(point, point + _bezierCurve.GetCubicCurveVelocity(i / (float)LINESTEPS));
            }
        }
    }
}