using System.Collections;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class ChainBlade : Skill
{
    [SerializeField] [Range(0, 100)] private float _minDamage = 3f;
    [SerializeField] [Range(0, 100)] private float _maxDamage = 5f;
    [SerializeField] private PassiveCombo_Scorpion _comboCounter;

    [SerializeField] private ChainArrow chainArrowPrefab;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private HeroComponent playerLinks;


    private ChainArrow _chainArrowPrefab;
    private Vector3 _clickPoint = Vector3.positiveInfinity;
    private Animator _animator;
    //private Character _target;

    private static readonly int chainBladeStart = Animator.StringToHash("ChainStart");
    private static readonly int chainBladeEnd = Animator.StringToHash("ChainEnd");
    private static readonly int chainBladeDestroy = Animator.StringToHash("ChainBladeDestroy");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => chainBladeStart;

    protected override bool IsCanCast
    {
        get
        {
            if (GetTargetCharacter() != null) return Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius;

            else return true;
        }
    }

    public float DamageRange => UnityEngine.Random.Range(_minDamage, _maxDamage);
    public PassiveCombo_Scorpion ComboCounter { get => _comboCounter; set => _comboCounter = value; }

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (float.IsPositiveInfinity(_clickPoint.x))
        {
            if (GetMouseButton)
            {
                if (GetTargetCharacter() != null)
                {
                    float distance = Vector3.Distance(_hero.transform.position, _clickPoint);

                    if (distance <= Radius) _clickPoint = GetTargetCharacter().transform.position;

                    else
                    {
                        //_target = GetTarget().character;
                        _clickPoint = GetTargetCharacter().transform.position;
                    }
                }

                else _clickPoint = GetMousePoint();
            }

            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_clickPoint);
        callbackDataSaved(targetInfo);
    }

    public void ChainBladeEnd()
    {
        if (_animator != null)
        {
            _animator.ResetTrigger(chainBladeStart);
            _animator.SetTrigger(chainBladeEnd);
        }
    }

    private void ChainBladeDestroy()
    {
        Hero.Move.StopLookAt();

        if (_animator != null)
        {
            _animator.ResetTrigger(chainBladeEnd);
            _animator.SetTrigger(chainBladeDestroy);
        }
    }


    protected override IEnumerator CastJob()
    {
        Hero.Move.StopMoveAndAnimationMove();
        CmdSpawnChainArrow(_clickPoint);
        yield return null;
    }

    protected override void ClearData()
    {
        _clickPoint = Vector3.positiveInfinity;

        ClearTarget();
        //_target = null;
    }

    public void ChainBladeCast()
    {
        AnimStartCastCoroutine();
        if ((Hero.transform.position - _clickPoint).sqrMagnitude > 0.001f)  Hero.Move.LookAtPosition(_clickPoint);
        Hero.Move.CanMove = false;
    }

    public void ChainBladeCastEnd()
    {
        AnimCastEnded();
        ChainBladeDestroy();
    }

    [Command]
    private void CmdSpawnChainArrow(Vector3 clickPoint)
    {

        Vector3 direction = (clickPoint - spawnPoint.transform.position).normalized;
        Vector3 flatDirection = new Vector3(direction.x, 0, direction.z).normalized;
        Vector3 targetPoint = spawnPoint.transform.position + flatDirection * Radius;

        var arrow = Instantiate(chainArrowPrefab, spawnPoint.transform.position, Quaternion.identity);
        _chainArrowPrefab = arrow;
        arrow.Init(playerLinks, 0, false, this);

        NetworkServer.Spawn(arrow.gameObject);
        SceneManager.MoveGameObjectToScene(arrow.gameObject, _hero.NetworkSettings.MyRoom);

        arrow.InitArrow(targetPoint, spawnPoint.transform, Radius, DamageRange);
        RpcInitArrow(arrow.gameObject, targetPoint);
    }

    [ClientRpc]
    private void RpcInitArrow(GameObject arrowObj, Vector3 targetPoint)
    {
        if (arrowObj == null) return;

        var arrow = arrowObj.GetComponent<ChainArrow>();
        arrow.Init(playerLinks, 0, false, this);
        arrow.InitArrow(targetPoint, spawnPoint.transform, Radius, DamageRange);
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _clickPoint = targetInfo.Points[0];
    }
}
