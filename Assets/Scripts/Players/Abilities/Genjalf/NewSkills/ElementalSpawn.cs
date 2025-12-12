using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ElementalSpawn : Skill
{
    private int _indexElemental = -1;
    private Vector3 _position;
    private int _prefIndex;
    private bool _isSpawned;
    private Character _airElement;
    private Character _waterElement;
    private Character _earthElement;
    private Character _fireElement;

    public Character AirElement => _airElement;
    public Character WaterElement => _waterElement;
    public Character EarthElement => _earthElement;
    public Character FireElement => _fireElement;
    public int IndexElemental { get { return _indexElemental; } set { _indexElemental = value; } }

    protected override bool IsCanCast => Vector3.Distance(_position, transform.position) <= Radius;

    protected override int AnimTriggerCastDelay => 0;

    protected override int AnimTriggerCast => 0;

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        _position = targetInfo.Points[0];
    }

    protected override IEnumerator CastJob()
    {
        while (_indexElemental == -1)
        {
            yield return null;
        }

        if (Hero.SpawnComponent.Units.Count > 0)
        {
            CmdDestroyUnit(0);
            Hero.SpawnComponent.Units.RemoveAt(0);
        }
        //Hero.SpawnComponent.UnitAdded += OnUnitAdded;
        Hero.SpawnComponent.CmdSpawnUnitInPoint(_position, IndexElemental);

        yield return null;
    }

    protected override void ClearData()
    {
        _position = Vector2.zero;
        _indexElemental = -1;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        while (_position == Vector3.zero)
        {
            if (GetMouseButton)
            {
                _position = GetMousePoint();
            }
            yield return null;
        }
        TargetInfo targetInfo = new();
        targetInfo.Points.Add(_position);
        callbackDataSaved(targetInfo);
    }

    [Command]
    private void CmdDestroyUnit(int index)
    {
        Debug.Log(Hero.SpawnComponent.Units[index].gameObject.name);
        NetworkServer.Destroy(Hero.SpawnComponent.Units[index].gameObject);

        Hero.SpawnComponent.Units.RemoveAt(index);
    }
}
