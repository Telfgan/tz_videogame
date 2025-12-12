using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class SubjugationMind : Skill
{
    //private Character _target;
    private Vector3 _targetPoint = Vector3.positiveInfinity;

    protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => Animator.StringToHash("PullingHealthCastDelay");
    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0) SetTarget(targetInfo.GetTargets()[0] as Character);
    }

    protected override IEnumerator CastJob()
    {
        CmdIntercept(GetTargetCharacter());

        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        if (multiMagic != null)
        {
            foreach (var character in multiMagic.PopPendingTargets())
            {
                TryPayCost();
                CmdIntercept(character);
            }
        }

        AfterCastJob();

        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        var multiMagic = Hero.CharacterState.GetState(States.MultiMagic) as MultiMagic;

        while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                //var temp = GetRaycastTarget();
                FindTargetCharacter();
                _targetPoint = GetMousePoint();

                //if (GetTarget() is MinionComponent minion) _target = minion;
                //else if (GetTarget() is HeroComponent heroComponent) _target = heroComponent;
                if (multiMagic != null) multiMagic.LastTarget = GetTargetCharacter();
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_targetPoint);
        callbackDataSaved(targetInfo);
    }

    [Command]
    private void CmdIntercept(Character character)
    {
        if (character == null) return;

        if (character is MinionComponent minion)
        {
            minion.SetAuthority(connectionToClient);

            if (Hero is HeroComponent hero)
            {
                hero.SpawnComponent.AddUnit(minion);
            }
        }
        else if (character is HeroComponent heroTarget)
        {
            var networkIdentity = heroTarget.GetComponent<NetworkIdentity>();
            if (networkIdentity == null) return;

            networkIdentity.RemoveClientAuthority();
            networkIdentity.AssignClientAuthority(connectionToClient);

            if (Hero is HeroComponent currentHero)
            {
                currentHero.SpawnComponent.AddUnit(heroTarget);
            }

            StartCoroutine(ReturnHeroControlAfterDelay(heroTarget, networkIdentity));
        }
    }

    private IEnumerator ReturnHeroControlAfterDelay(HeroComponent heroTarget, NetworkIdentity networkIdentity)
    {
        yield return new WaitForSeconds(4);

        if (networkIdentity != null)
        {
            networkIdentity.RemoveClientAuthority();
            networkIdentity.AssignClientAuthority(heroTarget.connectionToClient);

            if (Hero is HeroComponent currentHero)
            {
                currentHero.SpawnComponent.CmdRemoveUnit(heroTarget);
            }
        }
    }
}
