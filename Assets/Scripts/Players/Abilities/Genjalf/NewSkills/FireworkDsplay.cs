using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gangdollarff
{
    public class FireworkDsplay : Skill
    {
        [SerializeField] private Firework _firework;
        [SerializeField] private float _damageRangeMin = -2;
        [SerializeField] private float _damageRangeMax = 1;

        private List<float> _damageForTarget = new List<float>() { 1, .75f, .50f, .25f };

        private Vector3 _targetPoint = Vector3.positiveInfinity;
        //private Character _target;

        protected override int AnimTriggerCastDelay => 0;

        protected override int AnimTriggerCast => 0;

        protected override bool IsCanCast { get => CheckCanCast(); }

        private bool CheckCanCast()
        {
            return true;
        }

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            SetTarget((Character)targetInfo.GetTargets()[0]);
            _targetPoint = targetInfo.Points[0];
        }

        protected override IEnumerator CastJob()
        {
            DisableMove();

            float time = 0;
            //_firework.gameObject.SetActive(true);
            CmdSetActiveParticle(true);
            Hero.Move.RotateModifier = -700;

            while (time < CastStreamDuration)
            {
                yield return new WaitForSeconds(_manaCostRate);

                int count = 0;
                _firework.SortDamageablesByDistance(transform.position);

                foreach (var item in _firework.Damageables)
                {
                    if (((1 << item.gameObject.layer) & TargetsLayers) != 0)
                    {
                        if (item.TryGetComponent<IDamageable>(out IDamageable enemy) && count < 4)
                        {
                            float currentDamage = UnityEngine.Random.Range(Damage + _damageRangeMin, Damage + _damageRangeMax) * _damageForTarget[count];
                            count++;

                            Damage damage = new Damage
                            {
                                Value = Buff.Damage.GetBuffedValue(currentDamage),
                                Type = DamageType,
                                PhysicAttackType = AttackRangeType,
                            };

                            CmdApplyDamage(damage, item.gameObject);
                        }
                    }
                }
                time += _manaCostRate;
                yield return null;
            }
            ClearData();
        }

        protected override void ClearData()
        {
            Hero.Move.RotateModifier = 0;
            EnableMove();
            _firework.gameObject.SetActive(false);
            CmdSetActiveParticle(false);

            ClearTarget();
           // _target = null;
            _targetPoint = Vector3.positiveInfinity;
        }

        protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
        {
            while (float.IsPositiveInfinity(_targetPoint.x) && GetTargetCharacter() == null)
            {
                if (GetMouseButton)
                {
                    FindTargetCharacter();
                    //_target = GetTarget().character;
                    _targetPoint = GetTargetCharacter().Position;

                   // _target = GetRaycastTarget();
                    _targetPoint = GetMousePoint();
                }
                yield return null;
            }
            TargetInfo targetInfo = new();
            targetInfo.AddTarget(GetTargetCharacter());
            targetInfo.Points.Add(_targetPoint);
            callbackDataSaved(targetInfo);
        }

        private void EnableMove()
        {
            Hero.Animator.SetTrigger(HashAnimPlayer.AnimCancled);
            Hero.Move.IsMoveBlocked = false;
        }

        private void DisableMove()
        {
            Hero.Animator.SetTrigger("Fire");
            Hero.Move.IsMoveBlocked = true;
        }

        [Command]
        private void CmdSetActiveParticle(bool status)
        {
            ClientRpcSetActiveParticle(status);
        }

        [ClientRpc]
        private void ClientRpcSetActiveParticle(bool status)
        {
            _firework.gameObject.SetActive(status);
        }
    }
}

