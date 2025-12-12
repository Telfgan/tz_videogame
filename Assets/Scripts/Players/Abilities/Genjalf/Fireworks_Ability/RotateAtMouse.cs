using UnityEngine;

namespace Players.Abilities.Genjalf.Fireworks_Ability
{
    public class RotateAtMouse : MonoBehaviour
    {
        [SerializeField] CircleRendererAtMovingMouseTarget
            _circleRendererAtMovingMouseTarget;

        private void Update()
        {
            MoveAlongCircle();
            LookAtMouse();
        }

        private void MoveAlongCircle()
        {
            float angle = Mathf.Deg2Rad * (transform.eulerAngles.z + 90);
            float x = _circleRendererAtMovingMouseTarget.player.position.x +
                      _circleRendererAtMovingMouseTarget.radius * Mathf.Cos(angle);
            float y = _circleRendererAtMovingMouseTarget.player.position.y +
                      _circleRendererAtMovingMouseTarget.radius * Mathf.Sin(angle);
            transform.position = new Vector3(x, y, 0);
        }

        private void LookAtMouse()
        {
            Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }
}