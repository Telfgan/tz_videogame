using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gangdollarff
{
    public class ClapOfLight : Skill, IGodLightSpell
    {
        [SerializeField] private ParticleSystem _particle;
        [SerializeField] private float _pushRange = 1;
        [SerializeField] private float _pushDuration = 0.33f;

        public bool IsBaffed = false;

        public override string AdditionalDescription =>
            $"Расстояние толчка: {AbilityNameBox.ColorOpen}{_pushRange}{AbilityNameBox.ColorEnd}";

        protected override int AnimTriggerCastDelay => 0;

        protected override int AnimTriggerCast => 0;

        protected override bool IsCanCast => true;

        public bool IsEnabled { get; protected set; }

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            
        }

        public void ChangeMode()
        {
            IsEnabled = !IsEnabled;
        }

        protected override IEnumerator CastJob()
        {
            var colliders = Physics.OverlapSphere(transform.position, Radius, TargetsLayers);

            Damage damage = new Damage
            {
                Value = Buff.Damage.GetBuffedValue(Damage),
                Type = DamageType,
                PhysicAttackType = AttackRangeType,
            };

            CmdSetActiveParticle(true);

            foreach (var item in colliders)
            {
                if(item.TryGetComponent(out Character enemy))
                {
                    if (IsBaffed)
                        CooldownTime = CooldownTime - 2;

                    CmdApplyDamage(damage, enemy.gameObject);

                    Vector3 dirForPush;
                    Vector3 pointForPush;

                    if (IsEnabled)
                    {
                        dirForPush = (transform.position - enemy.transform.position).normalized;
                        pointForPush = enemy.transform.position + (dirForPush * _pushRange);
                    }
                    else
                    {
                        dirForPush = (enemy.transform.position - transform.position).normalized;
                        pointForPush = enemy.transform.position + (dirForPush * _pushRange);
                    }

                    CmdMoveTaget(enemy.gameObject, pointForPush, _pushDuration);
                }
            }
            yield return null;
        }

        protected override void ClearData()
        {
            
        }

        protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
        {
            yield return null;
        }

        [Command]
        private void CmdMoveTaget(GameObject target, Vector3 point, float time)
        {
            var enemyMove = target.GetComponent<MoveComponent>();
            enemyMove.TargetRpcDoMove(point, time);
        }

        [Command]
        private void CmdSetActiveParticle(bool status)
        {
            ClientRpcSetActiveParticle(status);
        }

        [ClientRpc]
        private void ClientRpcSetActiveParticle(bool status)
        {
            _particle.gameObject.SetActive(status);
        }
    }
}
