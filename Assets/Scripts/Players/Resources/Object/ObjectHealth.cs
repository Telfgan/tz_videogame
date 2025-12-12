using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHealth : Resource, IDamageable
{
    [Header("UI / Visual")]
    [SerializeField] private ObjectBar _objectBar;
    
    [Header("Data")]
    [SerializeField] private ObjectData _objectData;
    [SerializeField] private Object obj;

    [Header("Damage type ignored")]
    [SerializeField] private List<Schools> _ignoredSchools;
    [SerializeField] private List<AbilityForm> _ignoredForms;
    [SerializeField] private List<SkillType> _ignoredSkillTypes;

    public event Action OnDeath;

    public event Action<Damage, Skill> DamageTaken;
    //public event Action<float, DamageType, Skill> DamageTakenType;

    [SyncVar] private float _maxHealth;
    [SyncVar(hook = nameof(OnHealthChanged))]
    private float _currentHealth;

    [SyncVar] private float _resistMagicDamage = 0f;

    private Coroutine _hideBarCoroutine;
    private Coroutine _regenerationCoroutine;

    [SerializeField] private bool live = false;
    [SerializeField] private bool isDestroyOnDeath = true;
    [SerializeField] private bool isRegenerationEnabled = false;

    public bool IsDestroyOnDeath { get => isDestroyOnDeath; set => isDestroyOnDeath = value; }
    public ObjectData ObjectData => _objectData;
    public float ResistMagicDamage => _resistMagicDamage;

    public float CurrentHealth
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public bool IsRegenerationEnabled
    {
        get => isRegenerationEnabled;
        set
        {
            if (isRegenerationEnabled == value) return;

            isRegenerationEnabled = value;

            if (isRegenerationEnabled) ÑmdStartCustomRegeneration();
            else ÑmdStopCustomRegeneration();
        }
    }

    #region regeneration

    private Coroutine _fillCoroutine;

    private void OnDisable()
    {
        StopCustomRegeneration();
    }

    public void StartCustomRegeneration()
    {
        if (isRegenerationEnabled)
        {
            if (_regenerationCoroutine != null)
            {
                StopCoroutine(_regenerationCoroutine);
                _regenerationCoroutine = null;
            }

            _regenerationCoroutine = StartCoroutine(CustomRegenerationRoutine());
        }
    }

    public void StartCustomNegativeRegeneration()
    {
        if (_regenerationCoroutine != null)
        {
            StopCoroutine(_regenerationCoroutine);
            _regenerationCoroutine = null;
        }

        if (isRegenerationEnabled) _regenerationCoroutine = StartCoroutine(CustomNegativeRegenerationRoutine());
    }

    private void StopCustomRegeneration(bool immediate = false)
    {
        if (_regenerationCoroutine != null)
        {
            StopCoroutine(_regenerationCoroutine);
            _regenerationCoroutine = null;
        }

        if (immediate) StopAllCoroutines();
    }

    private void StopCustomNegativeRegeneration(bool immediate = false)
    {
        if (_regenerationCoroutine != null)
        {
            StopCoroutine(_regenerationCoroutine);
            _regenerationCoroutine = null;
        }

        if (immediate) StopAllCoroutines();
    }

    [Server]
    public void ServerStartFillHP(float targetValue, float duration)
    {
        if (_fillCoroutine != null) StopCoroutine(_fillCoroutine);
        _fillCoroutine = StartCoroutine(FillHPCoroutine(targetValue, duration));
    }

    [Server]
    public void ServerInterruptFillHP()
    {
        if (_fillCoroutine == null) return;

        StopCoroutine(_fillCoroutine);
        _fillCoroutine = null;

        RpcSyncHP(_currentHealth);
    }

    [ClientRpc]
    private void RpcSyncHP(float value)
    {
     _currentHealth = value;
     OnHealthChanged(0, _currentHealth);
    }

    private IEnumerator CustomRegenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_objectData.RegenerationInterval);

            if (_currentHealth <= 0)
                yield break;

            if (_currentHealth < MaxValue)
            {
                _currentHealth = Mathf.Min(MaxValue, _currentHealth + _objectData.RegenerationAmount);
                OnHealthChanged(0, _currentHealth);
            }
        }
    }

    private IEnumerator CustomNegativeRegenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(_objectData.RegenerationInterval);

            if (_currentHealth > 0)
            {
                _currentHealth = Mathf.Max(0, _currentHealth - 1);
                OnHealthChanged(_currentHealth, _currentHealth);
            }

            if (_currentHealth <= 0)
            {
                if (isServer) NetworkServer.Destroy(gameObject);
                else Destroy(gameObject);

                yield break;
            }
        }
    }


    private IEnumerator FillHPCoroutine(float targetValue, float duration)
    {
        float startValue = _currentHealth;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            float newHP = Mathf.Lerp(startValue, targetValue, t);
            _currentHealth = newHP;

            OnHealthChanged(_currentHealth, _currentHealth);

            yield return null;
        }

        _currentHealth = targetValue;
        OnHealthChanged(_currentHealth, _currentHealth);

        _fillCoroutine = null;
    }

    #endregion

    #region Initialization

    public void InitializeObject(ObjectData objectData)
    {
        _objectData = objectData;

        Initialize(objectData.MaxHealth, objectData.RegenerationAmount, objectData.RegenerationInterval, null);

        if (objectData.MaxEndurance)
        {
            _currentHealth = objectData.MaxHealth;
            ValuesObjectData(objectData);
        }

        else if (objectData.MinEndurance)
        {
            _currentHealth = 0;
            ValuesObjectData(objectData);
        }
    }

    private void ValuesObjectData(ObjectData objectData)
    {
        if (_objectBar != null)
        {
            _objectBar.SetMaxHealth(objectData.MaxHealth);
            _objectBar.SetHealth(_currentHealth);
            _objectBar.HideHealthBar();
        }
    }

    #endregion

    #region Take Damage

    public bool TryTakeDamage(ref Damage damage, Skill skill)
    {
        if (IsDamageIgnored(skill)) return false;
        if (TryEvade(damage.Type)) return false;

        if (_regenerationCoroutine == null) ÑmdStartCustomRegeneration();
        float damageValue = damage.Value;

        if (_currentHealth > 0)
        {
            _currentHealth -= damageValue;
             
            DamageTaken?.Invoke(damage, skill);
            //DamageTakenType?.Invoke(damageValue, damage.Type, skill);

            if (_objectBar != null && (_objectData == null || !_objectData.HideBar))
            {
                _objectBar.ShowHealthBar();
                _objectBar.SetHealth(_currentHealth);
            }

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
                if (obj != null) obj.IsDeath = true;

                GameObject target = transform.parent != null ? transform.parent.gameObject : gameObject;
                ÑmdStopCustomRegeneration();

                if (isDestroyOnDeath)
                {
                    if (isServer && target.TryGetComponent(out NetworkIdentity identity)) NetworkServer.Destroy(target);
                    else Destroy(target);

                }

                else
                {
                    if (isServer) ClienRpcActive(false);
                }
            }

            if (isServer) RpcPopupDamage(damage.Value);
            return true;
        }
        return false;
    }

    private bool TryEvade(DamageType damageType)
    {
        if (damageType == DamageType.Magical)
        {
            float roll = UnityEngine.Random.Range(0, 100);
            if (roll < _resistMagicDamage) return true;
        }

        return false;
    }

    #endregion

    #region UI / Visual

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        if (_objectBar == null || (_objectData != null && _objectData.HideBar)) return;

        _objectBar.SetHealth(newHealth);

        if (!Mathf.Approximately(newHealth, ObjectData.MaxHealth))
        {
            _objectBar.ShowHealthBar();

            if (_hideBarCoroutine != null)
            {
                StopCoroutine(_hideBarCoroutine);
                _hideBarCoroutine = null;
            }
        }
        else
        {
            if (_hideBarCoroutine != null)
            {
                StopCoroutine(_hideBarCoroutine);
                _hideBarCoroutine = null;
            }

            _objectBar.HideHealthBar();
        }
    }

    //private void ShowAndAutoHideBar()
    //{
    //    if (_objectBar == null) return;

    //    _objectBar.ShowHealthBar();

    //    if (_hideBarCoroutine != null)
    //        StopCoroutine(_hideBarCoroutine);

    //    _hideBarCoroutine = StartCoroutine(HideHealthBarAfterDelay(2f));
    //}

    //private IEnumerator HideHealthBarAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);

    //    if (_fillCoroutine == null)
    //        _objectBar.HideHealthBar();
    //}

    public void SetMagicEvade(float value)
    {
        _resistMagicDamage = Mathf.Clamp(value, 0f, 100f);
    }

    #endregion

    public void ReplaceObjectData(ObjectData newData)
    {
        _objectData = newData;
        _maxHealth = newData.MaxHealth;

        ServerStartFillHP(_maxHealth, 0f);
    }

    private void TryUpdateBar()
    {

    }

    [Command]
    public void CmdSetCurrentHealth(float newValue)
    {
        _currentHealth = Mathf.Clamp(newValue, 0, MaxValue);
        OnHealthChanged(0, _currentHealth);
    }

    [Server]
    public void ÑmdStartCustomRegeneration()
    {
        StopCustomNegativeRegeneration(true);
        StartCustomRegeneration();
        ClientRpcStartCustomRegeneration();
    }

    [Server]
    public void ÑmdStartCustomNegativeRegeneration()
    {
        StopCustomRegeneration(true);
        StartCustomNegativeRegeneration();
        //ClientRpcStartNegaiveCustomRegeneration();
    }

    [Server]
    public void ÑmdStopCustomRegeneration()
    {
        StopCustomRegeneration();
        ClientRpcStopCustomRegeneration();
    }

    [Server]
    public void ServerSetCurrentHealth(float newValue)
    {
        _currentHealth = Mathf.Clamp(newValue, 0, MaxValue);
        if (obj != null) obj.IsDeath = false;
        gameObject.SetActive(true);
        RpcSyncHP(_currentHealth);
    }

    [ClientRpc]
    private void RpcPopupDamage(float value)
    {
        Damage damage = new Damage { Value = value, Type = DamageType.Physical };
        DamageTaken?.Invoke(damage, null);
    }

    [ClientRpc]
    private void ClienRpcActive(bool value)
    {
        gameObject.SetActive(value);
    }

    [ClientRpc]
    private void ClientRpcStopCustomRegeneration()
    {
        StopCustomRegeneration();
    }

    [ClientRpc]
    public void ClientRpcStartCustomRegeneration()
    {
       StartCustomRegeneration();
    }

    [ClientRpc]
    public void ClientRpcStartNegaiveCustomRegeneration()
    {
        StartCustomRegeneration();
    }


    public void ShowPhantomValue(Damage phantomValue)
    {
        throw new NotImplementedException();
    }

    private bool IsDamageIgnored(Skill skill)
    {
        if (skill == null) return false;

        if (_ignoredSchools.Contains(skill.School)) return true;
        if (_ignoredForms.Contains(skill.AbilityForm)) return true;
        if (_ignoredSkillTypes.Contains(skill.SkillType)) return true;
        return false;
    }

    protected virtual void HookBonusMaxValueChanged(float oldValue, float newValue) { }
}
