using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Trap : Projectiles
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform pointTrapRight;
    [SerializeField] private Transform pointTrapLeft;
    [SerializeField] private List<GameObject> hitBoxes;
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private List<GameObject> ropes;

    private readonly List<BoxCollider> _boxes = new();
    private readonly List<Vector3> _baseSizes = new();

    private HeroComponent _owner;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private bool _secondFixed;
    private const float YFix = 0.2f;

    private List<Character> _charactersInTrigger = new List<Character>();

    private void Awake()
    {
        foreach (var hitBox in hitBoxes)
        {
            if (hitBox == null) continue;

            if (hitBox.TryGetComponent(out BoxCollider boxCollider))
            {
                _boxes.Add(boxCollider);
                _baseSizes.Add(boxCollider.size);
            }

            hitBox.SetActive(false);
        }
    }

    public void Init(HeroComponent owner, Skill skill, Vector3 startPosition, Vector3 endPosition)
    {
        _owner = owner;
        _skill = skill;
        _startPosition = startPosition;
        _endPosition = endPosition;
        _initialized = true;

        SetupTrapShape();
    }

    public void ResetPreview()
    {
        SetLine(pointTrapRight.position, pointTrapLeft.position);

        pointTrapLeft.gameObject.SetActive(true);
        foreach (var hitBox in hitBoxes) hitBox?.SetActive(false);

        _secondFixed = false;
    }

    public void FixSecondPoint()
    {
        _secondFixed = true;
        foreach (var hitBox in hitBoxes)
            hitBox?.SetActive(true);
    }

    private void SetLine(Vector3 a, Vector3 b)
    {
        a.y = b.y = YFix;
        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);
    }

    private void SetupTrapShape()
    {
        foreach (GameObject rope in ropes)
            if (rope.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
                meshRenderer.material = ropeMaterial;
    }

    public void Finalise()
    {
        SetLine(pointTrapRight.position, pointTrapLeft.position);
        FixSecondPoint();
    }

    public void HandleHit(Collider other)
    {
        if (!_initialized) return;

        if (other.TryGetComponent<Character>(out var target) && !_charactersInTrigger.Contains(target))
        {
            _charactersInTrigger.Add(target);

            if (target.TryGetComponent<CharacterState>(out CharacterState state))
            {
                state.AddState(States.Bound, 99999f, 0, _owner.gameObject, _skill.name);
            }
        }

        Destroy(gameObject);
    }

    public void UpdateLinePreview() => SetLine(pointTrapRight.position, pointTrapLeft.position);
}
