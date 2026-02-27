using System.Collections.Generic;
using UnityEngine;

public enum DecoratorSpacingMode
{
    ByCount,
    ByDistance,
    ByNormalizedDistance
}

public class SplineDecorator : MonoBehaviour
{
    [SerializeField] private Spline _spline;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private DecoratorSpacingMode _spacingMode = DecoratorSpacingMode.ByCount;
    [SerializeField] private int _count = 10;
    [SerializeField] private float _spacing = 1f;
    [SerializeField] private float _startOffset;
    [SerializeField] private float _endOffset;
    [SerializeField] private bool _alignToSpline = true;
    [SerializeField] private Vector3 _rotationOffset;
    [SerializeField] private Vector3 _positionOffset;
    [SerializeField] private bool _useLocalOffset = true;
    [SerializeField] private bool _includeEndpoints = true;
    [SerializeField] private bool _rebuildOnStart = true;

    private List<GameObject> _instances = new List<GameObject>();

    public Spline Spline
    {
        get => _spline;
        set
        {
            _spline = value;
            Rebuild();
        }
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = Mathf.Max(1, value);
            Rebuild();
        }
    }

    public float Spacing
    {
        get => _spacing;
        set
        {
            _spacing = Mathf.Max(0.01f, value);
            Rebuild();
        }
    }

    private void OnEnable()
    {
        if (_spline != null)
        {
            _spline.OnSplineModified += OnSplineModified;
        }
    }

    private void OnDisable()
    {
        if (_spline != null)
        {
            _spline.OnSplineModified -= OnSplineModified;
        }
    }

    private void Start()
    {
        if (_rebuildOnStart)
        {
            Rebuild();
        }
    }

    private void OnSplineModified()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        ClearInstances();

        if (_spline == null || _prefab == null) return;

        _spline.RebuildIfDirty();

        List<float> normalizedPositions = CalculatePositions();

        foreach (float normalizedPosition in normalizedPositions)
        {
            SpawnInstance(normalizedPosition);
        }
    }

    private List<float> CalculatePositions()
    {
        List<float> positions = new List<float>();

        float totalLength = _spline.GetLength();
        if (totalLength <= 0f) return positions;

        float startNormalized = _startOffset / totalLength;
        float endNormalized = 1f - (_endOffset / totalLength);

        if (startNormalized >= endNormalized) return positions;

        switch (_spacingMode)
        {
            case DecoratorSpacingMode.ByCount:
                CalculateByCount(positions, startNormalized, endNormalized);
                break;

            case DecoratorSpacingMode.ByDistance:
                CalculateByDistance(positions, totalLength, startNormalized, endNormalized);
                break;

            case DecoratorSpacingMode.ByNormalizedDistance:
                CalculateByNormalizedDistance(positions, startNormalized, endNormalized);
                break;
        }

        return positions;
    }

    private void CalculateByCount(List<float> positions, float start, float end)
    {
        if (_count <= 0) return;

        if (_count == 1)
        {
            positions.Add((start + end) * 0.5f);
            return;
        }

        int iterations = _includeEndpoints ? _count : _count + 2;
        float range = end - start;

        for (int i = 0; i < iterations; i++)
        {
            float t;
            if (_includeEndpoints)
            {
                t = start + range * ((float)i / (_count - 1));
            }
            else
            {
                t = start + range * ((float)(i + 1) / (iterations - 1));
                if (i == 0 || i == iterations - 1) continue;
            }

            positions.Add(t);
        }
    }

    private void CalculateByDistance(List<float> positions, float totalLength, float start, float end)
    {
        float startDistance = start * totalLength;
        float endDistance = end * totalLength;
        float availableLength = endDistance - startDistance;

        if (availableLength <= 0f) return;

        float currentDistance = _includeEndpoints ? startDistance : startDistance + _spacing;
        float finalDistance = _includeEndpoints ? endDistance : endDistance - _spacing;

        while (currentDistance <= finalDistance + 0.001f)
        {
            float normalizedPosition = currentDistance / totalLength;
            positions.Add(normalizedPosition);
            currentDistance += _spacing;
        }
    }

    private void CalculateByNormalizedDistance(List<float> positions, float start, float end)
    {
        float range = end - start;
        if (range <= 0f) return;

        float normalizedSpacing = _spacing;
        float current = _includeEndpoints ? start : start + normalizedSpacing;
        float final_ = _includeEndpoints ? end : end - normalizedSpacing;

        while (current <= final_ + 0.001f)
        {
            positions.Add(current);
            current += normalizedSpacing;
        }
    }

    private void SpawnInstance(float normalizedPosition)
    {
        SplineSample sample = _spline.EvaluateByNormalizedDistanceWithFrame(normalizedPosition);

        Vector3 position = sample.Position;
        Quaternion rotation = _alignToSpline ? sample.Rotation : Quaternion.identity;

        if (_useLocalOffset)
        {
            position += rotation * _positionOffset;
        }
        else
        {
            position += _positionOffset;
        }

        rotation *= Quaternion.Euler(_rotationOffset);

        GameObject instance = Instantiate(_prefab, position, rotation, transform);
        _instances.Add(instance);
    }

    public void ClearInstances()
    {
        foreach (GameObject instance in _instances)
        {
            if (instance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(instance);
                }
                else
                {
                    DestroyImmediate(instance);
                }
            }
        }

        _instances.Clear();
    }

    public void UpdateInstancePositions()
    {
        if (_spline == null) return;

        _spline.RebuildIfDirty();

        List<float> positions = CalculatePositions();

        for (int i = 0; i < _instances.Count && i < positions.Count; i++)
        {
            if (_instances[i] == null) continue;

            SplineSample sample = _spline.EvaluateByNormalizedDistanceWithFrame(positions[i]);

            Vector3 position = sample.Position;
            Quaternion rotation = _alignToSpline ? sample.Rotation : Quaternion.identity;

            if (_useLocalOffset)
            {
                position += rotation * _positionOffset;
            }
            else
            {
                position += _positionOffset;
            }

            rotation *= Quaternion.Euler(_rotationOffset);

            _instances[i].transform.position = position;
            _instances[i].transform.rotation = rotation;
        }
    }
}
