using System;
using System.Collections.Generic;
using UnityEngine;

public enum SplineType
{
    Linear,
    QuadraticBezier,
    CubicBezier,
    CatmullRom
}

public enum FrameMode
{
    Frenet,
    RotationMinimizing,
    ReferenceUp
}

public class Spline : MonoBehaviour
{
    [SerializeField] private List<SplinePoint> _controlPoints = new List<SplinePoint>();
    [SerializeField] private SplineType _splineType = SplineType.CubicBezier;
    [SerializeField] private FrameMode _frameMode = FrameMode.RotationMinimizing;
    [SerializeField] private Vector3 _referenceUp = Vector3.up;
    [SerializeField] private int _resolution = 64;
    [SerializeField] private bool _isLoop;
    [SerializeField] private float _catmullRomTension = 0.5f;

    private List<ISplineSegment> _segments = new List<ISplineSegment>();
    private ArcLengthTable _arcLengthTable = new ArcLengthTable();
    private SplineFrame[] _cachedFrames;
    private bool _isDirty = true;

    public event Action OnSplineModified;

    public int ControlPointCount => _controlPoints.Count;
    public int SegmentCount => _segments.Count;
    public float TotalLength => _arcLengthTable.TotalLength;
    public bool IsDirty => _isDirty;
    public bool IsLoop => _isLoop;
    public SplineType Type => _splineType;
    public FrameMode FrameMode => _frameMode;

    public int Resolution
    {
        get => _resolution;
        set
        {
            if (_resolution != value)
            {
                _resolution = Mathf.Max(2, value);
                SetDirty();
            }
        }
    }

    #region Initialization

    private void Awake()
    {
        RebuildIfDirty();
    }

    private void OnValidate()
    {
        SetDirty();
    }

    public void Initialize()
    {
        if (_controlPoints.Count == 0)
        {
            _controlPoints.Add(new SplinePoint(Vector3.zero));
            _controlPoints.Add(new SplinePoint(Vector3.forward * 2f));
            _controlPoints.Add(new SplinePoint(Vector3.forward * 4f));
            _controlPoints.Add(new SplinePoint(Vector3.forward * 6f));
        }
        SetDirty();
    }

    #endregion

    #region Dirty Flag System

    public void SetDirty()
    {
        _isDirty = true;
    }

    public void RebuildIfDirty()
    {
        if (!_isDirty) return;

        RebuildSegments();
        RebuildArcLengthTable();
        RebuildFrameCache();

        _isDirty = false;
        OnSplineModified?.Invoke();
    }

    private void RebuildSegments()
    {
        _segments.Clear();

        if (_controlPoints.Count < 2) return;

        switch (_splineType)
        {
            case SplineType.Linear:
                BuildLinearSegments();
                break;
            case SplineType.QuadraticBezier:
                BuildQuadraticSegments();
                break;
            case SplineType.CubicBezier:
                BuildCubicSegments();
                break;
            case SplineType.CatmullRom:
                BuildCatmullRomSegments();
                break;
        }
    }

    private void BuildLinearSegments()
    {
        for (int i = 0; i < _controlPoints.Count - 1; i++)
        {
            Vector3 p0 = _controlPoints[i].Position;
            Vector3 p1 = _controlPoints[i + 1].Position;
            _segments.Add(new LinearSegment(p0, p1));
        }

        if (_isLoop && _controlPoints.Count > 1)
        {
            Vector3 p0 = _controlPoints[_controlPoints.Count - 1].Position;
            Vector3 p1 = _controlPoints[0].Position;
            _segments.Add(new LinearSegment(p0, p1));
        }
    }

    private void BuildQuadraticSegments()
    {
        int segmentCount = (_controlPoints.Count - 1) / 2;
        for (int i = 0; i < segmentCount; i++)
        {
            int baseIndex = i * 2;
            if (baseIndex + 2 >= _controlPoints.Count) break;

            Vector3 p0 = _controlPoints[baseIndex].Position;
            Vector3 p1 = _controlPoints[baseIndex + 1].Position;
            Vector3 p2 = _controlPoints[baseIndex + 2].Position;
            _segments.Add(new QuadraticBezierSegment(p0, p1, p2));
        }
    }

    private void BuildCubicSegments()
    {
        int segmentCount = (_controlPoints.Count - 1) / 3;
        for (int i = 0; i < segmentCount; i++)
        {
            int baseIndex = i * 3;
            if (baseIndex + 3 >= _controlPoints.Count) break;

            Vector3 p0 = _controlPoints[baseIndex].Position;
            Vector3 p1 = _controlPoints[baseIndex].Position + _controlPoints[baseIndex].TangentOut;
            Vector3 p2 = _controlPoints[baseIndex + 3].Position + _controlPoints[baseIndex + 3].TangentIn;
            Vector3 p3 = _controlPoints[baseIndex + 3].Position;

            if (_controlPoints[baseIndex].AutoTangent)
            {
                p1 = _controlPoints[baseIndex + 1].Position;
            }
            if (_controlPoints[baseIndex + 3].AutoTangent)
            {
                p2 = _controlPoints[baseIndex + 2].Position;
            }

            _segments.Add(new CubicBezierSegment(p0, p1, p2, p3));
        }

        if (_isLoop && _controlPoints.Count >= 4)
        {
            int lastIndex = _controlPoints.Count - 1;
            Vector3 p0 = _controlPoints[lastIndex].Position;
            Vector3 p1 = _controlPoints[lastIndex].Position + _controlPoints[lastIndex].TangentOut;
            Vector3 p2 = _controlPoints[0].Position + _controlPoints[0].TangentIn;
            Vector3 p3 = _controlPoints[0].Position;
            _segments.Add(new CubicBezierSegment(p0, p1, p2, p3));
        }
    }

    private void BuildCatmullRomSegments()
    {
        if (_controlPoints.Count < 4) return;

        for (int i = 0; i < _controlPoints.Count - 3; i++)
        {
            Vector3 p0 = _controlPoints[i].Position;
            Vector3 p1 = _controlPoints[i + 1].Position;
            Vector3 p2 = _controlPoints[i + 2].Position;
            Vector3 p3 = _controlPoints[i + 3].Position;
            _segments.Add(new CatmullRomSegment(p0, p1, p2, p3, _catmullRomTension));
        }

        if (_isLoop && _controlPoints.Count >= 4)
        {
            int n = _controlPoints.Count;
            _segments.Add(new CatmullRomSegment(
                _controlPoints[n - 3].Position,
                _controlPoints[n - 2].Position,
                _controlPoints[n - 1].Position,
                _controlPoints[0].Position,
                _catmullRomTension));
            _segments.Add(new CatmullRomSegment(
                _controlPoints[n - 2].Position,
                _controlPoints[n - 1].Position,
                _controlPoints[0].Position,
                _controlPoints[1].Position,
                _catmullRomTension));
            _segments.Add(new CatmullRomSegment(
                _controlPoints[n - 1].Position,
                _controlPoints[0].Position,
                _controlPoints[1].Position,
                _controlPoints[2].Position,
                _catmullRomTension));
        }
    }

    private void RebuildArcLengthTable()
    {
        if (_segments.Count == 0)
        {
            _arcLengthTable = new ArcLengthTable();
            return;
        }

        _arcLengthTable.BuildMultiSegment(_segments.ToArray(), _resolution);
    }

    private void RebuildFrameCache()
    {
        if (_segments.Count == 0)
        {
            _cachedFrames = Array.Empty<SplineFrame>();
            return;
        }

        int totalSamples = _segments.Count * _resolution;
        _cachedFrames = new SplineFrame[totalSamples + 1];

        if (_frameMode == FrameMode.RotationMinimizing)
        {
            ComputeRotationMinimizingFrames();
        }
        else
        {
            ComputeSimpleFrames();
        }
    }

    private void ComputeSimpleFrames()
    {
        int sampleIndex = 0;
        for (int segmentIndex = 0; segmentIndex < _segments.Count; segmentIndex++)
        {
            ISplineSegment segment = _segments[segmentIndex];
            for (int i = 0; i <= _resolution; i++)
            {
                if (segmentIndex > 0 && i == 0)
                {
                    continue;
                }

                if (sampleIndex >= _cachedFrames.Length)
                    break;

                float t = (float)i / _resolution;
                Vector3 tangent = segment.EvaluateDerivative(t);

                if (_frameMode == FrameMode.Frenet)
                {
                    Vector3 secondDerivative = segment.EvaluateSecondDerivative(t);
                    _cachedFrames[sampleIndex] = FrameComputation.ComputeFrenetFrame(tangent, secondDerivative);
                }
                else
                {
                    _cachedFrames[sampleIndex] = FrameComputation.ComputeFrameWithReferenceUp(tangent, _referenceUp);
                }

                sampleIndex++;
            }
        }
    }

    private void ComputeRotationMinimizingFrames()
    {
        if (_segments.Count == 0 || _cachedFrames.Length == 0) return;

        Vector3 initialTangent = _segments[0].EvaluateDerivative(0f).normalized;
        SplineFrame currentFrame = FrameComputation.ComputeFrameWithReferenceUp(initialTangent, _referenceUp);
        _cachedFrames[0] = currentFrame;

        int sampleIndex = 1;
        for (int segmentIndex = 0; segmentIndex < _segments.Count; segmentIndex++)
        {
            ISplineSegment segment = _segments[segmentIndex];
            Vector3 previousPosition = segment.Evaluate(0f);
            Vector3 previousTangent = segment.EvaluateDerivative(0f).normalized;

            for (int i = 1; i <= _resolution; i++)
            {
                if (sampleIndex >= _cachedFrames.Length)
                    break;

                float t = (float)i / _resolution;
                Vector3 currentPosition = segment.Evaluate(t);
                Vector3 currentTangent = segment.EvaluateDerivative(t).normalized;

                currentFrame = FrameComputation.PropagateFrameDoubleReflection(
                    currentFrame, previousTangent, currentTangent, previousPosition, currentPosition);

                _cachedFrames[sampleIndex] = currentFrame;

                previousPosition = currentPosition;
                previousTangent = currentTangent;
                sampleIndex++;
            }
        }
    }

    #endregion

    #region Evaluation - T-Based

    public Vector3 Evaluate(float t)
    {
        RebuildIfDirty();

        if (_segments.Count == 0) return transform.position;

        t = Mathf.Clamp01(t);
        GetSegmentAndLocalT(t, out int segmentIndex, out float localT);

        Vector3 localPosition = _segments[segmentIndex].Evaluate(localT);
        return transform.TransformPoint(localPosition);
    }

    public SplineSample EvaluateWithFrame(float t)
    {
        RebuildIfDirty();

        if (_segments.Count == 0)
            return new SplineSample(transform.position, SplineFrame.Identity, 0f, 0f);

        t = Mathf.Clamp01(t);
        GetSegmentAndLocalT(t, out int segmentIndex, out float localT);

        Vector3 localPosition = _segments[segmentIndex].Evaluate(localT);
        SplineFrame frame = GetCachedFrame(t);
        float distance = _arcLengthTable.GetDistanceByT(t);

        SplineSample sample = new SplineSample(
            transform.TransformPoint(localPosition),
            TransformFrame(frame),
            t,
            distance
        );

        return sample;
    }

    public Vector3 EvaluateDerivative(float t)
    {
        RebuildIfDirty();

        if (_segments.Count == 0) return Vector3.forward;

        t = Mathf.Clamp01(t);
        GetSegmentAndLocalT(t, out int segmentIndex, out float localT);

        Vector3 localDerivative = _segments[segmentIndex].EvaluateDerivative(localT);
        return transform.TransformDirection(localDerivative);
    }

    public Vector3 GetTangent(float t)
    {
        return EvaluateDerivative(t).normalized;
    }

    #endregion

    #region Evaluation - Distance-Based

    public Vector3 EvaluateByDistance(float distance)
    {
        RebuildIfDirty();
        float t = _arcLengthTable.GetTByDistance(distance);
        return Evaluate(t);
    }

    public SplineSample EvaluateByDistanceWithFrame(float distance)
    {
        RebuildIfDirty();
        float t = _arcLengthTable.GetTByDistance(distance);
        return EvaluateWithFrame(t);
    }

    public Vector3 EvaluateByNormalizedDistance(float normalizedDistance)
    {
        return EvaluateByDistance(normalizedDistance * TotalLength);
    }

    public SplineSample EvaluateByNormalizedDistanceWithFrame(float normalizedDistance)
    {
        return EvaluateByDistanceWithFrame(normalizedDistance * TotalLength);
    }

    public float GetTByDistance(float distance)
    {
        RebuildIfDirty();
        return _arcLengthTable.GetTByDistance(distance);
    }

    public float GetDistanceByT(float t)
    {
        RebuildIfDirty();
        return _arcLengthTable.GetDistanceByT(t);
    }

    public float GetLength()
    {
        RebuildIfDirty();
        return _arcLengthTable.TotalLength;
    }

    #endregion

    #region Control Point Management

    public SplinePoint GetControlPoint(int index)
    {
        if (index < 0 || index >= _controlPoints.Count)
            throw new IndexOutOfRangeException();
        return _controlPoints[index];
    }

    public void SetControlPoint(int index, SplinePoint point)
    {
        if (index < 0 || index >= _controlPoints.Count)
            throw new IndexOutOfRangeException();

        _controlPoints[index] = point;
        SetDirty();
    }

    public void SetControlPointPosition(int index, Vector3 position)
    {
        if (index < 0 || index >= _controlPoints.Count)
            throw new IndexOutOfRangeException();

        SplinePoint point = _controlPoints[index];
        point.Position = position;
        _controlPoints[index] = point;
        SetDirty();
    }

    public void AddControlPoint(SplinePoint point)
    {
        _controlPoints.Add(point);
        SetDirty();
    }

    public void InsertControlPoint(int index, SplinePoint point)
    {
        _controlPoints.Insert(index, point);
        SetDirty();
    }

    public void RemoveControlPoint(int index)
    {
        if (index < 0 || index >= _controlPoints.Count) return;
        _controlPoints.RemoveAt(index);
        SetDirty();
    }

    public Vector3 GetControlPointWorldPosition(int index)
    {
        if (index < 0 || index >= _controlPoints.Count)
            throw new IndexOutOfRangeException();
        return transform.TransformPoint(_controlPoints[index].Position);
    }

    public void SetControlPointWorldPosition(int index, Vector3 worldPosition)
    {
        if (index < 0 || index >= _controlPoints.Count)
            throw new IndexOutOfRangeException();

        SplinePoint point = _controlPoints[index];
        point.Position = transform.InverseTransformPoint(worldPosition);
        _controlPoints[index] = point;
        SetDirty();
    }

    #endregion

    #region Utility

    private void GetSegmentAndLocalT(float globalT, out int segmentIndex, out float localT)
    {
        if (_segments.Count == 0)
        {
            segmentIndex = 0;
            localT = 0f;
            return;
        }

        float scaledT = globalT * _segments.Count;
        segmentIndex = Mathf.FloorToInt(scaledT);
        segmentIndex = Mathf.Clamp(segmentIndex, 0, _segments.Count - 1);
        localT = scaledT - segmentIndex;
        localT = Mathf.Clamp01(localT);
    }

    private SplineFrame GetCachedFrame(float t)
    {
        if (_cachedFrames == null || _cachedFrames.Length == 0)
            return SplineFrame.Identity;

        return FrameComputation.GetFrameFromCache(_cachedFrames, t);
    }

    private SplineFrame TransformFrame(SplineFrame localFrame)
    {
        return new SplineFrame(
            transform.TransformDirection(localFrame.Tangent),
            transform.TransformDirection(localFrame.Normal),
            transform.TransformDirection(localFrame.Binormal)
        );
    }

    public ISplineSegment GetSegment(int index)
    {
        RebuildIfDirty();
        if (index < 0 || index >= _segments.Count)
            throw new IndexOutOfRangeException();
        return _segments[index];
    }

    public Vector3 GetClosestPoint(Vector3 worldPoint, out float t, int searchIterations = 10)
    {
        RebuildIfDirty();

        t = 0f;
        if (_segments.Count == 0)
            return transform.position;

        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        float bestT = 0f;
        float bestDistSq = float.MaxValue;

        int coarseSteps = _segments.Count * 16;
        for (int i = 0; i <= coarseSteps; i++)
        {
            float sampleT = (float)i / coarseSteps;
            GetSegmentAndLocalT(sampleT, out int segIdx, out float localT);
            Vector3 point = _segments[segIdx].Evaluate(localT);
            float distSq = (point - localPoint).sqrMagnitude;

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestT = sampleT;
            }
        }

        float searchRadius = 1f / coarseSteps;
        for (int iter = 0; iter < searchIterations; iter++)
        {
            float left = Mathf.Max(0f, bestT - searchRadius);
            float right = Mathf.Min(1f, bestT + searchRadius);

            for (int i = 0; i <= 8; i++)
            {
                float sampleT = Mathf.Lerp(left, right, (float)i / 8);
                GetSegmentAndLocalT(sampleT, out int segIdx, out float localT);
                Vector3 point = _segments[segIdx].Evaluate(localT);
                float distSq = (point - localPoint).sqrMagnitude;

                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestT = sampleT;
                }
            }

            searchRadius *= 0.5f;
        }

        t = bestT;
        return Evaluate(bestT);
    }

    #endregion
}
