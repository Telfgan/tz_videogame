using Gangdollarff.EarthElemental;
using Gangdollarff.WaterElemental;
using Mirror;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class StateInfo
{
    public States State;
    public float Duration;
    public float DamageToExit;
    public GameObject PersonWhoShooted;
    public string SkillName;

	public StateInfo(States state, float duration, float damageToExit, GameObject personWhoShooted, string skillName)
    {
        State = state;
        Duration = duration;
        DamageToExit = damageToExit;
        PersonWhoShooted = personWhoShooted;
        SkillName = skillName;
    }
}

public abstract class AbstractCharacterState
{
	protected CharacterState _characterState;
	protected SkillManager _abilities;
	protected Health _health;
	public Character _personWhoMadeBuff;

	public int CurrentStacksCount = 0;
	public int MaxStacksCount = 0;
	public float duration;
	public bool CanStack = true;

	public virtual float RemainingDuration
	{
		get => duration;
		set => duration = value;
	}

	public abstract States State { get; }
	public abstract StateType Type { get; }
	public abstract BaffDebaff BaffDebaff { get; }
	public abstract List<StatusEffect> Effects { get; }
	

	public abstract void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName);
	public abstract void UpdateState();
	public abstract void ExitState();
	public abstract bool Stack(float time);
}

public abstract class AuraState : AbstractCharacterState
{
	protected Character _self;
    private Transform _auraCentre;
    private List<Character> _charactersInRadius = new();
    private List<Collider> _collidersKeysForRemove = new();
	private Dictionary<Collider, Character> _colliderToCharacter = new();
	private float _timeAfterLastEffect = 0;

    public abstract float Distance { get; }
    public abstract float EffectRate { get; }
    public abstract LayerMask LayerMask { get; }

    public abstract void EffectOnEnter(Character character);
    public abstract void EffectOnExit(Character character);
    public abstract void EffectOnStay(List<Character> characters);

    public override StateType Type => StateType.Aura;

    public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
    {
        _characterState = character;
		_auraCentre = character.transform;
		_self = personWhoMadeBuff;
	}

    public override void UpdateState()
    {
        if (NetworkServer.active == false)
        {
            _timeAfterLastEffect += Time.deltaTime;

            if (EffectRate > _timeAfterLastEffect)
				return;

			_timeAfterLastEffect = 0;

            var colliders = Physics.OverlapSphere(_auraCentre.position, Distance, LayerMask);

            foreach (KeyValuePair<Collider, Character> collider in _colliderToCharacter)
			{
				if (colliders.Contains(collider.Key) == false)
				{
                    EffectOnExit(collider.Value);
					_charactersInRadius.Remove(collider.Value);
					_collidersKeysForRemove.Add(collider.Key);
				}
			}
			foreach (var item in _collidersKeysForRemove)
			{
				_colliderToCharacter.Remove(item);
			}
			_collidersKeysForRemove.Clear();

            foreach (var collider in colliders)
			{
				if (_colliderToCharacter.ContainsKey(collider) == false && collider.TryGetComponent(out Character character))
				{
					_colliderToCharacter.Add(collider, character);
					_charactersInRadius.Add(character);
					EffectOnEnter(character);
				}
			}

            EffectOnStay(_charactersInRadius);
        }
    }

    public override void ExitState()
    {
        _characterState.RemoveState(this);
    }

    public override bool Stack(float time)
    {
        return false;
    }
}

public class DefaultState : AbstractCharacterState
{
	private List<StatusEffect> _effects = new List<StatusEffect>();
	public override States State => States.Default;
	public override StateType Type => StateType.Physical;
	public override BaffDebaff BaffDebaff => BaffDebaff.Baff;
	public override List<StatusEffect> Effects => _effects;

	public override void EnterState(CharacterState character, float durationToExit, float damageToExit, Character personWhoMadeBuff, string skillName)
	{

	}

	public override void UpdateState()
	{

	}

	public override void ExitState()
	{

	}

	public override bool Stack(float time)
	{
		return false;
	}
}

public abstract class HealStates : AbstractCharacterState
{
	public float HealingValue { get; set; }
}

public class CharacterState : NetworkBehaviour
{
	private Character _hero;
	private List<AbstractCharacterState> _currentStates = new List<AbstractCharacterState>();
	[SerializeField] private StateIcons _stateIcons;
	[SerializeField] private StateEffects _stateEffects;

	public bool invinsible = false;

	public StateEffects StateEffects => _stateEffects;
	public StateIcons StateIcons => _stateIcons;
	public List<AbstractCharacterState> CurrentStates => _currentStates;
	public Character Character => _hero;
	public event System.Action<AbstractCharacterState> OnStateAdded;

	public Dictionary<States, AbstractCharacterState> enumToState = new Dictionary<States, AbstractCharacterState>()
	{
		[States.Stun] = new StunnedState(),
		[States.Frozen] = new FrozenState(),
		[States.Frosting] = new FrostingState(),
		[States.Cooling] = new Cooling(),
		[States.Blind] = new BlindnessState(),
		[States.Invisible] = new InvisibleState(),
		[States.SchoolDebuff] = new AbilitySchoolDebuff(),
		[States.Desiccuration] = new Desiccuration(),
		[States.Plague] = new Plague(),
		[States.Curse] = new Curse(),
		[States.NorthernerEndurance] = new NorthernerEndurance(),
		[States.LastBreath] = new LastBreath(),
		[States.MagicBuff] = new MagicBuff(),
		[States.DarkShield] = new DarkShield(),
		[States.LightShield] = new LightShield(),
		[States.TiredSoul] = new TiredSoul(),
		[States.ReversePolarity] = new ReversePolarityState(),
		[States.SpiritEnergy] = new SpiritEnergyState(),
		[States.SpiritHealth] = new SpiritHealthState(),
		[States.ScorchedSoul] = new ScorchedSoul(),
		[States.Knockdown] = new Knockdown(),
		[States.IdealEvade] = new IdealEvade(),
		[States.Bleeding] = new BleedingDebuff(),
		[States.EmeraldSkin] = new EmeraldSkinState(),
		[States.DefenseReduction] = new DefenceReductionState(),
		[States.SparkTalentHealthBuff] = new SparkTalentHealthState(),
		[States.SelfHarm] = new SelfHarmState(),
		[States.InAir] = new InAirState(),
		[States.Immateriality] = new ImmaterialityState(),
		[States.CreeperInvisible] = new CreeperInvisibleState(),
		[States.PoisonBone] = new PoisonBoneState(),
		[States.WitheringPoison] = new WitheringPoisonState(),
		[States.BindingPoison] = new BindingPoisonState(),
		[States.PoisonCloud] = new PoisonCloudState(),
		[States.HealingPoisonCloud] = new HealingPoisonCloudState(),
		[States.EmpathicPoisons] = new EmpathicPoisonsState(),
		[States.HealingPoisonPerSecond] = new HealingPoisonPerSecondState(),
		[States.InstantHealingPoison] = new InstantHealingPoisonState(),
		[States.RegeneratingPoison] = new RegeneratingPoisonState(),
		[States.HeatedGlands] = new HeatedGlandsState(),
		[States.AbsorptionOfPoison] = new AbsorptionOfPoisonsState(),
		[States.Bleeding] = new BleedingState(),
		[States.ReducingHealing] = new ReducingHealingState(),
		[States.LowVoltage] = new LowVoltage(),
		[States.ComboState] = new ComboState(),
		[States.DisappointmentState] = new DisappointmentState(),
		[States.ManaRegen] = new ManaRegen(),
		[States.Stupefaction] = new Stupefaction(),
		[States.TentacleGrip] = new TentacleGrip(),
		[States.Restoration] = new RestorationState(),
		[States.Destruction] = new DestructionState(),
		[States.HardenedFlesh] = new HardenedFlesh(),
		[States.FocusingOnReflexesState] = new FocusingOnReflexesState(),
		[States.DivineEnhancement] = new DivineEnhancementState(),

		#region TerrifyingElfStates
		[States.InnerDarkness] = new InnerDarkness(),
		[States.Fear] = new Fear(),
		[States.Astral] = new AstralState(),
		[States.Silent] = new Silent(),
		[States.Irradiation] = new IrradiationState(),
		[States.Suppression] = new SuppressionState(),
		[States.WeakeningSilence] = new WeakeningSilence(),
		[States.PartialBlindness] = new PartialBlindness(),
		[States.Anxiety] = new Anxiety(),
		[States.HuntressMark] = new HuntressMark(),
		[States.Calmness] = new Calmness(),
		[States.Sleep] = new Sleep(),
		[States.ElvenSkill] = new ElvenSkill(),
		[States.Bound] = new Bound(),
		[States.ShadowTree] = new ShadowTree(),
		[States.MultiMagic] = new MultiMagic(),
		[States.FireFlash] = new FireFlash(),
		[States.WarmingUpState] = new WarmingUpState(),
		[States.HealingSlime] = new HealingSlime(),
		#endregion

		#region Gandollarf	
		[States.PowerOfEarth] = new PowerOfEarth(),
        [States.EarthsHealth] = new EarthsHealth(),
        [States.MagicWater] = new MagicWater(),
        [States.Burning] = new Burning(),
        [States.Burn] = new Burn(),
		[States.Discharge] = new Gangdollarff.AirElemental.Discharge(),
		[States.CoolingAura] = new CoolingAura(),
        #endregion

        #region Test Baff and Debaff
        [States.BaffState] = new BaffState(),
		[States.DebaffState] = new DebaffState(),
        #endregion

        #region Test
        [States.TestAuraState] = new TestAuraState(),
        #endregion
    };

	public void Initialize(Character hero)
	{
		_hero = hero;
		/*_health = health;
		_move = move;
		_stamina = stamina;*/
		if (_hero == null)
		{
			Debug.LogError("No required component in " + name + " " + gameObject.name);
		}
	}

	private void Update()
	{
		if (_currentStates.Count > 0)
		{
			for (int i = 0; i < _currentStates.Count; i++)
			{
				_currentStates[i].UpdateState();
			}
		}
	}

	public void Dispel(StateType type)
	{
		foreach (AbstractCharacterState state in _currentStates)
		{
			if (state.Type == type)
			{
				state.ExitState();
			}
		}
	}

	public bool Check(StatusEffect effect)
	{
		foreach (AbstractCharacterState state in _currentStates)
		{
			if (state.Effects.Contains(effect))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckForState(States state)
	{
		foreach (AbstractCharacterState states in _currentStates)
		{
			if (states.State == state)
			{
				return true;
			}
		}
		return false;
	}

	public int CheckStateStacks(States state)
	{
		foreach (AbstractCharacterState states in _currentStates)
		{
			if (states.State == state)
			{
				return states.CurrentStacksCount;
			}
		}
		return 0;
	}
	public bool CheckStateType(StateType type)
	{
		foreach (AbstractCharacterState state in _currentStates)
		{
			if (state.Type == type)
			{
				return true;
			}
		}
		return false;
	}

	public AbstractCharacterState GetState(States state)
	{
		foreach (AbstractCharacterState states in _currentStates)
		{
			if (states.State == state)
			{
				return states;
			}
		}
		return null;
	}

	[Command]
	public void CmdAddState(States state, float duration, float damageToExit, Schools schools, GameObject personWhoShooted, string skillName)
	{
		AddStateLogic(state, duration, damageToExit, schools, personWhoShooted, skillName);
		ClientAddState(state, duration, damageToExit, schools, personWhoShooted, skillName);
	}

	[Command]
	public void CmdAddState(States state, float duration, float damageToExit, GameObject personWhoShooted, string skillName)
	{
		AddStateLogic(state, duration, damageToExit, Schools.None, personWhoShooted, skillName);
		ClientAddState(state, duration, damageToExit, Schools.None, personWhoShooted, skillName);
	}

	public void AddState(States state, float duration, float damageToExit, GameObject personWhoShooted, string skillName)
	{
		AddStateLogic(state, duration, damageToExit, Schools.None, personWhoShooted, skillName);
		ClientAddState(state, duration, damageToExit, Schools.None, personWhoShooted, skillName);
	}

	[Command]
	public void CmdRemoveState(States state)
	{
		RemoveStateLogic(state);
		ClientRemoveState(state);
	}

	public void RemoveState(States state)
	{
		RemoveStateLogic(state);
		ClientRemoveState(state);
	}

	public void RemoveState(AbstractCharacterState newState)
	{
		if (!_currentStates.Contains(newState)) return;

		newState.CurrentStacksCount = 0;

		if (newState is IDamageable damageableShield)
		{
			RemoveShield(damageableShield);
		}
        if (_currentStates.Contains(newState))
		{
            _currentStates.Remove(newState);
			_stateIcons?.RemoveItemByState(newState.State);
		}
    }

	private void RemoveStateLogic(States stateName)
	{
		if (_currentStates.Count <= 0) return;

		var statesCopy = new List<AbstractCharacterState>(_currentStates);

		foreach (var state in statesCopy)
		{
			if (state.State == stateName)
			{
				if (state is IDamageable damageableShield)
				{
					RemoveShield(damageableShield);
				}

				state.ExitState();
				_currentStates.Remove(state);

				_stateIcons?.RemoveItemByState(stateName);
				break;
			}
		}
	}


	[ClientRpc]
	private void ClientAddState(States state, float duration, float damageToExit, Schools schools, GameObject personWhoShooted, string skillName)
	{
		AddStateLogic(state, duration, damageToExit, schools, personWhoShooted, skillName);
	}

	[ClientRpc]
	private void ClientRemoveState(States stateName)
	{
		RemoveStateLogic(stateName);
	}

	/*private void AddStateLogic(States state, float duration, float damageToExit, Schools school,
		GameObject personWhoShooted, string skillName)
	{
		if (invinsible) return;

		//Debug.Log(state);

		if (CheckForState(state))
		{
			for (int i = 0; i < currentStates.Count; i++)
			{
				if (currentStates[i].State == state)
				{
					if (currentStates[i].CurrentStacksCount < currentStates[i].MaxStacksCount)
					{
						var canStack = currentStates[i].Stack(duration);
						_stateIcons.ActivateIco(state, duration, 1, canStack);
					}
					else if (currentStates[i].MaxStacksCount == 0 || currentStates[i].CurrentStacksCount == currentStates[i].MaxStacksCount)
					{
						var canStack = currentStates[i].Stack(duration);
						_stateIcons.ActivateIco(state, duration, 0, canStack);
					}

					break;
				}
			}
		}
		else
		{
			CreateState(enumToState[state], state, duration, damageToExit, personWhoShooted, skillName, false);

			if (enumToState[state] is IDamageable damageableShield)
			{
				AddShield(damageableShield);
			}

			if (school != Schools.None)
			{
				var counterSpell = (AbilitySchoolDebuff)enumToState[state];
				counterSpell.canceledSchoool = school;
			}
		}
	}*/


	public void AddStateLogic(States state, float duration, float damageToExit, Schools school, GameObject personWhoShooted, string skillName, bool isCanDodgeMagState = false)
	{
		if (invinsible) return;

		for (int i = 0; i < _currentStates.Count; i++)
		{
			if (_currentStates[i].State == state)
			{
				if (!_currentStates[i].CanStack) break;
				if (_currentStates[i].MaxStacksCount == 0)
                {
					bool canStack = _currentStates[i].Stack(duration);
					int newMaxStack = _currentStates[i].MaxStacksCount;

					_stateIcons.ActivateIco(state, duration, 1, canStack, newMaxStack);

					MoveStateToEnd(i);
				}

				else
                {
					_currentStates[i].Stack(duration);
					_currentStates[i].duration = Mathf.Max(_currentStates[i].RemainingDuration, duration);
					float remaining = _currentStates[i].RemainingDuration > 0f ? _currentStates[i].RemainingDuration : duration;
					int newMaxStack = _currentStates[i].MaxStacksCount;

					_stateIcons.ActivateIco(state, remaining, 1, true, newMaxStack);

					MoveStateToEnd(i);
				}

				return;
			}
		}

		AbstractCharacterState stateInstance = enumToState[state];
		Health characterHealth = _hero.Health;
		float chanceDodgeMagDamage = Random.Range(0f, 100f);

		if (!isCanDodgeMagState)
		{
			if (stateInstance.Type == StateType.Magic && chanceDodgeMagDamage <= characterHealth.ResistMagDamage)
			{
				Debug.Log("CharacterState / DodgeMagDamage");
				return;
			}
		}

		CreateState(stateInstance, state, duration, damageToExit, personWhoShooted, skillName, false);

		OnStateAdded?.Invoke(stateInstance);

		if (stateInstance is IDamageable damageableShield)
		{
			AddShield(damageableShield);
		}

		if (school != Schools.None)
		{
			var counterSpell = (AbilitySchoolDebuff)stateInstance;
			counterSpell.canceledSchoool = school;
		}
	}

	private void CreateState(AbstractCharacterState state, States stateName, float duration, float damageToExit, GameObject personWhoShooted, string skillName, bool stack)
	{
		_currentStates.Add(state);

		state.duration = duration;

		if (personWhoShooted.TryGetComponent<Character>(out var character))
		{
			_currentStates[_currentStates.Count - 1].EnterState(this, duration, damageToExit, character, skillName);
		}
		else
		{
			_currentStates[_currentStates.Count - 1].EnterState(this, duration, damageToExit, null, skillName);
		}

		float remaining = state.RemainingDuration;
		int maxStacksCount = state.MaxStacksCount;
		_stateIcons.ActivateIco(stateName, remaining, 1, stack, maxStacksCount);
	}

	private void AddShield(IDamageable shield)
	{
		var health = _hero.GetComponent<Health>();
		if (health != null)
		{
			health.Shields.Add(shield);
		}
	}

	private void RemoveShield(IDamageable shield)
	{
		var health = _hero.GetComponent<Health>();
		if (health != null)
		{
			health.Shields.Remove(shield);
		}
	}

	public void DispelStates(StateType type, int targetTeamIndex, int playerTeamIndex, bool isDispelOneState = false)
	{
		if (_currentStates.Count == 0) return;

		List<AbstractCharacterState> statesToRemove = new List<AbstractCharacterState>();

		for (int i = _currentStates.Count - 1; i >= 0; i--)
		{
			AbstractCharacterState state = _currentStates[i];

			if (state.Type == type &&
				((targetTeamIndex == playerTeamIndex && state.BaffDebaff == BaffDebaff.Debaff) ||
				 (targetTeamIndex != playerTeamIndex && state.BaffDebaff == BaffDebaff.Baff)))
			{
				if (state.CurrentStacksCount > 1)
				{
					state.CurrentStacksCount--;
					ClientRpcRemoveIconCount();
				}
				else
				{
					statesToRemove.Add(state);
					if (isDispelOneState) break;
				}
			}
		}

		foreach (var state in statesToRemove)
		{
			RemoveState(state.State);
			_stateIcons.RemoveItemByState(state.State);
		}
	}

	public void DispelStates(StateType type, bool isAlly, bool isDispelOneState = false)
	{
		if (_currentStates.Count == 0) return;

		List<AbstractCharacterState> statesToRemove = new List<AbstractCharacterState>();

		for (int i = _currentStates.Count - 1; i >= 0; i--)
		{
			AbstractCharacterState state = _currentStates[i];

			if (state.Type == type &&
				((isAlly && state.BaffDebaff == BaffDebaff.Baff) ||
				 (!isAlly && state.BaffDebaff == BaffDebaff.Debaff)))
			{
				if (state.CurrentStacksCount > 1)
				{
					state.CurrentStacksCount--;
					ClientRpcRemoveIconCount();
				}
				else
				{
					statesToRemove.Add(state);
					if (isDispelOneState) break;
				}
			}
		}

		foreach (var state in statesToRemove)
		{
			RemoveState(state.State);
			_stateIcons.RemoveItemByState(state.State);
		}
	}

	[ClientRpc]
	private void ClientRpcRemoveIconCount()
	{
		_stateIcons?.RemoveIconCount();
	}

	private void MoveStateToEnd(int index)
	{
		if (index < 0 || index >= _currentStates.Count)
			return;

		// Сохраняем ссылку на состояние
		var state = _currentStates[index];

		// Удаляем элемент из текущей позиции
		_currentStates.RemoveAt(index);

		// Добавляем его в конец списка
		_currentStates.Add(state);
	}
}

public enum StateType
{
	Physical,
	Magic,
	Immaterial,
	Aura
}

public enum StatusEffect
{
	Others,
	Move,
	MoveSpeed,
	Ability,
	AbilitySchool,
	AbilitySpeed,
	Absorptions,
	Poison,
	Healing,
	Freezing,
	Stunning,
	Invisible,
	Strengthening, // For all State increasing/reduction Health/Mana/other values
	Immateriality,
	ReducingEfficiency,
	Restoration,
	Destruction,
	Evade,
}

public enum States
{
	CreeperInvisible,
	PoisonBone,
	WitheringPoison,
	BindingPoison,
	PoisonCloud,
	HealingPoisonCloud,
	EmpathicPoisons,
	HealingPoisonPerSecond,
	InstantHealingPoison,
	RegeneratingPoison,
	HeatedGlands,
	AbsorptionOfPoison,
	ReducingHealing,
	Immateriality,
	InAir,
	Default,
	Stun,
	Frozen,
	Frosting,
	Cooling,
	Blind,
	Invisible,
	SchoolDebuff,
	FormDebuf,
	Desiccuration,
	Plague,
	Curse,
	NorthernerEndurance,
	LastBreath,
	MagicBuff,
	DarkShield,
	LightShield,
	ReversePolarity,
	SpiritEnergy,
	SpiritHealth,
	TiredSoul,
	ScorchedSoul,
	Knockdown,
	IdealEvade,
	Bleeding,
	Absorption,
	EmeraldSkin,
	SparkTalentHealthBuff,
	DefenseReduction,
	SelfHarm,
	ShieldBaff,
	LowVoltage,
	ComboState,
	DisappointmentState,
	ManaRegen,
	InnerDarkness,
	Fear,
	Astral,
	Silent,
	Irradiation,
	Suppression,
	WeakeningSilence,
	PartialBlindness,
	Anxiety,
	HuntressMark,
	Calmness,
	Sleep,
	ElvenSkill,
	BaffState,
    DebaffState,
	Bound,
	ShadowTree,
    PowerOfEarth,
    EarthsHealth,
    MagicWater,
    Burning,
    Burn,
    TestAuraState,
	MultiMagic,
	FireFlash,
	Stupefaction,
	TentacleGrip,
    Discharge,
    CoolingAura,
	Restoration,
	Destruction,
	HardenedFlesh,
	FocusingOnReflexesState,
	WarmingUpState,
	DivineEnhancement,
	HealingSlime,
}
public enum BaffDebaff
{
	Baff,
	Debaff,
	Null,
}