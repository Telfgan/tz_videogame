using Mirror;
using UnityEngine;

public class SpitPoisonProjectile : Test_Projectile
{
    #region Variables

    private SpitPoison _spitPoison;
    private Skill _skill;

    private int _playerLayer;
    private int _poisonBoneStack;

    private float _energyDad;
    private float _damage;
    private float _lifeTimePoisonBoneStacks = 60.0f;

    private bool _isPlayer;
    private bool _isAllies;
    private bool _isEnemy;
    private bool _isActiveHealingSpitPoison;
    private bool _isActiveRestorationOfGlands;
    private bool _isActiveEatingAcid;
    private bool _isPlayerInvisible;

    #endregion

    #region OnTriggerEnter

    [Server]
    private void OnTriggerEnter(Collider collision)
    {
        if (_isActiveHealingSpitPoison)
        {
            if (_isPlayer)
            {
                if (collision.gameObject == _player.gameObject)
                {
                    _player.CharacterState.AddState(States.RegeneratingPoison, 4.0f, 0, _player.gameObject, _skill.Name);
                    Destroy(gameObject);
                }
            }
            else if (_isAllies)
            {
                if (collision.gameObject != _player.gameObject && _playerLayer == LayerMask.NameToLayer("Allies"))
                {
                    if (collision.TryGetComponent<Character>(out var alliesHealth))
                    {
                        alliesHealth.CharacterState.AddState(States.RegeneratingPoison, 4.0f, 0, _player.gameObject, _skill.Name);
                        Destroy(gameObject);
                    }
                }
                else if (!_isEnemy && collision.gameObject != _player.gameObject)
                {
                    return;
                }
            }   
            else if (_isEnemy)
            {
                if (collision.transform != _player.transform && _playerLayer != LayerMask.NameToLayer("Enemy"))
                {
                    if (collision.TryGetComponent<Character>(out var target))
                    {
                        _target = target;
                        _damage = Random.Range(4.0f, 12.0f);

                        DamageDeal();
                    }
                }
                else if (!_isAllies && collision.gameObject != _player.gameObject)
                {
                    return;
                }
            }
            else
            {
                if (collision.gameObject != _player.gameObject && _playerLayer != LayerMask.NameToLayer("Enemy"))
                {
                    if (collision.transform != _player.transform)
                    {
                        if (collision.TryGetComponent<Character>(out var target))
                        {
                            _target = target;

                            _damage = Random.Range(4.0f, 12.0f);

                            DamageDeal();
                        }
                    }
                }
            }
        }
        else
        {
            if (collision.transform != _player.transform && _playerLayer != LayerMask.NameToLayer("Enemy"))
            {
                if (collision.TryGetComponent<Character>(out var target))
                {
                    _target = target;

                    _damage = Random.Range(4.0f, 12.0f);

                    DamageDeal();
                }
            }
            
        }
    }

    #endregion

    #region MoveMethods

    public void MoveBallToTarget(Vector3 target)
    {
        Debug.Log("SpitPoisonProj / MoveBallToTarget / Start");

        MoveToTarget(target, _speed);
    }

    public void MoveBallOnMaxDistance(Vector3 point)
    {
        Debug.Log("SpitPoisonProj / MoveBallOnMaxDistance / point = " + point);

        MoveToPoint(point, _speed);
    }
    #endregion

    #region DamageMethods

    public override void DamageDeal()
    {
        float chanceOfBlindness = 0.3f;
        float numbersForChanceOfBlindness = Random.Range(0.0f, 1.0f);

        Damage _baseDamage = new Damage
        {
            Value = _skill.Buff.Damage.GetBuffedValue(_damage),
            Type = DamageType.Physical,
            PhysicAttackType = AttackRangeType.RangeAttack,
        };
        
        _target.Health.TryTakeDamage(ref _baseDamage, _skill);
        _target.DamageTracker.AddDamage(_baseDamage, null, isServerRequest: isServer);

        _target.CharacterState.AddState(States.PoisonBone, _lifeTimePoisonBoneStacks, 0, _player.gameObject, _skill.Name);

        if (_isActiveRestorationOfGlands && _poisonBoneStack > 0 && _target.CharacterState.CheckForState(States.PoisonBone))
        {
            //ReductionCooldownFromRestorationOfGlands();
        }

        _target.CharacterState.AddState(States.Blind, 6f, 0, _player.gameObject, _skill.Name);

        DestroyProjectile();        
    }
    private void ReductionCooldownFromRestorationOfGlands()
    {
        RpcReductionCooldownFromRestorationOfGlands(_player.gameObject);
    }
    #endregion

    #region InitializationMethods

    public void InitializationProjectile(Character dad, Skill skill, float energy,
        bool isActiveHealingSpitPoison, bool isActiveRestorationOfGlands, bool isPlayerInvisible, 
        bool isTargetPlayer, bool isTargetEnemy, bool isTargetAllies, int poisonBoneStack)
    {
        _player = dad;
        _energyDad = energy;
        _skill = skill;
        _isPlayerInvisible = isPlayerInvisible;

        _poisonBoneStack = poisonBoneStack;
        _isActiveRestorationOfGlands = isActiveRestorationOfGlands;
        _isActiveHealingSpitPoison = isActiveHealingSpitPoison;
        _isPlayer = isTargetPlayer;
        _isAllies = isTargetAllies;
        _isEnemy = isTargetEnemy;

        Invoke("TransparentProjectileOnServer", 0.15f);
        InitializationComponents();
    }

    private void InitializationComponents()
    {
        _spitPoison = _player.GetComponentInChildren<SpitPoison>();
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

