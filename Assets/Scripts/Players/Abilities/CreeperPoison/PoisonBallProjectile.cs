using Mirror;
using System.Collections;
using UnityEngine;

public class PoisonBallProjectile : Test_Projectile
{
    #region Variables

    [Header("PoisonBallProjectile Parameters")]
    [SerializeField] private Transform _transformBall;
    [SerializeField] private float _baseSizeBall;
    [SerializeField] private float _damage;
    [SerializeField] private float _baseDistancePush;
    [SerializeField] private float _baseDurationPush;
    [SerializeField] private float _durationInAir;
    [SerializeField] private float poisonBone;
    [SerializeField] private float _fastMovementSpeed;
    [SerializeField] private float _slowMovementSpeed;

    private PoisonBall _poisonBall;
    private Skill _skill;

    private int _currentCountBall;
    private int _poisonBoneStack;
    private int _playerLayer;

    #region FloatVariables
    private float _newDistancePush;
    private float _distanceIncreaseMultiplier = 0.5f;
    private float _multiplierDistanceFromTalent;
    #endregion

    #region BoolVaribales

    private bool _isFast;

    private bool _isPlayer;
    private bool _isAllies;
    private bool _isEnemy;
    private bool _isAlly;

    private bool _isActiveFootInstincts;
    private bool _isActiveRestorationOfGlands;
    private bool _isActiveHealingPoisonBall;
    private bool _isActiveWitheringPoison;
    private bool _isActiveVoluminousBall;
    private bool _isActiveBallEffect;

    private bool _isPushTarget;
    private bool _isPlayerInvisible;

    #endregion

    #endregion

    #region OnTriggerEnter

    [Server]
    private void OnTriggerEnter(Collider collision)
    {
        if (_isActiveHealingPoisonBall)
        {
            if (_isPlayer)
            {
                if (collision.gameObject == _player.gameObject)
                {
                    if (_isFast)
                    {
                        _player.CharacterState.AddState(States.HealingPoisonPerSecond, 7.0f, 0, _player.gameObject, _skill.Name);
                    }
                    else
                    {
                        _player.CharacterState.AddState(States.InstantHealingPoison, 0, 0, _player.gameObject, _skill.Name);
                    }

                    DestroyProjectile();
                }
            }
            else if (_isAllies)
            {
                if (collision.transform != _player.transform && _playerLayer == LayerMask.NameToLayer("Allies"))
                {
                    if (collision.TryGetComponent<Character>(out var alliesHealth))
                    {
                        if (_isFast)
                        {
                            alliesHealth.CharacterState.AddState(States.HealingPoisonPerSecond, 7.0f, 0, _player.gameObject, _skill.Name);
                        }
                        else
                        {
                            alliesHealth.CharacterState.AddState(States.InstantHealingPoison, 0, 0, _player.gameObject, _skill.Name);
                        }

                        DestroyProjectile();
                    }
                }
                else if (!_isEnemy && collision.transform != _player.transform)
                {
                    return;
                }
            }
            else if (_isEnemy)
            {
                if (collision.gameObject != _player.gameObject && _playerLayer != LayerMask.NameToLayer("Enemy"))
                {
                    if (collision.TryGetComponent<Character>(out var targetHealth))
                    {
                        _target = targetHealth;
                        DamageDeal();

                        if (_isActiveFootInstincts)
                        {
                            RpcReductionCooldownAtFootInstincts(_player.gameObject);
                        }

                        _poisonBall.LastTarget = targetHealth.gameObject;
                    }
                }
                else if (_isAlly && collision.gameObject != _player.gameObject)
                {
                    return;
                }
            }
            else
            {
                if (collision.gameObject != _player.gameObject && _playerLayer != LayerMask.NameToLayer("Enemy"))
                {
                    if (collision.TryGetComponent<Character>(out var targetHealth))
                    {
                        _target = targetHealth;

                        DamageDeal();

                        if (_isActiveFootInstincts)
                        {
                            RpcReductionCooldownAtFootInstincts(_player.gameObject);
                        }

                        _poisonBall.LastTarget = targetHealth.gameObject;
                    }
                }
            }
        }
        else
        {
            if (collision.gameObject != _player.gameObject && _playerLayer != LayerMask.NameToLayer("Enemy"))
            {
                if (collision.TryGetComponent<Character>(out var targetHealth))
                {
                    _target = targetHealth;

                    DamageDeal();

                    if (_isActiveFootInstincts)
                    {
                        RpcReductionCooldownAtFootInstincts(_player.gameObject);
                    }

                    _poisonBall.LastTarget = targetHealth.gameObject;
                }
            }
        }
    }

    #endregion

    #region MovementBall

    public void MoveBallToTarget(Vector3 target, bool isFast)
    {
        _isFast = isFast;

        float speed = isFast ? _fastMovementSpeed : _slowMovementSpeed;

        MoveToTarget(target, speed);
    }

    public void MoveBallOnMaxDistance(Vector3 point, bool isFast)
    {
        Debug.Log("PoisonBallProjectile / MoveBallOnMaxDistance");
        _isFast = isFast;

        float speed = isFast ? _fastMovementSpeed : _slowMovementSpeed;
        Debug.Log("PoisonBallProjectile / MoveBallOnMaxDistance / speed = " + speed);
        Debug.Log("PoisonBallProjectile / MoveBallOnMaxDistance / point = " + point);

        MoveToPoint(point, speed);
    }

    #endregion

    #region MakingDamageAndDebuffs

    public override void DamageDeal()
    {
        Damage _baseDamage = new Damage
        {
            Value = _skill.Buff.Damage.GetBuffedValue(_damage),
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.RangeAttack,
        };

        _target.Health.TryTakeDamage(ref _baseDamage, _skill);
        //_target.DamageTracker.AddDamage(_baseDamage, isServerRequest: isServer);

        if (_isActiveWitheringPoison)
        {
            _target.CharacterState.AddState(States.WitheringPoison, 6f, 0, _player.gameObject, _skill.Name);
        }

        //if (_restorationOfGlands.Data.IsOpen && _poisonBoneStack > 0 && _target.CharacterState.CheckForState(States.PoisonBone))
        //{
        //    ReductionCooldownFromRestorationOfGlands();
        //}

        _target.CharacterState.AddState(States.InAir, _durationInAir, 0, _player.gameObject, _skill.Name);
        _target.CharacterState.AddState(States.PoisonBone, poisonBone, 0, _player.gameObject, _skill.Name);

        PushEnemyDependingOnCountProjectile(_target, _baseDurationPush);
        
        DestroyProjectile();
    }

    private void PushEnemyDependingOnCountProjectile(Character target, float durationPush)
    {
        if (_isActiveBallEffect && _currentCountBall >= 2)
        {
            float multiplierPush = _currentCountBall * _distanceIncreaseMultiplier;
            _newDistancePush = _baseDistancePush + multiplierPush + _multiplierDistanceFromTalent;
        }
        else
        {
            _newDistancePush = _baseDistancePush;
        }

        PushEnemy(target, durationPush, _newDistancePush);
    }

    private void PushEnemy(Character target, float durationPush, float newDistancePush)
    {
        MoveComponent targetMove = target.GetComponent<MoveComponent>();
        Vector3 directionPush = (_transformBall.transform.position - targetMove.transform.position);

        newDistancePush = (newDistancePush * durationPush) / GlobalVariable.cellSize;

        Vector3 finalPoint = targetMove.transform.position + (_isPushTarget ? -directionPush : directionPush) * newDistancePush;
        finalPoint.y = 0;

        if (targetMove.connectionToClient != null) targetMove.TargetRpcDoMove(finalPoint, durationPush);

        else
        {
            StartCoroutine(ServerMove(targetMove, finalPoint, durationPush));
        }

        target.Move.CanMove = true;
    }

    private void ReductionCooldownFromRestorationOfGlands()
    {
        RpcReductionCooldownFromRestorationOfGlands(_player.gameObject);
    }

    private IEnumerator ServerMove(MoveComponent targetMove, Vector3 finalPoint, float duration)
    {
        float elapsed = 0f;
        Vector3 start = targetMove.transform.position;

        while (elapsed < duration)
        {
            float time = elapsed / duration;
            targetMove.transform.position = Vector3.Lerp(start, finalPoint, time);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetMove.transform.position = finalPoint;
    }

    #endregion

    #region InitializationProjectiles

    public void InitializationProjectileForPoisonBall(Character dad, Skill skill,
        float multiplierDistance, int poisonBoneStack,
        bool isTargetPlayer, bool isTargetEnemy, bool isTargetAllies,
        bool isActiveFootInstincts, bool isActiveRestorationOfGlands,
        bool isActiveTalentHealingPoisonBall, bool isActiveTalentWitheringPoison, 
        bool isActiveVoluminousBall, bool isActiveBallEffect,
        bool isPushTarget, bool isPlayerInvisible)
    {
        CheckActiveTalentVoluminousBall(isActiveVoluminousBall);

        InitializationBoolVariables(isTargetPlayer, isTargetEnemy, isTargetAllies, isPlayerInvisible,
            isActiveFootInstincts, isActiveRestorationOfGlands,
            isActiveTalentHealingPoisonBall, isActiveTalentWitheringPoison, isActiveBallEffect);

        _player = dad;
        _skill = skill;
        _isPushTarget = isPushTarget;

        _isPlayerInvisible = isPlayerInvisible;

        Invoke("TransparentProjectileOnServer", 0.15f);

        InitializationNumericVariables(multiplierDistance, poisonBoneStack);

        InitializationComponentsForCountProjectile();
    }

    private void CheckActiveTalentVoluminousBall(bool isActiveVoluminousBall)
    {
        if (isActiveVoluminousBall)
        {
            float multiplierSize = 1.2f;
            float newScaleX = _transformBall.localScale.x * multiplierSize;
            float newScaleY = _transformBall.localScale.y * multiplierSize;
            float newScaleZ = _transformBall.localScale.z * multiplierSize;

            _transformBall.localScale = new Vector3(newScaleX, newScaleY, newScaleZ);
        } 
        else
        {
            _transformBall.localScale = new Vector3(_baseSizeBall, _baseSizeBall, _baseSizeBall);
        }
    }

    private void InitializationNumericVariables(float multiplierDistance, int poisonBoneStack)
    {
        _multiplierDistanceFromTalent = multiplierDistance;
        _poisonBoneStack = poisonBoneStack;
    }

    private void InitializationBoolVariables(
        bool isTargetPlayer, bool isTargetEnemy, bool isTargetAllies, 
        bool isPlayerInvisible,bool isActiveFootInstincts, 
        bool isActiveRestorationOfGlands, bool isActiveTalentHealingPoisonBall, 
        bool isActiveTalentWitheringPoison, bool isActiveBallEffect)
    {
        _isPlayer = isTargetPlayer;
        _isAllies = isTargetAllies;
        _isEnemy = isTargetEnemy;

        _isActiveFootInstincts = isActiveFootInstincts;
        _isActiveRestorationOfGlands = isActiveRestorationOfGlands;
        _isActiveBallEffect = isActiveBallEffect;
        _isActiveHealingPoisonBall = isActiveTalentHealingPoisonBall;
        _isActiveWitheringPoison = isActiveTalentWitheringPoison;
    }

    private void InitializationComponentsForCountProjectile()
    {
        _poisonBall = _player.GetComponentInChildren<PoisonBall>();
        _currentCountBall = _poisonBall.CurrentCountBall;
    }

    #endregion

    #region ServerMethods

    [Server]
    private void TransparentProjectileOnServer()
    {
        if (isServer)
        {
            LayerDefinition(_player.gameObject);
        }
        if (isServer && _isPlayerInvisible)
        {
            RpcNewTransparencySprite(_player.gameObject, this.gameObject);
        }
    }

    [Server]
    private void LayerDefinition(GameObject player)
    {
        _playerLayer = player.layer;

        RpcLayerDefinition(player.layer);
    }

    #endregion

    #region ClientRpcMethods

    [ClientRpc]
    private void RpcNewTransparencySprite(GameObject player, GameObject projectile)
    {
        MeshRenderer projectileMaterial = projectile.GetComponent<MeshRenderer>();
        Color originalColor = projectileMaterial.material.color;

        if (projectileMaterial != null)
        {
            if (player.layer == LayerMask.NameToLayer("Allies"))
            {
                Color newTransparencySprite = originalColor;
                newTransparencySprite.a = 0.5f;
                _projectileRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, newTransparencySprite.a);
            }
            else if (player.layer == LayerMask.NameToLayer("Enemy"))
            {
                Color newTransparencySprite = originalColor;
                newTransparencySprite.a = 0.0f;
                _projectileRenderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, newTransparencySprite.a);
            }
        }
    }

    [ClientRpc]
    private void RpcLayerDefinition(int layer)
    {
        _playerLayer = layer;
    }

    [ClientRpc]
    private void RpcReductionCooldownAtFootInstincts(GameObject player)
    {
        var footInstincts = player.GetComponentInChildren<FootInstincts>();

        footInstincts.ReductionCooldownLightningMovement();
    }

    [ClientRpc]
    private void RpcReductionCooldownFromRestorationOfGlands(GameObject player)
    {
        var restorationOfGlands = player.GetComponentInChildren<RestorationOfGlands>();

        float baseChanceOfRestorationOfGlands = 0.1f;
        float chanceRestorationOfGlands = baseChanceOfRestorationOfGlands * _poisonBoneStack;

        if (Random.Range(0f, 1f) <= chanceRestorationOfGlands)
        {
            Debug.Log("SpitPoisonProj / If RestorationOfGlands.IsActive = true");
            restorationOfGlands.ReductionCooldown();
        }
    }
    #endregion

}