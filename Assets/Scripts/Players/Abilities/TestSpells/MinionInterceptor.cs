using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionInterceptor : Skill
{
    //private MinionComponent _target;

    protected override bool IsCanCast => true;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {

        SetTarget((MinionComponent)targetInfo.GetTargets()[0]);
    }

    protected override IEnumerator CastJob()
    {
        //CmdIntercept(_target.gameObject);
        CmdIntercept(GetTargetCharacter().gameObject);
        yield return null;
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter(true);
                //var temp = GetRaycastTarget();

                if (GetTargetCharacter() is MinionComponent minion)
                {
                    //_target = minion;
                }
                else
                {
                    ClearTarget();
                }
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.GetTargets().Add(GetTargetCharacter());
        callbackDataSaved?.Invoke(targetInfo);
    }

    [Command]
    private void CmdIntercept(GameObject minion)
    {
        minion.GetComponent<MinionComponent>().SetAuthority(connectionToClient);
        
        if(Hero is HeroComponent hero)
        {
            hero.SpawnComponent.AddUnit(minion.GetComponent<MinionComponent>());
        }
    }
}
