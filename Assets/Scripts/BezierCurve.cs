using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
                new Vector3(2f, 0, 0),
                new Vector3(3f, 0, 0),
            };
        }

        public Vector3 GetCurvePoint(float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1 - t;

            Vector3 point = oneMinusT * oneMinusT * Points[0] + 
                            2 * oneMinusT * t * Points[1] +
                            t * t * Points[2];

            return transform.TransformPoint(point);
        }
    }
}
