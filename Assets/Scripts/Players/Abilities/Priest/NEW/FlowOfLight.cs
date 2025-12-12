using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
public class FlowOfLight : Skill
{
    [Header("Flow Light Settings")]
    [SerializeField] private float buffDuration = 18f;
    [SerializeField] private GameObject effectPrefabLight;
    [SerializeField] private AbilityInfo lightInfo;

    [Header("Flow Dark Settings")]
    [SerializeField] private float debuffDuration = 18f;
    [SerializeField] private GameObject effectPrefabDark;
    [SerializeField] private AbilityInfo darkInfo;

    [SerializeField] private StunMagicPassiveSkill stunMagicPassiveSkill;

    [SyncVar(hook = nameof(OnModeChanged))] public bool isLightMode = true;
    public event Action OnModeChange;

    private GameObject _activeEffect;
    //private IDamageable _target;
   // private Character _targetCharacter;

    private bool IsAllyTarget(Character target) => target != null && target.gameObject.layer == LayerMask.NameToLayer("Allies");
    private bool IsEnemyTarget(Character target) => target != null && target.gameObject.layer == LayerMask.NameToLayer("Enemy");

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("FlowSpellStart");

    #region Talent
    private bool _spiritEnergyAddTalent;
    public void SpiritEnergyAddTalent(bool value) => _spiritEnergyAddTalent = value;
    #endregion

    protected override bool IsCanCast =>
		GetTargetCharacter() != null &&
        Vector3.Distance(GetTargetCharacter().transform.position, transform.position) <= Radius &&
        NoObstacles(GetTargetCharacter().transform.position, transform.position, _obstacle) &&
        ((isLightMode && IsAllyTarget(GetTargetCharacter())) || (!isLightMode && IsEnemyTarget(GetTargetCharacter())));

    private void OnEnable()
    {
        OnModeChange += UpdateMode;
        OnSkillCanceled += HandleSkillCanceled;
        UpdateMode();
    }

    private void OnDisable()
    {
        OnModeChange -= UpdateMode;
        OnSkillCanceled -= HandleSkillCanceled;
    }

    public void FlowLightCast() => AnimStartCastCoroutine();
    public void FlowLightthEnd() => AnimCastEnded();

    public void MoveFlowLight()
    {
        _hero.Move.CanMove = false;
        _hero.Move.StopMoveAndAnimationMove();
    }

    public void SwitchMode()
    {
        CmdSwitchMode();
    }

    private void HandleSkillCanceled()
    {
        if (_hero != null && _hero.Move != null)
        {
            Hero.Move.CanMove = true;
        }
    }

    private void OnModeChanged(bool oldValue, bool newValue)
    {
        //UpdateMode();
        OnModeChange?.Invoke();
    }

    private void UpdateMode()
    {
        School = isLightMode ? Schools.Light : Schools.Dark;
        AbilityInfoHero = isLightMode ? lightInfo : darkInfo;
    }

    private void ApplySpiritBuff(Character target)
    {
        if (!_spiritEnergyAddTalent || target == null) return;

        var stateComponent = target.GetComponent<CharacterState>();
        if (stateComponent == null) return;

        if (isLightMode) CmdStateSpiritEnergyOrHealth(stateComponent, States.SpiritEnergy, buffDuration);
        else CmdStateSpiritEnergyOrHealth(stateComponent, States.SpiritHealth, debuffDuration);
    }

    private void TryApplyExtraState(Character target)
    {
        if (!stunMagicPassiveSkill.IsFillingDestruction || target == null) return;

        var stateComponent = target.GetComponent<CharacterState>();
        if (stateComponent == null) return;

        if (!isLightMode && UnityEngine.Random.value <= 0.2f) CmdStateRestorationOrDestruction(stateComponent, States.Destruction, 12f);
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        //_targetCharacter = null;

        while (GetTargetCharacter() == null)
        {
            if (GetMouseButton)
            {
                FindTargetCharacter();
                //_target = GetRaycastTarget();
                /*if (_target != null && _target is Character character)
                {
                    if ((isLightMode && IsAllyTarget(character)) || (!isLightMode && IsEnemyTarget(character)))
                    {
                        _targetCharacter = character;
                        _targetCharacter.SelectedCircle.IsActive = true;
                    }
                }*/
            }
            yield return null;
        }

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.AddTarget(GetTargetCharacter());
        callbackDataSaved(targetInfo);
    }


    protected override IEnumerator CastJob()
    {
        if (GetTargetCharacter() == null || !IsCanCast)
        {
            TryCancel();
            yield break;
        }

        TryPayCost();
        CmdSpawnEffect(gameObject, GetTargetCharacter().gameObject);

        float elapsed = 0f;
        float interval = 1f;
        float tickValue = 8f;

        var manaResource = Hero.TryGetResource(ResourceType.Mana);
        Vector3 initialPosition = transform.position;
        float maxMoveDistance = 0.5f;

        while (elapsed < CastStreamDuration)
        {
            if (GetTargetCharacter() == null || !GetTargetCharacter().gameObject.activeSelf ||
                Input.GetMouseButtonDown(1) ||
                Vector3.Distance(transform.position, GetTargetCharacter().transform.position) > Radius ||
                Vector3.Distance(transform.position, initialPosition) > maxMoveDistance ||
                (manaResource != null && manaResource.CurrentValue < 1f))
            {

                _hero.Animator.ResetTrigger(AnimTriggerCast);
                _hero.NetworkAnimator.ResetTrigger(AnimTriggerCast);

                CmdCrossFade();
                _hero.Animator.CrossFade("FlowSpellEnd", 0.1f);

                TryCancel();
                CmdDestroyEffect();
                yield break;
            }

            if (elapsed % interval < Time.deltaTime)
            {
                if (isLightMode && IsAllyTarget(GetTargetCharacter()))
                {
                    Heal heal = new Heal { Value = tickValue };
                    CmdApplyHeal(heal, GetTargetCharacter().gameObject, this, Name);
                    TryApplyExtraState(GetTargetCharacter());
                    ApplySpiritBuff(GetTargetCharacter());
                }
                else if (!isLightMode && IsEnemyTarget(GetTargetCharacter()))
                {
                    Damage damage = new Damage
                    {
                        Value = tickValue,
                        Type = DamageType,
                        School = School
                    };
                    CmdApplyDamage(damage, GetTargetCharacter().gameObject);
                    TryApplyExtraState(GetTargetCharacter());
                    ApplySpiritBuff(GetTargetCharacter());
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        _hero.Animator.ResetTrigger(AnimTriggerCast);
        _hero.NetworkAnimator.ResetTrigger(AnimTriggerCast);

        CmdCrossFade();
        _hero.Animator.CrossFade("FlowSpellEnd", 0.1f);
        CmdDestroyEffect();
    }

    protected override void ClearData()
    {
        ClearTarget();
        //_target = null;
        _hero.Move.StopLookAt();
        CmdDestroyEffect();
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.GetTargets().Count > 0)
            SetTarget((Character)targetInfo.GetTargets()[0]);
    }

    [Command] private void CmdCrossFade() => _hero.Animator.CrossFade("FlowSpellEnd", 0.1f);

    [Command]
    private void CmdSwitchMode()
    {
        UpdateMode();
        isLightMode = !isLightMode;
    }

    [Command]
    private void CmdSpawnEffect(GameObject start, GameObject end)
    {
        if (effectPrefabDark == null || effectPrefabLight == null || start == null || end == null) return;

        GameObject effectInstance = null;

        if (!isLightMode) effectInstance = Instantiate(effectPrefabDark, start.transform.position, Quaternion.identity);
        else effectInstance = Instantiate(effectPrefabLight, start.transform.position, Quaternion.identity);

        SceneManager.MoveGameObjectToScene(effectInstance, _hero.NetworkSettings.MyRoom);
        NetworkServer.Spawn(effectInstance);

        _activeEffect = effectInstance;

        RpcInitEffect(effectInstance, start, end);
    }

    [Command]
    private void CmdDestroyEffect()
    {
        if (_activeEffect != null)
        {
            NetworkServer.Destroy(_activeEffect);
            _activeEffect = null;
        }
    }

    [Command]
private void CmdStateRestorationOrDestruction(NetworkIdentity targetNetIdentity, States states, float duration)
{
    if (targetNetIdentity == null) return;

    var stateComponent = targetNetIdentity.GetComponent<CharacterState>();
    if (stateComponent == null) return;

    stateComponent.AddState(states, duration, 0, gameObject, Name);
}
    [Command] private void CmdStateRestorationOrDestruction(CharacterState stateComponent, States states, float duration) => ClientRpcStateRestorationOrDestruction(stateComponent, states, duration);
    [Command] private void CmdStateSpiritEnergyOrHealth(CharacterState stateComponent, States states, float duration) => ClientRpcSpiritEnergyOrHealth(stateComponent, states, duration);

    [ClientRpc] private void ClientRpcSpiritEnergyOrHealth(CharacterState stateComponent, States states, float duration) { stateComponent.AddStateLogic(states, duration, 1f, Schools.None, gameObject, Name); }
    [ClientRpc] private void ClientRpcStateRestorationOrDestruction(CharacterState stateComponent, States states, float duration) { stateComponent.AddStateLogic(states, duration, 0, Schools.None, gameObject, "FlowOfLight"); }


        [ClientRpc]
    private void RpcInitEffect(GameObject effect, GameObject start, GameObject end)
    {
        if (effect == null) return;

        FlowLightEffect[] flows = effect.GetComponentsInChildren<FlowLightEffect>(true);
        foreach (var flow in flows)
        {
            flow.Initialize(start, end);
            flow.Activate();
        }

        if (flows.Length == 0)
        {
            Debug.LogWarning("FlowLightEffect не найден ни на одном дочернем объекте эффекта: " + effect.name);
        }
    }
}