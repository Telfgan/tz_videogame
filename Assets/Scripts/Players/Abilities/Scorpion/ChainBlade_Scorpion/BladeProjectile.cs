using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class BladeProjectile : NetworkBehaviour
{
    //[SerializeField] private BoxCollider2D _collider;
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private LayerMask _obstacleLayers;
    public Transform ChainLinkPoint;
    private float throwForce = 20f;
    public /*Rigidbody2D*/ Rigidbody _rb;
    private float _maxDistance;
    private Vector3 startPosition;
    private Vector3 _direction;
    private GameObject _player;
    [SyncVar] private bool _canPull = true;
    private ChainbladeType _type;

    public UnityEvent<GameObject> OnHit;

    public void Init(float maxDistance, Vector3 direction, GameObject player, ChainbladeType type)
    {
        Debug.Log("Im spawned");

        startPosition = transform.position;
        _maxDistance = maxDistance;
        _type = type;
        _direction = direction.normalized;
        float angle = Mathf.Atan2(_direction.z, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        _player = player;

        if(_type == ChainbladeType.Default)
            Destroy(gameObject, _maxDistance / throwForce);

        if (_type == ChainbladeType.Hook)
            StartCoroutine(DieOnDistance());
        //StartCoroutine(DieOnDistance());
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.LogWarning("BladeProjectile. TriggerEnter()!!");
        if (!_canPull)
            return;

        if (((1 << collision.gameObject.layer) & _obstacleLayers.value) != 0)
        {
            Debug.Log($"BladeProjectile hit OBSTACLE: {collision.gameObject.name}");

            if (_type == ChainbladeType.Default)
            {
                Destroy(gameObject);
            }

            SendInfo(null);
            _canPull = false;
            return;
        }

        if (((1 << collision.gameObject.layer) & _layerMask.value) != 0)
        {
            Debug.Log("HIT LAYER MASK");
            SendInfo(collision.gameObject);
            HitPerfomed();
            _canPull = false;
        }
    }

    private void SendInfo(GameObject target)
    {
        OnHit?.Invoke(target);
    }
    public void ThrowBlade(Vector3 endPoint)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(/*_direction * */new Vector3(_direction.x, 0, _direction.z) * throwForce, /*ForceMode2D.Impulse*/ ForceMode.Impulse);
    }

    private void HitPerfomed() 
    {
        //можно добавть визуал/партиклы при попадании
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        
    }
    private IEnumerator DieOnTimer()
    {
        float lifeTime = _maxDistance / throwForce;
        while (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
            yield return null;
        }
        if (_type == ChainbladeType.Hook)
        {
            _collider.isTrigger = false;
            SendInfo(null);
            // will be destroyed in ChainBlade_Scorpion after returning to parent
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator DieOnDistance()
    {
        float distance = Vector3.Distance(transform.position, startPosition);
        while(distance < _maxDistance)
        {
            distance = Vector3.Distance(transform.position, startPosition);
            yield return null;
        }
        if (_type == ChainbladeType.Hook)
        {
            //_collider.isTrigger = false;
            _canPull = false;
            SendInfo(null);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
