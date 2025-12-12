using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProjectile
{
    public float Speed { get; }
    public TargetInfo TargetInfo { get; }

    public Action<Projectile, GameObject> TargetReached { get; set; }

    public void SetTarget(TargetInfo targetInfo);
    public void SetSpeed(float speed);
    public void StartFly();
}
