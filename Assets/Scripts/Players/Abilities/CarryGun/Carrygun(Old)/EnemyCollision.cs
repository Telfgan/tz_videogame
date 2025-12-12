using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    public bool IsCollision = false;
    private GameObject _collision;

    private void Start()
    {
        StartCoroutine(DestroyThis());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision != null && collision.collider.CompareTag("Enemies") || collision.collider != null && collision.collider.CompareTag("Allies"))
        {
            IsCollision = true;
            _collision = collision.collider.gameObject;
            _collision.GetComponent<Rigidbody2D>().isKinematic = false;
            gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            _collision.GetComponent<MoveComponent>().CanMove = false;

        }
    }

    private IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(2f);
        if(_collision != null)
        {
            _collision.GetComponent<Rigidbody2D>().isKinematic = true;
            _collision.GetComponent<MoveComponent>().CanMove = true;

        }
        gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

        IsCollision = false;
        yield return new WaitForSeconds(1f);
        Destroy(this);
    }
}
