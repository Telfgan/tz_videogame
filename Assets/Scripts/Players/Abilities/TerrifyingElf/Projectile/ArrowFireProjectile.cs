using Unity.Mathematics;
using UnityEngine;
using System;

public class ArrowFireProjectile : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Rigidbody selfBody;

    public static event Action<Vector3> OnProjectilePathEnd;

    private void Start()
    {
        selfBody = GetComponent<Rigidbody>();
        selfBody.linearDamping = 0;
        selfBody.angularDamping = 0;
        selfBody.isKinematic = true;
    }

    public void OnPathEnd(float3 velocity)
    {
        Vector3 endPoint = transform.position;

        OnProjectilePathEnd?.Invoke(endPoint);

        Destroy(gameObject);
    }
}
