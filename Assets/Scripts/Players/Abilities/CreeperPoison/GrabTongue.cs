using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrabTongue : Skill
{
    [SerializeField] private TransparentPoisons _transparentPoisons;
    [SerializeField] private Character _player;
    [SerializeField] private GrabTongueProjectile _tongueProjectile;
    private Character _target;

    private Vector3 _mousePosition = Vector3.positiveInfinity;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private float _maxDistance = 10f;
    private bool _isCanAttract;
    private bool _isPlayerInvisible = false;

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => CheckCanCast();

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        Debug.LogError("DataError");
    }

    protected override void ClearData()
    {
        _target = null;
        _mousePosition = Vector3.positiveInfinity;
        _startPosition = Vector3.zero;
        _endPosition = Vector3.zero;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        if (_transparentPoisons.Data.IsOpen && _player.IsInvisible)
        {
            _isPlayerInvisible = true;
        }
        else
        {
            _isPlayerInvisible = false;
        }

        _startPosition = _player.transform.position;

        while (_target == null)
        {

            StartCoroutine(SearchingEnemiesWithDebuff());

            //if (Input.GetMouseButtonDown(0))
            //{
            //    _target = GetRaycastTarget();
            //    Debug.Log($"GrabTongue / PrepareJob / _target == {_target}");
            //    _mousePosition = GetMousePoint();

            //    if (Vector3.Distance(_startPosition, _mousePosition) <= Radius)
            //    {
            //        _endPosition = _mousePosition;
            //    }
            //}
            yield return null;
        }
        Debug.LogError("DataError");
    }

    protected override IEnumerator CastJob()
    {
        if (_target != null && _isCanAttract)
        {
            Debug.Log($"GrabTongue / CastJob / _target == {_target}");
            CreateTongueProjectile(_target, _startPosition, _endPosition);
        }
        yield return null;
    }

    private IEnumerator SearchingEnemiesWithDebuff()
    {
        while (true)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, Radius, _targetsLayers);
            foreach (Collider2D enemy in hits)
            {
                if (enemy != null)
                {
                    _target = enemy.gameObject.GetComponent<Character>();
                    _endPosition = _target.transform.position;

                    if (_target.CharacterState.CheckForState(States.InAir))
                    {
                        Debug.Log("CheckForState in SearchingEnemies");
                        _isCanAttract = true;
                    }
                    else
                    {
                        Debug.Log("IsCanAttract = false");
                        _isCanAttract = false;
                    }
                }
            }
            yield return null;
        }
    }

    private bool CheckCanCast()
    {
        if (_target != null)
            return Vector3.Distance(_player.transform.position, _target.transform.position) <= Radius;

        else 
            return false;
    }

    private void CreateTongueProjectile(Character target, Vector3 startPosition, Vector3 endPosition)
    {
        Debug.Log("CreateTongueProjectile");
        CmdCreateTongueProjectile(target, startPosition, endPosition, _isPlayerInvisible);
    }

    [Command]
    private void CmdCreateTongueProjectile(Character target, Vector3 startPosition, Vector3 endPosition, bool isPlayerInvisible)
    {
        Debug.Log("CmdCreateTongueProjectile");
        GameObject item = Instantiate(_tongueProjectile.gameObject, transform.position, Quaternion.identity);
        GrabTongueProjectile tongueProjectile = item.GetComponent<GrabTongueProjectile>();

        SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

        Debug.Log($"CmdCreate // Projectile = {tongueProjectile}, target = {target}");
        tongueProjectile.InitializationProjectile(_player, target, startPosition, endPosition, isPlayerInvisible);
        tongueProjectile.StartTongueAttract();

        NetworkServer.Spawn(item);

        RpcInitializationProjectile(tongueProjectile.gameObject, target, startPosition, endPosition, isPlayerInvisible);
    }

    [ClientRpc]
    private void RpcInitializationProjectile(GameObject projectile, Character target, Vector3 startPosition, Vector3 endPosition, bool isPlayerInvisible)
    {
        Debug.Log($"RpcCreate //Projectile = {projectile}, target = {target}");

        if (target && projectile != null)
        {
            Debug.Log("Rpc // after If");
            projectile.GetComponent<GrabTongueProjectile>().InitializationProjectile(_player, target, startPosition, endPosition, isPlayerInvisible);
            projectile.GetComponent<GrabTongueProjectile>().StartTongueAttract();
        }
    }
}
