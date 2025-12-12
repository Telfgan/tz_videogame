using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperInvisible : Skill
{
    #region Variables

    [Header("Talents")]
    [SerializeField] private AssasinPoison _assasinPoison;
    [SerializeField] private ReleaseFromSecrecy _releaseFromSecrecy;
    [SerializeField] private DesireToHide _desireToHide;
    [SerializeField] private FirstStrike _firstStrike;
    [SerializeField] private ContinuationAmbush _continuationAmbush;
    [SerializeField] private TransparentPoisons _transparentPoisons;
    [SerializeField] private PreparingForFight _preparingForFight;

    [Header("Ability")]
    [SerializeField] private CreeperStrike _creeperStrike;
    [SerializeField] private ColdBlood _coldBlood;

    [Header("Invisible Abilities")]
    [SerializeField] private List<Skill> _altAbilities = new();

    [Header("Ability Properties")]
    [SerializeField] private Character _player;
    [SerializeField] private SkinnedMeshRenderer _playerRenderer;
    
    private SpitPoison _spitPoison;
    private PoisonBall _poisonBall;

    private float _previousHealth;
    private float _currentHealth;
    private float _distanceWithoutEnemies = 6f;

    private bool _isInvisible = false;
    private bool _isPlayerSeen = true;
    private bool _isDamagedPlayer = false;
    private bool _isReadyToThreeHitForPreparingForFightTalent = false;
    private bool _isCanExitInvisible = false;
    private bool _isCreeperStrikeIsHit;
    private bool _isEnemy;

    private Coroutine _checkEnemiesCoroutine;
    private Coroutine _exitFromInvisibleCoroutine;
    public bool IsReadyToThreeHitForPreparingForFightTalent 
    { 
        get => _isReadyToThreeHitForPreparingForFightTalent; 
        set => _isReadyToThreeHitForPreparingForFightTalent = value; 
    }

    public bool IsInvisible { get => _isInvisible; set => _isInvisible = value; }

    protected override int AnimTriggerCast => 0;
    protected override int AnimTriggerCastDelay => 0;
    protected override bool IsCanCast => _isPlayerSeen == false && _isDamagedPlayer == false;

    #endregion

    #region PrepareAndCastJob

    protected override void ClearData()
    {
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        Debug.LogError("DataError");
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        ResetAltAbility();

        switch (_isInvisible)
        {
            case false:
                if (_checkEnemiesCoroutine == null)
                {
                    yield return _checkEnemiesCoroutine = StartCoroutine(CheckEnemiesAround());
                }
                break;

            case true:

                _exitFromInvisibleCoroutine = StartCoroutine(ExitFromInvisible());

                break;

            default:
        }
        Debug.LogError("DataError");
    }

    protected override IEnumerator CastJob()
    {
        if (_isInvisible && _transparentPoisons.Data.IsOpen)
        {
            if (_altAbilities != null)
            {
                foreach (IAltAbility altAbility in _altAbilities)
                {
                    if (altAbility is SpitPoison spitPoison)
                    {
                        _spitPoison = spitPoison;
                        _spitPoison.IsAltAbility = true;
                        _spitPoison.ResetAbilityParameters += OnResetSpitPoison;
                    }
                    if (altAbility is PoisonBall poisonBall)
                    {
                        _poisonBall = poisonBall;
                        _poisonBall.IsAltAbility = true;
                        _poisonBall.ResetAbilityParameters += OnResetPoisonBall;
                    }
                }
            }
            CmdTransparentPoisonsIncreaseManaCots();
        }

        if (_desireToHide.Data.IsOpen && _desireToHide.IsCanApplyInvisible)
        {
            Debug.Log("CreeperInvisible / desireToHide");

            CmdApplyInvis(_player.gameObject);
            yield break;
        }
        else if (_continuationAmbush.Data.IsOpen && _continuationAmbush.IsCanApplyInvisible)
        {
            Debug.Log("CreeperInvisible / continuationAmbushTalent");
            CmdApplyInvis(_player.gameObject);
            yield break;
        }
        else if (!_isInvisible)
        {
            yield return new WaitForSeconds(5f);

            Debug.Log("CreeperInvisible / else if (invisible)");

            EnteringInvisible();
        }
        yield return null;
    }

    private void OnResetPoisonBall()
    {
        _poisonBall.IsAltAbility = false;
        _poisonBall.ResetAbilityParameters -= OnResetPoisonBall;
    }

    private void OnResetSpitPoison()
    {
        _spitPoison.IsAltAbility = false;
        _spitPoison.ResetAbilityParameters -= OnResetPoisonBall;
    }

    public void EnteringInvisible()
    {
        CmdApplyInvis(_player.gameObject);
    }

    public void ExitingInvisible()
    {
        _isCreeperStrikeIsHit = _creeperStrike.IsHit;
        CmdRemoveInvisible(_player.gameObject, _isCreeperStrikeIsHit);
    }

    #endregion

    private void Update()
    {
        CheckCurrentHealthPlayer();
    }

    private void CheckCurrentHealthPlayer()
    {
        _currentHealth = _player.Health.CurrentValue;

        if (_currentHealth < _previousHealth)
        {
            if (isServer == false)
                ExitingInvisible();
        }

        _previousHealth = _currentHealth;
    }

    private void ResetAltAbility()
    {
        if (_spitPoison != null && _poisonBall != null)
        {
            _spitPoison.IsAltAbility = false;
            _poisonBall.IsAltAbility = false;
        }
    }

    #region Coroutines

    private IEnumerator CheckEnemiesAround()
    {
        while (_isPlayerSeen)
        {
            _isEnemy = false;

            Collider[] hitEnemies = Physics.OverlapSphere(_player.transform.position, _distanceWithoutEnemies, _targetsLayers);

            foreach (Collider enemy in hitEnemies)
            {
                if (enemy != null)
                {
                    _isEnemy = true;
                    break;
                }
            }

            if (!_isEnemy)
            {
                _isPlayerSeen = false;
            }
            else
            {
                _isPlayerSeen = true;
            }

            hitEnemies = null;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ExitFromInvisible()
    {
        while (_isCanExitInvisible == false)
        {
            if (Input.GetMouseButton(2))
            {
                _isCanExitInvisible = true;

                if (_isInvisible && _isCanExitInvisible)
                {
                    CmdRemoveInvisible(_player.gameObject, _isCreeperStrikeIsHit);
                }

                yield break;
            }
            yield return null;
        }
    }

    #endregion

    #region CommandMethods

    [Command]
    private void CmdApplyInvis(GameObject player)
    {
        _isInvisible = true;

        RpcApplyInvis();

        RpcMakeTransparentMaterialsPlayer(player);

        _player.CharacterState.AddState(States.CreeperInvisible, 0, 0, _player.gameObject, Name);
    }

    [Command]
    private void CmdRemoveInvisible(GameObject player, bool creeperStrikeIsHit)
    {
        _isInvisible = false;
        _isPlayerSeen = true;
        _isDamagedPlayer = false;

        RpcReturnTransparencyMaterialsPlayer(player);

        RpcRemoveInvisible(creeperStrikeIsHit);
    }

    [Command]
    private void CmdTransparentPoisonsIncreaseManaCots()
    {
        _transparentPoisons.IncreaseManaCost();
    }

    #endregion

    #region RpcMethods

    [ClientRpc]
    private void RpcMakeTransparentMaterialsPlayer(GameObject player)
    {
        var playerLayer = player.layer;

        player.GetComponent<Character>().IsInvisible = true;

        SkinnedMeshRenderer playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        Dictionary<Material, Color> playerMaterial = new Dictionary<Material, Color>();

        foreach (Material materialPlayer in playerRenderer.materials)
        {
            if (playerMaterial.ContainsKey(materialPlayer) == false)
                playerMaterial.Add(materialPlayer, materialPlayer.color);
        }

        if (playerLayer == LayerMask.NameToLayer("Allies"))
        {
            foreach (Material mat in playerRenderer.materials)
            {
                if (mat != null && playerMaterial.TryGetValue(mat, out Color matColor))
                {
                    var matColorAlpha = matColor;
                    matColorAlpha.a = 0.5f;

                    mat.color = new Color(matColor.r, matColor.g, matColor.b, matColorAlpha.a);
                }
            }
        }
        else if (playerLayer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (Material mat in playerRenderer.materials)
            {
                if (mat != null && playerMaterial.TryGetValue(mat, out Color matColor))
                {
                    var matColorAlpha = matColor;
                    matColorAlpha.a = 0.0f;

                    mat.color = new Color(matColor.r, matColor.g, matColor.b, matColorAlpha.a);
                }
            }
        }

        playerMaterial.Clear();
    }

    [ClientRpc]
    private void RpcReturnTransparencyMaterialsPlayer(GameObject player)
    {
        var playerLayer = player.layer;

        player.GetComponent<Character>().IsInvisible = true;

        SkinnedMeshRenderer playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>();
        Dictionary<Material, Color> playerMaterial = new Dictionary<Material, Color>();

        foreach (Material materialPlayer in playerRenderer.materials)
        {
            if (playerMaterial.ContainsKey(materialPlayer) == false)
                playerMaterial.Add(materialPlayer, materialPlayer.color);
        }

        if (playerLayer == LayerMask.NameToLayer("Allies"))
        {
            foreach (Material mat in playerRenderer.materials)
            {
                if (mat != null && playerMaterial.TryGetValue(mat, out Color matColor))
                {
                    var matColorAlpha = matColor;
                    matColorAlpha.a = 1f;

                    mat.color = new Color(matColor.r, matColor.g, matColor.b, matColorAlpha.a);
                }
            }
        }
        else if (playerLayer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (Material mat in playerRenderer.materials)
            {
                if (mat != null && playerMaterial.TryGetValue(mat, out Color matColor))
                {
                    var matColorAlpha = matColor;
                    matColorAlpha.a = 1f;

                    mat.color = new Color(matColor.r, matColor.g, matColor.b, matColorAlpha.a);
                }
            }
        }

        playerMaterial.Clear();
    }
    

    [ClientRpc]
    private void RpcApplyInvis()
    {
        _isInvisible = true;
    }

    [ClientRpc]
    private void RpcRemoveInvisible(bool creeperStrikeIsHit)
    {
        _isInvisible = false;

        if (_assasinPoison != null && _assasinPoison.Data.IsOpen)
            _assasinPoison.RemoveAllCharges();
        

        if (_releaseFromSecrecy != null && _releaseFromSecrecy.Data.IsOpen)
            _releaseFromSecrecy.ApplyBuff();
        

        if (_firstStrike != null && _firstStrike.Data.IsOpen && !_firstStrike.IsCanIncreaseCrit)
            _firstStrike.SetBoolTrue();
        

        if (_preparingForFight != null && _preparingForFight.Data.IsOpen)
            _isReadyToThreeHitForPreparingForFightTalent = true;
            

        if (_coldBlood.ColdBloodTalent != null && _coldBlood.ColdBloodTalent.Data.IsOpen)
            _coldBlood.ReducingAbilityCooldown();
        
        
        #region CancleCoroutines

        if (_checkEnemiesCoroutine != null)
        {
            StopCoroutine(CheckEnemiesAround());
            _checkEnemiesCoroutine = null;
        }
        if (_exitFromInvisibleCoroutine != null)
        {
            StopCoroutine(ExitFromInvisible());
            _exitFromInvisibleCoroutine = null;
        }

        #endregion

        _isPlayerSeen = true;
        _isDamagedPlayer = false;
        _isCanExitInvisible = false;
    }

    #endregion
}
