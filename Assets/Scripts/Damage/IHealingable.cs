using System;
using Mirror;

public struct Heal : NetworkMessage
{
    public float Value;
    public Skill DamageableSkill;
}
public interface IHealingable
{
    public event Action<float, Skill,string > HealTaked;

    public void Heal(ref Heal value, string sourceName, Skill skill);
}
