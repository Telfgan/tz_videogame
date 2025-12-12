using UnityEngine;

public class PullingHealthEffect : MonoBehaviour
{
    [SerializeField] private float particleSpeed = 20f;
    [SerializeField] private int particleCount = 200;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float waveAmplitude = 0.5f;
    [SerializeField] private const float heightOffset = 0.5f;

    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    private Camera mainCamera;

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
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    public void Deactivate()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        mainCamera = Camera.main;

        var mainModule = particleSystem.main;
        mainModule.maxParticles = particleCount;

        var emissionModule = particleSystem.emission;
        emissionModule.rateOverTime = particleCount;

        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void LateUpdate()
    {
        if (point1 == null || point2 == null) return;

        Vector3 startPosition = point2.transform.position + Vector3.up * heightOffset;
        Vector3 endPosition = point1.transform.position + Vector3.up * heightOffset;
        Vector3 currentDirection = (endPosition - startPosition).normalized;
        float distance = Vector3.Distance(startPosition, endPosition);

        var mainModule = particleSystem.main;
        mainModule.startLifetime = distance / particleSpeed;

        int activeParticles = particleSystem.GetParticles(particles);

        for (int i = 0; i < activeParticles; i++)
        {
            float progress = particles[i].remainingLifetime / mainModule.startLifetime.constant;
            Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, 1 - progress);

            float waveOffset = Mathf.Sin((progress + Time.time * waveFrequency) * Mathf.PI * 2) * waveAmplitude;

            Vector3 waveOffsetVector = CalculateWaveOffset(waveOffset, currentDirection);
            particles[i].position = basePosition + waveOffsetVector;
        }

        particleSystem.SetParticles(particles, activeParticles);
    }

    private Vector3 CalculateWaveOffset(float waveOffset, Vector3 direction)
    {
        Vector3 toCamera = mainCamera.transform.position - (point1.transform.position + point2.transform.position) * 0.5f;
        Vector3 perpendicular = Vector3.Cross(direction, toCamera).normalized;

        return perpendicular * waveOffset;
    }
}
