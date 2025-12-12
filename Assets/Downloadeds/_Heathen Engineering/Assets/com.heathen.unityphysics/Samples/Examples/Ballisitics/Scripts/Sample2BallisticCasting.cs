#if HE_SYSCORE
using HeathenEngineering.UnityPhysics;
using HeathenEngineering.UnityPhysics.API;
using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY\nIt is specific to this example scene and simply handles user input.")]
    public class Sample2BallisticCasting : MonoBehaviour
    {
        private TrickShot trickShot;

        private void Start()
        {
            trickShot = GetComponentInChildren<TrickShot>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                /***********************************************************************************
                 * Right Mouse button held
                 * Here we are simply moving the camera around so you can change your point of view
                 ***********************************************************************************/
                var rotationX = Input.GetAxis("Mouse X");
                var rotationY = Input.GetAxis("Mouse Y");
                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
                transform.localRotation = transform.localRotation * xQuaternion * yQuaternion;
                transform.LookAt(transform.position + transform.forward);
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                /***************************************************************
                 * When the Spacebar is not being held down
                 * Here we check for a ray cast hit under the mouse pointer
                 * if a hit is found we calculate the solution and aim at it
                 * we then apply that rotation to the emitter transform
                 ***************************************************************/

                //Find the world point under the mouse by casting a ray from the camera through the mouse pointer
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out var hit))
                {
                    //The ray cast hit something so we want to aim the emitter at it using the TrickShot's position, speed and constant acceleration
                    if (Ballistics.Solution(trickShot.transform.position, trickShot.speed, hit.point, trickShot.constantAcceleration, out Quaternion low, out Quaternion _) >= 1)
                    {
                        //We found at least 1 solution, so use the low angle as the rotation for the emitter this will cause it to "look" along the path such that the projectile will hit where the mouse is pointing
                        trickShot.transform.rotation = low;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                /********************************************************************************************************
                 * Left Mouse button held
                 * Simply shoot a projectile, this uses TrickShot to shoot so the projectile will be 
                 * deterministically controlled for the duration of the path according to the TrickShot settings.
                 ********************************************************************************************************/
                trickShot.Shoot();
            }
        }
    }
}

#endif