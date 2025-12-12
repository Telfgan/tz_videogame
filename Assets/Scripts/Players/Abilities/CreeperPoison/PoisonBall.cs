using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public struct PoisonBallInfo : NetworkMessage
{
    public int CountProjectiles;
    public int MaxCountProjectile;

    public float TimeBetweenAttack;
    public float StartTimeBetweenAttack;

    public bool IsProjectileCreate;
    public bool IsActiveTimer;
    public bool IsThreeProjectileOnOnetarget;
    public bool IsCanApplyInvisible;

    public bool IsOriginalTargetEnemy;
    public bool IsOriginalTargetAllies;
    public bool IsOriginalTargetPlayer;

    public bool IsHealingPoisonCloud;
}

public struct PoisonBallActiveTalentsInfo : NetworkMessage
{
    public bool IsActiveFootInstincts;
    public bool IsActiveRestorationOfGlands;
    public bool IsActiveTransparentPoisons;
    public bool IsActiveWitheringPoison;
    public bool IsActiveContinuationAmbush;
    public bool IsActiveHealingPoisonBall;
    public bool IsActiveHealingPoisonCloud;
    public bool IsActiveEnlargedGlands;
    public bool IsActiveVoluminousBall;
    public bool IsActiveInertialGlands;
    public bool IsActiveVolatilityOfPoisons;
    public bool IsActiveBallEffect;
}

public struct PoisonBallSpawnPointInfo : NetworkMessage
{
    public float SpawnPointX;
    public float SpawnPointY;
    public float SpawnPointZ;
}

public class PoisonBall : Skill, IAltAbility
{
    #region Variables

    [Header("Talents")]
    [SerializeField] private RestorationOfGlands _restorationOfGlands;
    [SerializeField] private TransparentPoisons _transparentPoisons;
    [SerializeField] private FootInstincts _footInstincts;
    [SerializeField] private HealingPoisonBall _healingPoisonBall;
    [SerializeField] private HealingPoisonCloud _healPoisonCloud;
    [SerializeField] private WitheringPoison _witheringPoison;
    [SerializeField] private EnlargedGlands _enlargedGlands;
    [SerializeField] private ContinuationAmbush _continuationAmbush;
    [SerializeField] private VoluminousBall _voluminousBall;
    [SerializeField] private InertialGlands _inertialGlands;
    [SerializeField] private AssasinPoison _assasinPoison;
    [SerializeField] private FlowOfPoisons _flowOfPoison;
    [SerializeField] private VolatilityOfPoisons _volatilityOfPoisons;
    [SerializeField] private BallEffect _ballEffect;

    [Header("Ability properties")]
    [SerializeField] private SpitPoison _spitPoison;
    [SerializeField] private PoisonBallProjectile _projectile;
    [SerializeField] private Character _player;
    [SerializeField] private ArrowRender _arrowPrefab;
    [SerializeField] private GameObject _spawnPoint;
    [SerializeField] private GameObject pointArrowRender;

    #region PoisonCloud
    [SerializeField] private PoisonDamagingCloudPrefab _poisonDamagingCloudPrefab;
    [SerializeField] private PoisonHealingCloudPrefab _poisonHealingCloudPrefab;
    private PoisonDamagingCloudPrefab _poisonDamagingCloud;
    private PoisonHealingCloudPrefab _poisonHealingCloud;
    private float _durationPoisonCloud = 6f;
    #endregion

    private PoisonBallInfo _poisonBallInfo = new PoisonBallInfo();
    private PoisonBallSpawnPointInfo _spawnPointInfo = new PoisonBallSpawnPointInfo();
    private PoisonBallActiveTalentsInfo _activeTalentsInfo = new PoisonBallActiveTalentsInfo();

    private ArrowRender[] _arrowRenderers = new ArrowRender[4];
    //private Character _currentTarget;
    private GameObject _pointArrowInstance;

    private Vector3 _firstMousePosition = Vector3.positiveInfinity;
    private Vector3 _secondMousePosition;
    private Vector3 _thirdMousePosition;

    private int _poisonBoneStacks = 0;

    private float _fastTimeCast = 0.4f;
    private float _slowTimeCast = 1.8f;
    private float _originalChargeCooldown;
    private float _baseCastWidth;
    private float _multiplierForPushDistance;
    private float _animTime;
    private float _baseMultiplierAnimationSpeed = 1f;

    #region BoolVariables

    private bool _isCanCheckActiveTalents = true;
    private bool _isCanSetSpawnPoint = true;
    private bool _isCanCheckTimerActive = true;
    private bool _isCanApplyInvisible;

    private bool _firstClickDone;
    private bool _secondClickDone;
    private bool _thirdClickDone;

    private bool _isTarget;
    private bool _isPushTarget;

    private bool _isFast;
    private bool _isBallCanBigger;
    private bool _isThreeProjectileOnOneTarget;

    private bool _isAbilityActive;

    #endregion

    private Coroutine _secondClickCoroutine;
    private Coroutine _thirdClickCoroutine;
    private Coroutine _mouseDetectionCoroutine;
    private Coroutine _checkingTalentsCoroutine;
    private Coroutine _setSpawnPointCoroutine;
    private Coroutine _checkTimerActiveCoroutine;

    public GameObject LastTarget { get; set; }
    public GameObject CurrentTarget { get; set; }
    public FootInstincts FootInstinctsTalent { get; set; }
    public RestorationOfGlands RestorationOfGlandsTalent { get; set; }
    public int CurrentCountBall { get => _poisonBallInfo.CountProjectiles; }
    public int PoisonBoneStack { get => _poisonBoneStacks; set => _poisonBoneStacks = value; }
    public bool IsAltAbility { get; set; }

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => Animator.StringToHash("PoisonBallCastDelayAnimTrigger");
    protected override bool IsCanCast => CheckCanCast();

    public event Action ResetAbilityParameters;
    public event Action AbilityChange;

    #endregion

    public void PayCostPoisonBall()
    {
        TryPayCost(true);
    }

    private void Start()
    {
        _baseCastWidth = _castWidth;
        _originalChargeCooldown = _chargeCooldown;

        _poisonBallInfo.StartTimeBetweenAttack = 15.0f;
        _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
        _poisonBallInfo.MaxCountProjectile = _maxCharges;
    }

    private float GetAnimationClipLength()
    {
        RuntimeAnimatorController animController = _player.Animator.runtimeAnimatorController;
        foreach (var clip in animController.animationClips)
        {
            if (clip.name == "PoisonBallCastAnimation")
            {
                return clip.length;
            }
        }
        return -1f;
    }

    #region PrepareAndStartJob

    protected override void ClearData()
    {
        if (_animTime > 0)
            _player.Animator.SetFloat("PoisonBallMultiplierSpeedAnimation", _baseMultiplierAnimationSpeed);

        if (_isAbilityActive)
        {
            float timerForCancelCoroutine = 0.2f;
            Invoke("CancelCoroutine", timerForCancelCoroutine);
        }

        ClearArrows();

        ClearTarget();
        //_currentTarget = null;

        _firstMousePosition = Vector3.positiveInfinity;
        _secondMousePosition = Vector3.zero;
        _thirdMousePosition = Vector3.zero;

        _isTarget = false;
        _secondClickDone = false;
        _thirdClickDone = false;
        _isAbilityActive = false;
        _firstClickDone = false;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        _isAbilityActive = true;

        StartCoroutine();

        CheckingActiveTalents();

        while (GetTargetCharacter() == null && float.IsPositiveInfinity(_firstMousePosition.x))
        {
            if (GetMouseButton)
            {
                FindTargetCharacter(true);
               // _currentTarget = GetTarget(true).character;

                CheckWhoTarget();
                _firstMousePosition = GetMousePoint();

                if (GetTargetCharacter() != null)
                {
                    //_player.Move.LookAtTransform(_currentTarget.transform);
                    _isTarget = true;
                }
                else
                {
                    _isTarget = false;
                }

                if (_arrowRenderers[0] == null)
                {
                    CreateArrowsParallelToPlayer();
                }

                _arrowRenderers[0]?.gameObject.SetActive(true);
                _arrowRenderers[1]?.gameObject.SetActive(true);
                _arrowRenderers[2]?.gameObject.SetActive(false);
                _arrowRenderers[3]?.gameObject.SetActive(false);

                _firstClickDone = true;
            }

            CooldownChange();
            yield return null;
        }

        _animTime = GetAnimationClipLength();

        yield return _secondClickCoroutine = StartCoroutine(SecondClick());

        yield return _thirdClickCoroutine = StartCoroutine(ThirdClick());

        UseAbility();

        // страшна, очень страшна
    }

    protected override IEnumerator CastJob()
    {
        ChooseWhichProjectileCreate();

        ClearArrows();

        ResetAbilityParameters?.Invoke();

        _player.Move.StopLookAt();

        yield return null;
    }

    private void UseAbility()
    {
        if (_secondClickDone && _thirdClickDone)
        {
            ChooseMovementDependingOnCountProjectiles();
        }
    }

    private void StartCoroutine()
    {
        if (_setSpawnPointCoroutine == null)
        {
            _setSpawnPointCoroutine = StartCoroutine(SetSpawnPointJob());
        }

        if (_mouseDetectionCoroutine == null)
        {
            _mouseDetectionCoroutine = StartCoroutine(UpdateMouseDetectionJob());
        }

        if (_checkingTalentsCoroutine == null)
        {
            _checkingTalentsCoroutine = StartCoroutine(CheckingActiveTalentsJob());
        }

        if (_checkTimerActiveCoroutine == null)
        {
            _checkTimerActiveCoroutine = StartCoroutine(CheckTimerActiveJob());
        }
    }

    private void CancelCoroutine()
    {
        if (_setSpawnPointCoroutine != null)
        {
            StopCoroutine(_setSpawnPointCoroutine);
            _setSpawnPointCoroutine = null;
        }

        if (_mouseDetectionCoroutine != null)
        {
            StopCoroutine(_mouseDetectionCoroutine);
            _mouseDetectionCoroutine = null;
        }

        if (_secondClickCoroutine != null)
        {
            StopCoroutine(_secondClickCoroutine);
            _secondClickCoroutine = null;
        }

        if (_thirdClickCoroutine != null)
        {
            StopCoroutine(_thirdClickCoroutine);
            _thirdClickCoroutine = null;
        }
    }

    private IEnumerator CheckTimerActiveJob()
    {
        while (_isCanCheckTimerActive)
        {
            if (_poisonBallInfo.IsActiveTimer)
            {
                Timer();
            }

            yield return null;
        }
    }

    private IEnumerator SetSpawnPointJob()
    {
        while (_isCanSetSpawnPoint)
        {
            SetSpawnPointPosition(_spawnPoint.transform.position.x, _spawnPoint.transform.position.y, _spawnPoint.transform.position.z);

            yield return null;
        }
    }

    #endregion

    #region CheckingMethods

    private void ContinuationAmbushApplyInvisible()
    {
        if (_activeTalentsInfo.IsActiveContinuationAmbush && _isCanApplyInvisible)
        {
            _continuationAmbush.CanApplyInvisible(true);
        }
    }

    private void InertialGlandsReductionCooldown()
    {
        if (_activeTalentsInfo.IsActiveInertialGlands && _isThreeProjectileOnOneTarget)
        {
            float newRemainingTime = 0.0f;
            _spitPoison.ReductionSetCooldown(newRemainingTime);
            _isThreeProjectileOnOneTarget = false;
        }
    }

    private IEnumerator CheckingActiveTalentsJob()
    {
        while (_isCanCheckActiveTalents)
        {
            InertialGlandsReductionCooldown();
            ContinuationAmbushApplyInvisible();

            yield return null;
        }
    }

    private void CheckingActiveTalents()
    {
        _activeTalentsInfo.IsActiveFootInstincts = _footInstincts.Data.IsOpen;
        _activeTalentsInfo.IsActiveRestorationOfGlands = _restorationOfGlands.Data.IsOpen;
        _activeTalentsInfo.IsActiveTransparentPoisons = _transparentPoisons.Data.IsOpen;
        _activeTalentsInfo.IsActiveWitheringPoison = _witheringPoison.Data.IsOpen;
        _activeTalentsInfo.IsActiveContinuationAmbush = _continuationAmbush.Data.IsOpen;
        _activeTalentsInfo.IsActiveHealingPoisonBall = _healingPoisonBall.Data.IsOpen;
        _activeTalentsInfo.IsActiveHealingPoisonCloud = _healPoisonCloud.Data.IsOpen;
        _activeTalentsInfo.IsActiveEnlargedGlands = _enlargedGlands.Data.IsOpen;
        _activeTalentsInfo.IsActiveVoluminousBall = _voluminousBall.Data.IsOpen;
        _activeTalentsInfo.IsActiveInertialGlands = _inertialGlands.Data.IsOpen;
        _activeTalentsInfo.IsActiveVolatilityOfPoisons = _volatilityOfPoisons.Data.IsOpen;
        _activeTalentsInfo.IsActiveBallEffect = _ballEffect.Data.IsOpen;

        #region VolatilityOfPoisonsTalentIsActive

        if (_activeTalentsInfo.IsActiveVolatilityOfPoisons && _poisonBoneStacks > 0)
        {
            float multiplier = _poisonBoneStacks * 0.1f;
            _multiplierForPushDistance = multiplier;
        }
        else
        {
            _multiplierForPushDistance = 0;
        }

        #endregion

        #region VoluminousBallTalentIsActive

        if (_activeTalentsInfo.IsActiveVoluminousBall && !_isBallCanBigger)
        {
            float multiplier = _baseCastWidth * 0.2f;
            _castWidth += multiplier;
            _isBallCanBigger = true;
        }
        else if (!_activeTalentsInfo.IsActiveVoluminousBall && _isBallCanBigger)
        {
            _castWidth = _baseCastWidth;
            _isBallCanBigger = false;
        }

        #endregion

        #region EnlargedGlandTalentIsActive

        if (_activeTalentsInfo.IsActiveEnlargedGlands && _maxCharges == 3)
        {
            AddMaxChargeCount();
            _poisonBallInfo.MaxCountProjectile = _maxCharges;
        }
        else if (!_activeTalentsInfo.IsActiveEnlargedGlands && _maxCharges >= 4)
        {
            DeductMaxChargeCount();
            _poisonBallInfo.MaxCountProjectile = _maxCharges;
        }

        #endregion
    }

    private void CheckWhoTarget()
    {
        if (GetTargetCharacter() != null)
        {
            if (GetTargetCharacter().gameObject == _player.gameObject)
            {
                _poisonBallInfo.IsOriginalTargetPlayer = true;
                _poisonBallInfo.IsOriginalTargetAllies = false;
                _poisonBallInfo.IsOriginalTargetEnemy = false;

                if (_activeTalentsInfo.IsActiveHealingPoisonCloud)
                {
                    _poisonBallInfo.IsHealingPoisonCloud = true;
                }
                else
                {
                    _poisonBallInfo.IsHealingPoisonCloud = false;
                }
            }
            else if (GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Allies"))
            {
                _poisonBallInfo.IsOriginalTargetPlayer = false;
                _poisonBallInfo.IsOriginalTargetAllies = true;
                _poisonBallInfo.IsOriginalTargetEnemy = false;

                if (_activeTalentsInfo.IsActiveHealingPoisonCloud)
                {
                    _poisonBallInfo.IsHealingPoisonCloud = true;
                }
                else
                {
                    _poisonBallInfo.IsHealingPoisonCloud = false;
                }
            }
            else if (GetTargetCharacter().gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                _poisonBallInfo.IsOriginalTargetPlayer = false;
                _poisonBallInfo.IsOriginalTargetAllies = false;
                _poisonBallInfo.IsOriginalTargetEnemy = true;

                if (_activeTalentsInfo.IsActiveHealingPoisonCloud)
                {
                    _poisonBallInfo.IsHealingPoisonCloud = false;
                }
            }
        }
        else
        {
            _poisonBallInfo.IsOriginalTargetPlayer = false;
            _poisonBallInfo.IsOriginalTargetAllies = false;
            _poisonBallInfo.IsOriginalTargetEnemy = false;
        }
    }

    private void Timer()
    {
        _poisonBallInfo.TimeBetweenAttack -= Time.deltaTime;

        if (_poisonBallInfo.TimeBetweenAttack < 0)
        {
            _poisonBallInfo.CountProjectiles = 0;
            _poisonBallInfo.IsActiveTimer = false;
            _poisonBallInfo.IsProjectileCreate = false;
            _poisonBallInfo.IsThreeProjectileOnOnetarget = false;
            _poisonBallInfo.IsCanApplyInvisible = false;
        }
    }

    private void CooldownChange()
    {
        if (_activeTalentsInfo.IsActiveHealingPoisonBall && (_poisonBallInfo.IsOriginalTargetAllies || _poisonBallInfo.IsOriginalTargetPlayer))
        {
            _chargeCooldown = _originalChargeCooldown / 2;
        }
        else
        {
            _chargeCooldown = _originalChargeCooldown;
        }
    }

    private bool CheckCanCast()
    {
        //Debug.Log("CheckCanCast PoisonBall");

        if (GetTargetCharacter() == null)
            return Vector3.Distance(_firstMousePosition, transform.position) <= Radius && NoObstacles(_firstMousePosition, _obstacle);

        return Vector3.Distance(_firstMousePosition, transform.position) <= Radius &&
            NoObstacles(_firstMousePosition, _obstacle) ||
            Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius &&
            NoObstacles(GetTargetCharacter().transform.position, _obstacle);

    }

    #endregion

    #region ChooseMoveSpeedProjectile

    private void ChooseMovementDependingOnCountProjectiles()
    {
        ChooseSpeed();
        ChooseDirectionPush();
        StartCoroutine(_isFast ? TimeCastForFastMoveProjectile() : TimeCastForSlowMoveProjectile());
    }

    private void ChooseSpeed()
    {
        if (_isTarget && GetTargetCharacter().gameObject != _player.gameObject)
        {
            _isFast = Vector3.Distance(_player.transform.position, _secondMousePosition) > Vector3.Distance(_player.transform.position, GetTargetCharacter().transform.position);
        }
        else
        {
            _isFast = Vector3.Distance(_player.transform.position, _secondMousePosition) > Vector3.Distance(_player.transform.position, _firstMousePosition);
        }
    }

    private void ChooseDirectionPush()
    {
        _isPushTarget = Vector3.Distance(_player.transform.position, _thirdMousePosition) > Vector3.Distance(_player.transform.position, _secondMousePosition);
    }

    private IEnumerator TimeCastForFastMoveProjectile()
    {
        _castDeley = _slowTimeCast;

        if (_animTime > 0)
        {
            float multiplierAnimTime = 0.8f;
            float animTimeMultiplier = _animTime / _castDeley - multiplierAnimTime;

            _player.Animator.SetFloat("PoisonBallMultiplierSpeedAnimation", animTimeMultiplier);
        }

        yield return null;
    }

    private IEnumerator TimeCastForSlowMoveProjectile()
    {
        _castDeley = _fastTimeCast;

        if (_animTime > 0)
        {
            float multiplierAnimTime = 3.7f;
            float animTimeMultiplier = _animTime / _castDeley - multiplierAnimTime;

            _player.Animator.SetFloat("PoisonBallMultiplierSpeedAnimation", animTimeMultiplier);
        }

        yield return null;
    }

    private void ChooseWhichProjectileCreate()
    {
        if (_isTarget)
        {
            CmdCreateProjectileForTarget(GetTargetCharacter().gameObject, GetTargetCharacter().transform.position,
                _poisonBallInfo.MaxCountProjectile, _multiplierForPushDistance, PoisonBoneStack,
                _isFast, _isPushTarget, IsAltAbility,
                _activeTalentsInfo.IsActiveFootInstincts, _activeTalentsInfo.IsActiveRestorationOfGlands,
                _activeTalentsInfo.IsActiveHealingPoisonBall, _activeTalentsInfo.IsActiveWitheringPoison, _activeTalentsInfo.IsActiveVoluminousBall, _activeTalentsInfo.IsActiveBallEffect,
                _activeTalentsInfo.IsActiveInertialGlands, _activeTalentsInfo.IsActiveContinuationAmbush,
                _poisonBallInfo.IsOriginalTargetEnemy, _poisonBallInfo.IsOriginalTargetPlayer, _poisonBallInfo.IsOriginalTargetAllies);

            CmdApplyPoisonCloud(_poisonBallInfo.IsHealingPoisonCloud, _durationPoisonCloud);
        }
        else
        {
            CmdCreateProjectileForFlyingMaxDistance(_firstMousePosition,
                _poisonBallInfo.MaxCountProjectile, _multiplierForPushDistance, PoisonBoneStack,
                _isFast, _isPushTarget, IsAltAbility,
                _activeTalentsInfo.IsActiveFootInstincts, _activeTalentsInfo.IsActiveRestorationOfGlands,
                _activeTalentsInfo.IsActiveHealingPoisonBall, _activeTalentsInfo.IsActiveWitheringPoison, _activeTalentsInfo.IsActiveVoluminousBall, _activeTalentsInfo.IsActiveBallEffect,
                _activeTalentsInfo.IsActiveInertialGlands, _activeTalentsInfo.IsActiveContinuationAmbush,
                _poisonBallInfo.IsOriginalTargetEnemy, _poisonBallInfo.IsOriginalTargetPlayer, _poisonBallInfo.IsOriginalTargetAllies);

            CmdApplyPoisonCloud(_poisonBallInfo.IsHealingPoisonCloud, _durationPoisonCloud);
        }
    }

    #endregion

    #region ArrowManagement

    private void CreateArrowsParallelToPlayer()
    {
        if (_arrowPrefab == null || pointArrowRender == null) return;
        Quaternion rotation = Quaternion.identity;
        Vector3 center = GetTargetCharacter() != null ? GetTargetCharacter().transform.position : _firstMousePosition;
        center.y = 0.8f;

        _pointArrowInstance = Instantiate(pointArrowRender, center, Quaternion.identity);

        Vector3 playerPosition = _player.transform.position;
        playerPosition.y = 0.8f;

        Vector3 directionPoint = _player.transform.position - _pointArrowInstance.transform.position;
        directionPoint.y = 0f;
        if (directionPoint != Vector3.zero) _pointArrowInstance.transform.rotation = Quaternion.LookRotation(directionPoint);

        Vector3 directionToTarget = (center - playerPosition).normalized;

        Vector3[] spawnOffsets = new Vector3[4]
        {
        directionToTarget,
        -directionToTarget,
        directionToTarget * 1.5f,
        -directionToTarget * 1.5f
        };

        for (int i = 0; i < _arrowRenderers.Length; i++)
        {
            Vector3 spawnPos = center + spawnOffsets[i];

            if (i % 2 != 0) rotation = Quaternion.LookRotation(spawnPos - playerPosition);
            else rotation = Quaternion.LookRotation(playerPosition - spawnPos);

            _arrowRenderers[i] = Instantiate(_arrowPrefab, spawnPos, rotation, _pointArrowInstance.transform);
            RotateArrowChild(_arrowRenderers[i].gameObject, -90);
            _arrowRenderers[i].gameObject.SetActive(false);
        }
    }

    private void RotateArrowChild(GameObject arrow, float zRotation)
    {
        if (arrow == null) return;

        Transform childArrow = arrow.transform.GetChild(0);
        float currentXRotation = childArrow.localEulerAngles.x;

        childArrow.localRotation = Quaternion.Euler(currentXRotation, 0, zRotation);
    }


    private void SetArrowVisibility(int arrowIndex, bool isVisible)
    {
        if (arrowIndex >= 0 && arrowIndex < _arrowRenderers.Length && _arrowRenderers[arrowIndex] != null)
        {
            _arrowRenderers[arrowIndex].gameObject.SetActive(isVisible);
        }
    }

    private void ClearArrows()
    {
        foreach (var arrow in _arrowRenderers)
        {
            if (arrow != null)
            {
                Destroy(arrow.gameObject);
            }

            if (_pointArrowInstance != null)
            {
                Destroy(_pointArrowInstance);
                _pointArrowInstance = null;
            }
        }
        Debug.Log("Arrows cleared.");
    }

    #endregion

    #region MouseClick

    private IEnumerator SecondClick()
    {
        while (!_secondClickDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _secondClickDone = true;

                _arrowRenderers[0].SetTransparentMaterial();
                _arrowRenderers[1].SetTransparentMaterial();

                _secondMousePosition = GetMousePoint();

                if (GetTargetCharacter() != null)
                {
                    Vector3 currentMousePosition = GetMousePoint();
                    if (currentMousePosition.x < _secondMousePosition.x && currentMousePosition.z < _secondMousePosition.z)
                    {
                        SetArrowVisibility(1, true);
                        SetArrowVisibility(3, false);
                    }
                    else
                    {
                        SetArrowVisibility(3, true);
                        SetArrowVisibility(1, false);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator ThirdClick()
    {
        while (!_thirdClickDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _arrowRenderers[0].SetDeafaultMaterail();
                _arrowRenderers[1].SetDeafaultMaterail();

                _thirdClickDone = true;
                _thirdMousePosition = GetMousePoint();
            }
            yield return null;
        }
    }

    #endregion

    #region Update Method for Mouse Movement Detection

    private IEnumerator UpdateMouseDetectionJob()
    {
        while (_thirdClickDone == false)
        {
            if (_pointArrowInstance != null)
            {
                Vector3 dir = _player.transform.position - _pointArrowInstance.transform.position;
                dir.y = 0f;
                if (dir != Vector3.zero) _pointArrowInstance.transform.rotation = Quaternion.LookRotation(dir);
            }

            Vector3 currentMousePosition = GetMousePoint();

            if (_firstClickDone && !_secondClickDone)
            {
                UpdateArrowHighlight(0, 1, currentMousePosition);
            }

            if (_secondClickDone)
            {
                UpdateArrowHighlight(2, 3, currentMousePosition);
            }

            yield return null;
        }
    }

    private void UpdateArrowHighlight(int index1, int index2, Vector3 currentMousePosition)
    {
        Vector3 arrowPosition = _arrowRenderers[index1].transform.position;

        Vector3 direction = (arrowPosition - currentMousePosition).normalized;

        bool isHorizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.z);

        if (isHorizontal)
        {
            if (Input.GetAxis("Mouse X") > 0)
            {
                SetArrowVisibility(index1, true);
                SetArrowVisibility(index2, false);
            }
            else if (Input.GetAxis("Mouse X") < 0)
            {
                SetArrowVisibility(index2, true);
                SetArrowVisibility(index1, false);
            }
        }
        else
        {
            if (Input.GetAxis("Mouse Y") > 0)
            {
                SetArrowVisibility(index1, true);
                SetArrowVisibility(index2, false);
            }
            else if (Input.GetAxis("Mouse Y") < 0)
            {
                SetArrowVisibility(index2, true);
                SetArrowVisibility(index1, false);
            }
        }
    }

    #endregion

    #region Command Methods

    [Command]
    private void CmdCreateProjectileForTarget(GameObject target, Vector3 targetPosition,
        int maxCountProjectiles, float multiplierForPushDistance, int poisonBoneStack,
        bool isFast, bool isPushTarget, bool isPlayerInvisible,
        bool isActiveFootInstincts, bool isActiveRestorationOfGlands,
        bool isActiveHealingPoisonBall, bool isActiveWitheringPoison, bool isActiveVoluminousBall, bool isActiveBallEffect,
        bool isActiveInertialGlands, bool isActiveContinuationAmbush,
        bool isTargetEnemy, bool isTargetPlayer, bool isTargetAllies)

    {

        CurrentTarget = target;
        FootInstinctsTalent = _footInstincts;
        RestorationOfGlandsTalent = _restorationOfGlands;

        if (LastTarget == CurrentTarget)
        {
            _poisonBallInfo.CountProjectiles += 1;
            _poisonBallInfo.IsProjectileCreate = true;
        }
        else
        {
            _poisonBallInfo.IsActiveTimer = false;
            _poisonBallInfo.IsThreeProjectileOnOnetarget = false;
            _poisonBallInfo.IsCanApplyInvisible = false;
            _poisonBallInfo.CountProjectiles = 1;
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
        }

        if (_poisonBallInfo.CountProjectiles >= 3 && isActiveInertialGlands)
        {
            _poisonBallInfo.IsThreeProjectileOnOnetarget = true;
            RpcIsThreeProjectileOnOneTarget(_poisonBallInfo.IsThreeProjectileOnOnetarget);
        }

        if (_poisonBallInfo.CountProjectiles >= 4 && isActiveContinuationAmbush)
        {
            _poisonBallInfo.IsCanApplyInvisible = true;
            RpcIsCanApplyInvisible(_poisonBallInfo.IsCanApplyInvisible);
        }

        if (_poisonBallInfo.CountProjectiles < maxCountProjectiles && LastTarget == CurrentTarget)
        {
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
            _poisonBallInfo.IsActiveTimer = true;
        }

        Vector3 spawnPosition = new Vector3(_spawnPointInfo.SpawnPointX, _spawnPointInfo.SpawnPointY, _spawnPointInfo.SpawnPointZ);

        GameObject item = Instantiate(_projectile.gameObject, spawnPosition, Quaternion.identity);
        PoisonBallProjectile poisonBallProjectile = item.GetComponent<PoisonBallProjectile>();

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        poisonBallProjectile.InitializationProjectileForPoisonBall(_player, this,
            multiplierForPushDistance, poisonBoneStack,
            isTargetPlayer, isTargetEnemy, isTargetAllies,
            isActiveFootInstincts, isActiveRestorationOfGlands,
            isActiveHealingPoisonBall, isActiveWitheringPoison, isActiveVoluminousBall, isActiveBallEffect,
            isPushTarget, isPlayerInvisible
            );

        poisonBallProjectile.MoveBallToTarget(targetPosition, isFast);

        NetworkServer.Spawn(item);

        if (_poisonBallInfo.CountProjectiles > maxCountProjectiles)
        {
            _poisonBallInfo.IsActiveTimer = false;

            _poisonBallInfo.CountProjectiles = 1;
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
            _poisonBallInfo.IsThreeProjectileOnOnetarget = false;
            _poisonBallInfo.IsCanApplyInvisible = false;
        }
    }

    [Command]
    private void CmdCreateProjectileForFlyingMaxDistance(Vector3 point,
        int maxCountProjectiles, float multiplierForPushDistance, int poisonBoneStack,
        bool isFast, bool isPushTarget, bool isPlayerInvisible,
        bool isActiveFootInstincts, bool isActiveRestorationOfGlands,
        bool isActiveHealingPoisonBall, bool isActiveWitheringPoison, bool isActiveVoluminousBall, bool isActiveBallEffect,
        bool isActiveInertialGlands, bool isActiveContinuationAmbush,
        bool isTargetEnemy, bool isTargetPlayer, bool isTargetAllies)
    {
        _player.Health.Add(-100f);

        RestorationOfGlandsTalent = _restorationOfGlands;
        FootInstinctsTalent = _footInstincts;
        CurrentTarget = LastTarget;

        if (LastTarget == CurrentTarget)
        {
            _poisonBallInfo.CountProjectiles += 1;
            _poisonBallInfo.IsProjectileCreate = true;
        }
        else
        {
            _poisonBallInfo.IsActiveTimer = false;
            _poisonBallInfo.IsThreeProjectileOnOnetarget = false;
            _poisonBallInfo.IsCanApplyInvisible = false;
            _poisonBallInfo.CountProjectiles = 1;
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
        }

        if (_poisonBallInfo.CountProjectiles >= 3 && isActiveInertialGlands)
        {
            _poisonBallInfo.IsThreeProjectileOnOnetarget = true;
            RpcIsThreeProjectileOnOneTarget(_poisonBallInfo.IsThreeProjectileOnOnetarget);
        }

        if (_poisonBallInfo.CountProjectiles >= 4 && isActiveContinuationAmbush)
        {
            _poisonBallInfo.IsCanApplyInvisible = true;
            RpcIsCanApplyInvisible(_poisonBallInfo.IsCanApplyInvisible);
        }

        if (_poisonBallInfo.CountProjectiles < maxCountProjectiles && LastTarget == CurrentTarget)
        {
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
            _poisonBallInfo.IsActiveTimer = true;
        }

        Vector3 spawnPosition = new Vector3(_spawnPointInfo.SpawnPointX, _spawnPointInfo.SpawnPointY, _spawnPointInfo.SpawnPointZ);

        GameObject item = Instantiate(_projectile.gameObject, spawnPosition, Quaternion.identity);
        PoisonBallProjectile poisonBallProjectile = item.GetComponent<PoisonBallProjectile>();

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        poisonBallProjectile.InitializationProjectileForPoisonBall(_player, this,
            multiplierForPushDistance, poisonBoneStack,
            isTargetPlayer, isTargetEnemy, isTargetAllies,
            isActiveFootInstincts, isActiveRestorationOfGlands,
            isActiveHealingPoisonBall, isActiveWitheringPoison, isActiveVoluminousBall, isActiveBallEffect,
            isPushTarget, isPlayerInvisible
            );

        poisonBallProjectile.MoveBallOnMaxDistance(point, isFast);

        NetworkServer.Spawn(item);

        if (_poisonBallInfo.CountProjectiles >= maxCountProjectiles)
        {
            _poisonBallInfo.IsActiveTimer = false;

            _poisonBallInfo.CountProjectiles = 1;
            _poisonBallInfo.TimeBetweenAttack = _poisonBallInfo.StartTimeBetweenAttack;
            _poisonBallInfo.IsThreeProjectileOnOnetarget = false;
            _poisonBallInfo.IsCanApplyInvisible = false;
        }
    }

    [Command]
    private void CmdApplyPoisonCloud(bool isHealingCloud, float duration)
    {
        if (!isHealingCloud)
        {
            if (_poisonDamagingCloud == null && _poisonDamagingCloudPrefab.PoisonDamageCloud == null)
            {
                _player.CharacterState.AddState(States.PoisonCloud, duration, 0, _player.gameObject, Name);

                _poisonDamagingCloud = Instantiate(_poisonDamagingCloudPrefab, _player.transform.position, Quaternion.identity);

                _poisonDamagingCloudPrefab.PoisonDamageCloud = _poisonDamagingCloud;
                SceneManager.MoveGameObjectToScene(_poisonDamagingCloudPrefab.PoisonDamageCloud.gameObject, _hero.NetworkSettings.MyRoom);

                _poisonDamagingCloudPrefab.PoisonDamageCloud.InitializationProjectile(_player, duration);
                _poisonDamagingCloudPrefab.PoisonDamageCloud.AddStack();

                NetworkServer.Spawn(_poisonDamagingCloud.gameObject);

                //Debug.Log("PoisonBall / CmdApplyPoisonCloud / if / _poisonDamagingCloud = " + _poisonDamagingCloud);
                //Debug.Log("PoisonBall / CmdApplyPoisonCloud / if / _poisonDamagingCloudPrefab.PoisonDamageCloud = " + _poisonDamagingCloudPrefab.PoisonDamageCloud);
            }
            else
            {
                //Debug.Log("PoisonBall / CmdApplyPoisonCloud / else / _poisonDamagingCloud = " + _poisonDamagingCloudPrefab.PoisonDamageCloud);
                _player.CharacterState.AddState(States.PoisonCloud, duration, 0, _player.gameObject, Name);
                _poisonDamagingCloudPrefab.PoisonDamageCloud.AddStack();
            }
        }
        else
        {
            if (_poisonHealingCloud == null && _poisonHealingCloudPrefab.PoisonHealingCloud == null)
            {
                _player.CharacterState.AddState(States.HealingPoisonCloud, duration, 0, _player.gameObject, Name);

                _poisonHealingCloud = Instantiate(_poisonHealingCloudPrefab, transform.position, Quaternion.identity);
                _poisonHealingCloudPrefab.PoisonHealingCloud = _poisonHealingCloud;
                SceneManager.MoveGameObjectToScene(_poisonHealingCloudPrefab.PoisonHealingCloud.gameObject, _hero.NetworkSettings.MyRoom);

                _poisonHealingCloudPrefab.PoisonHealingCloud.InitializationProjectile(_player, duration);
                _poisonHealingCloudPrefab.PoisonHealingCloud.AddStack();

                NetworkServer.Spawn(_poisonHealingCloud.gameObject);
            }
            else
            {
                _player.CharacterState.AddState(States.HealingPoisonCloud, duration, 0, _player.gameObject, Name);
                _poisonHealingCloudPrefab.PoisonHealingCloud.AddStack();
            }
        }
        RpcApply(_poisonDamagingCloudPrefab.PoisonDamageCloud, _poisonHealingCloudPrefab.PoisonHealingCloud, duration, isHealingCloud);
    }

    [Command]
    private void SetSpawnPointPosition(float spawnPointX, float spawnPointY, float spawnPointZ)
    {
        _spawnPointInfo.SpawnPointX = spawnPointX;
        _spawnPointInfo.SpawnPointY = spawnPointY;
        _spawnPointInfo.SpawnPointZ = spawnPointZ;
    }

    #endregion

    [ClientRpc]
    private void RpcApply(PoisonDamagingCloudPrefab poisonDamagingCloud, PoisonHealingCloudPrefab poisonHealingCloud, float duration, bool isHealingCloud)
    {
        //Debug.Log("PoisonBall / RpcApply / if (poisonDamagingCloud != null) = " + poisonDamagingCloud);
        if (poisonDamagingCloud != null)
        {
            poisonDamagingCloud.InitializationProjectile(_player, duration);
            poisonDamagingCloud.AddStack();
        }

        if (poisonHealingCloud != null && isHealingCloud)
        {
            poisonHealingCloud.InitializationProjectile(_player, duration);
            poisonHealingCloud.AddStack();
        }
    }

    [TargetRpc]
    private void RpcIsThreeProjectileOnOneTarget(bool isThreePorjectileOnOneTarget)
    {
        _isThreeProjectileOnOneTarget = isThreePorjectileOnOneTarget;
    }

    [TargetRpc]
    private void RpcIsCanApplyInvisible(bool isCanApplyInvisible)
    {
        _isCanApplyInvisible = isCanApplyInvisible;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }
}