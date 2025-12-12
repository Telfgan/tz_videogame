using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AutoAttackSkill : Skill
{
    [Header("AutoAttack settings")]
    [SerializeField] private float _attackZoneSize;
    [SerializeField] protected float _attackDelay = 0f;
    [SerializeField] protected float _chargeAttackDelay;
    
    //protected Character _target;
    private bool _isAutoattackMode = true;
    private Coroutine _autoAttackCoroutine;
    private bool _isAttacking = false;
    private Vector2 _lastTargetPosition;
    private bool _isPlayCastAnimAA;


    float time = 0;
	float duration = 1f;

    Color startColor = Color.green;
    Color endColor = new Color(0, 1, 0, 0f);

	public float AttackDelay { get => Buff.AttackSpeed.GetBuffedValue(_attackDelay); }
    public Character Target { get => GetTargetCharacter(); }
    public Vector2 LastTargetPosition { get => _lastTargetPosition; }
    public override bool IsPayCostStartCooldown { get => false; }
    public bool IsAutoattackMode { get => _isAutoattackMode; set => _isAutoattackMode = value; }
    public Character LastTarget { get; private set; }
    protected override bool IsCanCast
    {
        get
        {
            if (Target == null)
                return false;

            return NoObstacles(Target.transform.position, _obstacle) && IsTargetInRadius(Radius, Target.transform); ;
        }
    }

    protected override int AnimTriggerCast => 0;

    protected abstract int AnimTriggerAutoAttack { get; }

    private void Update()
    {
        if (GetTargetCharacter() == null)
        {
            return;
        }

		if (_isAttacking)
		{
			startColor = Color.green;
			endColor = new Color(0, 1, 0, 0f);
		}
        else
        {
			startColor = Color.red;
			endColor = new Color(1, 0, 0, 0f);
		}
		float t = Mathf.PingPong(Time.time * duration, 1f);
		Color currentColor = Color.Lerp(startColor, endColor, t);
		//_target.SelectedCircle.Circle.color = currentColor;
		_skillRender.DrawRadiusColor(Radius, currentColor);
	}


	protected abstract void CastAction();

    protected override IEnumerator CastJob()
    {
        yield return _autoAttackCoroutine = StartCoroutine(AutoAttackJob());
    }

    protected override void ClearData()
    {
        if (GetTargetCharacter() != null)
        {
			//_target.SelectedCircle.Circle.color = Color.green;
			GetTargetCharacter().SelectedCircle.IsActive = false;
		}
		_skillRender.SetColor(Color.green);

		
        if (_autoAttackCoroutine != null)
        {
            StopCoroutine(_autoAttackCoroutine);
            _autoAttackCoroutine = null;
        }

        _isAttacking = false;
        ClearTarget();
        //_target = null;
        if (Hero.Move.CanMove == false) Hero.Move.CanMove = true;
        _hero.Move.StopLookAt();
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback)
    {
        while (Target == null)
        {
            if (GetMouseButton)
            {
             //   _target = GetRaycastTarget();

                if(GetTargetCharacter() != null)
					GetTargetCharacter().SelectedCircle.IsActive = true;
			}
            yield return null;
        }


        _hero.Move.LookAtTransform(Target.transform);
    }

    public void SwitchAutoMode()
    {
        _isAutoattackMode = !_isAutoattackMode;
    }

    public void Pause()
    {
        if (_autoAttackCoroutine != null)
        {
            StopCoroutine(_autoAttackCoroutine);
            _autoAttackCoroutine = null;
        }
        _isAttacking = false;
    }

    public void Continue()
    {
        if (_autoAttackCoroutine == null && Target != null)
        {
            _autoAttackCoroutine = StartCoroutine(AutoAttackJob());
        }
    }

    protected void AnimCastAction()
    {
        CastAction();
    }

    protected override void AnimCastEnded()
    {
        base.AnimCastEnded();
        _isPlayCastAnimAA = false;
    }

    protected virtual IEnumerator AutoAttackJob()
    {
		while (Target != null)
        {
            if (IsTargetInRadius(Radius + _attackZoneSize, Target.transform))
            {
                if (IsTargetInRadius(Radius, Target.transform))
                    _isAttacking = true;

                if (_isAttacking && NoObstacles(Target.transform.position, _obstacle))
                {
                    _lastTargetPosition = Target.transform.position;
                    LastTarget = GetTargetCharacter();

                    if (_chargeAttackDelay > 0)
                        yield return StartCastDeleyCoroutine(_chargeAttackDelay);

                    //yield return new WaitForSeconds(AttackSpeed);

                    if (IsTargetInRadius(Radius + _attackZoneSize, Target.transform) && NoObstacles(Target.transform.position, _obstacle) && IsCooldowned)
                    {
                        if (TryPayCost(true))
                        {
                            if(AnimTriggerAutoAttack != 0)
                            {
                                _isPlayCastAnimAA = true;
                                Hero.Animator.SetFloat(HashAnimPlayer.CastSpeed, Buff.AttackSpeed.Multiplier);
                                _hero.Animator.SetTrigger(AnimTriggerAutoAttack);
                                _hero.NetworkAnimator.SetTrigger(AnimTriggerAutoAttack);

                                while (_isPlayCastAnimAA)
                                {
                                    if((IsTargetInRadius(Radius + _attackZoneSize, Target.transform) && NoObstacles(Target.transform.position, _obstacle) && IsCooldowned) == false)
                                    {
                                        _hero.Animator.SetTrigger(HashAnimPlayer.AnimCancled);
                                        _hero.NetworkAnimator.SetTrigger(HashAnimPlayer.AnimCancled);
                                        _isPlayCastAnimAA = false;
                                    }
                                    yield return null;
                                }
                            }
                            else
                            {
                                CastAction();
                            }

                            if (_isAutoattackMode == false)
                            {
                                ClearData();
                                yield break;
                            }
                        }
                    }
                    yield return new WaitForSeconds(AttackDelay);
                }
			}
            else
            {
                _isAttacking = false;
            }
            yield return null;
        }
        _autoAttackCoroutine = null;
    }

	Color ColourChanging(float fadeStart, float fadeTime, Color objectColor, Color fadeColor)
	{
		if (fadeStart < fadeTime)
		{
			fadeStart += Time.deltaTime * fadeTime;

			return Color.Lerp(objectColor, fadeColor, fadeStart);
		}
        else
        {
            return Color.Lerp(objectColor, fadeColor, fadeStart);
        }
	}
}
