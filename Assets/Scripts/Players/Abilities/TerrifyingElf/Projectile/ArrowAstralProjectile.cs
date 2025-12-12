using Mirror;
using UnityEngine;

public class ArrowAstralProjectile : Projectiles
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float duration;
    [SerializeField] private float durationMinion;
    [SerializeField] private float _lifeTime = 5f;

    public float Duration => duration;

    public void StartFly(Vector3 direction)
    {
        if (_rb != null)
        {
            _rb.linearVelocity = direction * _speed;
        }

        Destroy(gameObject, _lifeTime);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out IDamageable target))
        {
            if (other.gameObject != _dad.gameObject)
            {
                if (target is Character character)
                {
                    if (character is HeroComponent heroComponent) AddState(heroComponent, duration);

                    else if (character is MinionComponent minionComponent) AddState(minionComponent, durationMinion);
                }
            }
        }
    }

    private void AddState(Character targetState, float duration)
    {
        targetState.CharacterState.AddState(States.Astral, duration, 0, _skill.Hero.gameObject, _skill.name);

        Destroy(gameObject);
    }
}
