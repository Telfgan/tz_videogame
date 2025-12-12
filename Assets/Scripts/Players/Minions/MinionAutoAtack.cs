//using Mirror;
//using System.Collections;
//using UnityEngine;

//public class MinionAutoAtack : AutoAttackSkill
//{
//    [SerializeField] private MinionArrowProjectile projectile;
//    [SerializeField] private MinionComponent minionLinks;
//    [SerializeField] private Transform spawnPoint;

//    private bool _isDelayActive = false;
//    private Coroutine _autoAttackCoroutine;

//    protected override int AnimTriggerAutoAttack => 0;
//    protected override int AnimTriggerCastDelay => 0;
//    protected override bool IsCanCast => true;

//    private void Start()
//    {
//        _autoAttackCoroutine = StartCoroutine(AutoAttackRoutine());
//    }

//    private void OnDestroy()
//    {
//        if (_autoAttackCoroutine != null)
//        {
//            StopCoroutine(_autoAttackCoroutine);
//        }
//    }

//    private IEnumerator AutoAttackRoutine()
//    {
//        while (true)
//        {
//            if (IsAutoattackMode && !_isDelayActive)
//            {
//                if (TryGetClosestTarget(out var target))
//                {
//                    _hero.Animator.SetTrigger(AnimTriggerCastDelay);
//                    _hero.NetworkAnimator.SetTrigger(AnimTriggerCastDelay);

//                    yield return new WaitForSeconds(0.2f);

//                    CmdCreateProjectileAtTarget(target);
//                    _isDelayActive = true;

//                    yield return new WaitForSeconds(AttackDelay);
//                    _isDelayActive = false;
//                }
//            }
//            yield return null;
//        }
//    }

//    private bool TryGetClosestTarget(out Character target)
//    {
//        Collider[] hits = Physics.OverlapSphere(transform.position, Radius, _targetsLayers);
//        target = null;
//        float minDistance = Mathf.Infinity;

//        foreach (var hit in hits)
//        {
//            if (hit.TryGetComponent(out Character character))
//            {
//                float distance = Vector3.Distance(transform.position, character.transform.position);
//                if (distance < minDistance)
//                {
//                    minDistance = distance;
//                    target = character;
//                }
//            }
//        }
//        return target != null;
//    }

//    [Command]
//    private void CmdCreateProjectileAtTarget(Character target)
//    {
//        if (target == null) return;

//        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
//        Vector3 direction = (target.transform.position - spawnPosition).normalized;

//        MinionArrowProjectile arrow = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(direction));
//        arrow.Init(minionLinks, 0, false, this);
//        NetworkServer.Spawn(arrow.gameObject);
//        arrow.StartFly(direction);
//        RpcInit(arrow.gameObject);
//    }

//    [ClientRpc]
//    private void RpcInit(GameObject projectileObject)
//    {
//        if (projectileObject == null) return;

//        MinionArrowProjectile arrow = projectileObject.GetComponent<MinionArrowProjectile>();
//        if (arrow != null)
//        {
//            arrow.Init(minionLinks, 0, false, this);
//        }
//    }

//    protected override void CastAction()
//    {
//        throw new System.NotImplementedException();
//    }
//}