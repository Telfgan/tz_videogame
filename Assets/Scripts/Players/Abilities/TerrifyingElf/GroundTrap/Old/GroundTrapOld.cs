//using Mirror;
//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class GroundTrapOLd : Skill
//{
//    [SerializeField] private Trap trapPrefab;
//    [SerializeField] private HeroComponent owner;
//    [SerializeField] private DrawCircleAlternative minDistanceRadiusCircle;
//    [SerializeField] private ArrowTrapProjectile arrowTrapProjectile;
//    [SerializeField] private Transform spawnPoint;
//    [SerializeField] private float minDistanceRadius = 2f;
//    [SerializeField] private float distanceforTrap = 2.1f;
//    [SerializeField] private float newHealth = 80;
//    [SerializeField] private ObjectData groundData;

//    [Header("Talents")]
//    [SerializeField] private bool isGroundHealthTalent;

//    [Header("Raycast masks")]
//    [SerializeField] private LayerMask groundLayer;

//    private Color minDistanceGreenColor = Color.green;
//    private Color minDistanceRedColor = Color.red;

//    private Trap _preview;
//    private bool _isStartPointPlaced;
//    private Vector3 _startPosition, _endPosition;
//    private float baseHealth = 23;

//    protected override bool IsCanCast => _isStartPointPlaced ||
//                                         Vector3.Distance(transform.position, _endPosition) <= Radius &&
//                                         Vector3.Distance(transform.position, _endPosition) >= minDistanceRadius &&
//                                         Vector3.Distance(_startPosition, _endPosition) <= distanceforTrap;

//    protected override int AnimTriggerCastDelay => Animator.StringToHash("Shot");
//    protected override int AnimTriggerCast => 0;


//    private void OnDestroy() => OnSkillCanceled -= HandleSkillCanceled;
//    private void OnEnable() => OnSkillCanceled += HandleSkillCanceled;

//    private Vector3 GetMousePointOnGround(float y = 0f)
//    {
//        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
//        {
//            Vector3 point = hit.point;
//            point.y = y;
//            return point;
//        }

//        return Vector3.positiveInfinity;
//    }

//    private void HandleSkillCanceled()
//    {
//        if (_hero?.Move != null) Hero.Move.CanMove = true;

//        if (_preview != null)
//        {
//            Destroy(_preview.gameObject);
//            _preview = null;
//        }

//        Hero.Animator.speed = 1;
//        minDistanceRadiusCircle?.Clear();
//    }


//    private void UpdateMinRadiusCircle(Vector3 mousePos)
//    {
//        bool inside = Vector3.Distance(transform.position, mousePos) < minDistanceRadius;
//        var color = inside ? minDistanceRedColor : minDistanceGreenColor;

//        if (minDistanceRadiusCircle != null)
//        {
//            minDistanceRadiusCircle.SetColor(color);
//            minDistanceRadiusCircle.Draw(minDistanceRadius);
//        }
//    }

//    private bool InsideRadius(Vector3 position)
//    {
//        float direction = Vector3.Distance(transform.position, position);
//        return direction >= minDistanceRadius && direction <= Radius;
//    }

//    private void SetGroundNewHealth()
//    {
//        groundData.MaxHealth = newHealth;
//        CmdSetGorundNewHealth();
//    }

//    private void SetGroundBaseHealth()
//    {
//        groundData.MaxHealth = baseHealth;
//        CmdSetGorundBaseHealth();
//    }

//    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
//    {
//        if (isGroundHealthTalent) SetGroundNewHealth();
//        else SetGroundBaseHealth();

//        Debug.Log($"GroundTrapHealth: {groundData.MaxHealth}");

//        Hero.Animator.speed = Hero.Animator.speed / CastDeley;

//        Hero.Move.CanMove = false;
//        Hero.Move.StopMoveAnimation();

//        _preview = Instantiate(trapPrefab);
//        _preview.ResetPreview();

//        minDistanceRadiusCircle?.SetColor(minDistanceGreenColor);
//        minDistanceRadiusCircle?.Draw(minDistanceRadius);

//        while (!_isStartPointPlaced)
//        {
//            Vector3 position = GetMousePointOnGround();
//            if (float.IsPositiveInfinity(position.x)) { yield return null; continue; }

//            _preview.transform.position = position;
//            UpdateMinRadiusCircle(position);

//            if (InsideRadius(position) && GetMouseButton)
//            {
//                _startPosition = position;
//                _preview.transform.position = _startPosition;
//                _isStartPointPlaced = true;
//                _preview.gameObject.SetActive(true);
//                continue;
//            }
//            _preview.transform.position = position;
//            yield return null;
//        }

//        _preview.transform.GetChild(1).gameObject.SetActive(true);

//        yield return new WaitUntil(() => !GetMouseButton);
//        yield return new WaitForSeconds(0.1f);

//        while (true)
//        {
//            Vector3 rawPos = GetMousePointOnGround();
//            if (float.IsPositiveInfinity(rawPos.x)) { yield return null; continue; }

//            Vector3 dir = rawPos - _startPosition;
//            float dist = dir.magnitude;

//            if (dist > distanceforTrap) rawPos = _startPosition + dir.normalized * distanceforTrap;

//            bool blocked = Physics.Raycast(_startPosition + Vector3.up * 0.1f, dir.normalized, dist, _obstacle);

//            _preview.UpdateSecondPoint(rawPos);
//            UpdateMinRadiusCircle(rawPos);

//            bool inOuterRadius = Vector3.Distance(transform.position, rawPos) <= Radius;
//            bool outOfInnerRing = Vector3.Distance(transform.position, rawPos) >= minDistanceRadius;

//            if (GetMouseButton && inOuterRadius && outOfInnerRing && !blocked)
//            {
//                _endPosition = rawPos;
//                break;
//            }

//            yield return null;
//        }

//        minDistanceRadiusCircle?.Clear();

//        TargetInfo targetInfo = new TargetInfo();
//        targetInfo.Points.Add(_startPosition); targetInfo.Points.Add(_endPosition);
//        callbackDataSaved.Invoke(targetInfo);
//    }

//    protected override IEnumerator CastJob()
//    {
//        if (_preview) Destroy(_preview.gameObject);
//        CmdSpawnGroundTrap(_startPosition, _endPosition);

//        ClearData();
//        HandleSkillCanceled();
//        _preview = null;
//        yield break;
//    }

//    protected override void ClearData()
//    {
//        Hero.Animator.speed = 1;
//        Hero.Move.CanMove = true;
//        _isStartPointPlaced = false;
//    }

//    public void AnimSpawnTrapProjectile()
//    {
//        if (!isClient || !owner) return;

//        CmdSpawnArrowProjectile(_endPosition);
//    }

//    public override void LoadTargetData(TargetInfo targetInfo)
//    {
//        if (targetInfo.Points.Count < 2) return;

//        _startPosition = targetInfo.Points[0];
//        _endPosition = targetInfo.Points[1];

//        if (_preview == null) { _preview = Instantiate(trapPrefab); }
//        _preview.transform.position = _startPosition;
//        _preview.transform.GetChild(1).gameObject.SetActive(true);
//        _preview.UpdateSecondPoint(_endPosition);
//    }



//    [Command] private void CmdSetGorundNewHealth() => groundData.MaxHealth = newHealth;
//    [Command] private void CmdSetGorundBaseHealth() => groundData.MaxHealth = baseHealth;

//    [Command]
//    private void CmdSpawnGroundTrap(Vector3 startPosition, Vector3 endPosition)
//    {
//        Trap trap = Instantiate(trapPrefab, startPosition, Quaternion.identity);
//        trap.Init(owner, this, startPosition, endPosition);
//        trap.Finalise(startPosition, endPosition);
//        SceneManager.MoveGameObjectToScene(trap.gameObject, Hero.NetworkSettings.MyRoom);
//        NetworkServer.Spawn(trap.gameObject);
//        RpcInit(trap.netIdentity, startPosition, endPosition);
//    }

//    [Command]
//    void CmdSpawnArrowProjectile(Vector3 targetPosition)
//    {
//        if (!arrowTrapProjectile) return;

//        Vector3 startPos = spawnPoint ? spawnPoint.position : transform.position;
//        Vector3 direction = (targetPosition - startPos).normalized;
//        if (direction == Vector3.zero) return;

//        var arrow = Instantiate(arrowTrapProjectile, startPos, Quaternion.LookRotation(direction));

//        SceneManager.MoveGameObjectToScene(arrow.gameObject, Hero.NetworkSettings.MyRoom);
//        NetworkServer.Spawn(arrow.gameObject);

//        arrow.StartFly(targetPosition);

//        RpcInitArrow(arrow.netIdentity, targetPosition);
//    }


//    [ClientRpc]
//    void RpcInitArrow(NetworkIdentity arrowId, Vector3 targetPos)
//    {
//        if (arrowId && arrowId.TryGetComponent(out ArrowTrapProjectile arrow)) arrow.StartFly(targetPos);
//    }

//    [ClientRpc]
//    protected void RpcInit(NetworkIdentity groundTrap, Vector3 startPosition, Vector3 endPositionb)
//    {
//        if (groundTrap && groundTrap.TryGetComponent(out Trap trap))
//        {
//            trap.Init(owner, this, startPosition, endPositionb);
//            trap.Finalise(startPosition, endPositionb);
//            trap.FixSecondPoint();
//        }
//    }

//    #region Talent
//    public void GroundTrapHealthActiveTalent(bool value) => isGroundHealthTalent = value;
//    #endregion
//}
