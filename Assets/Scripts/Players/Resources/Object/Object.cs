using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public class Object : NetworkBehaviour, ITargetable
{
    [Header("UI")]
    [SerializeField] private SelectedCircle _selectedCircle;
    [SerializeField] private MinimapMarker _minimapMarker;

    [SerializeField] private ObjectData _objectData;
    [SerializeField] private ObjectHealth _objectHealth;
    [SerializeField] private List<Resource> _resources;
    [SerializeField] private int _indexTeam;
    [SerializeField] private UIObjectComponents uiComponent;

    [SerializeField] private bool live = false;
    [SerializeField] private bool isDestroyOnDeath = true;
    [SerializeField] private bool isTower = false;

    private bool _isDeath;

    public event Action<Object> Died;

    public bool Live { get => live; set => live = value; }
    public UIObjectComponents UIComponent => uiComponent;
    public ObjectData ObjectData => _objectData;
    public ObjectHealth ObjectHealth => _objectHealth;
    public List<Resource> Resources => _resources;
    public SelectedCircle SelectedCircle => _selectedCircle;
    public Transform TargetTransform => transform;
    public bool IsDeath { get => _isDeath; set => _isDeath = value; }
    public bool DestroyOnDeath => isDestroyOnDeath;
    public bool IsTower => isTower;

    public int IndexTeam { get => _indexTeam; set => _indexTeam = value; }

    public Vector3 Position => Transform.position;
    public Transform Transform => transform;

    public bool IsTargetable => !_isDeath;

    private void OnDestroy() => _objectHealth.OnDeath -= ServerOnDeath;

    public void Initialize()
    {
        foreach (var resource in Resources)
            if (resource.Type == ResourceType.Health) resource.Initialize(_objectData.MaxHealth, _objectData.RegenerationAmount, 0, null);

        _objectHealth.InitializeObject(_objectData);
        _objectHealth.OnDeath += ServerOnDeath;
        if (_minimapMarker != null) _minimapMarker.IsActive = true;
    }

    private void OnDied()
    {
        _isDeath = true;
        Died?.Invoke(this);
    }

    private void Start() => Initialize();
    public override void OnStartServer() => base.OnStartServer();
    public override void OnStopServer() => base.OnStopServer();

    [Server]
    private void ServerOnDeath()
    {
        OnDied();
        RpcClientOnDied();
    }
    [ClientRpc] private void RpcClientOnDied() => OnDied();
}
