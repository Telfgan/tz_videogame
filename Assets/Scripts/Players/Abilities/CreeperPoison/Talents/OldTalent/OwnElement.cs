using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnElement : Talent
{
    [SerializeField] private CreeperStrike _creeperStrike;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private List<GameObject> _enemiesWithDebuff = new();
    [SerializeField] private float _radiusSearching;

    private int _currentStacksPoison;
    private int _currentAllStacks;
    private int _previousAllStacks;
    private int _currentStacksAtckSpeed;

    private float _baseIncreaseAttackSpeed = 0.1f;
    private float _baseAttackSpeed;
    private float _increasedAttackSpeed;
    private float _maxMinimumAttackSpeed = 0.1f;

    private PoisonBoneState _poisonBoneState;
    private EmpathicPoisonsState _empathicPoisonState;
    private WitheringPoisonState _witheringPoisonState;
    private BindingPoisonState _bindingPoisonState;

    private Coroutine _searchingDebuffOnEnemeies;

    public override void Enter()
    {
        SetActive(true);

        _baseAttackSpeed = _creeperStrike.CastDeley;

        StartSearchingEnemies();
    }

    public override void Exit()
    {
        if (_searchingDebuffOnEnemeies != null)
        {
            StopCoroutine(_searchingDebuffOnEnemeies);
            _searchingDebuffOnEnemeies = null;
        }

        SetActive(false);
    }

    private void StartSearchingEnemies()
    {
        _searchingDebuffOnEnemeies = StartCoroutine(SearchingDebuffOnEnemy(_enemyLayer));
    }

    private IEnumerator SearchingDebuffOnEnemy(LayerMask enemyLayer)
    {
        Debug.Log("OwnElement / SearchingDebuffOnEnemy");
        while (Data.IsOpen)
        {
            _enemiesWithDebuff.Clear();
            _currentStacksPoison = 0;
            _currentAllStacks = 0;

            Collider[] enemies = Physics.OverlapSphere(character.transform.position, _radiusSearching, enemyLayer);

            if (enemies != null)
            {
                foreach (Collider target in enemies)
                {
                    var targetWithDebuff = target.GetComponent<CharacterState>();

                    if (targetWithDebuff.Check(StatusEffect.Poison))
                    {
                        Debug.Log("OwnElement / SearchingDebuffOnEnemy / targetWithDebuff");

                        AdvertisementStates(targetWithDebuff);

                        _enemiesWithDebuff.Add(target.gameObject);

                        if (_bindingPoisonState != null)
                        {
                            _currentStacksPoison += _bindingPoisonState.CurrentStacks;
                        }
                        if (_poisonBoneState != null)
                        {
                            _currentStacksPoison += _poisonBoneState.CurrentStacks;
                        }
                        if (_empathicPoisonState != null)
                        {
                            _currentStacksPoison += _empathicPoisonState.CurrentStacks; ;
                        }
                        if (_witheringPoisonState != null)
                        {
                            _currentStacksPoison += _witheringPoisonState.CurrentStacks;
                        }

                    }
                }

                _currentAllStacks += _currentStacksPoison;

                if (_currentAllStacks != _previousAllStacks)
                {
                    while (_currentStacksAtckSpeed < _currentAllStacks)
                    {
                        if (_currentAllStacks > 0 && _creeperStrike.CastDeley > _maxMinimumAttackSpeed)
                        {
                            IncreaseAttackSpeed();
                            _previousAllStacks = _currentAllStacks;
                        }
                    }
                    yield return null;
                }
            }
            if (_currentAllStacks != _currentStacksAtckSpeed && _currentAllStacks == 0 || _currentAllStacks < _previousAllStacks)
            {
                while (_currentStacksAtckSpeed > _currentAllStacks)
                {
                    ResetAttackSpeed();
                }
                _previousAllStacks = 0;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void IncreaseAttackSpeed()
    {
        _currentStacksAtckSpeed++;

        _increasedAttackSpeed = _baseAttackSpeed - _baseIncreaseAttackSpeed;

        _creeperStrike.Buff.AttackSpeed.IncreasePercentage(_increasedAttackSpeed);
        Debug.Log("OwnElement / IncreaseAttackSpeed / CurrentAttackSpeed = " + _creeperStrike.CastDeley);
    }

    private void ResetAttackSpeed()
    {
        if (_creeperStrike.CastDeley < _baseAttackSpeed)
        {
            _creeperStrike.Buff.AttackSpeed.ReductionPercentage(_increasedAttackSpeed);
            Debug.Log("OwnElement / ResetAttackSpeed / CurrentAttackSpeed = " + _creeperStrike.CastDeley);
            _currentStacksAtckSpeed--;
        }
    }

    private void AdvertisementStates(CharacterState targetWithDebuff)
    {
        _bindingPoisonState = (BindingPoisonState)targetWithDebuff.GetState(States.BindingPoison);
        Debug.Log("OwnElement / AdvertisementStates / _bindingPoisonState = " + _bindingPoisonState);
        _poisonBoneState = (PoisonBoneState)targetWithDebuff.GetState(States.PoisonBone);
        Debug.Log("OwnElement / AdvertisementStates / _poisonBoneState = " + _poisonBoneState);
        _empathicPoisonState = (EmpathicPoisonsState)targetWithDebuff.GetState(States.EmpathicPoisons);
        Debug.Log("OwnElement / AdvertisementStates / _empathicPoisonState = " + _empathicPoisonState);
        _witheringPoisonState = (WitheringPoisonState)targetWithDebuff.GetState(States.WitheringPoison);
        Debug.Log("OwnElement / AdvertisementStates / _witheringPoisonState = " + _witheringPoisonState);
    }
}