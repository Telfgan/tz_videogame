using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPrefab : MonoBehaviour
{
    private float _timer;
    public float TimeDestroy = 0.5f;

    void Update()
    {        
        _timer = _timer + Time.deltaTime;
        if (_timer > TimeDestroy) Destroy(gameObject);
    }

}
