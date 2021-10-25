using System;
using UnityEditor;
using UnityEngine;

namespace Project
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInpsector : UnityEditor.Editor
    {
        private BezierSpline _bezierSplineCurve;
        private Transform _bezierSplineTransform;
        private const int LINESTEPS = 10;

        private void OnSceneGUI()
        {
            _bezierSplineCurve = target as BezierSpline;

            _bezierSplineTransform = _bezierSplineCurve.transform;

            Handles.color = Color.gray;

            Vector3 p0 = ShowPoint(0);
            for (int i = 1; i < _bezierSplineCurve.Points.Length; i += 3) {
                Handles.color = Color.gray;
                Vector3 p1 = ShowPoint(i);
                Vector3 p2 = ShowPoint(i + 1);
                Vector3 p3 = ShowPoint(i + 2);
			
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);
			
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);

                Handles.color = Color.green;
                Vector3 startPoint = _bezierSplineCurve.GetCubicCurvePoint(p0, p1, p2, p3, 0f);
                Handles.DrawLine(startPoint, startPoint + _bezierSplineCurve.GetCubicCurveVelocity(p0, p1, p2, p3, 0f));
                for (int j = 1; j <= LINESTEPS; j++)
                {
                    Vector3 point = _bezierSplineCurve.GetCubicCurvePoint(p0, p1, p2, p3,j / (float) LINESTEPS);
                    Handles.DrawLine(point, point + _bezierSplineCurve.GetCubicCurveVelocity(p0, p1, p2, p3, j / (float)LINESTEPS));
                }

                p0 = p3;
            }
            //Handles.DrawBezier(point0, point3, point1, point2, Color.white, null, 2f);
            //ShowDirections();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _bezierSplineCurve = target as BezierSpline;
            if (GUILayout.Button("Add Curve"))
            {
                _bezierSplineCurve.AddCurve();
            }
        }

        private Vector3 ShowPoint(int index)
        {
            Vector3 point = _bezierSplineTransform.TransformPoint(_bezierSplineCurve.Points[index]);
            Handles.DoPositionHandle(point, _bezierSplineTransform.rotation);

            return point;
        }

        //private void ShowDirections()
        //{
        //    Handles.color = Color.green;
        //    Vector3 startPoint = _bezierSplineCurve.GetCubicCurvePoint(0f);
        //    Handles.DrawLine(startPoint, startPoint + _bezierSplineCurve.GetCubicCurveVelocity(0f));
        //    for (int i = 1; i <= LINESTEPS; i++)
        //    {
        //        Vector3 point = _bezierSplineCurve.GetCubicCurvePoint(i / (float) LINESTEPS);
        //        Handles.DrawLine(point, point + _bezierSplineCurve.GetCubicCurveVelocity(i / (float)LINESTEPS));
        //    }
        //}
    }
}