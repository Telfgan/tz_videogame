using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FIreRaycast : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private ParticleSystem _particleSystem;
    private ParticleSystem.MainModule main;
    bool particleHit = false;
    bool isActive = true;

    private void Awake()
    {
        _particleSystem = this.GetComponent<ParticleSystem>();
        main = _particleSystem.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive)
        {
            CastRay(transform.TransformDirection(Vector2.up), 4f, layerMask);
        }
    }

    public void SwitchTurnOn(bool shouldBeActive)
    {
        isActive = shouldBeActive;
        if(shouldBeActive) _particleSystem.Play();
        else _particleSystem.Stop();
    }

    private void CastRay(Vector3 dir, float distance, LayerMask layer)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, layer);

        if (hit.collider != null)
        {
            if(!particleHit)
            {
                particleHit = true;
                _particleSystem.Stop();
                _particleSystem.Play();
            }
            Debug.DrawLine(transform.position, hit.point, Color.red);

            main.startLifetime = 0.1f * hit.distance; // 0.1f = (1f / max speed партикла)
        }
        else
        {
            main.startLifetime = 0.12f * distance; // тут немного больше

            if (particleHit)
            {
                particleHit = false;
            }         
            Debug.DrawLine(transform.position, transform.position + dir * distance, Color.green);
        }
    }
}
