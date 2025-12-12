using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour, IProjectile
{
    [SerializeField] protected ParticleSystem _trailParticle;
    [SerializeField] protected ParticleSystem _destroyParticle;
    [SerializeField] protected float _speed = 10;
    [SerializeField] private bool _selfDestroyInEndPoint = true;
    [SerializeField] private float _lifeTime = 10;
    
    private Transform _target;
    private TargetInfo _targetInfo;

    public Transform Target => _target;

    public float Speed => _speed;

    public TargetInfo TargetInfo => _targetInfo;

    public Action<Projectile, GameObject> TargetReached { get; set; }

    public event UnityAction<Projectile, GameObject> EndPointReached;

    private void Start()
    {
        if(_trailParticle != null)
            _trailParticle = Instantiate(_trailParticle, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        if (_trailParticle != null)
            _trailParticle.transform.position = transform.position;
    }

    protected virtual void OnDestroy()
    {
        if (_destroyParticle != null)
        {
            _destroyParticle = Instantiate(_destroyParticle, transform.position, Quaternion.identity);
            _destroyParticle.Play();
        }
        if (_trailParticle != null)
        {
            _trailParticle.Stop();
        }
    }

    public void SetTarget(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void StartFly()
    {
        throw new NotImplementedException();
    }

    public float GetDistanceToTarget()
    {
        if (_target != null)
            return (transform.position - _target.position).magnitude;

        return 10000;
    }

    public void StartFly(Vector3 position, bool directionMove = false)
    {
        var direction = (position - transform.position).normalized;

        Destroy(gameObject, _lifeTime);

        if (directionMove)
            StartCoroutine(InfiniteFlyCoroutine(direction));
        else
            StartCoroutine(FlyCoroutine(position));
    }

    public void StartFly(Transform target, bool follow = false)
    {
        _target = target;

        Destroy(gameObject, _lifeTime);

        if (follow)
            StartCoroutine(FlyCoroutine());
        else
            StartFly(_target.position);
    }

    private void Rotate(Vector3 lookPoint)
    {
        transform.LookAt(lookPoint, Vector3.forward);
        transform.Rotate(new Vector3(90, 0, 0));
    }

    private IEnumerator FlyCoroutine(Vector3 position)
    {
        Rotate(position);

        while (transform.position != position)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, _speed * Time.deltaTime);
            yield return null;
        }
        EndPointReached?.Invoke(this, null);

        if (_selfDestroyInEndPoint)
            Destroy(gameObject);
    }

    private IEnumerator FlyCoroutine()
    {
        while (transform.position != _target.position)
        {
            Rotate(_target.position);

            transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
            yield return null;
        }
        EndPointReached?.Invoke(this, _target.gameObject);

        if (_selfDestroyInEndPoint)
            Destroy(gameObject);
    }

    private IEnumerator InfiniteFlyCoroutine(Vector3 direction)
    {
        while (true)
        {
            Debug.Log(direction);
            Debug.Log(transform.position);
            transform.position += _speed * direction * Time.deltaTime;
            yield return null;
        }
    }
}
