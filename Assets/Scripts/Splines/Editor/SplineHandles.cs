using UnityEngine;
using UnityEditor;

public static class SplineHandles
{
    private static readonly Color CurveColor = new Color(1f, 0.8f, 0.2f, 1f);
    private static readonly Color TangentLineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    private static readonly Color ControlPointColor = new Color(0.2f, 0.8f, 1f, 1f);
    private static readonly Color TangentPointColor = new Color(1f, 0.5f, 0.2f, 1f);
    private static readonly Color DirectionColor = new Color(0.2f, 1f, 0.3f, 0.8f);
    private static readonly Color NormalColor = new Color(1f, 0.3f, 0.3f, 0.8f);
    private static readonly Color BinormalColor = new Color(0.3f, 0.3f, 1f, 0.8f);

    private const float ControlPointHandleSize = 0.08f;
    private const float TangentHandleSize = 0.05f;
    private const float DirectionArrowLength = 0.5f;

    public static void DrawSplineCurve(Spline spline, int samplesPerSegment = 20)
    {
        if (spline.SegmentCount == 0) return;

        Handles.color = CurveColor;

        int totalSamples = spline.SegmentCount * samplesPerSegment;
        Vector3 previousPoint = spline.Evaluate(0f);

        for (int i = 1; i <= totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            Vector3 currentPoint = spline.Evaluate(t);
            Handles.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    public static void DrawSplineCurveBezier(Spline spline)
    {
        if (spline.SegmentCount == 0 || spline.Type != SplineType.CubicBezier) return;

        for (int i = 0; i < spline.SegmentCount; i++)
        {
            ISplineSegment segment = spline.GetSegment(i);
            Vector3[] points = segment.GetControlPoints();

            if (points.Length != 4) continue;

            Vector3 p0 = spline.transform.TransformPoint(points[0]);
            Vector3 p1 = spline.transform.TransformPoint(points[1]);
            Vector3 p2 = spline.transform.TransformPoint(points[2]);
            Vector3 p3 = spline.transform.TransformPoint(points[3]);

            Handles.DrawBezier(p0, p3, p1, p2, CurveColor, null, 2f);
        }
    }

    public static bool DrawControlPointHandle(Vector3 position, Quaternion rotation, float size, out Vector3 newPosition)
    {
        float handleSize = HandleUtility.GetHandleSize(position) * size;

        Handles.color = ControlPointColor;
        if (Handles.Button(position, rotation, handleSize, handleSize * 1.2f, Handles.SphereHandleCap))
        {
        }

        EditorGUI.BeginChangeCheck();
        newPosition = Handles.PositionHandle(position, rotation);
        return EditorGUI.EndChangeCheck();
    }

    public static bool DrawTangentHandle(Vector3 anchorPosition, Vector3 tangentPosition, Quaternion rotation, float size, out Vector3 newTangentPosition)
    {
        Handles.color = TangentLineColor;
        Handles.DrawLine(anchorPosition, tangentPosition);

        float handleSize = HandleUtility.GetHandleSize(tangentPosition) * size;

        Handles.color = TangentPointColor;
        Handles.SphereHandleCap(0, tangentPosition, rotation, handleSize, EventType.Repaint);

        EditorGUI.BeginChangeCheck();
        newTangentPosition = Handles.PositionHandle(tangentPosition, rotation);
        return EditorGUI.EndChangeCheck();
    }

    public static void DrawControlPoints(Spline spline)
    {
        Transform transform = spline.transform;
        Quaternion rotation = Tools.pivotRotation == PivotRotation.Local ? transform.rotation : Quaternion.identity;

        for (int i = 0; i < spline.ControlPointCount; i++)
        {
            Vector3 worldPos = spline.GetControlPointWorldPosition(i);
            float handleSize = HandleUtility.GetHandleSize(worldPos) * ControlPointHandleSize;

            Handles.color = ControlPointColor;
            Handles.SphereHandleCap(0, worldPos, rotation, handleSize, EventType.Repaint);

            Handles.Label(worldPos + Vector3.up * handleSize * 3f, i.ToString(), EditorStyles.boldLabel);
        }
    }

    public static void DrawDirections(Spline spline, int sampleCount = 10, bool showNormals = false, bool showBinormals = false)
    {
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            SplineSample sample = spline.EvaluateWithFrame(t);
            float arrowLength = HandleUtility.GetHandleSize(sample.Position) * DirectionArrowLength;

            Handles.color = DirectionColor;
            Handles.DrawLine(sample.Position, sample.Position + sample.Tangent * arrowLength);

            if (showNormals)
            {
                Handles.color = NormalColor;
                Handles.DrawLine(sample.Position, sample.Position + sample.Normal * arrowLength * 0.5f);
            }

            if (showBinormals)
            {
                Handles.color = BinormalColor;
                Handles.DrawLine(sample.Position, sample.Position + sample.Binormal * arrowLength * 0.5f);
            }
        }
    }

    public static void DrawDistanceMarkers(Spline spline, float spacing = 1f, float markerSize = 0.1f)
    {
        float totalLength = spline.GetLength();
        if (totalLength <= 0f) return;

        int markerCount = Mathf.FloorToInt(totalLength / spacing);

        for (int i = 0; i <= markerCount; i++)
        {
            float distance = i * spacing;
            SplineSample sample = spline.EvaluateByDistanceWithFrame(distance);
            float size = HandleUtility.GetHandleSize(sample.Position) * markerSize;

            Handles.color = Color.white;
            Handles.DrawWireDisc(sample.Position, sample.Tangent, size);
        }
    }

    public static void DrawTangentLines(Spline spline)
    {
        if (spline.Type != SplineType.CubicBezier) return;

        Transform transform = spline.transform;
        Handles.color = TangentLineColor;

        for (int i = 0; i < spline.SegmentCount; i++)
        {
            ISplineSegment segment = spline.GetSegment(i);
            Vector3[] points = segment.GetControlPoints();

            if (points.Length != 4) continue;

            Vector3 p0 = transform.TransformPoint(points[0]);
            Vector3 p1 = transform.TransformPoint(points[1]);
            Vector3 p2 = transform.TransformPoint(points[2]);
            Vector3 p3 = transform.TransformPoint(points[3]);

            Handles.DrawDottedLine(p0, p1, 4f);
            Handles.DrawDottedLine(p2, p3, 4f);
        }
    }

    public static void DrawFrameGizmo(SplineSample sample, float size = 0.3f)
    {
        size *= HandleUtility.GetHandleSize(sample.Position);

        Handles.color = DirectionColor;
        Handles.ArrowHandleCap(0, sample.Position, Quaternion.LookRotation(sample.Tangent), size, EventType.Repaint);

        Handles.color = NormalColor;
        Handles.DrawLine(sample.Position, sample.Position + sample.Normal * size * 0.7f);

        Handles.color = BinormalColor;
        Handles.DrawLine(sample.Position, sample.Position + sample.Binormal * size * 0.7f);
    }
}
