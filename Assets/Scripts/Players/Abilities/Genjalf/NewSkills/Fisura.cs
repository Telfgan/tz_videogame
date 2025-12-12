using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gangdollarff
{
    public class Fisura : Skill, IGodLightSpell
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private FisuraTile _fisuraPref;
        [SerializeField] private float _fisuraDuration = 6;
        [SerializeField, Range(1, 7)] private int _fisuraMaxLenght;

        private Vector3 _startPoint = Vector3.zero;
        private Vector3 _endPoint = Vector3.zero;
        private FisuraTile _fisuraTail;
        private float _tempCastDeley = 1;

        public override string AdditionalDescription =>
            $"Длительность: {AbilityNameBox.ColorOpen}{_fisuraDuration} сек{AbilityNameBox.ColorEnd}";

        protected override int AnimTriggerCastDelay => Animator.StringToHash("FisuraCast");

        protected override int AnimTriggerCast => Animator.StringToHash("Fisura");

        protected override bool IsCanCast => CheckCanCast();

        public bool IsEnabled { get; set; }

        private bool CheckCanCast()
        {
            return Vector3.Distance(_startPoint, transform.position) <= Radius;
        }

        public void AnimCastFisura()
        {
            AnimStartCastCoroutine();
        }

        public void AnimFisuraEnd()
        {
            AnimCastEnded();
        }

        public override void LoadTargetData(TargetInfo targetInfo)
        {
            _startPoint = targetInfo.Points[0];
            _endPoint = targetInfo.Points[1];
        }

        public void ChangeMode()
        {
            if (IsEnabled)
            {
                IsEnabled = false;

                _castDeley = _tempCastDeley;
            }
            else
            {
                IsEnabled = true;

                _tempCastDeley = _cooldownTime;
                _cooldownTime = 0;
            }
        }

        protected override IEnumerator CastJob()
        {
            CmdUse(_startPoint, _endPoint);
            yield return null;
        }

        protected override void ClearData()
        {
            _lineRenderer.positionCount = 0;
            _startPoint = Vector3.zero;
            _endPoint = Vector3.zero;
        }

        protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
        {
            TargetInfo targetInfo = new();
            //_lineRenderer.transform.parent = null;
            //_lineRenderer.positionCount = 2;

            while (targetInfo.Points.Count != 1)
            {
                if (GetMouseButton)
                    targetInfo.Points.Add(GetMousePoint());

                yield return null;
            }
            //_lineRenderer.SetPosition(0, _startPoint + Vector3.up / 10);
            yield return new WaitForSeconds(0.1f);

            while (targetInfo.Points.Count != 2)
            {
                if (Input.GetMouseButton(0))
                    targetInfo.Points.Add(GetMousePoint());

                //_lineRenderer.SetPosition(1, GetMousePoint() + Vector3.up / 10);
                yield return null;
            }
            callbackDataSaved.Invoke(targetInfo);
            _lineRenderer.positionCount = 0;
            yield return null;
        }

        [Command]
        private void CmdUse(Vector3 startPoint, Vector3 endPoint)
        {
            GameObject item = Instantiate(_fisuraPref.gameObject, startPoint, Quaternion.identity);

            SceneManager.MoveGameObjectToScene(item, _hero.NetworkSettings.MyRoom);

            NetworkServer.Spawn(item);

            _fisuraTail = item.GetComponent<FisuraTile>();

            _fisuraTail.SetStartPosition(startPoint);
            _fisuraTail.SetEndPosition(endPoint);

            _fisuraTail.Build();

            StartCoroutine(DurationJob());
        }

        private IEnumerator DurationJob()
        {
            yield return new WaitForSecondsRealtime(_fisuraDuration);
            NetworkServer.Destroy(_fisuraTail.gameObject);
        }
    }
}
