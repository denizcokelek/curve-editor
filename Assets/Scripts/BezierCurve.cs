using System;
using UnityEngine;

namespace Project
{
    public class BezierCurve : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] _points;

        public Vector3 GetPoint (int index) {
            return _points[index];
        }

        private void Awake()
        {
            Reset();
        }

        public void Reset()
        {
            _points = new Vector3[]
            {
                new Vector3(1f, 0, 0),
                new Vector3(2f, 0, 1.5f),
                new Vector3(3f, 0, 0),
            };
        }

        #region Quadratic Curve

            public Vector3 GetPointAtCurve(float t)
            {
                t = Mathf.Clamp01(t);
                float oneMinusT = 1 - t;

                Vector3 point = oneMinusT * oneMinusT * _points[0] + 
                                2 * oneMinusT * t * _points[1] +
                                t * t * _points[2];

                return transform.TransformPoint(point);
            }

            public Vector3 GetDirection(float t)
            {
                Vector3 velocity = 2 * (t - 1) * (_points[0] - _points[1]) + 
                                   2 * t * (_points[2] - _points[1]) - 
                                   transform.position;

                return transform.TransformPoint(velocity).normalized;
            }

        #endregion

        #region Cubic Curve

        public Vector3 GetCubicCurvePoint(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = oneMinusT * oneMinusT * oneMinusT * _points[0] +
                            3f * oneMinusT * oneMinusT * t * _points[1] +
                            3f * oneMinusT * t * t * _points[2] +
                            t * t * t * _points[3];

            return transform.TransformPoint(point);
        }

        public Vector3 GetCubicCurveVelocity(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = 3f * oneMinusT * oneMinusT * (_points[1] - _points[0]) +
                            6f * oneMinusT * t * (_points[2] - _points[1]) +
                            3 * t * t * (_points[3] - _points[2]);

            return transform.TransformPoint(point).normalized;
        }

        #endregion
        
    }
}
