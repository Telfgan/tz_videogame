using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        if(Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
            transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, Camera.main.transform.eulerAngles.y + 180, transform.rotation.eulerAngles.z);
        }
    }
}
