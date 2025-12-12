using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowsIntoSkyProjectile : NetworkBehaviour
{
    [SerializeField] private float impactLifeTime = 2;
    [SerializeField] private float nextDamageTime = 1;

    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject circle;
    [SerializeField] private SphereCollider sphereCollider;

    [SerializeField] private bool silenceTalentActive;
    [SerializeField] private bool lastStreamTalent;
    [SerializeField] private bool shotAstralManaActive;

    [SerializeField] private bool isDamage;

    private HeroComponent _dad;
    private Skill _skill;
    private Character _character;
    private float _damage;

    private readonly HashSet<Collider> _damagedThisTick = new();

    public GameObject Arrow { get => arrow; set => arrow = value; }
    public GameObject Circle { get => circle; set => circle = value; }

    public virtual void Init(HeroComponent dad, Skill skill, float damage, bool silenceTalentActive, bool lastStreamTalent, bool shotAstralManaActive)
    {
        this.silenceTalentActive = silenceTalentActive;
        this.lastStreamTalent = lastStreamTalent;
        this.shotAstralManaActive = shotAstralManaActive;

        _dad = dad;
        _skill = skill;
        _damage = damage;

        if (_dad != null && _dad.TryGetComponent<Character>(out Character character)) _character = character;
    }

    public void Activate()
    {
        Arrow.SetActive(true);
        circle.SetActive(true);
        Invoke("ActiveCollider", nextDamageTime);
        Destroy(gameObject, impactLifeTime);
    }

    private void ActiveCollider() => sphereCollider.enabled = true;

    [Server]
    private void OnTriggerStay(Collider other)
    {
        //if (other.gameObject == _dad.gameObject) return;
        if (((1 << other.gameObject.layer) & _skill.TargetsLayers.value) == 0) return;
        if (!_damagedThisTick.Add(other)) return;

        TryApplyDamageAndEffects(other);
    }

    #region ApplyAdditionalDamage(Old)
    //private void ApplyAdditionalDamage(float damageValue)
    //{
    //    foreach (var enemyCollider in enemyColliders)
    //    {
    //        if (enemyCollider.TryGetComponent<IDamageable>(out IDamageable target) && enemyCollider != _character.gameObject)
    //        {
    //            ApplyDamage(damageValue, DamageType.Magical, target);

    //            if (enemyCollider.TryGetComponent<Character>(out Character character))
    //            {
    //                var targetState = character.CharacterState;

    //                if (targetState != null)
    //                {
    //                    targetState.AddState(States.Irradiation, 9, 0, _character.gameObject, this.name);

    //                    if (shotAstralManaActive && targetState.CheckForState(States.Astral)) RestoreMana();
    //                    if (targetState.CheckForState(States.Silent) && silenceTalentActive) targetState.AddState(States.WeakeningSilence, 4, 4, _character.gameObject, this.name);
    //                }
    //            }
    //        }
    //    }

    //    //foreach (var objectCollider in objectColliders)
    //    //{
    //    //    if (objectCollider.TryGetComponent<ReconnaissanceFireAura>(out ReconnaissanceFireAura aura) && tripleShotTalentActive)
    //    //        if (FindObjectOfType<NatureTalent_6>() != null && !_tripleShot) StartCoroutine(SpawnAdditionalDamageZones(aura));
    //    //}
    //}
    #endregion

    #region ApplyDamageToEnemiesInZone(Old)
    //private void ApplyDamageToEnemiesInZone(Collider collider)
    //{
    //    foreach (var enemyCollider in enemyColliders)
    //    {
    //        if (enemyCollider.TryGetComponent<IDamageable>(out IDamageable target) && enemyCollider != _character.gameObject)
    //        {
    //            ApplyDamage(_damage, DamageType.Magical, target);

    //            if (enemyCollider.TryGetComponent<Character>(out Character character))
    //            {
    //                var targetState = character.CharacterState;

    //                if (targetState != null)
    //                {
    //                    targetState.AddState(States.Irradiation, 9, 0, _character.gameObject, this.name);

    //                    if (shotAstralManaActive && targetState.CheckForState(States.Astral)) RestoreMana();

    //                    if (targetState.CheckForState(States.Silent) && silenceTalentActive) targetState.AddState(States.WeakeningSilence, 4, 4, _character.gameObject, this.name);
    //                }
    //            }
    //        }
    //    }

    //    //foreach (var objectCollider in objectColliders)
    //    //{
    //    //    if (objectCollider.TryGetComponent<ReconnaissanceFireAura>(out ReconnaissanceFireAura aura) && tripleShotTalentActive)
    //    //    {
    //    //        if (FindObjectOfType<NatureTalent_6>() != null && !_tripleShot)
    //    //        {
    //    //            _tripleShot = true;
    //    //            StartCoroutine(SpawnAdditionalDamageZones(aura));
    //    //        }
    //    //    }
    //    //}

    //    //if (!_tripleShot) StopDamageZone();
    //}
    #endregion

    private void TryApplyDamageAndEffects(Collider colldier)
    {
        if (colldier.TryGetComponent<IDamageable>(out var dmgTarget))
            ApplyDamage(_damage, DamageType.Magical, dmgTarget);

        if (colldier.TryGetComponent<Character>(out var victim))
            ApplyStatesAndTalents(victim);
    }

    private void ApplyStatesAndTalents(Character character)
    {
        CharacterState characterState = character.CharacterState;
        if (characterState == null) return;

        if (lastStreamTalent) characterState.AddState(States.InnerDarkness, 13, 0, _character.gameObject, name);
        characterState.AddState(States.Irradiation, 9, 0, _character.gameObject, name);

        if (shotAstralManaActive && characterState.CheckForState(States.Astral))
            RestoreMana();

        if (silenceTalentActive && characterState.CheckForState(States.Silent)) characterState.AddState(States.WeakeningSilence, 4, 3, _character.gameObject, name);
    }

    private void ApplyDamage(float damage, DamageType damageType, IDamageable target)
    {
        Damage _damage = new Damage
        {
            Value = damage,
            Type = damageType,
            PhysicAttackType = AttackRangeType.RangeAttack,
        };

        if (target is Component targetComponent)
        {
            _skill.ApplyDamage(_damage, targetComponent.gameObject);
            //CmdApplyDamage(targetComponent.gameObject, _damage, null);
        }
    }

    private void RestoreMana()
    {
        if (_character.TryGetResource(ResourceType.Mana) is Mana manaResource)
        {
            float manaToRestore = manaResource.MaxValue * 0.03f;
            manaResource.Add(manaToRestore);
            _character.CharacterState.AddState(States.ManaRegen, 1, 0, _character.gameObject, this.name);
        }
    }

    //private IEnumerator SpawnAdditionalDamageZones(ReconnaissanceFireAura aura)
    //{
    //    yield return new WaitForSeconds(1f);
    //    ApplyAdditionalDamage(Damage / 2);

    //    if (aura.StateDark)
    //    {
    //        yield return new WaitForSeconds(1f);
    //        ApplyAdditionalDamage(Damage / 4);
    //        _tripleShot = false;
    //        StopDamageZone();
    //        yield break;
    //    }

    //    _tripleShot = false;
    //    StopDamageZone();
    //    yield break;
    //}
}
