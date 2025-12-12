using Mirror;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SongOfSleep : Skill
{
    [SerializeField] private Character playerLinks;
    [SerializeField] private DrawCircleAlternative drawCircle;
    [SerializeField] private float duration;

    private Coroutine _radiusJob;
    private Vector3 _centerPoint = Vector3.positiveInfinity;
    private bool _isSleepInnerDarknessTalentActive = false;
    
    protected override bool IsCanCast => !IsCasting;

    private static readonly int _animTrigger = Animator.StringToHash("SongSpellCastDelayAnimTrigger");

    protected override int AnimTriggerCastDelay => _animTrigger;
    protected override int AnimTriggerCast => 0;

    public bool IsSleepInnerDarknessTalentActive { get => _isSleepInnerDarknessTalentActive; set => _isSleepInnerDarknessTalentActive = value; }

    public override void LoadTargetData(TargetInfo targetInfo) => _centerPoint = targetInfo.Points.Count > 0 ? targetInfo.Points[0] : playerLinks.transform.position;

    public void SongOfSleepMove()
    {
        _hero.Move.StopMoveAndAnimationMove();
        Hero.Move.CanMove = false;
    }

    private void OnDestroy()
    {
        OnSkillCanceled -= HandleSkillCanceled;
    }

    private void OnEnable()
    {
        OnSkillCanceled += HandleSkillCanceled;
    }

    private void HandleSkillCanceled()
    {
        if (_hero?.Move != null)
        {
            Hero.Move.CanMove = true;
            Hero.Move.StopLookAt();
        }
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        StartRadiusRender();

        while (float.IsPositiveInfinity(_centerPoint.x))
        {
            if (GetMouseButton)
            {
                StopRadiusRender();
                _centerPoint = playerLinks.transform.position;

                yield break;
            }

            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_centerPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (_centerPoint != Vector3.positiveInfinity)
        {
            ApplyStateEnemiesInZone();
            Hero.Move.CanMove = true;
            ClearData();
            yield return null;
        }
    }

    protected override void ClearData()
    {
        _centerPoint = Vector3.positiveInfinity;
        StopRadiusRender();
    }

    private void StartRadiusRender()
    {
        if (drawCircle == null) return;

        drawCircle.Draw(Radius);
        _radiusJob = StartCoroutine(RadiusColorJob());
    }

    private void StopRadiusRender()
    {
        if (_radiusJob != null)
        {
            StopCoroutine(_radiusJob);
            _radiusJob = null;
        }
        drawCircle?.Clear();
    }

    private void ApplyStateEnemiesInZone()
    {
        Collider[] hitColliders = Physics.OverlapSphere(_centerPoint, Radius, TargetsLayers);
        foreach (var hitCollider in hitColliders) if (hitCollider.gameObject != Hero.gameObject) ApplyEnemiesZone(hitCollider);
    }

    private void ApplyEnemiesZone(Collider hitCollider)
    {
        if (hitCollider.TryGetComponent<HeroComponent>(out HeroComponent enemy))
        {
            var targetState = enemy.GetComponent<CharacterState>();
            if (targetState != null) CmdSongOfSleep(targetState);
        }
    }

    [Command] private void CmdSongOfSleep(CharacterState targetState) => targetState.AddState(States.Sleep, duration, 0, Hero.gameObject, this.name);

    private IEnumerator RadiusColorJob()
    {
        var wait = new WaitForSeconds(0.1f);
        while (true)
        {
            bool enemyInside = Physics.OverlapSphere(transform.position, Radius, TargetsLayers).Any(collider => collider.TryGetComponent<Character>(out var character)
            && character != playerLinks);

            drawCircle.SetColor(enemyInside ? Color.green : Color.red);
            yield return wait;
        }
    }

    #region Talent

    public void SleepInnerDarknessTalent(bool value) => _isSleepInnerDarknessTalentActive = value;

    #endregion
}
