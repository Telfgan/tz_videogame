using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    public Vector3 Position { get; }
    public Transform Transform { get; }

    public bool IsTargetable { get; }

}
