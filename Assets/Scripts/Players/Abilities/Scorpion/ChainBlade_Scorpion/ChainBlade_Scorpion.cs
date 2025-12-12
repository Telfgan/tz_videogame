using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum ChainbladeType
{
    Default,
    Hook
}
public class ChainBlade_Scorpion : Skill
{
    [Header("Ability settings")]
    [SerializeField][Range(0, 100)] private float _minDamage = 3f;
    [SerializeField][Range(0, 100)] private float _maxDamage = 5f;
    [SerializeField] private Character _playerLinks;
    [SerializeField] private NetworkIdentity _playerIdentity;

    [SerializeField] private PassiveCombo_Scorpion _comboCounter;
    [SerializeField] private float _range;
    [SerializeField] private ChainController _chainPrefab;
    [SerializeField] private ChainController _chain;

    [SerializeField] private GameObject _projectilePrefab;
    private GameObject _projectile;
    private BladeProjectile _blade;

    private Character enemy;
    private bool bladeDestroyed = false;
    private ChainbladeType _type;

    private GameObject _tempTarget;
    private MoveComponent _tempTargetMove;

    public float DamageRange => Random.Range(_minDamage, _maxDamage);

    protected override bool IsCanCast
    {
        get
        {
            return true;
        }
    }

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => Animator.StringToHash("Cast ChainBlade");

    public void AnimCastChainBlade()
    {
        AnimStartCastCoroutine();
    }

    public void AnimChainBladeEnd()
    {
        AnimCastEnded();
    }
    protected override IEnumerator CastJob()
    {
        Vector3 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Vector3 direction = /*Camera.main.ScreenToWorldPoint(Input.mousePosition)*/GetMousePoint()  - transform.position;
        direction.y = 0;

        CmdCreateProjectile(8f, direction, this.gameObject, _type); // <--- event hit is here

        CmdThrowBlade(dir);

        yield return null;
    }

    private IEnumerator PullEnemy(Character enemy)
    {
        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        enemy.GetComponent<MoveComponent>().CanMove = false;

        while (distance >= 2.5f)
        {
            Debug.Log("Pulling");

            Vector3 pullDirection = (transform.position - enemy.transform.position).normalized;
            Vector3 pullForce = pullDirection * 10f * Time.deltaTime;

            Pull(enemy, pullForce);

            distance = Vector3.Distance(transform.position, enemy.transform.position);
            yield return null;
        }

        enemy.Move.CanMove = true;
        Destroy(_chain.gameObject);
    }

    private IEnumerator ReturnBlade(Transform bladeTransform, GameObject chainGameObject)
    {
        _hero.Move.CanMove = true;
        //_blade._rb.isKinematic = true;
        //_blade._rb.velocity = (transform.position - _projectile.transform.position).normalized * 20f;

        while (Vector2.Distance(transform.position, bladeTransform.position) > 2.9f)
        {
            /*_blade*/bladeTransform.GetComponent<BladeProjectile>()._rb.linearVelocity = (transform.position - bladeTransform.position).normalized * 20f;
            yield return null;
        }
          
        Destroy(chainGameObject);
        Destroy(bladeTransform.gameObject);
    }

    [Command]
    private void CmdCreateProjectile(float maxDistance, Vector3 direction, GameObject parent, ChainbladeType type)
    {
        //blade spawn
        Vector3 spawnPosition = transform.position + Vector3.up * 2f;

        _projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_projectile, _hero.NetworkSettings.MyRoom);

        _blade = _projectile.GetComponent<BladeProjectile>();
        _blade.Init(maxDistance, direction, parent, type);

        NetworkServer.Spawn(_projectile);

        //Damage
        _projectile.GetComponent<BladeProjectile>().OnHit.AddListener(target =>
        {
            if (target == null)
                return;

            Damage damage = new Damage
            {
                Value = Buff.Damage.GetBuffedValue(DamageRange),
                Type = DamageType,
            };

            //CmdApplyDamage(damage, target.gameObject);

            //DealDamage(damage, target.gameObject);

            ApplyDamage(damage,target.gameObject);
        });

        //Hook
        if (type == ChainbladeType.Hook)
        {
            _hero.Move.CanMove = false;
            _projectile.GetComponent<BladeProjectile>().OnHit.AddListener(target =>
            {
                if (target != null && target.TryGetComponent<Character>(out Character character))
                {
                    enemy = character;
                    _chain.targetID = enemy.GetComponent<NetworkIdentity>().netId;
                    NetworkServer.Destroy(_blade.gameObject);
                    StartCoroutine(PullEnemy(enemy));
                }
                else
                {
                    StartCoroutine(ReturnBlade(_projectile.transform, _chain.gameObject));
                }
                bladeDestroyed = true;
            });

            //chain spawn
            GameObject item = Instantiate(_chainPrefab.gameObject);

            SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);
            _chain = item.GetComponent<ChainController>();
            
            NetworkServer.Spawn(item);
            _chain.targetID = _blade.netId;
            _chain.parentID = _playerLinks.GetComponent<NetworkIdentity>().netId;
        }
    }

    [Command]
    private void CmdThrowBlade(Vector3 direction)
    {
        _projectile.GetComponent<BladeProjectile>().ThrowBlade(direction);
    }

    private void ResetValue()
    {
        bladeDestroyed = false;
        _hero.Move.CanMove = true;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while(true)
        {
            if (GetMouseButton)
            {
                break;
            }
            yield return null;
        }

        if (_playerLinks.Resources.First(o=>o.Type == ResourceType.Mana || o.Type == ResourceType.Energy).CurrentValue >= 1)
        {
            _type = ChainbladeType.Hook;
            _skillEnergyCosts[0].resourceCost = 1;
        }
        else
        {
            _type = ChainbladeType.Default;
            _skillEnergyCosts[0].resourceCost = 1;
        }

        yield return null;
    }

    protected override void ClearData()
    {
        
    }

    private void  DealDamage(Damage damage, GameObject hp)
    {
        Debug.LogError("nenavizhu");
        /*
        if (_tempTargetForDamage != hp.transform)
        {
            _tempTargetForDamage = hp.transform;
            _tempHPForDamage = hp.GetComponent<Health>();
        }
        _tempHPForDamage.TryTakeDamage(ref damage, this);*/
    }


    private void Pull(Character target, Vector3 force)
    {
        if (_tempTarget != target.gameObject)
        {
            _tempTarget = target.gameObject;
            _tempTargetMove = _tempTarget.GetComponent<MoveComponent>();
        }

        _tempTargetMove.TargetRpcAddTransformPosition(force);
        _comboCounter.AddSkill(target, this);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        throw new NotImplementedException();
    }
}
