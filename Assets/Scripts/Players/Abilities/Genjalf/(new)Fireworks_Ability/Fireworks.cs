using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireworks : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRendererVerical;
    [SerializeField] private SpriteRenderer _spriteRendererHorizontal;
    [SerializeField] private BoxCollider2D _colliderVerical;
    [SerializeField] private BoxCollider2D _colliderHorizontal;

    private float _inactiveTransparency = 0.35f;
    private Transform _target;
    private List<Collider2D> _collisions = new List<Collider2D>();

    public List<Collider2D> Collisions { get => _collisions; set => _collisions = value; }

    private void Start()
    {
        SetInactive();
    }

    private void Update()
    {
        if(_target != null)
        {
            RotateAtTarget(_target);
        }
    }

    public void Activate()
    {
        _colliderVerical.enabled = true;
        _colliderHorizontal.enabled = true;

        _spriteRendererVerical.color = new Color
            (
            _spriteRendererVerical.color.r,
            _spriteRendererVerical.color.g,
            _spriteRendererVerical.color.b,
            1
            );
        _spriteRendererHorizontal.color = new Color
            (
            _spriteRendererHorizontal.color.r,
            _spriteRendererHorizontal.color.g,
            _spriteRendererHorizontal.color.b,
            1
            );
    }

    public void SetInactive()
    {
        _colliderVerical.enabled = false;
        _colliderHorizontal.enabled = false;

        _spriteRendererVerical.color = new Color
            (
            _spriteRendererVerical.color.r,
            _spriteRendererVerical.color.g,
            _spriteRendererVerical.color.b,
            _inactiveTransparency
            );
        _spriteRendererHorizontal.color = new Color
            (
            _spriteRendererHorizontal.color.r,
            _spriteRendererHorizontal.color.g,
            _spriteRendererHorizontal.color.b,
            _inactiveTransparency
            );
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void RotateAtMouse()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    public void SetLength(float value)
    {
        Vector2 newSize = new Vector2(_spriteRendererVerical.size.x, value);
        _spriteRendererVerical.size = newSize;
        _colliderVerical.size = newSize;

        _spriteRendererVerical.transform.Translate(new Vector3(0, value, 0));
    }

    public void SetWidth(float value)
    {
        Vector2 newSize = new Vector2(value, _spriteRendererVerical.size.y);
        _spriteRendererVerical.size = newSize;
        _colliderVerical.size = newSize;
    }

    public void SetPositionForExtraWidth(float value)
    {
        _spriteRendererHorizontal.transform.Translate(new Vector3(0, value, 0));
    }

    public void SetExtraWidth(float value)
    {
        Vector2 newSize = new Vector2(value, _spriteRendererHorizontal.size.y);
        _spriteRendererHorizontal.size = newSize;
        _colliderHorizontal.size = newSize;

        newSize = new Vector2(_spriteRendererHorizontal.size.x, _spriteRendererVerical.size.x);
        _spriteRendererHorizontal.size = newSize;
        _colliderHorizontal.size = newSize;
    }

    public void RotateAtTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _collisions.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _collisions.Remove(collision);
    }
}
