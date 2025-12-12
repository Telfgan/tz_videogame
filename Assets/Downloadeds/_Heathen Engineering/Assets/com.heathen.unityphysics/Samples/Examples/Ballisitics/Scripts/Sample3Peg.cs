#if HE_SYSCORE

using UnityEngine;

namespace HeathenEngineering.Demos
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Sample3Peg : MonoBehaviour
    {
        public Color normal;
        public Color hit;
        public float fadeTime;

        SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, normal, Time.deltaTime * (1f / fadeTime));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            spriteRenderer.color = hit;
        }
    }
}

#endif