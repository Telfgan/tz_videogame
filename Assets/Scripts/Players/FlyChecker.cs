using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyChecker : MonoBehaviour
{
    public Action OffedGround;
    public Action ReachGround;

    private void OnTriggerEnter(Collider other)
    {
        ReachGround?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        OffedGround?.Invoke();
    }
}
