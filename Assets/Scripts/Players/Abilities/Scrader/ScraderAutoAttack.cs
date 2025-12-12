using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScraderAutoAttack : MonoBehaviour
{
    [SerializeField] private SpellMoveTo spellMoveTo;

    public void OnAutoAttackAnimationHitScrader() => spellMoveTo.OnAutoAttackAnimationHit();
    public void OnAutoAttackAnimationEndScrader() => spellMoveTo.OnAutoAttackAnimationEnd();
}
