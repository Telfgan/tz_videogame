using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gangdollarff
{
    public class AbsorptionBall : Skill, IGodLightSpell
    {
        [SerializeField] private Shield _shieldPref;
        [SerializeField] private float _shieldValue = 40;
        [SerializeField] private float _shieldDuration = 2;

        private float _tempCooldownTime = 5f;

        private Shield _shield;

        public override string AdditionalDescription =>
            $"Ёффективность: {AbilityNameBox.ColorOpen}{_shieldValue} ед.{AbilityNameBox.ColorEnd}" +
            $"\nƒлительность: {AbilityNameBox.ColorOpen}{_shieldDuration} сек{AbilityNameBox.ColorEnd}";

        protected override int AnimTriggerCastDelay => 0;

        protected override int AnimTriggerCast => 0;

        protected override bool IsCanCast => true;

        public float ShieldDuration { get => _shieldDuration; set => _shieldDuration = value; }

        public bool IsEnabled { get; protected set; }

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            
        }

        public void ChangeMode()
        {
            if (IsEnabled)
            {
                IsEnabled = false;

                _cooldownTime = _tempCooldownTime;
            }
            else
            {
                IsEnabled = true;

                _tempCooldownTime = _cooldownTime;
                _cooldownTime = 0;
            }
        }

        protected override IEnumerator CastJob()
        {
            CmdAddShield();
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
        private void CmdAddShield()
        {
            if(_shield != null)
            {
                NetworkServer.Destroy(_shield.gameObject);
            }

            var shield = Instantiate(_shieldPref, transform.position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(shield.gameObject, _hero.NetworkSettings.MyRoom);
            shield.Initialize(_shieldValue, DamageType.Both);
            NetworkServer.Spawn(shield.gameObject);
            _shield = shield;
            Hero.Health.Shields.Add(shield);
            StartCoroutine(ShieldJob());

            ClientRpcShieldFollow(_shield.gameObject);
        }

        [ClientRpc]
        private void ClientRpcShieldFollow(GameObject shield)
        {
            shield.GetComponent<Shield>().FollowTo(transform);
        }

        private IEnumerator ShieldJob()
        {
            yield return new WaitForSecondsRealtime(_shieldDuration);

            if (_shield != null)
            {
                _shield.TryUse(99999);
                NetworkServer.Destroy(_shield.gameObject);
            }
        }
    }
}