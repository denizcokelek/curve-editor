using UnityEngine;

public static class BezierMath
{
    #region De Casteljau Algorithm

    public static Vector3 DeCasteljau(Vector3[] points, float t)
    {
        int n = points.Length;
        if (n == 0) return Vector3.zero;
        if (n == 1) return points[0];

        Vector3[] temp = new Vector3[n];
        System.Array.Copy(points, temp, n);

        for (int level = n - 1; level > 0; level--)
        {
            for (int i = 0; i < level; i++)
            {
                temp[i] = Vector3.Lerp(temp[i], temp[i + 1], t);
            }
        }

        return temp[0];
    }

    public static Vector3 DeCasteljauDerivative(Vector3[] points, float t)
    {
        int n = points.Length;
        if (n < 2) return Vector3.zero;

        Vector3[] derivativePoints = new Vector3[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            derivativePoints[i] = (n - 1) * (points[i + 1] - points[i]);
        }

        return DeCasteljau(derivativePoints, t);
    }

    public static Vector3 DeCasteljauSecondDerivative(Vector3[] points, float t)
    {
        int n = points.Length;
        if (n < 3) return Vector3.zero;

        Vector3[] firstDerivativePoints = new Vector3[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            firstDerivativePoints[i] = (n - 1) * (points[i + 1] - points[i]);
        }

        Vector3[] secondDerivativePoints = new Vector3[n - 2];
        for (int i = 0; i < n - 2; i++)
        {
            secondDerivativePoints[i] = (n - 2) * (firstDerivativePoints[i + 1] - firstDerivativePoints[i]);
        }

        return DeCasteljau(secondDerivativePoints, t);
    }

    #endregion

    #region Polynomial Form - Linear

    public static Vector3 EvaluateLinear(Vector3 p0, Vector3 p1, float t)
    {
        return p0 + t * (p1 - p0);
    }

    public static Vector3 EvaluateLinearDerivative(Vector3 p0, Vector3 p1)
    {
        return p1 - p0;
    }

    #endregion

    #region Polynomial Form - Quadratic Bezier

    public static Vector3 EvaluateQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 +
               2f * oneMinusT * t * p1 +
               t * t * p2;
    }

    public static Vector3 EvaluateQuadraticDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return 2f * oneMinusT * (p1 - p0) +
               2f * t * (p2 - p1);
    }

    public static Vector3 EvaluateQuadraticSecondDerivative(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return 2f * (p2 - 2f * p1 + p0);
    }

    #endregion

    #region Polynomial Form - Cubic Bezier

    public static Vector3 EvaluateCubic(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float oneMinusT = 1f - t;
        float oneMinusT2 = oneMinusT * oneMinusT;
        float oneMinusT3 = oneMinusT2 * oneMinusT;
        float t2 = t * t;
        float t3 = t2 * t;

        return oneMinusT3 * p0 +
               3f * oneMinusT2 * t * p1 +
               3f * oneMinusT * t2 * p2 +
               t3 * p3;
    }

    public static Vector3 EvaluateCubicDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float oneMinusT = 1f - t;
        float oneMinusT2 = oneMinusT * oneMinusT;
        float t2 = t * t;

        return 3f * oneMinusT2 * (p1 - p0) +
               6f * oneMinusT * t * (p2 - p1) +
               3f * t2 * (p3 - p2);
    }

    public static Vector3 EvaluateCubicSecondDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float oneMinusT = 1f - t;

        return 6f * oneMinusT * (p2 - 2f * p1 + p0) +
               6f * t * (p3 - 2f * p2 + p1);
    }

    #endregion

    #region Catmull-Rom

    public static Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 tangent1 = tension * (p2 - p0);
        Vector3 tangent2 = tension * (p3 - p1);

        float h1 = 2f * t3 - 3f * t2 + 1f;
        float h2 = -2f * t3 + 3f * t2;
        float h3 = t3 - 2f * t2 + t;
        float h4 = t3 - t2;

        return h1 * p1 + h2 * p2 + h3 * tangent1 + h4 * tangent2;
    }

    public static Vector3 EvaluateCatmullRomDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f)
    {
        float t2 = t * t;

        Vector3 tangent1 = tension * (p2 - p0);
        Vector3 tangent2 = tension * (p3 - p1);

        float dh1 = 6f * t2 - 6f * t;
        float dh2 = -6f * t2 + 6f * t;
        float dh3 = 3f * t2 - 4f * t + 1f;
        float dh4 = 3f * t2 - 2f * t;

        return dh1 * p1 + dh2 * p2 + dh3 * tangent1 + dh4 * tangent2;
    }

    public static Vector3 EvaluateCatmullRomSecondDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float tension = 0.5f)
    {
        Vector3 tangent1 = tension * (p2 - p0);
        Vector3 tangent2 = tension * (p3 - p1);

        float ddh1 = 12f * t - 6f;
        float ddh2 = -12f * t + 6f;
        float ddh3 = 6f * t - 4f;
        float ddh4 = 6f * t - 2f;

        return ddh1 * p1 + ddh2 * p2 + ddh3 * tangent1 + ddh4 * tangent2;
    }

    public static void CatmullRomToCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tension,
        out Vector3 b0, out Vector3 b1, out Vector3 b2, out Vector3 b3)
    {
        b0 = p1;
        b1 = p1 + tension * (p2 - p0) / 3f;
        b2 = p2 - tension * (p3 - p1) / 3f;
        b3 = p2;
    }

    #endregion

    #region Utility

    public static float EstimateArcLength(ISplineSegment segment, int samples)
    {
        if (samples < 2) samples = 2;

        float length = 0f;
        Vector3 previousPoint = segment.Evaluate(0f);

        for (int i = 1; i <= samples; i++)
        {
            float t = (float)i / samples;
            Vector3 currentPoint = segment.Evaluate(t);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return length;
    }

    public static float EstimateArcLengthIntegration(ISplineSegment segment, int samples)
    {
        if (samples < 2) samples = 2;

        float length = 0f;
        float step = 1f / samples;

        for (int i = 0; i < samples; i++)
        {
            float t0 = (float)i / samples;
            float t1 = (float)(i + 1) / samples;
            float tMid = (t0 + t1) * 0.5f;

            float v0 = segment.EvaluateDerivative(t0).magnitude;
            float v1 = segment.EvaluateDerivative(t1).magnitude;
            float vMid = segment.EvaluateDerivative(tMid).magnitude;

            length += (step / 6f) * (v0 + 4f * vMid + v1);
        }

        return length;
    }

    #endregion
}
