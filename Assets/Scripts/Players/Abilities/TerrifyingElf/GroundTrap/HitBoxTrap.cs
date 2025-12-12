using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitBoxTrap : NetworkBehaviour, IDamageable
{
    [SerializeField] private ObjectHealth owner;
    [SerializeField] private bool isHit;

    private Trap _trap;

    public event Action<Damage, Skill> DamageTaken
    {
        add { if (owner != null) owner.DamageTaken += value; }
        remove { if (owner != null) owner.DamageTaken -= value; }
    }

    private void Awake()
    {
        if (owner == null) owner = GetComponentInParent<ObjectHealth>();
        _trap = GetComponentInParent<Trap>();
    }

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        if (owner == null) return false;
        return owner.TryTakeDamage(ref damage, skill);
    }

    public void ShowPhantomValue(Damage phantomValue)
    {
        if (owner != null) owner.ShowPhantomValue(phantomValue);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (_trap != null && !isHit) _trap.HandleHit(other);
    }

}
