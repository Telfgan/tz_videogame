using UnityEngine;

public class CastPrefab : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    private float _alpha;

    void Update()
    {
        Transform childObject = transform.Find("GameObject");
        _alpha = Mathf.Lerp(1f, 0.9f, (childObject.localScale.x - 0.9f) / (0f - 0.9f));

        SetAlpha(_alpha);
        if (childObject.localScale.x == 1)
        {
            Destroy(gameObject);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color currentColor = SpriteRenderer.color;
        currentColor.a = alpha;
        SpriteRenderer.color = currentColor;
    }

}

