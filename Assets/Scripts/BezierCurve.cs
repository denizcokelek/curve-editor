using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Project
{
    public class BezierCurve : MonoBehaviour
    {
        public Vector3[] Points;

        public void Reset()
        {
            Points = new Vector3[]
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

                Vector3 point = oneMinusT * oneMinusT * Points[0] + 
                                2 * oneMinusT * t * Points[1] +
                                t * t * Points[2];

                return transform.TransformPoint(point);
            }

            public Vector3 GetDirection(float t)
            {
                Vector3 velocity = 2 * (t - 1) * (Points[0] - Points[1]) + 
                                   2 * t * (Points[2] - Points[1]) - 
                                   transform.position;

                return transform.TransformPoint(velocity).normalized;
            }

        #endregion

        #region Cube Curve

        public Vector3 GetCubicCurvePoint(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = oneMinusT * oneMinusT * oneMinusT * Points[0] +
                            3f * oneMinusT * oneMinusT * t * Points[1] +
                            3f * oneMinusT * t * t * Points[2] +
                            t * t * t * Points[3];

            return transform.TransformPoint(point);
        }

        public Vector3 GetCubicCurveVelocity(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = 3f * oneMinusT * oneMinusT * (Points[1] - Points[0]) +
                            6f * oneMinusT * t * (Points[2] - Points[1]) +
                            3 * t * t * (Points[3] - Points[2]);

            return transform.TransformPoint(point).normalized;
        }

        #endregion
        
    }
}
