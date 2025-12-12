using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BlockPassiveSkill : Skill, IPassiveSkill
{
    [SerializeField] private float durationWindowsBoost = 2f;
    [SerializeField] private float blockChance = 50;

    private Coroutine _boostWindow;
    private bool _isCooldownActive = false;
    private Character _attacker;
    //private Character _target;
    private List<Character> _validAttackers = new();

    #region Skill
    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => 0;
    public override void LoadTargetData(TargetInfo targetInfo) => throw new NotImplementedException();
    protected override IEnumerator CastJob() => null;
    protected override void ClearData() => throw new NotImplementedException();
    protected override IEnumerator PrepareJob(Action<TargetInfo> targetDataSavedCallback) => throw new NotImplementedException();
    #endregion

    private void OnEnable()
    {
        Hero.Health.Block += PlayBlockAnimation;
        Hero.Health.Evaded += OnHeroEvade;
        Hero.Health.OnBeforeTakeDamage += OnBeforeTakeDamage;
    }

    private void OnDisable()
    {
        Hero.Health.Block -= PlayBlockAnimation;
        Hero.Health.Evaded -= OnHeroEvade;
        Hero.Health.OnBeforeTakeDamage -= OnBeforeTakeDamage;
    }

    private void OnHeroEvade()
    {
        if (_boostWindow != null || _attacker == null) return;
        TargetRpcStartBlockPassiveSkillBoostWindow(connectionToClient, _attacker.netId);
    }

    private void OnBeforeTakeDamage(Damage damage, Skill skill)
    {
        if (skill == null || skill.Hero == null) return;

        _attacker = skill.Hero;
        if (!_validAttackers.Contains(_attacker)) Hero.Health.BlockChance = 0f;
    }

    public void TryStartBlockPassiveSkillBoostWindow(Character target)
    {
        if (_isCooldownActive || _boostWindow != null || target == null) return;
        CmdAddAttacker(target);
        _boostWindow = StartCoroutine(BlockPassiveSkillBoostWindow());
    }

    private IEnumerator BlockPassiveSkillBoostWindow()
    {
        if (_boostWindow != null) StopCoroutine(_boostWindow);
        Hero.Health.CmdSetBlockChance(blockChance);
        _isCooldownActive = true;
        Hero.Health.BlockChance = blockChance;
        Disactive = false;

        yield return new WaitForSeconds(durationWindowsBoost);

        Hero.Health.CmdResetBlockChance();
        ResetDisactive();

        yield return new WaitForSeconds(6f);
        _isCooldownActive = false;
    }

    private void PlayBlockAnimation()
    {
        if (isServer) TargetRpcPlayBlockAnimation(Hero.connectionToClient);
        Hero.Health.ResetBlockChance();
        ClientRpcResetDisactive();
    }

    private void ResetDisactive()
    {
        _attacker = null;
        ClearTarget();
        //_target = null;
        Disactive = true;
        _boostWindow = null;
    }

    [ClientRpc] private void ClientRpcResetDisactive() => ResetDisactive();

    [TargetRpc]
    private void TargetRpcStartBlockPassiveSkillBoostWindow(NetworkConnection target, uint attackerNetId)
    {
        if (NetworkClient.spawned.TryGetValue(attackerNetId, out NetworkIdentity identity))
        {
            Character attacker = identity.GetComponent<Character>();
            if (attacker != null) TryStartBlockPassiveSkillBoostWindow(attacker);
        }
    }

    [TargetRpc] private void TargetRpcPlayBlockAnimation(NetworkConnection target) => Hero.Animator.SetTrigger(Animator.StringToHash("BlockTrigger"));

    [Command]
    private void CmdAddAttacker(Character target)
    {
        _validAttackers.Clear();
        _validAttackers.Add(target);
    }    
}