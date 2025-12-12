    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionMove : MoveComponent
{
    //[SerializeField] private NavMeshAgent _agent;

    //protected override void Move()
    //{
    //    _agent.speed = _currentSpeed;

    //    var animDir = transform.InverseTransformPoint(_agent.velocity + transform.position);
    //    _anim.SetFloat(HashAnimPlayer.VelocityZ, animDir.z);
    //    _anim.SetFloat(HashAnimPlayer.VelocityX, animDir.x);
    //}

    //protected override void RotateAtCursor()
    //{
    //    /*
    //    var animDir = transform.InverseTransformPoint(_agent.velocity + transform.position);
    //    var transformRotate = transform.eulerAngles;
    //    transform.LookAt(animDir);
    //    transform.eulerAngles = (new Vector3(transformRotate.x, transform.eulerAngles.y, transformRotate.z));
    //    */
    //}
}
