using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class Tentacles : Skill
{
    [SerializeField] private Character _player;
    [SerializeField] private TentacleProjectile tentaclesPrefab;
    [SerializeField] private TentacleProjectile tentaclesPreview;
    [SerializeField] private AttackingPsionicEnergy _attackingPsionicEnergy;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private SpawnComponent _spawnComponent;
    [SerializeField] private float _radiusTarget = 0.5f;

    private bool _isPlacingTentacles = false;
    private bool _isClickedOnGround = false;

    private Vector3 _spawnPoint = Vector3.positiveInfinity;
    private HashSet<Character> _charactersInPreview = new HashSet<Character>();

    //private Character _target;
    private TentacleProjectile _previewInstance;
    private TentacleProjectile _previewInstancePrefab;
    private TentacleProjectile _currentTentacle;
    private Coroutine _radiusUpdateCoroutine;
    private MinionComponent _currentMinion;
    private float _spentAttackingPsiEnergy;

    #region Talent
    private bool _isPsionicsTalentThree = false;
    private bool _isCocoonSpawnTalent = false;
    private bool _isAttractionTentacleTalent = false;

    public void PsionicsTalentThree(bool value) => _isPsionicsTalentThree = value;
    public void CocoonSpawnTalent(bool value) => _isCocoonSpawnTalent = value;
    public void AttractionTentacleTalent(bool value) => _isAttractionTentacleTalent = value;
    #endregion

    public TentacleProjectile CurrentTentacle { get => _currentTentacle; set => _currentTentacle = value; }

    protected override int AnimTriggerCastDelay => Animator.StringToHash("Spell");
    protected override int AnimTriggerCast => 0;
    protected override bool IsCanCast => (GetTargetCharacter() != null || _isClickedOnGround) && _spawnPoint != Vector3.positiveInfinity;

    private bool IsValidVector(Vector3 vector)
    {
        return !(float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z) ||
                 float.IsInfinity(vector.x) || float.IsInfinity(vector.y) || float.IsInfinity(vector.z));
    }

    protected override void ClearData()
    {
        _skillRender.IsOverrideClosestTarget = false;
        _isClickedOnGround = false;

        _isPlacingTentacles = false;
        _spawnPoint = Vector3.positiveInfinity;
        ClearTarget();
        //_target = null;
        _spentAttackingPsiEnergy = 0f;
        Hero.Move.CanMove = true;
        _player.Move.StopLookAt();

        if (_previewInstance != null) Destroy(_previewInstance.gameObject);
        if (_previewInstancePrefab != null) Destroy(_previewInstancePrefab.gameObject);

        if (_radiusUpdateCoroutine != null)
        {
            StopCoroutine(_radiusUpdateCoroutine);
            _radiusUpdateCoroutine = null;
        }
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        _skillRender.IsOverrideClosestTarget = true;

        Vector3 mousePositionStart = GetMousePoint();

        _previewInstance = Instantiate(tentaclesPreview, mousePositionStart, Quaternion.identity);
        _previewInstance.IsAttractionTentacle = _isAttractionTentacleTalent;
        _skillRender.DrawRadius(_radius);
        _radiusUpdateCoroutine = StartCoroutine(UpdateRadiusColor());

        while (GetTargetCharacter() == null)
        {
            Vector3 mousePosition = GetMousePoint();
            //float distance = Vector3.Distance(mousePosition, transform.position);

            _previewInstance.transform.position = mousePosition;

            if (GetMouseButton && !_isPlacingTentacles)
            {
                //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitTarget))
                {
                    if (_isAttractionTentacleTalent)// && hitTarget.collider.TryGetComponent<Character>(out Character character) && ((1 << character.gameObject.layer) & TargetsLayers.value) != 0)
                    {
                        FindTargetCharacter();
                        _isPlacingTentacles = true;
                        //_target = character;
                        if (GetTargetCharacter() != null)
                        {
                            _previewInstance.transform.SetParent(GetTargetCharacter().transform);

                            _previewInstancePrefab = Instantiate(tentaclesPreview, _previewInstance.transform.position, Quaternion.identity);
                            _previewInstancePrefab.Tentacle.SetActive(true);
                            _previewInstancePrefab.IsPreview = false;

                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }

                    else
                    {
                        //_spawnPoint = hitTarget.point;
                        _spawnPoint = GetMousePoint();
                        float distance = Vector3.Distance(transform.position, _spawnPoint);

                        Collider[] colliders = Physics.OverlapSphere(_spawnPoint, _radiusTarget);
                        foreach (var collider in colliders)
                        {
                            if (collider.TryGetComponent<Character>(out Character target))
                            {
                                _isPlacingTentacles = true;
                                //_target = target;
                                if(GetTargetCharacter() != null)
                                    _previewInstance.transform.SetParent(GetTargetCharacter().transform);

                                if (_isAttractionTentacleTalent)
                                {
                                    _previewInstancePrefab = Instantiate(tentaclesPreview, _previewInstance.transform.position, Quaternion.identity);
                                    _previewInstancePrefab.Tentacle.SetActive(true);
                                    _previewInstancePrefab.IsPreview = false;
                                }

                                yield return new WaitForSeconds(0.1f);
                                break;
                            }
                        }

                        if (GetTargetCharacter() == null && distance <= Radius && _isCocoonSpawnTalent)
                        {
                            if (!IsValidVector(_spawnPoint)) yield break;

                            _isClickedOnGround = true;
                            yield break;
                        }
                    }
                }
            }

            yield return null;
        }

        if (!_isAttractionTentacleTalent) _spawnPoint = GetTargetCharacter().transform.position;

        if (_isAttractionTentacleTalent)
        {
            while (true)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                float sphereRadius = 0.1f;
                float maxDistance = 100f;

                if (GetMouseButton)
                {
                    if (!IsCooldowned)
                    {
                        yield return null;
                        continue;
                    }

                    if (Physics.SphereCast(ray, sphereRadius, out hit, maxDistance, _obstacle))
                    {
                        yield return null;
                        continue;
                    }

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, TargetsLayers))
                    {
                        if (hit.collider.TryGetComponent<Character>(out Character clickedCharacter))
                        {
                            Vector3 targetPosition = clickedCharacter.transform.position;

                            if (!IsValidVector(targetPosition))
                            {
                                yield return null;
                                continue;
                            }

                            _spawnPoint = targetPosition;
                            _player.Move.LookAtTransform(GetTargetCharacter().transform);
                            break;
                        }
                    }

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                    {
                        Vector3 groundPoint = hit.point;

                        Vector3 direction = groundPoint - _previewInstance.transform.position;
                        float distanceToCaster = direction.magnitude;

                        if (distanceToCaster > _previewInstance.Radius)
                            direction = direction.normalized * _previewInstance.Radius;

                        if (_previewInstancePrefab != null)
                            _previewInstancePrefab.transform.position = _previewInstance.transform.position + direction;

                        float distanceToTarget = Vector3.Distance(_previewInstancePrefab.transform.position, transform.position);

                        if (distanceToTarget <= Radius)
                        {
                            Vector3 potentialSpawnPoint = _previewInstancePrefab.transform.position;

                            if (!IsValidVector(potentialSpawnPoint))
                            {
                                yield return null;
                                continue;
                            }

                            _spawnPoint = potentialSpawnPoint;
                            _player.Move.LookAtTransform(GetTargetCharacter().transform);
                            break;
                        }
                    }
                }

                if (_previewInstancePrefab != null)
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                    {
                        Vector3 hoverPoint = hit.point;

                        Vector3 dir = hoverPoint - _previewInstance.transform.position;
                        float distance = dir.magnitude;

                        if (distance > _previewInstance.Radius)
                            dir = dir.normalized * _previewInstance.Radius;

                        _previewInstancePrefab.transform.position = _previewInstance.transform.position + dir;
                    }
                }

                yield return null;
            }
        }

        TrySpendAttackingPsi();
        Hero.Move.CanMove = false;
        Hero.Move.StopMoveAndAnimationMove();
        if (_previewInstance != null) Destroy(_previewInstance.gameObject);

        TargetInfo targetInfo = new TargetInfo();
        targetInfo.Points.Add(_spawnPoint);
        callbackDataSaved(targetInfo);
    }

    protected override IEnumerator CastJob()
    {
        if (!IsValidVector(_spawnPoint)) yield break;

        if (GetTargetCharacter() != null) CmdSpawnTentacles(_spawnPoint, GetTargetCharacter(), _spentAttackingPsiEnergy);

        else
        {
            if (_isCocoonSpawnTalent) SpawnWomb(_spawnPoint);
        }

        ClearData();
        _skillRender.StopDrawRadius();
        yield return null;
    }

    private IEnumerator UpdateRadiusColor()
    {
        while (true)
        {
            bool isPreviewInsideRadius = false;
            bool isCharacterInsidePreview = false;

            HashSet<Character> newCharactersInPreview = new HashSet<Character>();

            if (_previewInstance != null)
            {
                Collider[] hitColliders = Physics.OverlapSphere(_previewInstance.transform.position, Area + 500);

                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.TryGetComponent<Character>(out Character character) && character != _player)
                    {
                        float distanceToCharacter = Vector3.Distance(_previewInstance.transform.position, character.transform.position);

                        if (distanceToCharacter <= Area)
                        {
                            isCharacterInsidePreview = true;
                            character.SelectedCircle.SwitchClostestTarget(true);
                        }
                        else
                        {
                            character.SelectedCircle.SwitchClostestTarget(false);
                        }

                        newCharactersInPreview.Add(character);
                    }
                }

                if (GetTargetCharacter() == null)
                {
                    float distanceToPreview = Vector3.Distance(transform.position, _previewInstance.transform.position);
                    isPreviewInsideRadius = distanceToPreview <= (_radius + _previewInstance.Radius);
                }
            }

            if (_previewInstancePrefab != null && GetTargetCharacter() != null)
            {
                float distanceToPrefab = Vector3.Distance(transform.position, _previewInstancePrefab.transform.position);
                isPreviewInsideRadius = distanceToPrefab <= _radius;
            }

            _previewInstance.SetRadiusColor(isCharacterInsidePreview ? Color.green : Color.red);
            _skillRender.DrawRadiusColor(_radius, isPreviewInsideRadius ? Color.green : Color.red);

            _charactersInPreview = newCharactersInPreview;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void TrySpendAttackingPsi()
    {
        if (_attackingPsionicEnergy.IsAttackingPsiEnergy && _attackingPsionicEnergy.CurrentValue > 0f)
        {
            _spentAttackingPsiEnergy = _attackingPsionicEnergy.CurrentValue;
            CmdUseAttackingEnergy(_attackingPsionicEnergy.CurrentValue);
        }
    }

    private void SpawnWomb(Vector3 position)
    {
        if (!IsValidVector(position)) return;
        _spawnComponent.CmdSpawnEnemyPoint(position, Quaternion.identity, null, 0, false, Hero);

        CmdTentacleWomb();
    }

    [Command]
    private void CmdSpawnTentacles(Vector3 position, Character target, float _spentAttackingPsiEnergy)
    {
        if (!IsValidVector(position)) return;
        if (target == null) return;

        _currentTentacle = Instantiate(tentaclesPrefab, position, Quaternion.identity);
        SceneManager.MoveGameObjectToScene(_currentTentacle.gameObject, _hero.NetworkSettings.MyRoom);

        _currentTentacle.Init(_player, target, position, target.transform.position, true, _isPsionicsTalentThree, _isAttractionTentacleTalent, _spentAttackingPsiEnergy, this);

        NetworkServer.Spawn(_currentTentacle.gameObject);
        RpcInitTentacles(_currentTentacle.gameObject, target, position, _spentAttackingPsiEnergy);

        if (_radiusUpdateCoroutine != null)
        {
            StopCoroutine(_radiusUpdateCoroutine);
            _radiusUpdateCoroutine = null;
        }
    }

    [Command]
    private void CmdUseAttackingEnergy(float value)
    {
        _attackingPsionicEnergy.CurrentValue -= value;
    }

    [Command]
    private void CmdTentacleWomb()
    {
        RpcTentacleWomb();
    }

    [ClientRpc]
    private void RpcInitTentacles(GameObject tentacleObject, Character target, Vector3 position, float _spentAttackingPsiEnergy)
    {
        if (!IsValidVector(position)) return;
        if (tentacleObject == null) return;

        tentacleObject.GetComponent<TentacleProjectile>().Init(_player, target, position, target.transform.position, true, _isPsionicsTalentThree, _isAttractionTentacleTalent, _spentAttackingPsiEnergy, this);
    }

    [ClientRpc]
    private void RpcTentacleWomb()
    {
        foreach (var womb in _spawnComponent.Units) if (womb.TryGetComponent<CocoonSpawn>(out CocoonSpawn cocoonSpawn)) cocoonSpawn.Tentacle = this;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo.Points.Count > 0) _spawnPoint = targetInfo.Points[0];
        if (targetInfo.GetTargets().Count > 0 && targetInfo.GetTargets()[0] is Character character) SetTarget(character);
    }

    public void SetCurrentMinion(MinionComponent newMinion)
    {
        _currentMinion = newMinion;
    }
}