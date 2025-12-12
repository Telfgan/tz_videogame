//using Mirror;
//using System.Collections.Generic;
//using UnityEngine;

//public class TrapOld : Projectiles
//{
//    [SerializeField] private LineRenderer lineRenderer;
//    [SerializeField] private Transform pointTrapRight;
//    [SerializeField] private Transform pointTrapLeft;
//    [SerializeField] private List<GameObject> hitBoxes;
//    [SerializeField] private Material ropeMaterial;
//    [SerializeField] private List<GameObject> ropes;

//    private readonly List<BoxCollider> _boxes = new();
//    private readonly List<Vector3> _baseSizes = new();

//    private HeroComponent _owner;
//    private Vector3 _startPosition;
//    private Vector3 _endPosition;

//    private bool _secondFixed;
//    private const float YFix = 0.2f;

//    private List<Character> _charactersInTrigger = new List<Character>();

//    private void Awake()
//    {
//        foreach (var hitBox in hitBoxes)
//        {
//            if (hitBox == null) continue;
//            if (hitBox.TryGetComponent(out BoxCollider boxCollider))
//            {
//                _boxes.Add(boxCollider);
//                _baseSizes.Add(boxCollider.size);
//            }

//            hitBox.SetActive(false);
//        }
//    }


//    public void Init(HeroComponent owner, Skill skill, Vector3 startPosition, Vector3 endPosition)
//    {
//        _owner = owner;
//        _skill = skill;
//        _startPosition = startPosition;
//        _endPosition = endPosition;
//        _initialized = true;

//        SetupTrapShape();
//    }

//    public void ResetPreview()
//    {
//        lineRenderer.positionCount = 2;
//        SetLine(pointTrapRight.position, pointTrapRight.position);

//        pointTrapLeft.gameObject.SetActive(false);
//        _secondFixed = false;

//        for (int i = 0; i < hitBoxes.Count; i++)
//        {
//            if (hitBoxes[i] == null) continue;
//            hitBoxes[i].SetActive(false);
//            if (_boxes.Count > i)
//            {
//                _boxes[i].size = _baseSizes[i];
//                _boxes[i].center = new Vector3(_boxes[i].center.x, _boxes[i].center.y, 0f);
//            }
//        }
//    }

//    public void UpdateSecondPoint(Vector3 worldPos)
//    {
//        if (_secondFixed) return;

//        worldPos.y = pointTrapRight.position.y;
//        pointTrapLeft.position = worldPos;
//        SetLine(pointTrapRight.position, pointTrapLeft.position);
//    }

//    public void FixSecondPoint()
//    {
//        _secondFixed = true;
//        foreach (var hitBox in hitBoxes) hitBox?.SetActive(true);
//    }

//    private void SetLine(Vector3 a, Vector3 b)
//    {
//        a.y = b.y = YFix;
//        lineRenderer.SetPosition(0, a);
//        lineRenderer.SetPosition(1, b);
//    }

//    private void SetupTrapShape()
//    {
//        Vector3 dir = _endPosition - _startPosition;
//        transform.position = _startPosition + dir * 0.5f;

//        foreach (GameObject rope in ropes) if (rope.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer)) meshRenderer.material = ropeMaterial;
//    }

//    public void Finalise(Vector3 start, Vector3 end)
//    {
//        if (!pointTrapLeft.gameObject.activeSelf)
//            pointTrapLeft.gameObject.SetActive(true);

//        _startPosition = start;
//        _endPosition = end;

//        transform.position = start;
//        pointTrapLeft.position = end;

//        SetLine(pointTrapRight.position, pointTrapLeft.position);

//        StretchHitBoxes(start, end);
//        FixSecondPoint();
//    }

//    private void StretchHitBoxes(Vector3 start, Vector3 end)
//    {
//        Vector3 direction = end - start;
//        float length = Mathf.Max(0.01f, direction.magnitude);
//        Vector3 forward = direction.normalized;

//        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

//        for (int i = 0; i < hitBoxes.Count; ++i)
//        {
//            if (hitBoxes[i] == null || _boxes.Count <= i) continue;

//            hitBoxes[i].transform.position = start + direction * 0.5f;
//            hitBoxes[i].transform.rotation = rotation;

//            var box = _boxes[i];
//            var baseSize = _baseSizes[i];

//            box.size = new Vector3(baseSize.x, baseSize.y, length);

//            box.center = new Vector3(box.center.x, box.center.y, 0f);
//        }
//    }


//    public void HandleHit(Collider other)
//    {
//        if (!_initialized) return;

//        if (other.TryGetComponent<Character>(out var target) && !_charactersInTrigger.Contains(target))
//        {
//            _charactersInTrigger.Add(target);
//            if (target.TryGetComponent<CharacterState>(out CharacterState state)) state.AddState(States.Bound, 99999f, 0, _owner.gameObject, _skill.name);
//        }

//        Destroy(gameObject);
//    }
//}
