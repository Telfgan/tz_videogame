using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShotAstral : Skill
{
    [SerializeField] private ArrowAstralProjectile _projectile;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private HeroComponent _playerLinks;

    private const string _startAnimTrigger = "ShotAstralCastDelayTrigger";
    private const string _endAnimTrigger = "ShotCastDelayEndAnimTrigger";

    private Vector3 _targetPoint = Vector3.positiveInfinity;
    //private Character _target;

    protected override int AnimTriggerCastDelay => Animator.StringToHash(_startAnimTrigger);
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast =>
        Vector3.Distance(_targetPoint, transform.position) <= Radius &&
        NoObstacles(_targetPoint, transform.position, _obstacle);

    private void OnDestroy() => OnSkillCanceled -= HandleSkillCanceled;

    protected override IEnumerator PrepareJob(Action<TargetInfo> callback)
    {
        OnSkillCanceled += HandleSkillCanceled;
        Hero.Animator.speed /= CastDeley;
        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        while (float.IsPositiveInfinity(_targetPoint.x))
        {
            if (GetMouseButton)
            {
                Vector3 click = GetMousePoint();

                if (IsPointInRadius(Radius, click) && NoObstacles(click, transform.position, _obstacle))
                {
                    _targetPoint = click;
                    FindTargetCharacter();
                    if (GetTargetCharacter() is Character player && player == _playerLinks)
                    {
                        _playerLinks.CharacterState.CmdAddState(States.Astral, _projectile.Duration, 0, gameObject, "ShotAstral");
                        TryCancel(true);
                        yield break;
                    }

                    if (GetTargetCharacter() is Character character)
                    {
                        //_target = character;
                        if (multiMagic != null) multiMagic.LastTarget = GetTargetCharacter();
                        Hero.Move.LookAtTransform(character.transform);
                    }

                    else Hero.Move.LookAtPosition(_targetPoint);
                    Hero.Move.CanMove = false;
                }
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_targetPoint);
        callback(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (!IsCanCast)
        {
            HandleSkillCanceled();
            ClearData();
            yield break;
        }

        CmdCreateProjectileAtPosition(_targetPoint);

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        if (multiMagic != null)
        {
            foreach (var character in multiMagic.PopPendingTargets())
            {
                TryPayCost();
                CmdCreateProjectileAtPosition(character.transform.position);
            }
        }

        WorkAnimator(_startAnimTrigger, _endAnimTrigger);

        HandleSkillCanceled();
        ClearData();
    }

    private void HandleSkillCanceled()
    {
        if (Hero?.Move != null)
        {
            Hero.Move.CanMove = true;
            Hero.Animator.speed = 1;
            AfterCastJob();
            Hero.Move.StopLookAt();
        }
    }

    private void WorkAnimator(string oldTrigger, string newTrigger)
    {
        _hero.Animator.ResetTrigger(Animator.StringToHash(oldTrigger));
        _hero.NetworkAnimator.ResetTrigger(Animator.StringToHash(oldTrigger));
        _hero.Animator.CrossFade(newTrigger, 0.1f);
        CmdCrossFade(newTrigger);
    }

    [Command]
    private void CmdCreateProjectileAtPosition(Vector3 position)
    {
        Vector3 spawnPoition = _spawnPoint ? _spawnPoint.position : transform.position;
        Vector3 direction = (position - spawnPoition).normalized;
        if (direction == Vector3.zero) return;

        var projectile = Instantiate(_projectile, spawnPoition, Quaternion.LookRotation(direction));
        projectile.Init(_playerLinks, 0, false, this);
        SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(projectile.gameObject);
        projectile.StartFly(direction);
        RpcInit(projectile.gameObject);
    }

    [Command] private void CmdCrossFade(string trigger) => _hero.Animator.CrossFade(trigger, 0.1f);

    [ClientRpc]
    private void RpcInit(GameObject gameObject)
    {
        if (gameObject && gameObject.TryGetComponent(out ArrowAstralProjectile arrowAstralProjectile)) arrowAstralProjectile.Init(_playerLinks, 0, false, this);
    }

    protected override void ClearData()
    {
        ClearTarget();
       // _target = null;
        _targetPoint = Vector3.positiveInfinity;
    }

    public override void LoadTargetData(TargetInfo targetInfo) => _targetPoint = targetInfo.Points[0];
}
