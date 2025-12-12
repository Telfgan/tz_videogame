using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReconnaissanceFireAura : NetworkBehaviour
{
    [SerializeField] private float partialBlindnessDuration = 5f;
    [SerializeField] private float innerDarknessDuration = 13;
    [SerializeField] private GameObject fireEffect;
    [SerializeField] private GameObject fireEffectDark;
    [SerializeField] private bool fireDarkTalent;
    [SerializeField] private bool partialBlindnessTalent;
    [SerializeField] private FlameLightPulse flameLightPulse;
    [SerializeField] private LayerMask characterLayer;

    public event Action<bool> OnStateDarkTalentChanged;

    private readonly List<Character> charactersInZone = new();
    private readonly HashSet<uint> clientIds = new();
    private Coroutine effectCoroutine;

    [SyncVar(hook = nameof(OnStateDarkChanged))]
    private bool stateDark;

    public bool FireDarkTalent { get => fireDarkTalent; set => fireDarkTalent = value; }
    public bool PartialBlindnessTalent { get => partialBlindnessTalent; set => partialBlindnessTalent = value; }
    public bool StateDark { get => stateDark; set => stateDark = value; }

    [Server]
    private void RemoveAuthority()
    {
        var id = netIdentity;
        if (id.connectionToClient != null) id.RemoveClientAuthority();
    }

    private void OnDestroy()
    {
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);


        foreach (var character in charactersInZone) ForceExit(character);
        foreach (var id in clientIds.ToArray()) RemoveCharacter(id);

        charactersInZone.Clear();
        clientIds.Clear();
    }

    private void ForceExit(Character character)
    {
        if (character == null) return;
        if (character.TryGetComponent<CharacterState>(out var state) && state.GetState(States.FireFlash) is FireFlash flash) flash.SwitchToFinite();
    }

    private void OnStateDarkChanged(bool oldValue, bool newValue)
    {
        SwitchEffectFire();
        OnStateDarkTalentChanged?.Invoke(newValue);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & characterLayer.value) == 0) return;

        if (other.TryGetComponent<Character>(out Character character) && !charactersInZone.Contains(character))
        {
            charactersInZone.Add(character);
            RpcAddCharacter(character.netId);
            if (effectCoroutine == null) effectCoroutine = StartCoroutine(ApplyPartialBlindnessPeriodically());
        }
    }

    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & characterLayer.value) == 0) return;

        if (other.TryGetComponent<Character>(out Character character))
        {
            charactersInZone.Remove(character);
            ForceExit(character);
            RpcRemoveCharacter(character.netId);


            if (charactersInZone.Count == 0 && effectCoroutine != null)
            {
                StopCoroutine(effectCoroutine);
                effectCoroutine = null;
            }
        }
    }

    private IEnumerator ApplyPartialBlindnessPeriodically()
    {
        var wait = new WaitForSeconds(1f);

        while (charactersInZone.Count > 0)
        {
            foreach (Character character in charactersInZone)
            {
                if (character == null || !character.TryGetComponent(out CharacterState state)) continue;

                if (stateDark && fireDarkTalent)
                {
                    state.AddState(States.FireFlash, 9999, 0f, gameObject, name);
                    var flash = state.GetState(States.FireFlash) as FireFlash;

                    if (UnityEngine.Random.Range(0, 100) < flash.Chance)
                    {
                        Debug.Log($"Chance: {flash.Chance}");
                        state.AddState(States.InnerDarkness, innerDarknessDuration, 0f, gameObject, "ReconnaissanceFireAuraDark");
                    }
                    continue;
                }

                if (partialBlindnessTalent) state.AddState(States.PartialBlindness, partialBlindnessDuration, 0f, gameObject, "partialBlindnessTalent");
                else state.AddState(States.PartialBlindness, partialBlindnessDuration, 0f, gameObject, "ReconnaissanceFireAura");
            }

            yield return wait;
        }

        effectCoroutine = null;
    }

    public void ApplyFireWorshipperTalentEffect(bool isActive)
    {
        if (isActive)
        {
            transform.localScale += Vector3.one;
            if (fireEffect != null) fireEffect.transform.localScale += Vector3.one;
            if (fireEffectDark != null) fireEffectDark.transform.localScale += Vector3.one;
            if (this.TryGetComponent<VisionComponent>(out VisionComponent vision)) vision.VisionRange += 1;

            if (flameLightPulse != null)
            {
                flameLightPulse.FlameLight.range += 1;
                Vector3 position = flameLightPulse.transform.position;
                position.y -= 1f;
                flameLightPulse.transform.position = position;
            }
        }
    }

    public void SwitchEffectFire()
    {
        fireEffect.SetActive(false);
        fireEffectDark.SetActive(true);
    }

    [ClientRpc]
    private void RpcAddCharacter(uint netId)
    {
        if (!NetworkClient.spawned.TryGetValue(netId, out var id)) return;
        if (!clientIds.Add(netId)) return;
    }

    [ClientRpc] private void RpcRemoveCharacter(uint netId) => RemoveCharacter(netId);

    private void RemoveCharacter(uint netId)
    {
        if (!clientIds.Remove(netId)) return;

        if (NetworkClient.spawned.TryGetValue(netId, out var id) &&
            id.TryGetComponent(out CharacterState state))
        {
            (state.GetState(States.FireFlash) as FireFlash)?.SwitchToFinite();
        }
    }
}