using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    private Spline _spline;
    private Transform _splineTransform;
    private int _selectedPointIndex = -1;

    private bool _showDirections = true;
    private bool _showNormals;
    private bool _showBinormals;
    private bool _showDistanceMarkers;
    private float _distanceMarkerSpacing = 1f;
    private int _directionSampleCount = 10;

    private SerializedProperty _splineTypeProperty;
    private SerializedProperty _frameModeProperty;
    private SerializedProperty _referenceUpProperty;
    private SerializedProperty _resolutionProperty;
    private SerializedProperty _isLoopProperty;
    private SerializedProperty _catmullRomTensionProperty;

    private void OnEnable()
    {
        _spline = (Spline)target;
        _splineTransform = _spline.transform;

        _splineTypeProperty = serializedObject.FindProperty("_splineType");
        _frameModeProperty = serializedObject.FindProperty("_frameMode");
        _referenceUpProperty = serializedObject.FindProperty("_referenceUp");
        _resolutionProperty = serializedObject.FindProperty("_resolution");
        _isLoopProperty = serializedObject.FindProperty("_isLoop");
        _catmullRomTensionProperty = serializedObject.FindProperty("_catmullRomTension");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawSplineSettings();
        EditorGUILayout.Space();
        DrawVisualizationSettings();
        EditorGUILayout.Space();
        DrawSplineInfo();
        EditorGUILayout.Space();
        DrawControlPointList();
        EditorGUILayout.Space();
        DrawActionButtons();

        if (serializedObject.ApplyModifiedProperties())
        {
            _spline.SetDirty();
        }
    }

    private void DrawSplineSettings()
    {
        EditorGUILayout.LabelField("Spline Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_splineTypeProperty);
        EditorGUILayout.PropertyField(_frameModeProperty);

        if (_spline.FrameMode == FrameMode.ReferenceUp)
        {
            EditorGUILayout.PropertyField(_referenceUpProperty);
        }

        EditorGUILayout.PropertyField(_resolutionProperty);
        EditorGUILayout.PropertyField(_isLoopProperty);

        if (_spline.Type == SplineType.CatmullRom)
        {
            EditorGUILayout.PropertyField(_catmullRomTensionProperty);
        }
    }

    private void DrawVisualizationSettings()
    {
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);

        _showDirections = EditorGUILayout.Toggle("Show Directions", _showDirections);

        if (_showDirections)
        {
            EditorGUI.indentLevel++;
            _directionSampleCount = EditorGUILayout.IntSlider("Sample Count", _directionSampleCount, 2, 50);
            _showNormals = EditorGUILayout.Toggle("Show Normals", _showNormals);
            _showBinormals = EditorGUILayout.Toggle("Show Binormals", _showBinormals);
            EditorGUI.indentLevel--;
        }

        _showDistanceMarkers = EditorGUILayout.Toggle("Show Distance Markers", _showDistanceMarkers);
        if (_showDistanceMarkers)
        {
            EditorGUI.indentLevel++;
            _distanceMarkerSpacing = EditorGUILayout.FloatField("Spacing", _distanceMarkerSpacing);
            _distanceMarkerSpacing = Mathf.Max(0.1f, _distanceMarkerSpacing);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawSplineInfo()
    {
        EditorGUILayout.LabelField("Spline Info", EditorStyles.boldLabel);

        _spline.RebuildIfDirty();

        EditorGUILayout.LabelField("Control Points", _spline.ControlPointCount.ToString());
        EditorGUILayout.LabelField("Segments", _spline.SegmentCount.ToString());
        EditorGUILayout.LabelField("Total Length", $"{_spline.TotalLength:F2} units");
    }

    private void DrawControlPointList()
    {
        EditorGUILayout.LabelField("Control Points", EditorStyles.boldLabel);

        for (int i = 0; i < _spline.ControlPointCount; i++)
        {
            EditorGUILayout.BeginHorizontal();

            bool isSelected = _selectedPointIndex == i;
            GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

            if (GUILayout.Button($"Point {i}", GUILayout.Width(70)))
            {
                _selectedPointIndex = i;
                SceneView.RepaintAll();
            }

            GUI.backgroundColor = Color.white;

            SplinePoint point = _spline.GetControlPoint(i);
            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = EditorGUILayout.Vector3Field("", point.Position);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Control Point");
                point.Position = newPosition;
                _spline.SetControlPoint(i, point);
                EditorUtility.SetDirty(_spline);
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Undo.RecordObject(_spline, "Remove Control Point");
                _spline.RemoveControlPoint(i);
                if (_selectedPointIndex >= _spline.ControlPointCount)
                    _selectedPointIndex = _spline.ControlPointCount - 1;
                EditorUtility.SetDirty(_spline);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Point"))
        {
            Undo.RecordObject(_spline, "Add Control Point");

            Vector3 newPosition = Vector3.zero;
            if (_spline.ControlPointCount > 0)
            {
                SplinePoint lastPoint = _spline.GetControlPoint(_spline.ControlPointCount - 1);
                Vector3 direction = _spline.ControlPointCount > 1
                    ? (lastPoint.Position - _spline.GetControlPoint(_spline.ControlPointCount - 2).Position).normalized
                    : Vector3.forward;
                newPosition = lastPoint.Position + direction * 2f;
            }

            _spline.AddControlPoint(new SplinePoint(newPosition));
            _selectedPointIndex = _spline.ControlPointCount - 1;
            EditorUtility.SetDirty(_spline);
        }

        if (GUILayout.Button("Initialize"))
        {
            Undo.RecordObject(_spline, "Initialize Spline");
            _spline.Initialize();
            EditorUtility.SetDirty(_spline);
        }

        if (GUILayout.Button("Force Rebuild"))
        {
            _spline.SetDirty();
            _spline.RebuildIfDirty();
            SceneView.RepaintAll();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {
        _spline = (Spline)target;
        _splineTransform = _spline.transform;

        _spline.RebuildIfDirty();

        DrawCurve();
        DrawHandles();
        DrawVisualization();
        HandleInput();
    }

    private void DrawCurve()
    {
        if (_spline.Type == SplineType.CubicBezier)
        {
            SplineHandles.DrawSplineCurveBezier(_spline);
            SplineHandles.DrawTangentLines(_spline);
        }
        else
        {
            SplineHandles.DrawSplineCurve(_spline);
        }
    }

    private void DrawHandles()
    {
        Quaternion rotation = Tools.pivotRotation == PivotRotation.Local
            ? _splineTransform.rotation
            : Quaternion.identity;

        for (int i = 0; i < _spline.ControlPointCount; i++)
        {
            Vector3 worldPos = _spline.GetControlPointWorldPosition(i);

            float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;
            bool isSelected = _selectedPointIndex == i;

            Handles.color = isSelected ? Color.yellow : new Color(0.2f, 0.8f, 1f, 1f);

            if (Handles.Button(worldPos, rotation, handleSize, handleSize * 1.5f, Handles.SphereHandleCap))
            {
                _selectedPointIndex = i;
                Repaint();
            }

            if (isSelected)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.DoPositionHandle(worldPos, rotation);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Move Control Point");
                    _spline.SetControlPointWorldPosition(i, newWorldPos);
                    EditorUtility.SetDirty(_spline);
                }
            }

            Handles.Label(worldPos + Vector3.up * handleSize * 4f, i.ToString(), EditorStyles.boldLabel);
        }
    }

    private void DrawVisualization()
    {
        if (_showDirections)
        {
            SplineHandles.DrawDirections(_spline, _directionSampleCount, _showNormals, _showBinormals);
        }

        if (_showDistanceMarkers)
        {
            SplineHandles.DrawDistanceMarkers(_spline, _distanceMarkerSpacing);
        }
    }

    private void HandleInput()
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.Delete:
                case KeyCode.Backspace:
                    if (_selectedPointIndex >= 0 && _selectedPointIndex < _spline.ControlPointCount)
                    {
                        Undo.RecordObject(_spline, "Delete Control Point");
                        _spline.RemoveControlPoint(_selectedPointIndex);
                        _selectedPointIndex = Mathf.Min(_selectedPointIndex, _spline.ControlPointCount - 1);
                        EditorUtility.SetDirty(_spline);
                        e.Use();
                    }
                    break;

                case KeyCode.Escape:
                    _selectedPointIndex = -1;
                    Repaint();
                    e.Use();
                    break;
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, _splineTransform.position);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 localPoint = _splineTransform.InverseTransformPoint(hitPoint);

                Undo.RecordObject(_spline, "Add Control Point");
                _spline.AddControlPoint(new SplinePoint(localPoint));
                _selectedPointIndex = _spline.ControlPointCount - 1;
                EditorUtility.SetDirty(_spline);
                e.Use();
            }
        }
    }
}
