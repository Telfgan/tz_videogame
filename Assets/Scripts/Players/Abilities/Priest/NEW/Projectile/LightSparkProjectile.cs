using Mirror;
using UnityEngine;

public class LightSparkProjectile : Projectiles
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float waveAmplitudeMin = 0.2f;
    [SerializeField] private float waveAmplitudeMax = 0.5f;
    [SerializeField] private float waveFrequencyMin = 1f;
    [SerializeField] private float waveFrequencyMax = 3f;
    [SerializeField] private float speed = 5f;

    [SerializeField] public ParticleSystem particleSystem;

    private SparkOfLight _skillReference;
    private float _attackDelay;
    private Vector3 _direction;
    private float _waveAmplitude;
    private float _waveFrequency;
    private float _startTime;

    private Character _target;

    public void Init(HeroComponent dad, bool isLightMode, SparkOfLight skill, float distance, float attackDelay, Character target)
    {
        _dad = dad;
        _skillReference = skill;
        _distance = distance;
        _attackDelay = attackDelay;

        _waveAmplitude = Random.Range(waveAmplitudeMin, waveAmplitudeMax);
        _waveFrequency = Random.Range(waveFrequencyMin, waveFrequencyMax);

        _startTime = Time.time;

        _target = target;
    }

    public void StartFly(Vector3 direction)
    {
        _direction = direction.normalized;

        if (particleSystem != null) particleSystem.Play();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_rb == null || _target == null) return;

        Vector3 targetPosition = _target.transform.position + Vector3.up;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        float elapsedTime = Time.time - _startTime;
        Vector3 forwardMovement = directionToTarget * (speed * Time.deltaTime);

        Vector3 waveOffset = Vector3.up * Mathf.Sin(elapsedTime * _waveFrequency) * _waveAmplitude;
        Vector3 sideOffset = Vector3.right * Mathf.Sin(elapsedTime * _waveFrequency * 0.5f) * (_waveAmplitude * 0.5f);

        transform.position += forwardMovement + waveOffset + sideOffset;

        if (particleSystem != null)
        {
            particleSystem.transform.position = transform.position;
        }
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != _dad.gameObject)
        {
            if (other.gameObject.TryGetComponent<Character>(out Character character) && character == _target)
            {
                _skillReference.HandleMode(character);
                if (particleSystem != null) particleSystem.Stop();

                Destroy(gameObject, 0.1f);
            }
        }
    }
}