using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBreath_Prefab : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer _triangleSprite;

    public List<Collider> _collisions = new List<Collider>();
    public List<FIreRaycast> _flames = new List<FIreRaycast>();

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponentInChildren<Collider>();
    }
    private void Start()
    {
        //_collider.enabled = false;
        //SetParticlesState(false);

    }
    public void Activate()
    {
        _triangleSprite.enabled = false;
        _collider.enabled = true;
        SetParticlesState(true);
    }
    public void RotateAtMouse()
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        //CmdSyncRotation(angle);
    }
    public void SetParticlesState(bool shouldBeActive)
    {
        foreach (FIreRaycast flame in _flames)
        {
            flame.SwitchTurnOn(shouldBeActive);
        }
    }
    public void StartFollowingMouse()
    {
        StartCoroutine(FollowMouse());
    }
    private IEnumerator FollowMouse()
    {
        while (true)
        {
            Debug.Log("Rotate");
            Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90), 30f * Time.deltaTime);
            CmdSyncRotation(angle);
            yield return null;
        }
    }
    [Command]
    private void CmdSyncRotation(float angle)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90), 30f * Time.deltaTime);
        Debug.Log(angle);
    }
    private void OnTriggerEnter(Collider other)
    {
        _collisions.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _collisions.Remove(other);
    }
}
