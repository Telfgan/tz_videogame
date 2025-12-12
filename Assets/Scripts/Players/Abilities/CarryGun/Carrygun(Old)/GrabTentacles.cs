//using Mirror;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class GrabTentacles : Skill
//{
//    #region Variables
//    [SerializeField] private DrawCircle _circlePrefab;

//    [SerializeField] private Character _player;
//    [SerializeField] private BasePsionicEnergy _psionicEnergy;
//    [SerializeField] private AttackingPsionicEnergy _attackingPsionicEnergy;
//    [SerializeField] private GrabTentaclesPrefab _tentaclesPrefab;

//    private DrawCircle _circleOnTarget;

//    private Vector3 _firstTentaclesPoint = Vector3.positiveInfinity;
//    private Vector3 _secondTentaclesPoint;
//    private Vector3 _pointForSearchingTargets;

//    private Character _target;
//    private List<Character> _targets = new();

//    private int _playerLayer;
//    private float _delayCast = 1.2f;
//    private float _baseDamage;

//    private bool _isAttackingPsiEnergyActive = false;
//    private bool _isTargetChoose = false;
//    private bool _isTarget = false;
//    private bool _isFirstPointDone = false;
//    private bool _isSecondPointDone = false;
//    private bool _isFirstPointTarget = false;

//    private Coroutine _chooseFirstTentaclesPointCoroutine;
//    private Coroutine _chooseSecondTentaclesPointCoroutine;
//    private Coroutine _searchTargetsCoroutine;

//    protected override int AnimTriggerCast => 0;
//    protected override int AnimTriggerCastDelay => 0;
//    #endregion

//    #region PrepareAndCastJob
//    protected override bool IsCanCast => CheckCanCast();

//    public override void LoadTargetData(TargetInfo targetInfo)
//    {
//        throw new System.NotImplementedException();
//    }

//    protected override void ClearData()
//    {
//        _isAttackingPsiEnergyActive = false;
//        _isTargetChoose = false;
//        _isTarget = false;
//        _isFirstPointDone = false;
//        _isSecondPointDone = false;
//        _isFirstPointTarget = false;

//        _target = null;
//        _firstTentaclesPoint = Vector3.positiveInfinity;
//        _secondTentaclesPoint = Vector3.zero;
//        _pointForSearchingTargets = Vector3.zero;

//        if (_chooseFirstTentaclesPointCoroutine != null)
//        {
//            StopCoroutine(_chooseFirstTentaclesPointCoroutine);
//            _chooseFirstTentaclesPointCoroutine = null;
//        }
//        if (_chooseSecondTentaclesPointCoroutine != null)
//        {
//            StopCoroutine(_chooseSecondTentaclesPointCoroutine);
//            _chooseSecondTentaclesPointCoroutine = null;
//        }
//    }

//    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
//    {
//        _playerLayer = _player.gameObject.layer;

//        _castDeley = _delayCast;

//        while (_target == null && float.IsPositiveInfinity(_firstTentaclesPoint.x))
//        {
//            if (GetMouseButton)
//            {
//                yield return _chooseFirstTentaclesPointCoroutine = StartCoroutine(ChooseFirstTentaclesPointJob());

//                if (_isFirstPointDone)
//                {
//                    yield return _chooseSecondTentaclesPointCoroutine = StartCoroutine(ChooseSecondTentaclesPointJob());
//                }
//            }

//            yield return null;
//        }

//        throw new System.NotImplementedException();
//        // targetDataSavedCallback(Data) 
//    }

//    protected override IEnumerator CastJob()
//    {
//        if (_isTarget)
//        {
//            InstantiateTentacles();
//        }
//        else
//        {
//            TryCancel(true);
//        }
//        yield return null;
//    }

//    private bool CheckCanCast()
//    {
//        if (_target == null)
//        {
//            return Vector3.Distance(_firstTentaclesPoint, _player.transform.position) <= Radius && NoObstacles(_firstTentaclesPoint, _obstacle);
//        }
//        else if (_target != null)
//        {
//            return Vector3.Distance(_firstTentaclesPoint, _player.transform.position) <= Radius && NoObstacles(_firstTentaclesPoint, _obstacle) 
//                && Vector3.Distance(_secondTentaclesPoint, _target.transform.position) <= Radius && NoObstacles(_secondTentaclesPoint, _obstacle);
//        }
//        else
//        {
//            return Vector3.Distance(_firstTentaclesPoint, _player.transform.position) <= Radius && NoObstacles(_firstTentaclesPoint, _obstacle) 
//                && Vector3.Distance(_secondTentaclesPoint, _player.transform.position) <= Radius && NoObstacles(_firstTentaclesPoint, _obstacle);

//        }
//    }

//    private IEnumerator SearchTargetsJob()
//    {
//        while (!_isTargetChoose)
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                _pointForSearchingTargets = GetMousePoint();
//                _targets = GetCloserTargets(_pointForSearchingTargets, Area);
//                foreach (var target in _targets)
//                {
//                    if (target != null)
//                    {
//                        _target = target;
//                    }
//                    yield break;
//                }
//                _isTargetChoose = true;
//            }
//            yield return null;
//        }
//    }

//    private IEnumerator ChooseFirstTentaclesPointJob()
//    {
//        while (!_isFirstPointDone)
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                yield return _searchTargetsCoroutine = StartCoroutine(SearchTargetsJob());

//                if (_target != null)
//                {
//                    _skillRender.StopDrawRadius();

//                    _firstTentaclesPoint = _target.transform.position;
//                    _isTarget = true;
//                    _isFirstPointTarget = true;

//                    DrawCircleOnTarget(_target.transform, Radius);
//                }
//                else if (_target != null && _target.gameObject.layer == _playerLayer)
//                {
//                    TryCancel(true);
//                }
//                else
//                {
//                    _firstTentaclesPoint = _pointForSearchingTargets;
//                    _isFirstPointTarget = false;
//                }

//                _isFirstPointDone = true;
//                _isTargetChoose = false;

//                if (_searchTargetsCoroutine != null)
//                {
//                    StopCoroutine(_searchTargetsCoroutine);
//                    _searchTargetsCoroutine = null;
//                }
//            }
//            yield return null;
//        }
//    }

//    private IEnumerator ChooseSecondTentaclesPointJob()
//    {
//        while (!_isSecondPointDone)
//        {
//            if (Input.GetMouseButtonDown(0))
//            {
//                if (_target == null && !_isTarget && !_isFirstPointTarget)
//                {
//                    yield return _searchTargetsCoroutine = StartCoroutine(SearchTargetsJob());

//                    if (_target != null && _target.gameObject.layer == _playerLayer || _target == null)
//                    {
//                        TryCancel(true);
//                    }
//                    else
//                    {
//                        _secondTentaclesPoint = _target.transform.position;
//                        _isTarget = true;
//                    }
//                }
//                else if (_target != null && _target.gameObject.layer == _playerLayer)
//                {
//                }
//                else
//                {
//                    _secondTentaclesPoint = GetMousePoint();
//                }
//                _isSecondPointDone = true;
//            }
//            yield return null;
//        }

//        if (_circleOnTarget != null)
//        {
//            ClearCircleOnTarget();
//        }
//    }
//    #endregion

//    #region CircleOnTargetDraw
//    private void DrawCircleOnTarget(Transform targetTransform, float radius)
//    {
//        _circleOnTarget = Instantiate(_circlePrefab, targetTransform);
//        _circleOnTarget.Draw(radius);
//    }

//    private void ClearCircleOnTarget()
//    {
//        _circleOnTarget.Clear();
//        Destroy(_circleOnTarget);
//        _circleOnTarget = null;
//    }
//    #endregion

//    private void InstantiateTentacles()
//    {
//        _isAttackingPsiEnergyActive = _attackingPsionicEnergy.IsAttackingPsiEnergy;
//        //_baseDamage = _attackingPsionicEnergy.CurrentAttackingPsiEnergy;        
        
//        if (_isFirstPointTarget)
//        {
//            CmdInstantiateTentacles(_player.gameObject, _target.gameObject, _secondTentaclesPoint, _firstTentaclesPoint, _isAttackingPsiEnergyActive, _baseDamage);
//        }
//        else
//        {
//            CmdInstantiateTentacles(_player.gameObject, _target.gameObject, _firstTentaclesPoint, _secondTentaclesPoint, _isAttackingPsiEnergyActive, _baseDamage);
//        }
//    }
    
//    [Command]
//    private void CmdInstantiateTentacles(GameObject player, GameObject target, Vector3 pointInstantiate, Vector3 endPoint,
//        bool isAttackingPsiEnergyActive, float currentDamage)
//    {
//        Character targetCharacter = target.GetComponent<Character>();
//        ReducingHealingState reducingHealingState;
//        if (isAttackingPsiEnergyActive)
//        {
//            Debug.Log("GrabTentacles / baseDamage = " + currentDamage);

//            if (currentDamage > 10 && currentDamage < 20)
//            {
//                Debug.Log("GrabTentacles / if < 20");

//                targetCharacter.CharacterState.DispelStates(StateType.Magic, targetCharacter.NetworkSettings.TeamIndex, _player.NetworkSettings.TeamIndex, true);
//            }
//            else if (currentDamage > 20 && currentDamage < 30)
//            {
//                Debug.Log("GrabTentacles / else if > 20");

//                targetCharacter.CharacterState.AddState(States.ReducingHealing, 6.0f, 0f, _player.gameObject, null);
//                reducingHealingState = (ReducingHealingState)targetCharacter.CharacterState.GetState(States.ReducingHealing);
//                if (reducingHealingState != null)
//                {
//                    //reducingHealingState.CurrentValue = 0.4f;
//                    Debug.Log("GrabTentacles / else if > 20 / CurrentValue = ");
//                }
//            }
//            else if (currentDamage == 30)
//            {
//                Debug.Log("GrabTentacles / else if == 30");
//                targetCharacter.CharacterState.AddState(States.ReducingHealing, 6.0f, 0f, _player.gameObject, null);
//                reducingHealingState = (ReducingHealingState)targetCharacter.CharacterState.GetState(States.ReducingHealing);
//                if (reducingHealingState != null)
//                {
//                    //reducingHealingState.CurrentValue = 0.8f; 
//                    Debug.Log("GrabTentacles / else if == 30 / CurrentValue = ");
//                }
//            }
//            UseAttackingEnergy(currentDamage);
//        }
//        else
//        {
//            currentDamage = 0;
//            isAttackingPsiEnergyActive = false;
//        }

//        GameObject item = Instantiate(_tentaclesPrefab.gameObject, pointInstantiate, Quaternion.identity);
//        GrabTentaclesPrefab projectile = item.GetComponent<GrabTentaclesPrefab>();

//        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

//        projectile.InitializationProjectile(player, target, pointInstantiate, endPoint, isAttackingPsiEnergyActive, currentDamage);

//        projectile.StartTentaclesGrab();

//        NetworkServer.Spawn(item);

//        RpcInstantiateTentacles(projectile.gameObject, player, target, pointInstantiate, endPoint, isAttackingPsiEnergyActive, currentDamage);
//    }

//    private void UseAttackingEnergy(float value)
//    {
//        //_attackingPsionicEnergy.CurrentAttackingPsiEnergy -= value;
//    }

//    [ClientRpc]
//    private void RpcInstantiateTentacles(GameObject projectile, GameObject player, GameObject target, Vector3 instantiatePoint, Vector3 endPoint,
//        bool isAttackingPsienergyActive, float currentDamage)
//    {
//        projectile.GetComponent<GrabTentaclesPrefab>().InitializationProjectile(player, target, instantiatePoint, endPoint, isAttackingPsienergyActive, currentDamage);
//        projectile.GetComponent<GrabTentaclesPrefab>().StartTentaclesGrab();
//    }
//}
