using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public abstract class Character : NetworkBehaviour, IDamageable, IHealingable, ITargetable
{
	[SerializeField] private CharacterData _playerData;
	[SerializeField] private UserNetworkSettings _networkSettings; 
	[SerializeField] private Rigidbody _rigidbody;
	[SerializeField] private Collider _collider;
	[SerializeField] private Level _lvl;
	[SerializeField] private Animator _animator;
	[SerializeField] private NetworkAnimator _networkAnimator;
	[SerializeField] private Health _healthComponent;
	[SerializeField] private MoveComponent _playerMove; 
	[SerializeField] private SkillManager _abilities;
	[SerializeField] private CharacterState _characterState;
	[SerializeField] private UIPlayerComponents uiComponent;
	[SerializeField] private SelectComponent _selectComponent;
	[SerializeField] private DamageTracker _damageTracker;
	[SerializeField] private List<Resource> _resources;
	[SerializeField] private SelectedCircle _selectedCircle;
	[SerializeField] private SpawnComponent _spawnComponent;
	[SerializeField] private VisionComponent _visionComponent;
	[SerializeField] private Auras _auras;
	[SerializeField] private TargetSeeker _targetSeeker;
	[SerializeField] private Character _characterParent;

	[SyncVar] private int _killCounter;
	[SyncVar] private float _damageTakeCounter;
	[SyncVar] private int _assystCounter;
	[SyncVar] private int _deadsCounter;
	[SyncVar] private float _damageGetCounter;
	[SyncVar(hook = nameof(OnCharacterParentChanged))]
	private uint _characterParentNetId;
	private bool _isInvisible;
	private bool _isDead = false;

	public SpawnComponent SpawnComponent => _spawnComponent;
	public CharacterData Data => _playerData;
	public UserNetworkSettings NetworkSettings => _networkSettings;
	public Collider Collider => _collider;
	public Rigidbody Rigidbody => _rigidbody;
	public Health Health => _healthComponent;
	public Level LVL => _lvl;
	public MoveComponent Move => _playerMove;
	public SkillManager Abilities => _abilities;
	public CharacterState CharacterState => _characterState;
	public UIPlayerComponents UIComponent => uiComponent;
	public SelectComponent SelectComponent => _selectComponent;
	public DamageTracker DamageTracker => _damageTracker;
	public List<Resource> Resources => _resources;
	public SelectedCircle SelectedCircle => _selectedCircle;
    public Animator Animator => _animator;
	public TargetSeeker TargetSeeker => _targetSeeker;
    public NetworkAnimator NetworkAnimator => _networkAnimator;
	public Character CharacterParent
	{
		get => _characterParent;
		set
		{
			_characterParent = value;
			if (isServer && _characterParent != null) _characterParentNetId = _characterParent.netId;
		}
	}
	public bool IsInvisible
    {
        get => _isInvisible;

        set
        {
            _isInvisible = value;

            if (_isInvisible)
            {
                OnDisappeared?.Invoke();
            }
            else
            {
                OnAppeared?.Invoke();
            }
        }
    }
    public bool IsDead => _isDead;

    public int KillCounter { get => _killCounter; set { _killCounter = value; Killed?.Invoke(); } }
    public int AssystCounter { get => _assystCounter; set => _assystCounter = value; }
    public int DeadsCounter { get => _deadsCounter; set => _deadsCounter = value; }
    public float DamageTakeCounter { get => _damageTakeCounter; set => _damageTakeCounter = value; }
    public float DamageGetCounter { get => _damageGetCounter; set => _damageGetCounter = value; }

    public VisionComponent VisionComponent { get => _visionComponent; set => _visionComponent = value; }
    public Vector3 Position => transform.position;
    public Transform Transform => transform;
    public Auras Auras { get => _auras; }

    public bool IsTargetable => !_isDead;

    public static event Action<Character> ServerOnUnitSpawned;
	public static event Action<Character> ServerOnUnitDeleted; 
	public static event Action<Character> AuthorityOnUnitSpawned;
	public static event Action<Character> AuthorityOnUnitDeleted;
    public event Action OnDisappeared;
    public event Action OnAppeared;
    public event Action<Damage, Skill> DamageTaken;
    public event Action<Damage, GameObject> DamageGeted;
    public event Action<float, Skill, string> HealTaked;
	public event Action<Character> Died;
	public event Action Killed;

	protected override void OnValidate()
    {
		base.OnValidate();

        if (_collider == null)
        {
			Debug.LogError("Fill in field Collider on prefab " + gameObject.name);
        }
    }

    public virtual void Initialize()
	{
		Move.Initialize(Data.GetAttributeValue(AttributeNames.Speed), Rigidbody , true);
		CharacterState.Initialize(this);
		SelectComponent.Initialize(Move,Abilities,UIComponent);
		//_visionComponent.VisionRange = Data.GetAttributeValue(AttributeNames.VisionRadius);

		foreach (var resource in Resources)
		{
			if (resource.Type == ResourceType.Health)
			{
				resource.Initialize(
					 Data.GetAttributeValue(AttributeNames.Health), 
					Data.GetAttributeValue(AttributeNames.HpRegen), 
					Data.GetAttributeValue(AttributeNames.HpRegenDelay), 
					Data);
			}
			if (resource.Type == ResourceType.Energy)
			{
				resource.Initialize(
					 Data.GetAttributeValue(AttributeNames.Energy), 
					Data.GetAttributeValue(AttributeNames.EnergyRegen), 
					Data.GetAttributeValue(AttributeNames.EnergyRegenDelay), 
					Data);
			}
			if (resource.Type == ResourceType.Mana)
			{
				resource.Initialize(
					 Data.GetAttributeValue(AttributeNames.Mana), 
					Data.GetAttributeValue(AttributeNames.ManaRegen), 
					Data.GetAttributeValue(AttributeNames.ManaRegenDelay), 
					Data);
			}
			if (resource.Type == ResourceType.Rune)
			{
				resource.Initialize(
					 Data.GetAttributeValue(AttributeNames.Rune), 
					Data.GetAttributeValue(AttributeNames.RuneRegen), 
					Data.GetAttributeValue(AttributeNames.RuneRegenDelay), 
					Data);
			}
		}

		Health.Died += AddDeadCounter;
	}

	private void OnCharacterParentChanged(uint oldNetId, uint newNetId)
	{
		if (newNetId == 0) return;
		if (NetworkClient.spawned.TryGetValue(newNetId, out var identity)) _characterParent = identity.GetComponent<Character>();
	}
	private void Start()
	{
		Initialize();
	}

	public void ResuceVisionRange(float value)
    {
		TargetRpcResuceVisionRange(value);
	}

	public void IncraseVisionRange(float value)
    {
		TargetRpcIncraseVisionRange(value);
	}

	public void AddVisionRange(float value)
	{
		TargetRpcAddVisionRange(value);
	}

	[Server]
	public void ServerResetAll()
    {
		ResetAll();
		RpcResetAll();
	}

	[Server]
	private void ServerOnDied()
	{
		OnDied();
		RpcOnDied();
	}

#if UNITY_EDITOR
	[ContextMenu(nameof(ResetAll))]
	private void ServerResetAllTest()
	{
		ResetAll();
		RpcResetAll();
	}
#endif

	public override void OnStartServer()
	{
		ServerOnUnitSpawned?.Invoke(this);
		Health.Died += ServerOnDied;

		if (_characterParentNetId != 0 && NetworkServer.spawned.TryGetValue(_characterParentNetId, out var identity)) _characterParent = identity.GetComponent<Character>();
	}

	public override void OnStopServer()
	{
		ServerOnUnitDeleted?.Invoke(this);
		Health.Died -= ServerOnDied;
	}

	public override void OnStartClient()
	{
		if (!isClientOnly && !isOwned)
		{
			return;
		}
		AuthorityOnUnitSpawned?.Invoke(this);
	}

	public override void OnStopClient()
	{
		if (!isClientOnly && !isOwned)
		{
			return;
		}
		AuthorityOnUnitDeleted?.Invoke(this);
	}
	
	public Resource TryGetResource(ResourceType type)
	{
		return Resources.FirstOrDefault(r => r.Type == type);
	}

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
		bool b = Health.TryTakeDamage(ref damage, skill);
		_damageTakeCounter += damage.Value;
		return b;
    }

	[Command (requiresAuthority = false)]
	public void CmdTryTakeDamage(Damage damage, Skill skill)
    {
        TryTakeDamage(ref damage, skill);
    }

    public void ShowPhantomValue(Damage phantomValue)
    {
		Health.ShowPhantomValue(phantomValue);
	}

    public void Heal(ref Heal value, string sourceName, Skill skill)
    {
		Health.Heal(ref value, sourceName, skill);
	}

	public void DamageGet(Damage damage, GameObject target)
	{
		DamageGeted?.Invoke(damage, target);
	}

	protected virtual void OnDied()
    {
		if(_isDead) return;

		Died?.Invoke(this);

		_isDead = true;
		_animator.SetBool(HashAnimPlayer.IsDead, true);
		_collider.enabled = false;
		_rigidbody.isKinematic = true;
		_playerMove.enabled = false;
		_abilities.CancleAllSkills();

		foreach (var item in _resources)
        {
			item.enabled = false;
        }

		DeleteStates();
	}

	protected virtual void ResetAll()
	{
		_isDead = false;
		_animator.SetBool(HashAnimPlayer.IsDead, false);
		_collider.enabled = true;
		_rigidbody.isKinematic = false;
		_playerMove.enabled = true;

		foreach (var item in _resources)
		{
			item.enabled = true;

			if(isServer)
				item.ResetValue();
		}
	}

	private void DeleteStates()
    {
		var statesCopy = new List<AbstractCharacterState>(_characterState.CurrentStates);
		foreach (var state in statesCopy)
		{
			_characterState.RemoveState(state.State);
		}
	}

	private void AddDeadCounter()
    {
		_deadsCounter++;
    }

	[ClientRpc]
	private void RpcResetAll()
	{
		ResetAll();
	}

	[ClientRpc]
	private void RpcOnDied()
	{
		OnDied();
	}

	[TargetRpc]
	private void TargetRpcResuceVisionRange(float value)
	{
		_visionComponent.VisionRange /= value;
	}

	[TargetRpc]
	private void TargetRpcIncraseVisionRange(float value)
	{
		_visionComponent.VisionRange *= value;
	}

	[TargetRpc]
	private void TargetRpcAddVisionRange(float value)
	{
		_visionComponent.VisionRange += value;
	}
}
