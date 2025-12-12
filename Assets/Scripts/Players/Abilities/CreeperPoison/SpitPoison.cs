using Mirror;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public struct SpitPoisonSpawnPointInfo : NetworkMessage
{
    public float SpawnPointX;
    public float SpawnPointY;
    public float SpawnPointZ;
}

public class SpitPoison : Skill, IAltAbility
{
    [Header("Talents")]
    [SerializeField] private RestorationOfGlands _restorationOfGlands;
    [SerializeField] private HealingSpitPoison _healingSpitPoison;
    [SerializeField] private HealingPoisonCloud _healPoisonCloud;
    [SerializeField] private TransparentPoisons _transparentPoisons;
    //[SerializeField] private EatingAcid _eatingAcid;

    [Header("Ability Properties")]
    [SerializeField] private SpitPoisonProjectile _projectile;
    [SerializeField] private Character _player;
    [SerializeField] private GameObject _spawnPoint;

    #region PoisonCloud

    [SerializeField] private PoisonDamagingCloudPrefab _poisonDamagingCloudPrefab;
    [SerializeField] private PoisonHealingCloudPrefab _poisonHealingCloudPrefab;
    private PoisonDamagingCloudPrefab _poisonDamagingCloud;
    private PoisonHealingCloudPrefab _poisonHealingCloud;
    private float _durationPoisonCloud = 6f;

    #endregion

    private SpitPoisonSpawnPointInfo _spawnPointInfo = new SpitPoisonSpawnPointInfo();

    private Vector3 _mousePos = Vector3.positiveInfinity;

    private Character _currentTarget;

    private int _poisonBoneStack = 0;

    private float _originalCooldown;

    private bool _isActiveHealingSpitPoison;
    private bool _isActiveRestorationOfGlands;
    private bool _isHealingPoisonCloud = false;
    private bool _isPlayerInvisible = false;

    private bool _isOriginalTargetEnemy;
    private bool _isOriginalTargetAllies;
    private bool _isOriginalTargetPlayer;
    private bool _isAbilityActive = false;

    private Coroutine _setSpawnPointCoroutine;

    public bool IsAltAbility { get; set; }
    public int PoisonBoneStack { get => _poisonBoneStack; set => _poisonBoneStack = value; }

    public event Action ResetAbilityParameters;
    public event Action AbilityChange;

    private static readonly int spitPoisonTrigger = Animator.StringToHash("SpitPoisonCastAnimTrigger");

    protected override int AnimTriggerCast => spitPoisonTrigger;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => CheckCanCast();

    public void AnimSpitPoisonCast()
    {
        AnimStartCastCoroutine();
    }   

    public void AnimSpitPoisonCastEnd()
    {
        AnimCastEnded();
    }

    protected void Start()
    {
        _originalCooldown = _cooldownTime;
    }

    private void Update()
    {
        if (_isAbilityActive)
            SetSpawnPoint(_spawnPoint.transform.position.x, _spawnPoint.transform.position.y, _spawnPoint.transform.position.z);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _currentTarget = (Character)targetInfo.GetTargets()[0];
        _mousePos = targetInfo.Points[0];
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        _isAbilityActive = true;

        if (_setSpawnPointCoroutine == null)
            _setSpawnPointCoroutine = StartCoroutine(SetSpawnPointJob());

        CooldownChange();

        CheckActiveTalents();

        while (_currentTarget == null && float.IsPositiveInfinity(_mousePos.x))
        {
            if (GetMouseButton)
            {
                //_currentTarget = GetRaycastTarget(true);

                ChooseTarget();

                _mousePos = GetMousePoint();
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.AddTarget(_currentTarget);
        targetInfo.Points.Add(_mousePos);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        Shoot();

        ResetAbilityParameters?.Invoke();

        yield return null;
    }

    protected override void ClearData()
    {
        _isAbilityActive = false;

        _isHealingPoisonCloud = false;
        _isActiveHealingSpitPoison = false;

        _isOriginalTargetAllies = false;
        _isOriginalTargetEnemy = false;
        _isOriginalTargetPlayer = false;

        _currentTarget = null;

        _mousePos = Vector3.positiveInfinity;
    }

    private IEnumerator SetSpawnPointJob()
    {
        while (_isAbilityActive)
        {
            SetSpawnPoint(_spawnPoint.transform.position.x, _spawnPoint.transform.position.y, _spawnPoint.transform.position.z);

            yield return null;
        }

        StopCoroutine(_setSpawnPointCoroutine);
        _setSpawnPointCoroutine = null;
    }

    private bool CheckCanCast()
    {
        if (_currentTarget == null)
            return Vector3.Distance(_mousePos, transform.position) <= Radius && NoObstacles(_mousePos, _obstacle);

        return Vector3.Distance(_mousePos, transform.position) <= Radius && NoObstacles(_mousePos, _obstacle) ||
               Vector3.Distance(_currentTarget.transform.position, transform.position) <= Radius && NoObstacles(_currentTarget.transform.position, _obstacle);
    }

    private void CooldownChange()
    {
        if (_isActiveHealingSpitPoison)
        {
            if (_isOriginalTargetAllies || _isOriginalTargetPlayer)
            {
                if (_cooldownTime == _originalCooldown)
                {
                    _cooldownTime /= 3;
                }
            }
            else
            {
                _cooldownTime = _originalCooldown;
            }
        }
        else
        {
            _cooldownTime = _originalCooldown;
        }
    }

    private void CheckActiveTalents()
    {
        //_isActiveEatingAcid = _eatingAcid.Data.IsOpen;
        _isActiveHealingSpitPoison = _healingSpitPoison.Data.IsOpen;
        _isActiveRestorationOfGlands = _restorationOfGlands.Data.IsOpen;
    }

    private void ChooseTarget()
    {
        if (_currentTarget != null)
        {
            if (_currentTarget.gameObject == _player.gameObject)
            {
                _isOriginalTargetPlayer = true;
                _isOriginalTargetAllies = false;
                _isOriginalTargetEnemy = false;
                if (_healPoisonCloud.Data.IsOpen && _isActiveHealingSpitPoison)
                {
                    _isHealingPoisonCloud = true;
                }
            }
            else if (_currentTarget.gameObject.layer == LayerMask.NameToLayer("Allies"))
            {
                _isOriginalTargetPlayer = false;
                _isOriginalTargetAllies = true;
                _isOriginalTargetEnemy = false;
                if (_isActiveHealingSpitPoison && _healPoisonCloud.Data.IsOpen)
                {
                    if (_healPoisonCloud.Data.IsOpen)
                    {
                        _isHealingPoisonCloud = true;
                    }
                }
            }
            else if (_currentTarget.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                _isOriginalTargetPlayer = false;
                _isOriginalTargetAllies = false;
                _isOriginalTargetEnemy = true;
                if (_healPoisonCloud.Data.IsOpen && _isActiveHealingSpitPoison)
                {
                    _isHealingPoisonCloud = false;
                }
            }
        }
        else
        {
            _isOriginalTargetPlayer = false;
            _isOriginalTargetAllies = false;
            _isOriginalTargetEnemy = false;

            if (_mousePos != Vector3.zero)
            {
                _currentTarget = null;
            }
        }
    }

    private void Shoot()
    {
        if (_currentTarget != null)
        {
            CmdInstantiateProjectileToTarget(_currentTarget.gameObject, _player.Resources.FirstOrDefault()!.CurrentValue,
                _isActiveHealingSpitPoison, _isActiveRestorationOfGlands, IsAltAbility,
                _isOriginalTargetPlayer, _isOriginalTargetEnemy, _isOriginalTargetAllies);

            //CmdApplyPoisonCloud(_isHealingPoisonCloud, _durationPoisonCloud);
        }
        else
        {
            CmdInstantiateProjectileToPoint(_mousePos, _player.Resources.FirstOrDefault()!.CurrentValue,
                _isActiveHealingSpitPoison, _isActiveRestorationOfGlands, IsAltAbility,
                _isOriginalTargetPlayer, _isOriginalTargetEnemy, _isOriginalTargetAllies);

            //CmdApplyPoisonCloud(_isHealingPoisonCloud, _durationPoisonCloud);
        }

        _player.Move.CanMove = true;
    }

    #region Command Methods

    [Command]
    private void SetSpawnPoint(float spawnPointX, float spawnPointY, float spawnPointZ)
    {
        _spawnPointInfo.SpawnPointX = spawnPointX;
        _spawnPointInfo.SpawnPointY = spawnPointY;
        _spawnPointInfo.SpawnPointZ = spawnPointZ;
    }

    [Command]
    private void CmdInstantiateProjectileToTarget(GameObject target, float manaValue,
        bool isActiveHealingSpitPoison, bool isActiveRestorationOfGlands, bool isPlayerInvisible,
        bool isTargetPlayer, bool isTargetEnemy, bool isTargetAllies)
    {
        Vector3 spawnPosition = new Vector3 (_spawnPointInfo.SpawnPointX, _spawnPointInfo.SpawnPointY, _spawnPointInfo.SpawnPointZ);

        GameObject item = Instantiate(_projectile.gameObject, spawnPosition, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        SpitPoisonProjectile projectile = item.GetComponent<SpitPoisonProjectile>();

        projectile.InitializationProjectile(_player, this, _player.Resources.FirstOrDefault()!.CurrentValue,
            isActiveHealingSpitPoison, isActiveRestorationOfGlands, isPlayerInvisible,
            isTargetPlayer, isTargetEnemy, isTargetAllies, PoisonBoneStack);

        projectile.MoveBallToTarget(target.transform.position);

        NetworkServer.Spawn(item);
    }

    [Command]
    private void CmdInstantiateProjectileToPoint(Vector3 point, float manaValue,
        bool isActiveHealingSpitPoison, bool isActiveRestorationOfGlands, bool isPlayerInvisible,
        bool isTargetPlayer, bool isTargetEnemy, bool isTargetAllies)
    {
        Vector3 spawnPosition = new Vector3(_spawnPointInfo.SpawnPointX, _spawnPointInfo.SpawnPointY, _spawnPointInfo.SpawnPointZ);

        GameObject item = Instantiate(_projectile.gameObject, spawnPosition, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        SpitPoisonProjectile projectile = item.GetComponent<SpitPoisonProjectile>();

        projectile.InitializationProjectile(_player, this, _player.Resources.FirstOrDefault()!.CurrentValue,
            isActiveHealingSpitPoison, isActiveRestorationOfGlands, isPlayerInvisible,
            isTargetPlayer, isTargetEnemy, isTargetAllies, PoisonBoneStack);

        projectile.MoveBallOnMaxDistance(point);

        NetworkServer.Spawn(item);
    }

    [Command]
    private void CmdApplyPoisonCloud(bool isHealingCloud, float duration)
    {
        if (!isHealingCloud)
        {
            if (_poisonDamagingCloud == null && _poisonDamagingCloudPrefab.PoisonDamageCloud == null)
            {
                _player.CharacterState.AddState(States.PoisonCloud, duration, 0, _player.gameObject, Name);

                _poisonDamagingCloud = Instantiate(_poisonDamagingCloudPrefab, transform.position, Quaternion.identity);
                _poisonDamagingCloudPrefab.PoisonDamageCloud = _poisonDamagingCloud;

                SceneManager.MoveGameObjectToScene(_poisonDamagingCloudPrefab.PoisonDamageCloud.gameObject, _hero.NetworkSettings.MyRoom);

                _poisonDamagingCloudPrefab.PoisonDamageCloud.InitializationProjectile(_player, duration);
                _poisonDamagingCloudPrefab.PoisonDamageCloud.AddStack();

                NetworkServer.Spawn(_poisonDamagingCloud.gameObject);
            }
            else
            {

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


    #endregion

    #region ClientRpc Methods

    [ClientRpc]
    private void RpcApply(PoisonDamagingCloudPrefab poisonDamagingCloud, PoisonHealingCloudPrefab poisonHealingCloud, float duration, bool isHealingCloud)
    {
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

    #endregion
}
