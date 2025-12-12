#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Sample4RandomRoller : MonoBehaviour
    {
        public Rigidbody selfBody;
        public float scalar;
        public float frequency;

        private void Start()
        {
            Invoke(nameof(AddForce), frequency);
        }

        private void AddForce()
        {
            var random = UnityEngine.Random.insideUnitCircle;
            selfBody.AddForce(new Vector3(random.x * scalar, 0, random.y * scalar));
            Invoke(nameof(AddForce), frequency);
        }
    }
}

#endif