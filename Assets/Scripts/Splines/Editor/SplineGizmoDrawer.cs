using UnityEngine;
using UnityEditor;

public static class SplineGizmoDrawer
{
    private static readonly Color GizmoCurveColor = new Color(1f, 0.8f, 0.2f, 0.5f);
    private static readonly Color GizmoPointColor = new Color(0.2f, 0.8f, 1f, 0.5f);

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable)]
    private static void DrawSplineGizmoNonSelected(Spline spline, GizmoType gizmoType)
    {
        if (spline.SegmentCount == 0)
        {
            spline.RebuildIfDirty();
            if (spline.SegmentCount == 0) return;
        }

        DrawSimplifiedCurve(spline, 10);
        DrawControlPointGizmos(spline);
    }

    [DrawGizmo(GizmoType.Selected)]
    private static void DrawSplineGizmoSelected(Spline spline, GizmoType gizmoType)
    {
    }

    private static void DrawSimplifiedCurve(Spline spline, int samplesPerSegment)
    {
        Gizmos.color = GizmoCurveColor;

        int totalSamples = Mathf.Max(spline.SegmentCount, 1) * samplesPerSegment;
        Vector3 previousPoint = spline.Evaluate(0f);

        for (int i = 1; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 currentPoint = spline.Evaluate(t);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private static void DrawControlPointGizmos(Spline spline)
    {
        Gizmos.color = GizmoPointColor;

        for (int i = 0; i < spline.ControlPointCount; i++)
        {
            Vector3 worldPos = spline.GetControlPointWorldPosition(i);
            Gizmos.DrawSphere(worldPos, 0.05f);
        }
    }
}
