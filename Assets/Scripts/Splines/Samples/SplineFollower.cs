using UnityEngine;

public enum FollowMode
{
    ByTime,
    ByDistance,
    ByNormalizedDistance
}

public enum LoopMode
{
    Once,
    Loop,
    PingPong
}

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private Spline _spline;
    [SerializeField] private FollowMode _followMode = FollowMode.ByNormalizedDistance;
    [SerializeField] private LoopMode _loopMode = LoopMode.Loop;
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _startOffset;
    [SerializeField] private bool _alignToSpline = true;
    [SerializeField] private bool _useRotationMinimizingFrame = true;
    [SerializeField] private Vector3 _rotationOffset;
    [SerializeField] private bool _playOnStart = true;

    private float _currentProgress;
    private float _direction = 1f;
    private bool _isPlaying;

    public Spline Spline
    {
        get => _spline;
        set => _spline = value;
    }

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public float Progress
    {
        get => _currentProgress;
        set
        {
            _currentProgress = Mathf.Clamp01(value);
            UpdatePosition();
        }
    }

    public bool IsPlaying => _isPlaying;

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
        _currentProgress = _startOffset;

        if (_playOnStart)
        {
            Play();
        }
        else
        {
            UpdatePosition();
        }
    }

    private void Update()
    {
        if (!_isPlaying || _spline == null) return;

        UpdateProgress();
        UpdatePosition();
    }

    private void UpdateProgress()
    {
        float delta = _speed * Time.deltaTime * _direction;

        switch (_followMode)
        {
            case FollowMode.ByTime:
            case FollowMode.ByNormalizedDistance:
                _currentProgress += delta;
                break;

            case FollowMode.ByDistance:
                float totalLength = _spline.GetLength();
                if (totalLength > 0f)
                {
                    _currentProgress += delta / totalLength;
                }
                break;
        }

        HandleLooping();
    }

    private void HandleLooping()
    {
        switch (_loopMode)
        {
            case LoopMode.Once:
                if (_currentProgress >= 1f || _currentProgress <= 0f)
                {
                    _currentProgress = Mathf.Clamp01(_currentProgress);
                    Stop();
                }
                break;

            case LoopMode.Loop:
                if (_currentProgress >= 1f)
                {
                    _currentProgress -= 1f;
                }
                else if (_currentProgress < 0f)
                {
                    _currentProgress += 1f;
                }
                break;

            case LoopMode.PingPong:
                if (_currentProgress >= 1f)
                {
                    _currentProgress = 1f;
                    _direction = -1f;
                }
                else if (_currentProgress <= 0f)
                {
                    _currentProgress = 0f;
                    _direction = 1f;
                }
                break;
        }
    }

    private void UpdatePosition()
    {
        if (_spline == null) return;

        SplineSample sample = _spline.EvaluateByNormalizedDistanceWithFrame(_currentProgress);

        transform.position = sample.Position;

        if (_alignToSpline)
        {
            Quaternion rotation = sample.Rotation;
            transform.rotation = rotation * Quaternion.Euler(_rotationOffset);
        }
    }

    private void OnSplineModified()
    {
        UpdatePosition();
    }

    public void Play()
    {
        _isPlaying = true;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Stop()
    {
        _isPlaying = false;
        _currentProgress = _startOffset;
        _direction = 1f;
    }

    public void SetProgress(float normalizedProgress)
    {
        _currentProgress = Mathf.Clamp01(normalizedProgress);
        UpdatePosition();
    }

    public void SetDistance(float distance)
    {
        if (_spline == null) return;

        float totalLength = _spline.GetLength();
        if (totalLength > 0f)
        {
            _currentProgress = Mathf.Clamp01(distance / totalLength);
            UpdatePosition();
        }
    }

    public void Reverse()
    {
        _direction *= -1f;
    }
}
