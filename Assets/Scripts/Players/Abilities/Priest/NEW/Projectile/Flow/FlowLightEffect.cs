using UnityEngine;

public class FlowLightEffect : MonoBehaviour
{
    [Header("Flow Settings")]
    [SerializeField] private float particleSpeed = 20f;
    [SerializeField] private int particleCount = 200;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float waveAmplitude = 0.5f;
    [SerializeField] private const float heightOffset = 0.5f;

    [Header("Spread Settings")]
    [SerializeField] private bool _isSpreadParticles = false;
    [SerializeField] private bool _isReverse = false;
    [SerializeField] private float spreadMinTime = 0.2f;
    [SerializeField] private float spreadMaxTime = 0.8f;
    [SerializeField] private float spreadMinPower = 1f;
    [SerializeField] private float spreadMaxPower = 2f;

    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles;

    private Vector3[] _splitDirections;
    private float[] _splitTime;
    private bool[] _hasSplit;
    private Vector3[] _splitStartPosition;

    private Camera _mainCamera;

    public GameObject point1;
    public GameObject point2;

    public void Initialize(GameObject startPoint, GameObject endPoint)
    {
        point1 = startPoint;
        point2 = endPoint;
    }

    public void SetTarget(GameObject endPoint)
    {
        point2 = endPoint;
    }

    public void Activate()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Play();
        }
    }

    public void Deactivate()
    {
        if (_particleSystem != null)
        {
            _particleSystem.Stop();

            for (int i = 0; i < _splitTime.Length; i++)
            {
                _splitTime[i] = 0f;
                _hasSplit[i] = false;
                _splitDirections[i] = Vector3.zero;
                _splitStartPosition[i] = Vector3.zero;
            }
        }
    }

    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _mainCamera = Camera.main;

        var mainModule = _particleSystem.main;
        mainModule.maxParticles = particleCount;

        var emissionModule = _particleSystem.emission;
        emissionModule.rateOverTime = particleCount;

        int max = mainModule.maxParticles;
        _particles = new ParticleSystem.Particle[max];
        _splitDirections = new Vector3[max];
        _splitTime = new float[max];
        _hasSplit = new bool[max];
        _splitStartPosition = new Vector3[max];
    }

    void LateUpdate()
    {
        if (point1 == null || point2 == null) return;

        Vector3 startPosition = point1.transform.position + Vector3.up * heightOffset;
        Vector3 endPosition = point2.transform.position + Vector3.up * heightOffset;
        Vector3 currentDirection = (endPosition - startPosition).normalized;
        float distance = Vector3.Distance(startPosition, endPosition);

        var mainModule = _particleSystem.main;

        if (!_isSpreadParticles) mainModule.startLifetime = distance / particleSpeed;

        int activeParticles = _particleSystem.GetParticles(_particles);

        for (int i = 0; i < activeParticles; i++)
        {
            float lifetime = mainModule.startLifetime.constant;
            float currentLifetime = _particles[i].remainingLifetime;
            float progress = currentLifetime / lifetime;
            float normalizedAge = 1f - progress;

            if (_isSpreadParticles && !_hasSplit[i])
            {
                _splitTime[i] = Random.Range(spreadMinTime, spreadMaxTime);
                _splitDirections[i] = GetRandomSplitDirection(currentDirection);
                _splitStartPosition[i] = Vector3.Lerp(endPosition, startPosition, _splitTime[i]);
                _hasSplit[i] = true;
            }

            Vector3 basePosition;

            if (_isSpreadParticles && normalizedAge < _splitTime[i])
            {
                basePosition = Vector3.Lerp(endPosition, startPosition, progress);

                float waveOffset = Mathf.Sin((progress + Time.time * waveFrequency) * Mathf.PI * 2) * waveAmplitude;
                Vector3 waveOffsetVector = CalculateWaveOffset(waveOffset, currentDirection);

                _particles[i].position = basePosition + waveOffsetVector;
            }

            if (_isSpreadParticles)
            {
                bool isSplitNow = normalizedAge >= _splitTime[i] || _isReverse;

                if (!isSplitNow)
                {
                    basePosition = Vector3.Lerp(endPosition, startPosition, progress);

                    float waveOffset = Mathf.Sin((progress + Time.time * waveFrequency) * Mathf.PI * 2) * waveAmplitude;
                    Vector3 waveOffsetVector = CalculateWaveOffset(waveOffset, currentDirection);

                    _particles[i].position = basePosition + waveOffsetVector;
                }

                else
                {
                    float timeSinceSplit = (normalizedAge - _splitTime[i]) * lifetime;
                    if (_isReverse) timeSinceSplit = normalizedAge * lifetime;

                    Vector3 splitStartPos = _splitStartPosition[i];
                    _particles[i].position = splitStartPos + _splitDirections[i] * (timeSinceSplit * particleSpeed);
                }
            }

            else
            {
                basePosition = Vector3.Lerp(endPosition, startPosition, progress);

                float waveOffset = Mathf.Sin((progress + Time.time * waveFrequency) * Mathf.PI * 2) * waveAmplitude;
                Vector3 waveOffsetVector = CalculateWaveOffset(waveOffset, currentDirection);

                _particles[i].position = basePosition + waveOffsetVector;
            }
        }

        _particleSystem.SetParticles(_particles, activeParticles);
    }

    private Vector3 CalculateWaveOffset(float waveOffset, Vector3 direction)
    {
        Vector3 toCamera = _mainCamera.transform.position - (point1.transform.position + point2.transform.position) * 0.5f;
        Vector3 perpendicular = Vector3.Cross(direction, toCamera).normalized;
        return perpendicular * waveOffset;
    }

    private Vector3 GetRandomSplitDirection(Vector3 baseDirection)
    {
        Vector3 random = Random.onUnitSphere;
        Vector3 perpendicular = Vector3.Cross(baseDirection, random).normalized;
        Vector3 direction = perpendicular * Random.Range(spreadMinPower, spreadMaxPower);

        if (_isReverse) direction *= -1f;
        return direction;
    }
}
