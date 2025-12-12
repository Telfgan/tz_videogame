using UnityEngine;

public class MoveScraderAnim : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private MinionMove _minionMove;
    [SerializeField] private SpellMoveTo spell;

    private void OnEnable()
    {
        spell.DoMove += HandleDoMove;
    }

    private void OnDisable()
    {
        spell.DoMove -= HandleDoMove;
    }

    private void HandleDoMove(GameObject gameObject) => _animator?.SetTrigger("MoveScared");
}