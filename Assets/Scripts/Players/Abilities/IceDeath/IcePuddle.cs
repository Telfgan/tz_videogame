using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class IcePuddle : Skill
{
    [Header("Puddle prefabs & preview")]
    [SerializeField] private IcePuddleObject _puddle;
    [SerializeField] private IcePuddleObject _puddleBig;
    [SerializeField] private GameObject preViewPuddlePrefab;

    //[Header("Optional visuals (left for backward compatibility)")]
    //[SerializeField] private GameObject _lowePoint;
    //[SerializeField] private DecalProjector _puddleProjector;

    [Header("Ability settings")]
    [SerializeField] private SeriesOfStrikes _seriesOfStrikes;
    [SerializeField] private float _timeToDestroy = 3f;
    [SerializeField] private MoveComponent _move;
    [SerializeField] private AudioClip _audioClip;

    [Header("Raycast")]
    [SerializeField] private LayerMask _groundLayer;

    private AudioSource _audioSource;
    private Energy _energy;
    private GameObject _preViewPuddle;

    private Vector3 _placedPosition = Vector3.positiveInfinity;
    private float _placedAngleDeg;

    private bool _shooted = false;
    private bool _lastHit = false;

    private bool _talentPuddleSize = false;
    private bool _talentFrostingFrozen = false;
    private bool _talentEvadeDadBoost = false;
    private bool _iceDeathInIcePudleTalent;

    // private Vector3 _mousePos;
    // private float _angle;
    // private float _angle2;
    // private float _angle3;
    // private bool _enabled = false;
    // private bool _secondPoind = false;
    // private bool _crutch = false;
    // private float _timer = 2;
    // private float _time = 0;

    protected override bool IsCanCast
    {
        get
        {
            return _shooted;
        }
    }

    protected override int AnimTriggerCastDelay => 0;
    protected override int AnimTriggerCast => Animator.StringToHash("IcePuddle");

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        for (int i = 0; i < Hero.Resources.Count; i++)
            if (Hero.Resources[i].Type == ResourceType.Energy)
                _energy = (Energy)Hero.Resources[i];
    }

    private void UpdatePreviewAtMouse()
    {
        Vector3 mousePos = GetMousePointOnGround();
        if (float.IsPositiveInfinity(mousePos.x)) return;

        if (_preViewPuddle && !_preViewPuddle.activeSelf)
            _preViewPuddle.SetActive(true);

        if (_preViewPuddle)
        {
            _preViewPuddle.transform.position = mousePos;

            Vector3 dir = _hero.transform.position - mousePos;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(dir);
                _preViewPuddle.transform.rotation = Quaternion.Euler(-90f, rotation.eulerAngles.y, 0f);
            }
        }
    }

    private Vector3 GetMousePointOnGround()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _groundLayer))
        {
            return hit.point;
        }
        return Vector3.positiveInfinity;
    }

    public override void LoadTargetData(TargetInfo targetInfo)
    {
        if (targetInfo == null || targetInfo.Points == null || targetInfo.Points.Count == 0) return;

        _placedPosition = targetInfo.Points[0];
        var dir = _placedPosition - _hero.transform.position;
        _placedAngleDeg = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg - 90f;

        _shooted = true;
    }

    protected override IEnumerator PrepareJob(Action<TargetInfo> callbackDataSaved)
    {
        if (preViewPuddlePrefab != null) _preViewPuddle = Instantiate(preViewPuddlePrefab);


        while (true)
        {
            UpdatePreviewAtMouse();


            if (GetMouseButton)
            {
                _placedPosition = GetMousePointOnGround();
                if (float.IsPositiveInfinity(_placedPosition.x))
                {
                    yield return null;
                    continue;
                }


                float dist = Vector3.Distance(_hero.transform.position, _placedPosition);
                if (dist > Radius)
                {
                    yield return null;
                    continue;
                }


                Vector3 direction = (_hero.transform.position - _placedPosition).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
                    _placedAngleDeg = lookRot.eulerAngles.y;
                }


                var info = new TargetInfo();
                info.Points.Add(_placedPosition);
                callbackDataSaved?.Invoke(info);


                _shooted = true;
                break;
            }


            yield return null;
        }


        if (_preViewPuddle) _preViewPuddle.SetActive(false);
    }

    protected override IEnumerator CastJob()
    {
        _lastHit = _seriesOfStrikes.MakeHit(null, AbilityForm.Magic, 1, 0, 0);

        Shoot();
        yield return null;
    }

    protected override void ClearData()
    {
        _move?.StopLookAt();

        _shooted = false;
        _placedPosition = Vector3.positiveInfinity;
        _placedAngleDeg = 0f;

        if (_preViewPuddle) _preViewPuddle.SetActive(false);

        // _enabled = false;
        // _secondPoind = false;
        // _crutch = false;
        // _time = 0;
        // _angle = _angle2 = _angle3 = 0;
        // _mousePos = Vector3.zero;
    }

    private void Shoot()
    {
        if (float.IsPositiveInfinity(_placedPosition.x))
            return;

        int timeToAdd = (int)_energy.CurrentValue / 5;
        if (timeToAdd > 4) timeToAdd = 4;

        _timeToDestroy += timeToAdd;
        _energy.CmdUse(timeToAdd * 5);

        Buff.AttackSpeed.ReductionPercentage(1 + _seriesOfStrikes.GetMultipliedSpeed() / 100);
        Buff.AttackSpeed.IncreasePercentage(1 + _seriesOfStrikes.GetMultipliedSpeed() / 100);

        if (_lastHit) CmdCreateProjecttileBig(_placedAngleDeg, _timeToDestroy, _placedPosition, _lastHit && _talentPuddleSize, _talentEvadeDadBoost, _talentFrostingFrozen);
        else CmdCreateProjecttile(_placedAngleDeg, _timeToDestroy, _placedPosition, _lastHit && _talentPuddleSize, _talentEvadeDadBoost, _talentFrostingFrozen);
    }

    [Command]
    private void CmdCreateProjecttileBig(float angle, float manaValue, Vector3 position, bool lastHit, bool talentEvade, bool talentFrostingFrozen)
    {
        IcePuddleObject projectile = Instantiate(_puddleBig, position, Quaternion.Euler(-90, -angle, 0));
        SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
        projectile.Init(Hero, manaValue, lastHit, this);
        projectile.SetTalents(talentEvade, talentFrostingFrozen);

        NetworkServer.Spawn(projectile.gameObject);

        RpcPlayShotSound();
        RpcInit(projectile.gameObject, manaValue, lastHit);
    }

    [Command]
    private void CmdCreateProjecttile(float angle, float manaValue, Vector3 position, bool lastHit, bool talentEvade, bool talentFrostingFrozen)
    {
        IcePuddleObject projectile = Instantiate(_puddle, position, Quaternion.Euler(-90, -angle, 0));
        SceneManager.MoveGameObjectToScene(projectile.gameObject, _hero.NetworkSettings.MyRoom);
        projectile.Init(Hero, manaValue, lastHit, this);
        projectile.SetTalents(talentEvade, talentFrostingFrozen);

        NetworkServer.Spawn(projectile.gameObject);

        RpcPlayShotSound();
        RpcInit(projectile.gameObject, manaValue, lastHit);
    }

    [ClientRpc]
    private void RpcInit(GameObject obj, float manaValue, bool lastHit)
    {
        var puddle = obj.GetComponent<IcePuddleObject>();
        if (!puddle) return;

        puddle.Init(Hero, manaValue, lastHit, this);
        puddle.IceDeathInIcePudleTalentActive(_iceDeathInIcePudleTalent);
    }

    [ClientRpc]
    private void RpcPlayShotSound()
    {
        if (_audioSource != null && _audioClip != null)
            _audioSource.PlayOneShot(_audioClip);
    }

    public void SetTalentPuddleSize(bool active) => _talentPuddleSize = active;
    public void SetTalentFrostingFrozen(bool value) => _talentFrostingFrozen = value;
    public void SetTalentEvadeDadBoost(bool value) => _talentEvadeDadBoost = value;
    public void IceDeathInIcePudleTalentActive(bool value, string text)
    {
        _iceDeathInIcePudleTalent = value;
        AbilityInfoHero.FinalDescription = value ? AbilityInfoHero.Description + $" {text}" : AbilityInfoHero.Description;
    }

    public void IcePuddleCast() => AnimStartCastCoroutine();
    public void IcePuddleEnd()
    {
        AnimCastEnded();
        if (_move) _move.CanMove = true;
    }

    public void StopMoveIcePuddle()
    {
        if (_move) _move.CanMove = false;
    }

    /*
    private Vector3 InstantiatePoint()
    {
        Vector3 worldPosition = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            worldPosition = hit.point;
        }

        float distance = Vector3.Distance(gameObject.transform.position, worldPosition);
        if (distance <= _radius)
        {
            _crutch = false;
            return worldPosition;
        }
        else
        {
            _crutch = true;
            Vector3 direction = (worldPosition - gameObject.transform.position).normalized;
            Vector3 spawnPosition = gameObject.transform.position + direction * _radius;
            return spawnPosition;
        }
    }

    private Vector3 InstantiatePoint2()
    {
        Vector3 worldPosition = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            worldPosition = hit.point;
        }

        worldPosition += (worldPosition - _preViewPuddle.transform.position).normalized * 2;
        float distance = Vector3.Distance(gameObject.transform.position, worldPosition);
        if (distance <= _radius)
        {
            _crutch = false;
            return worldPosition;
        }
        else
        {
            _crutch = true;
            Vector3 direction = (worldPosition - gameObject.transform.position).normalized;
            Vector3 spawnPosition = gameObject.transform.position + direction * _radius;
            return spawnPosition;
        }
    }

    private void PlacePuddle()
    {
        if (!_secondPoind)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                _mousePos = hit.point;
            }
            Vector3 lookDir = _mousePos - _hero.transform.position;
            _angle = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg - 90f;
            _preViewPuddle.transform.rotation = Quaternion.Euler(-90, -_angle, 0);
            _preViewPuddle.transform.position = InstantiatePoint();
        }
        else
        {
            Vector3 _mousePos2 = Vector3.zero;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                _mousePos2 = hit.point;
            }
            Vector3 lookDir = _mousePos2 - _preViewPuddle.transform.position;
            _angle2 = Mathf.Atan2(lookDir.z, lookDir.x) * Mathf.Rad2Deg + 90f;

            _lowePoint.transform.position = InstantiatePoint2();
            if (!_crutch)
            {
                _angle3 = _angle2;
                _preViewPuddle.transform.rotation = Quaternion.Euler(-90, -_angle2, 0);
            }
        }
    }

    private void Timer()
    {
        if (_lastHit)
        {
            _time += Time.deltaTime;
            if (_time >= _timer)
            {
                _lastHit = false;
                _preViewPuddle.transform.localScale = Vector3.one;
            }
        }
    }
    */
}
