using System.Collections;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

public class SkillRenderer : NetworkBehaviour
{
    [SerializeField] private Character hero;
    [SerializeField] private DrawCircle _circle;
    [SerializeField] private CircleArea _areaPref;
    [SerializeField] private SphereArea _damageZonePref;
    [SerializeField] private SkillCircleRanderer _skillCircleRandererPref;
    [SerializeField] private AbilityLineRenderer _line;
    [SerializeField] private LineZoneRender _lineZoneRender;
    [SerializeField] private LineZoneRender _lineZoneRenderForQueue;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Color _colorForAllies = Color.green;
    [SerializeField] private Color _colorForEnemies = Color.red;
    [SerializeField] private Color _colorForEnd;
    [SerializeField] private Color _colorForStart;

    private List<Character> _targets = new List<Character>();
    private List<LineZoneRender> _lineZoneRenders = new();
    private bool _isOverrideClosestTarget = false;
    //private SphereArea _tempDamageZone;
    private CircleArea _tempArea;
    private float _lineStartLength;
   // private float _lineEndLength;
    private float _boxLength;
    private float _boxWidth;
    private float _circleRadius;
    private BoxArea _lineStartImage;
    //private BoxArea _lineEndImage;
    private SkillCircleRanderer _drawAutoAttackRadius;
    private Character _hovered;
    private Character _hoveredTarget;

    private Coroutine _previewDamageCoroutine;
    private readonly HashSet<Health> _previewSet = new();

    private Coroutine _drawLineCoroutine;
    private Coroutine _drawAreaCoroutine;
    private Coroutine _drawClosestTargetCoroutine;
    private Coroutine _drawRadiusCoroutine;
    private Coroutine _drawAutoAttackRadiusCoroutine;
    private Coroutine _dynamicRadiusColorCoroutine;
    private Coroutine _hoverHighlightCoroutine;

    //public SphereArea TempDamageZone => _tempDamageZone;
    public CircleArea TempDamageZone => _tempArea;
    private readonly Queue<CircleArea> _drawnZonesQueue = new();

    [Header("Cursor Target")]
    [SerializeField] private Texture2D _cursorPrepareTexture;
    [SerializeField] private Texture2D _cursorPrepareLightTexture;
    [SerializeField] private Texture2D _cursorDefaultTexture;


    private Vector2 _cursorPrepareHotspot = Vector2.zero;
    private Vector2 _cursorDefaultHotspot = Vector2.zero;

    private void Awake()
    {
        if (_cursorPrepareTexture != null) _cursorPrepareHotspot = new Vector2(_cursorPrepareTexture.width / 2f, _cursorPrepareTexture.height / 2f);
    }

    public void SetPrepareCursorLight() => UnityEngine.Cursor.SetCursor(_cursorPrepareLightTexture, _cursorPrepareHotspot, CursorMode.Auto);
    public void SetPrepareCursor() => UnityEngine.Cursor.SetCursor(_cursorPrepareTexture, _cursorPrepareHotspot, CursorMode.Auto);
    public void ResetCursor() => UnityEngine.Cursor.SetCursor(_cursorDefaultTexture, _cursorDefaultHotspot, CursorMode.Auto);

    public bool IsOverrideClosestTarget
    {
        get => _isOverrideClosestTarget;
        set
        {
            _isOverrideClosestTarget = value;
            if (_isOverrideClosestTarget) StopDrawClosestTarget();
        }
    }

    public SkillCircleRanderer SkillCircleRandererPref { get => _skillCircleRandererPref; }

    private Character _tempTarget;

    [Command]
    public void CmdDrawDamageZone(Vector3 position, float radius, Damage damage, GameObject player)
    {
        RpcDrawDamageZone(position, radius, damage, player);
    }

    public void StartPreview(float radius, Damage damage, LayerMask layerMask)
    {
        if (_previewDamageCoroutine != null) StopCoroutine(_previewDamageCoroutine);
        _previewSet.Clear();
        _previewDamageCoroutine = StartCoroutine(PreviewDamageJob(radius, damage, layerMask));
    }

    [ClientRpc]
    public void RpcDrawDamageZone(Vector3 position, float radius, Damage damage, GameObject player)
    {
		/* _tempDamageZone = Instantiate(_damageZonePref, position, Quaternion.identity);
		 _tempDamageZone.SetSize(radius, damage);

		 Color zoneColor = player.layer == LayerMask.NameToLayer("Allies") ? _colorForAllies : _colorForEnemies;
		 _tempDamageZone.SetColor(zoneColor);*/

		_tempArea = Instantiate(_areaPref, position, Quaternion.identity);
		_tempArea.SetSize(radius, damage);

		Color zoneColor = player.layer == LayerMask.NameToLayer("Allies") ? _colorForAllies : _colorForEnemies;
		_tempArea.SetColor(zoneColor);

        _drawnZonesQueue.Enqueue(_tempArea);
    }

    [Command]
    public void CmdStopDrawDamageZone()
    {
        RpsStopDrawDamageZone();
    }

    [Command]
    public void CmdRemoveNextDamageZone()
    {
        RpcRemoveNextDamageZone();
    }

    [ClientRpc]
    public void RpcRemoveNextDamageZone()
    {
        if (_drawnZonesQueue.Count > 0)
        {
            var zone = _drawnZonesQueue.Dequeue();
            if (zone != null) Destroy(zone.gameObject);
        }
    }

    [ClientRpc]
    public void RpsStopDrawDamageZone()
    {
		/* if (_tempDamageZone != null)
		 {
			 Destroy(_tempDamageZone.gameObject);
		 }*/
		if (_tempArea != null)
		{
			Destroy(_tempArea.gameObject);
		}
	}

    #region preview 
    public void StopPreview()
    {
        if (_previewDamageCoroutine != null)
        {
            StopCoroutine(_previewDamageCoroutine);
            _previewDamageCoroutine = null;
        }

        foreach (var health in _previewSet) if (health != null) health.ShowPhantomValue(new Damage { Value = 0, Type = DamageType.Physical });

        _previewSet.Clear();
    }

    private IEnumerator PreviewDamageJob(float radius, Damage damage, LayerMask layerMask)
    {
        while (true)
        {
            if (!TryGetMousePoint(out Vector3 position))
            {
                yield return null;
                continue;
            }

            Collider[] colliders = Physics.OverlapSphere(position, radius, layerMask);
            HashSet<Health> current = new HashSet<Health>();

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out Health hp) && collider.transform != transform.parent)
                {
                    current.Add(hp);
                    hp.ShowPhantomValue(damage);
                }
            }

            foreach (var health in _previewSet.Except(current)) if (health != null) health.ShowPhantomValue(new Damage { Value = 0, Type = DamageType.Physical });

            _previewSet.Clear();
            foreach (var health in current) _previewSet.Add(health);

            yield return null;
        }
    }

    private bool TryGetMousePoint(out Vector3 point)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var hits = Physics.RaycastAll(ray, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore).OrderBy(h => h.distance);

        foreach (var hit in hits)
        {
            if ((_layerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                point = hit.point;
                return true;
            }
        }

        point = Vector3.zero;
        return false;
    }
    #endregion

    public void StartDrawLineForZone(Skill skill)
    {
        _lineZoneRender.StartDraw(skill);
    }

    public void StartDrawAllLineForZone(Vector3[] vector3s)
    {
        _lineZoneRenderForQueue.StartDraw(vector3s);
    }

    public void StopDrawLineForZone()
    {
        _lineZoneRender.StopDraw();
    }

    public void StopDrawAllLineForZone()
    {
        _lineZoneRenderForQueue.StopDraw();
    }

    public void DrawRadius(float radius)
    {
        _circle.Draw(radius);
    }

    public void StopDrawRadius()
    {
        _circle.Clear();
        if (_drawRadiusCoroutine != null)
        {
            StopCoroutine(_drawRadiusCoroutine);
            _drawRadiusCoroutine = null;
        }
    }

    public void DrawRadiusColor(float radius, Color color) 
    {
        _circle.SetColor(color);
        _circle.Draw(radius);
    }

    public void SetColor(Color color)
    {
        _circle.SetColor(color);
    }

    public void DrawArea(float radius, Damage damage, LayerMask layerMask, CircleArea area = null)
    {
        if (area == null)
            area = _areaPref;

        _circleRadius = radius;
        _drawAreaCoroutine = StartCoroutine(DrawAreaJob(radius, damage, layerMask, area));
    }

    public void StopDrawArea()
    {
        if (_drawAreaCoroutine != null)
            StopCoroutine(_drawAreaCoroutine);

        if(_tempArea != null)
            Destroy(_tempArea.gameObject);
    }

    public void DrawLine(float length, float width, Damage damage, LayerMask layerMask, AbilityLineRenderer line = null)
    {
        if (line == null)
            line = _line;
        _boxWidth = length;
        _boxWidth = width;
        _drawLineCoroutine = StartCoroutine(DrawLineJob(length, width, damage, layerMask, line));
    }

    public void StopDrawLine()
    {
        if (_drawLineCoroutine != null)
            StopCoroutine(_drawLineCoroutine);

        if (_lineStartImage != null)
            Destroy(_lineStartImage.gameObject);

     /*   if (_lineEndImage != null)
            Destroy(_lineEndImage.gameObject);*/
    }

    public void DrawClosestTarget(float radius, LayerMask TargetsLayers, Character player)
    {
        if (_isOverrideClosestTarget) return;

        if (_hoveredTarget != null) _hoveredTarget.SelectedCircle.SwitchSelectCircle(true);
        _drawClosestTargetCoroutine = StartCoroutine(DrawClosestTargetJob(radius, TargetsLayers, player));
    }

    public void StopDrawClosestTarget()
    {
		if (_drawClosestTargetCoroutine != null)
        {
            StopCoroutine(_drawClosestTargetCoroutine);
            _drawClosestTargetCoroutine = null;

            foreach (var target in _targets) if (target != null) target.SelectedCircle.SwitchStroke(false);
        }    

        if(_tempTarget != null)
        {
            _tempTarget.SelectedCircle.SwitchClostestTarget(false);
            _tempTarget = null;
        }

        if (_hoveredTarget != null)
        {
            _hoveredTarget.SelectedCircle.SwitchSelectCircle(false);
            _hoveredTarget = null;
        }

        _targets.Clear();
    }

    public void SetSizeBox(float width, float lenght)
    {
        _boxWidth = width;
        _boxLength = lenght;
    }

    public void SetRadiusArea(float radiusArea)
    {
        _circleRadius = radiusArea;
    }

    public void StartDynamicRadiusColor(float radius)
    {
        if (_dynamicRadiusColorCoroutine != null)
            StopCoroutine(_dynamicRadiusColorCoroutine);

        _dynamicRadiusColorCoroutine = StartCoroutine(DynamicRadiusColorJob(radius));
    }

    public void StopDynamicRadiusColor()
    {
        if (_dynamicRadiusColorCoroutine != null)
        {
            StopCoroutine(_dynamicRadiusColorCoroutine);
            _dynamicRadiusColorCoroutine = null;
        }
    }

    public void StartDrawAutoAttackRadius(float radius)
    {
        if (_skillCircleRandererPref != null)
        {
            _drawAutoAttackRadius = Instantiate(_skillCircleRandererPref, transform);
            _drawAutoAttackRadius.StartDraw(radius);
            _drawAutoAttackRadius.StartBlink(1);
        }
    }

    public void StopDrawAutoAttackRadius()
    {
        if (_drawAutoAttackRadius != null)
        {
            Destroy(_drawAutoAttackRadius.gameObject);
            _drawAutoAttackRadius = null;
        }
    }

    public void StartHoverHighlight()
    {
        if (_hoverHighlightCoroutine == null)  _hoverHighlightCoroutine = StartCoroutine(HoverHighlightJob());
    }

    public void StopHoverHighlight()
    {
        if (_hoverHighlightCoroutine != null)
        {
            StopCoroutine(_hoverHighlightCoroutine);
            _hoverHighlightCoroutine = null;

            if (_hovered != null)
            {
                var prev = _hovered.GetComponentInChildren<SelectedCircle>();
                if (prev != null) prev.SwitchStroke(false);
                _hovered = null;
            }
        }
    }


    private void RotateAtMouse(Transform transform)
    {
		Vector3 worldPosition = Vector3.zero;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit,  Mathf.Infinity,  _layerMask, QueryTriggerInteraction.Ignore)) worldPosition = hit.point;
		//Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
		Vector3 dir = worldPosition - gameObject.transform.position;
		float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, - angle + 90, 0);
    }

    private void UpdateHoverTargetAlways(Character self)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.transform.TryGetComponent<Character>(out Character hoveredChar) && hoveredChar != self)
            {
                if (_hovered != hoveredChar)
                {
                    _hovered = hoveredChar;

                    if (_hovered != null)
                    {
                        if (_hovered.TryGetComponent<UIPlayerComponents>(out var uICharacter)) uICharacter.CircleSelect1.SwitchStroke(true);
                    }         
                }
            }
            else
            {
                if (_hovered != null && _hovered.TryGetComponent<UIPlayerComponents>(out var uICharacter)) uICharacter.CircleSelect1.SwitchStroke(false);
                _hovered = null;
            }
        }
        else
        {
            if (_hovered != null && _hovered.TryGetComponent<UIPlayerComponents>(out var uICharacter)) uICharacter.CircleSelect1.SwitchStroke(false);
            _hovered = null;
        }
    }


    private IEnumerator HoverHighlightJob()
    {
        while(true)
        {
            UpdateHoverTargetAlways(hero);
            yield return null;
        }
    }

    private IEnumerator DrawAutoAttackRadiusJob(float radius, Transform target)
    {
        yield return null;
    }

    private IEnumerator DrawRadiusJob(float radius)
    {
        while (true)
        {
            bool hasEnemyInRadius = false;

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<Character>(out Character character) && character != hero)
                {
                    hasEnemyInRadius = true;
                    break;
                }
            }

            _circle.SetColor(hasEnemyInRadius ? _colorForAllies : _colorForEnemies);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DrawLineJob(float length, float width, Damage damage,  LayerMask layerMask, AbilityLineRenderer line)
    {
        _boxLength = length;
        _boxWidth = width;
        _lineStartImage = Instantiate(line.Start, transform);
		_lineStartImage.SetSize(_boxWidth, _boxLength, damage);
		//  _lineEndImage = Instantiate(line.End, transform);

		_lineStartImage.SetColor(_colorForStart);
      //  _lineEndImage.SetColor(_colorForEnd);

        while (true)
        {
            RotateAtMouse(_lineStartImage.transform);
            //RotateAtMouse(_lineEndImage.transform);

			//_lineStartImage.SetSize(width, length, damage);
			_lineStartImage.SetSize(_boxWidth, _boxLength, damage);
			//_lineEndImage.SetSize(width, length, damage);
		//	_lineEndImage.SetSize(_boxWidth, _boxLength, damage);

			/*Vector3 mouse = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, 0, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            var vector = (mouse - transform.position);
            var dir = vector.normalized;



            RaycastHit2D rayHit = Physics2D.Raycast(transform.position, dir, length * 2, layerMask);

            if (rayHit)
            {
                float distance = Vector2.Distance(transform.position, rayHit.transform.position);

                _lineStartImage.SetSize(width, distance / 2 + 0.3f, damage);
                _lineEndImage.SetSize(width, length, damage);
            }
            else
            {
                _lineStartImage.SetSize(width, length, damage);
                _lineEndImage.SetSize(width, length, damage);
            }*/
            yield return null;
        }
    }

    private IEnumerator DrawAreaJob(float radius, Damage damage, LayerMask layerMask, CircleArea areaPref)
    {
		Vector3 worldPosition = Vector3.zero;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask, QueryTriggerInteraction.Ignore)) worldPosition = hit.point;

		//Vector3 mouse = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,0 , Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
		Vector3 mouse = new Vector3(worldPosition.x, 0 , worldPosition.z);

        _tempArea = Instantiate(areaPref, mouse, Quaternion.Euler(0, 0, 0));
        _tempArea.SetSize(_circleRadius, damage);

        while (true)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask, QueryTriggerInteraction.Ignore))
			{
				worldPosition = hit.point;
			}
			// mouse = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,0 , Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
			mouse = new Vector3(worldPosition.x, 0 , worldPosition.z);
            _tempArea.transform.position = mouse;
            yield return null;
        }
    }

    private IEnumerator DrawClosestTargetJob(float radius, LayerMask TargetsLayers, Character player)
    {
        while (true)
        {
            if (IsOverrideClosestTarget) yield return null;

            _targets.RemoveAll(character => character == null);

            Collider[] collider = Physics.OverlapSphere(transform.position, radius + 500);

            foreach (var item in collider)
            {
                if (collider.Length > 0 && item.transform.TryGetComponent<Character>(out Character enemy))
                {
                    if (enemy == player) continue;
                    if(_targets.Contains(enemy) == false) _targets.Add(enemy);
                }
            }

            if (_targets.Count == 0)
            {
                if (_tempTarget != null)
                {
                    _tempTarget.SelectedCircle.SwitchClostestTarget(false);
                    _tempTarget = null;
                }
                yield return null;
                continue;
            }

            _targets = _targets.Where(character => character != null).OrderBy(character => Vector3.Distance(character.transform.position, transform.position)).ToList();

            if (_targets.Count > 0)
            {
				foreach (var target in _targets)
				{
                    if (target == null) continue;

                    if (Vector3.Distance(target.transform.position, transform.position) <= radius)
					{
						target.SelectedCircle.SwitchStroke(true);
					}
					else
					{
						target.SelectedCircle.SwitchStroke(false);
					}
				}

				if (_tempTarget != null)
                {
                    if (Vector3.Distance(_tempTarget.transform.position, transform.position) > Vector3.Distance(_targets[0].transform.position, transform.position))
                    {
                        _tempTarget.SelectedCircle.SwitchClostestTarget(false);
                        _tempTarget = _targets[0];
                    }
                }

                _tempTarget = _targets[0];
                _tempTarget.SelectedCircle.SwitchClostestTarget(true);

                float distanceToTarget = Vector3.Distance(_tempTarget.transform.position, transform.position);

                if (distanceToTarget <= radius) _tempTarget.SelectedCircle.SetColorTargetVariant(Color.green);
                else _tempTarget.SelectedCircle.SetColorTargetVariant(Color.red);

                if (_hoveredTarget != null)
                {
                    float distanceToTargetHover = Vector3.Distance(_hoveredTarget.transform.position, transform.position);

                    if (distanceToTargetHover <= radius) _hoveredTarget.SelectedCircle.SetColorSelectProjector(Color.green);
                    else _hoveredTarget.SelectedCircle.SetColorSelectProjector(Color.red);
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, TargetsLayers))
            {
                if (hit.transform.TryGetComponent<Character>(out Character hoveredChar))
                {
                    if (hoveredChar != player && _targets.Contains(hoveredChar))
                    {
                        if (_hoveredTarget != hoveredChar)
                        {
                            if (_hoveredTarget != null)
                            {
                                _hoveredTarget.SelectedCircle.SwitchSelectCircle(false);
                            }

                            _hoveredTarget = hoveredChar;
                            _hoveredTarget.SelectedCircle.SwitchSelectCircle(true);
                            SetPrepareCursorLight();
                        }
                    }
                    else
                    {
                        if (_hoveredTarget != null)
                        {
                            _hoveredTarget.SelectedCircle.SwitchSelectCircle(false);
                            _hoveredTarget = null;
                            SetPrepareCursor();
                        }
                    }
                }
                else
                {
                    if (_hoveredTarget != null)
                    {
                        _hoveredTarget.SelectedCircle.SwitchSelectCircle(false);
                        _hoveredTarget = null;
                        SetPrepareCursor();
                    }
                }
            }
            else
            {
                if (_hoveredTarget != null)
                {
                    _hoveredTarget.SelectedCircle.SwitchSelectCircle(false);
                    _hoveredTarget = null;
                    SetPrepareCursor();
                }
            }

            yield return null;
        }
    }

    private IEnumerator DynamicRadiusColorJob(float Radius)
    {
        while (true)
        {
            if (_tempArea != null && _circle != null)
            {
                float distance = Vector3.Distance(_tempArea.transform.position, transform.position);

                if (distance <= Radius) _circle.SetColor(_colorForAllies);
                else _circle.SetColor(_colorForEnemies);

                _circle.Draw(Radius);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
