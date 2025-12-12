using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class IcePuddleBigObject : Projectiles
{
    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private float radiusBetweenPoints = 1.6f;

    private bool _iceDeathInIcePudleTalent = false;

    private float _timeToDestroy = 0;
    private float _angle = 0;
    private bool _playerInside = false;

    public override void Init(Character dad, float timeToDestroy, bool lastHit, Skill skill)
    {
        _dad = dad;
        _skill = skill;
        _timeToDestroy = timeToDestroy;
        _initialized = true;

        SetupDecalArea(_angle);
        StartCoroutine(DestroyAfterTime());
    }

    public void SetupDecalArea(float angle)
    {
        _angle = angle;
        GenerateZone();
    }

    private void GenerateZone()
    {
        float angleStep = 72f;
        float rotationOffset = _angle;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[6];
        int[] triangles = new int[15];

        vertices[0] = Vector3.zero;
        for (int i = 1; i <= 5; i++)
        {
            float angle = (rotationOffset + (i - 1) * angleStep) * Mathf.Deg2Rad;
            vertices[i] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radiusBetweenPoints;
        }

        for (int i = 0; i < 5; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i == 4 ? 1 : i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
            filter.mesh = mesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = false;
            meshCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        _timeToDestroy -= Time.deltaTime;
        if (_timeToDestroy <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_initialized || _dad == null) return;

        if (other.gameObject == _dad.gameObject)
        {
            _playerInside = true;
            if (_iceDeathInIcePudleTalent)
                _dad.Health.IncreaseRegen(1.01f);
        }

        if (other.TryGetComponent<Character>(out var character) && character != _dad)
        {
            float duration = 2f;

            if (_iceDeathInIcePudleTalent)
                character.CharacterState.AddState(States.Frosting, duration, 0, _dad.gameObject, _skill.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_initialized || _dad == null) return;

        if (other.gameObject == _dad.gameObject && _playerInside)
        {
            _dad.Health.DecreaseRegen(1.01f);
            _playerInside = false;
        }
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(_timeToDestroy);
        Destroy(gameObject);
    }

    public void IceDeathInIcePudleTalentActive(bool value)
    {
        _iceDeathInIcePudleTalent = value;
    }
}
