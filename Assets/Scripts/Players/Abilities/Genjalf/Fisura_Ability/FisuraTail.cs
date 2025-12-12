using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FisuraTail : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private BoxCollider2D _collider;
    [SerializeField] private BoxCollider2D _trigger;

    public Vector2 Size { get => _spriteRenderer.size; }

    private void Awake()
    {
        _collider.enabled = false;
        _trigger.enabled = false;
    }

    public void Activate(float livetime)
    {
        _collider.enabled = true;
        _trigger.enabled = true;
        Destroy(gameObject, livetime);
    }

    public void Rotate()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    public void Rotate(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetSize(Vector2 vector2)
    {
        _spriteRenderer.size = vector2;
        _collider.size = vector2;
        _trigger.size = vector2 * .95f;

        _spriteRenderer.transform.Translate(new Vector3(0, vector2.y, 0));
        _collider.transform.Translate(new Vector3(0, vector2.y, 0));
    }

    public void SetSizeWithoutOffset(Vector2 vector2)
    {
        _spriteRenderer.size = vector2;
        _collider.size = vector2;
        _trigger.size = vector2 * .95f;
    }

    public void AddLength(float value)
    {
        Vector2 newSize = new Vector2(_spriteRenderer.size.x, _spriteRenderer.size.y + value);
        _spriteRenderer.size = newSize;
        _collider.size = newSize;
        _trigger.size = newSize * .95f;

        _spriteRenderer.transform.Translate(new Vector3(0, value, 0));
        _collider.transform.Translate(new Vector3(0, value, 0));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MoveComponent>(out MoveComponent player))
        {
            player.GetComponent<Rigidbody2D>().isKinematic = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MoveComponent>(out MoveComponent player))
        {
            player.GetComponent<Rigidbody2D>().isKinematic = false;
        }
    }
}
