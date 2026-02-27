using System;
using UnityEngine;

[Serializable]
public class ArcLengthTable
{
    [SerializeField] private float[] _distances;
    [SerializeField] private float[] _parameters;
    [SerializeField] private float _totalLength;
    [SerializeField] private int _resolution;

    public float TotalLength => _totalLength;
    public int Resolution => _resolution;
    public bool IsValid => _distances != null && _distances.Length > 0;

    public ArcLengthTable()
    {
        _distances = Array.Empty<float>();
        _parameters = Array.Empty<float>();
        _totalLength = 0f;
        _resolution = 0;
    }

    public ArcLengthTable(int resolution)
    {
        _resolution = Mathf.Max(2, resolution);
        _distances = new float[_resolution + 1];
        _parameters = new float[_resolution + 1];
        _totalLength = 0f;
    }

    public void Build(ISplineSegment segment, int resolution)
    {
        _resolution = Mathf.Max(2, resolution);
        _distances = new float[_resolution + 1];
        _parameters = new float[_resolution + 1];

        _distances[0] = 0f;
        _parameters[0] = 0f;

        Vector3 previousPoint = segment.Evaluate(0f);
        float accumulatedLength = 0f;

        for (int i = 1; i <= _resolution; i++)
        {
            float t = (float)i / _resolution;
            Vector3 currentPoint = segment.Evaluate(t);
            accumulatedLength += Vector3.Distance(previousPoint, currentPoint);

            _distances[i] = accumulatedLength;
            _parameters[i] = t;

            previousPoint = currentPoint;
        }

        _totalLength = accumulatedLength;
    }

    public void BuildWithIntegration(ISplineSegment segment, int resolution)
    {
        _resolution = Mathf.Max(2, resolution);
        _distances = new float[_resolution + 1];
        _parameters = new float[_resolution + 1];

        _distances[0] = 0f;
        _parameters[0] = 0f;

        float accumulatedLength = 0f;
        float step = 1f / _resolution;

        for (int i = 1; i <= _resolution; i++)
        {
            float t0 = (float)(i - 1) / _resolution;
            float t1 = (float)i / _resolution;
            float tMid = (t0 + t1) * 0.5f;

            float v0 = segment.EvaluateDerivative(t0).magnitude;
            float v1 = segment.EvaluateDerivative(t1).magnitude;
            float vMid = segment.EvaluateDerivative(tMid).magnitude;

            float segmentLength = (step / 6f) * (v0 + 4f * vMid + v1);
            accumulatedLength += segmentLength;

            _distances[i] = accumulatedLength;
            _parameters[i] = t1;
        }

        _totalLength = accumulatedLength;
    }

    public void BuildMultiSegment(ISplineSegment[] segments, int resolutionPerSegment)
    {
        int segmentCount = segments.Length;
        int totalSamples = segmentCount * resolutionPerSegment;

        _resolution = totalSamples;
        _distances = new float[totalSamples + 1];
        _parameters = new float[totalSamples + 1];

        _distances[0] = 0f;
        _parameters[0] = 0f;

        float accumulatedLength = 0f;
        int sampleIndex = 1;

        for (int segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
        {
            ISplineSegment segment = segments[segmentIndex];
            Vector3 previousPoint = segment.Evaluate(0f);

            for (int i = 1; i <= resolutionPerSegment; i++)
            {
                float localT = (float)i / resolutionPerSegment;
                float globalT = (segmentIndex + localT) / segmentCount;

                Vector3 currentPoint = segment.Evaluate(localT);
                accumulatedLength += Vector3.Distance(previousPoint, currentPoint);

                _distances[sampleIndex] = accumulatedLength;
                _parameters[sampleIndex] = globalT;

                previousPoint = currentPoint;
                sampleIndex++;
            }
        }

        _totalLength = accumulatedLength;
    }

    public float GetTByDistance(float distance)
    {
        if (!IsValid) return 0f;

        distance = Mathf.Clamp(distance, 0f, _totalLength);

        if (distance <= 0f) return 0f;
        if (distance >= _totalLength) return 1f;

        int low = 0;
        int high = _distances.Length - 1;

        while (low < high - 1)
        {
            int mid = (low + high) / 2;
            if (_distances[mid] < distance)
                low = mid;
            else
                high = mid;
        }

        float d0 = _distances[low];
        float d1 = _distances[high];

        if (Mathf.Approximately(d0, d1))
            return _parameters[low];

        float ratio = (distance - d0) / (d1 - d0);
        return Mathf.Lerp(_parameters[low], _parameters[high], ratio);
    }

    public float GetDistanceByT(float t)
    {
        if (!IsValid) return 0f;

        t = Mathf.Clamp01(t);

        if (t <= 0f) return 0f;
        if (t >= 1f) return _totalLength;

        int low = 0;
        int high = _parameters.Length - 1;

        while (low < high - 1)
        {
            int mid = (low + high) / 2;
            if (_parameters[mid] < t)
                low = mid;
            else
                high = mid;
        }

        float t0 = _parameters[low];
        float t1 = _parameters[high];

        if (Mathf.Approximately(t0, t1))
            return _distances[low];

        float ratio = (t - t0) / (t1 - t0);
        return Mathf.Lerp(_distances[low], _distances[high], ratio);
    }

    public float GetNormalizedTByNormalizedDistance(float normalizedDistance)
    {
        return GetTByDistance(normalizedDistance * _totalLength);
    }

    public void GetBracket(float distance, out int lowIndex, out int highIndex, out float ratio)
    {
        distance = Mathf.Clamp(distance, 0f, _totalLength);

        lowIndex = 0;
        highIndex = _distances.Length - 1;

        while (lowIndex < highIndex - 1)
        {
            int mid = (lowIndex + highIndex) / 2;
            if (_distances[mid] < distance)
                lowIndex = mid;
            else
                highIndex = mid;
        }

        float d0 = _distances[lowIndex];
        float d1 = _distances[highIndex];

        ratio = Mathf.Approximately(d0, d1) ? 0f : (distance - d0) / (d1 - d0);
    }

    public float GetDistanceAtIndex(int index)
    {
        if (!IsValid || index < 0 || index >= _distances.Length)
            return 0f;
        return _distances[index];
    }

    public float GetParameterAtIndex(int index)
    {
        if (!IsValid || index < 0 || index >= _parameters.Length)
            return 0f;
        return _parameters[index];
    }
}
