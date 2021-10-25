using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Project
{
    public class BezierSpline : MonoBehaviour
    {
        public Vector3[] Points;

        public void Reset()
        {
            Points = new Vector3[]
            {
                new Vector3(1f, 0, 0),
                new Vector3(2f, 0, 0),
                new Vector3(3f, 0, 0),
                new Vector3(4f, 0, 0),
            };
        }

        public void AddCurve()
        {
            Vector3 point = Points[Points.Length - 1];

            Array.Resize(ref Points, Points.Length +3);

            for (int i = 0; i < 3; i++)
            {
                point.x += 1;
                Points[Points.Length - (3 - i)] = point;
            }
        }

        #region Quadratic Curves

            public Vector3 GetCurvePoint(float t)
            {
                t = Mathf.Clamp01(t);
                float oneMinusT = 1 - t;

                Vector3 point = oneMinusT * oneMinusT * Points[0] + 
                                2 * oneMinusT * t * Points[1] +
                                t * t * Points[2];

                return transform.TransformPoint(point);
            }

            public Vector3 GetVelocity(float t)
            {
                Vector3 velocity = 2 * (t - 1) * (Points[0] - Points[1]) + 2 * t * (Points[2] - Points[1]) - transform.position;

                return transform.TransformPoint(velocity).normalized;
            }

        #endregion

        #region Cube Curbes

        public Vector3 GetCubicCurvePoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = oneMinusT * oneMinusT * oneMinusT * p0 +
                            3f * oneMinusT * oneMinusT * t * p1 +
                            3f * oneMinusT * t * t * p2 +
                            t * t * t * p3;

            return point;
        }

        public Vector3 GetCubicCurveVelocity(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = 3f * oneMinusT * oneMinusT * (p1 - p0) +
                            6f * oneMinusT * t * (p2 - p1) +
                            3 * t * t * (p3 - p2);

            return point.normalized;
        }

        #endregion
        
    }
}
