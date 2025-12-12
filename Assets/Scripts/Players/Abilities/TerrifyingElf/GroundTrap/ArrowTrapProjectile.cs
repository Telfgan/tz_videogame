using Mirror;
using UnityEngine;

public class ArrowTrapProjectile : Projectiles
{
    [SerializeField] float _speed = 10f;
    [SerializeField] float _lifeTime = 10f;
    [SerializeField] bool _selfDestroyInEndPoint = true;

    [Header("Trap anchors")]
    [SerializeField] GameObject trapLeft;
    [SerializeField] GameObject trapRight;

    [Header("Visual")]
    [SerializeField] LineRenderer lineRenderer;

    Vector3 _targetPosition;
    bool _isFlyingToTarget;

    void Awake()
    {
        if (lineRenderer) lineRenderer.positionCount = 2;
        if (!trapRight) trapRight = gameObject;
    }

    void Update()
    {
        if (_isFlyingToTarget) FlyTowardsTarget();

        if (lineRenderer)
        {
            lineRenderer.SetPosition(0, trapLeft.transform.position);
            lineRenderer.SetPosition(1, trapRight.transform.position);
        }
    }

    public void StartFly(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
        _isFlyingToTarget = true;
        Destroy(gameObject, _lifeTime);
    }

    void FlyTowardsTarget()
    {
        Vector3 direction = (_targetPosition - transform.position).normalized;
        float step = _speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, _targetPosition) <= step)
        {
            transform.position = _targetPosition;
            _isFlyingToTarget = false;
            if (isServer) NetworkServer.Destroy(gameObject);

        }

        else transform.position += direction * step;
    }
}
