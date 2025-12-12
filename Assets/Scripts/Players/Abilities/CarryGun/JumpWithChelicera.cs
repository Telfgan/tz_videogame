using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class JumpWithChelicera : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private CheliceraStrike _cheliceraeStrike;
    [SerializeField] private ClawStrike clawStrike;
    [SerializeField] private CooldownEnergy cooldownEnergy;
    [SerializeField] private float basePsi = 1f;
    [SerializeField] private float distanceJump;
    [SerializeField] private float cooldownJump = 12f;

    private Animator _animator;
    //private IDamageable _target;
    //private Character _runtimeTarget;
    private Vector3 _mousePosition = Vector3.positiveInfinity;

    private static readonly int jumpStart = Animator.StringToHash("JumpStart");
    private static readonly int jumpEnd = Animator.StringToHash("JumpEnd");

    private float _delayBeforeJump = 0.3f;
    private float _minDistance = 0.6f;
    private float _additionalDamageInPercentage;

    private bool _isTarget = false;
    private bool _isJumpDone = false;
    private bool _hasDealtDamage = false;
    private bool _isCheliceraStrikeCast = false;

    public override bool IsPayCostStartCooldown => false;
    protected override int AnimTriggerCast => jumpStart;
    protected override int AnimTriggerCastDelay => 0;

    //public Character RuntimeTarget { get => _runtimeTarget; set => _runtimeTarget = value; }
    //public IDamageable Target { get => _target; set => _target = value; }
    public bool IsJumpDone { get => _isJumpDone; set => _isJumpDone = value; }
    public bool IsCheliceraStrikeCast { get => _isCheliceraStrikeCast; set => _isCheliceraStrikeCast = value; }
    public float CooldownJump { get => cooldownJump; set => cooldownJump = value; }

    protected override bool IsCanCast => GetTargetCharacter() != null && cooldownEnergy.CurrentValue >= cooldownJump && CheckCanCast();

    private bool isJumpWithCheliceraChanceDamageCrit = false;
    public void JumpWithCheliceraChanceDamageCrit(bool value) => isJumpWithCheliceraChanceDamageCrit = value;

    private void Start() => _animator = GetComponent<Animator>();
    private void OnDestroy() => Canceled -= HandleJumpWithCheliceraEnd;
    private void OnEnable() => Canceled += HandleJumpWithCheliceraEnd;

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
        _mousePosition = Vector3.positiveInfinity;
        _hasDealtDamage = false;
    }

    public void JumpWithCheliceraAnimationMove()
    {
        if (_hero == null || _hero.Move == null) return;
        _hero.Move.StopMoveAndAnimationMove();
        _hero.Move.CanMove = false;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        _castDeley = _delayBeforeJump;
        //_runtimeTarget = null;

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget();
                //if (_target is Character characterTarget) _runtimeTarget = characterTarget;
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        if (GetTargetCharacter() != null) targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved?.Invoke(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() != null) ExecuteJump(GetTargetCharacter());
        yield return null;
    }

    private void ExecuteJump(IDamageable target)
    {
        if (target == null) return;

        _isJumpDone = true;
        float distanceToTarget = Vector2.Distance(((MonoBehaviour)target).transform.position, _player.transform.position);

        _additionalDamageInPercentage = distanceToTarget < 1 ? 0.1f : 0.2f + Mathf.Floor((distanceToTarget - 1f)) * 0.2f;

        Vector3 direction = (((MonoBehaviour)target).transform.position - transform.position).normalized;
        _isCheliceraStrikeCast = true;
        clawStrike.DurationChanceApplyBleedingWithJump();

        if (target is Character character)
            CmdExecuteJump(_player.gameObject, character.netId, direction, _additionalDamageInPercentage);
        else if (target is NetworkBehaviour nb)
            CmdExecuteJumpToPosition(_player.gameObject, ((MonoBehaviour)target).transform.position, nb.netId, _additionalDamageInPercentage);
    }

    private void HandleJumpAnimEnd()
    {
        if (_animator != null)
        {
            float transitionDuration = 0.15f;
            _animator.CrossFade(jumpEnd, transitionDuration);
        }
    }

    public void HandleJumpWithCheliceraEnd()
    {
        _animator.applyRootMotion = false;
        _player.Move.StopLookAt();
        Hero.Move.CanMove = true;
    }

    [Command]
    private void CmdExecuteJump(GameObject player, uint targetNetId, Vector3 direction, float additionalDamage)
    {
        if (!NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity identity)) return;
        Character targetCharacter = identity.GetComponent<Character>();
        if (targetCharacter == null) return;

        MoveComponent playerMove = player.GetComponent<MoveComponent>();
        Vector3 jumpPosition = Vector3.MoveTowards(targetCharacter.transform.position, player.transform.position, _minDistance);
        playerMove.TargetRpcDoMove(jumpPosition, distanceJump / 10);
        StartCoroutine(TrackMovementDuringJumpCoroutine(playerMove, targetCharacter.netId, additionalDamage));
    }

    [Command]
    private void CmdExecuteJumpToPosition(GameObject player, Vector3 targetPosition, uint targetNetId, float additionalDamage)
    {
        MoveComponent playerMove = player.GetComponent<MoveComponent>();
        Vector3 jumpPosition = Vector3.MoveTowards(targetPosition, player.transform.position, _minDistance);
        playerMove.TargetRpcDoMove(jumpPosition, distanceJump / 10);

        StartCoroutine(TrackMovementDuringJumpCoroutine(playerMove, targetNetId, additionalDamage));
    }

    private IEnumerator TrackMovementDuringJumpCoroutine(MoveComponent playerMove, uint targetNetId, float additionalDamage)
    {
        Vector3 lastPlayerPos = playerMove.transform.position;

        float playerDistanceAccumulator = 0f;
        float stopDistance = _minDistance + 0.5f;

        Transform targetTransform = null;
        IDamageable targetDamageable = null;

        if (NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity identity))
        {
            targetTransform = identity.transform;
            targetDamageable = identity.GetComponent<IDamageable>();
        }

        while (targetTransform != null && Vector3.Distance(playerMove.transform.position, targetTransform.position) > stopDistance)
        {
            Vector3 currentPlayerPos = playerMove.transform.position;
            float playerMoved = Vector3.Distance(lastPlayerPos, currentPlayerPos);
            playerDistanceAccumulator += playerMoved;

            while (playerDistanceAccumulator >= 0.1f)
            {
                playerDistanceAccumulator -= 0.1f;
                if (_player != null && _player.TryGetComponent(out BasePsionicEnergy psiEnergy))
                    psiEnergy.AddAndResetDecay(basePsi);
            }

            lastPlayerPos = currentPlayerPos;
            yield return null;
        }

        RpcHandleJumpAnimEnd();

        if (targetDamageable is NetworkBehaviour net)
            RpcCheliceraeStrikeByNetId(net.netId, additionalDamage);
    }

    [ClientRpc]
    private void RpcHandleJumpAnimEnd()
    {
        HandleJumpAnimEnd();
    }

    [ClientRpc]
    private void RpcCheliceraeStrike(Character target, float additionalDamage)
    {
        _cheliceraeStrike.ChanceCritDamageEvolutionFour = isJumpWithCheliceraChanceDamageCrit ? 0.3f : 0.15f;
        _cheliceraeStrike.SetAdditionalDamage(additionalDamage);
        _cheliceraeStrike.SetTarget(GetTargetCharacter());
        _cheliceraeStrike.CheliceraStrikeCast();
        _cheliceraeStrike.ClearDataCheliceraStrike();
    }

    [ClientRpc]
    private void RpcCheliceraeStrikeByNetId(uint netId, float additionalDamage)
    {
        if (!NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity identity)) return;

        if (identity.TryGetComponent(out IDamageable target))
        {
            _cheliceraeStrike.ChanceCritDamageEvolutionFour = isJumpWithCheliceraChanceDamageCrit ? 0.3f : 0.15f;
            _cheliceraeStrike.SetAdditionalDamage(additionalDamage);
            _cheliceraeStrike.SetTarget(GetTargetCharacter());
            _cheliceraeStrike.CheliceraStrikeCast();
            _cheliceraeStrike.ClearDataCheliceraStrike();
        }
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
    }

    public void JumpWithCheliceraCast() => AnimStartCastCoroutine();
    public void JumpWithCheliceraEnd()
    {
        HandleJumpWithCheliceraEnd();
        ClearData();
        AnimCastEnded();
    }

    public void ApplyRootTrue()
    {
        IncreaseSetCooldown(CooldownTime);
        JumpWithCheliceraAnimationMove();
        _animator.applyRootMotion = true;
    }

    public void JumpEndSpeedAnim()
    {
        float timeDelay = distanceJump / 10;
        _player.Animator.SetFloat("JumpEndSpeed", 1f / timeDelay);
    }

    private bool CheckCanCast()
    {
        if (GetTargetCharacter() == null) return false;
        return Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius && NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle);
    }
}
