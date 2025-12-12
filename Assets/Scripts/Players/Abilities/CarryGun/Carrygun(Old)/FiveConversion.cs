using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FiveConversion : MonoBehaviour
{
    public GameObject ActiveBar;
    public GameObject ManaBar;
    public GameObject Abilities;
    public Button ButtonAbility;
    public float PsionicaActive;
    public Text text;

    public delegate void UseActivePsionicaHandler(float value, GameObject target);
    public event UseActivePsionicaHandler UseActivePsionicaEvent;


    private float _psionica;
    private float _timer = 0f;
    private bool _isTimerActive = false;
    private GameObject _player;

    void Start()
    {
        _player = transform.parent.gameObject;
        PsionicaActive = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5) && _player.GetComponent<MoveComponent>().IsSelect)
        {
            ButtonAbility.onClick.Invoke();
        }
        text.text = "Активная пси: " + PsionicaActive.ToString();

        float newScaleX = PsionicaActive / 30.0f;
        ActiveBar.transform.localScale = new Vector3(newScaleX, 1.0f, 1.0f);

        if (_isTimerActive)
        {
            _timer += Time.deltaTime;
            if (_timer >= 3f)
            {
                PsionicaActive = 0f;
                _timer = 0f;
                _isTimerActive = false;
            }
        }

        if (_psionica < PsionicaActive)
        {
            ManaBar.GetComponentInChildren<SpriteRenderer>().sortingOrder = 0;

        }
        else
        {
            ManaBar.GetComponentInChildren<SpriteRenderer>().sortingOrder = -2;
        }
    }
    public void MakeActive()
    {
        if (_player.GetComponent<MoveComponent>().IsSelect && _psionica != 0)
        {
            Abilities.GetComponent<GlobalCooldown>().StartGlobalCooldown();

            _timer = 0f;
            _isTimerActive = true;

            if (_psionica >= 30)
            {
                _player.GetComponent<PsionicaMelee>().UsePsionica(30);
                PsionicaActive = 30;
            }
            else if (_psionica < 30 && _psionica != 0)
            {
                PsionicaActive += _psionica;
                _player.GetComponent<PsionicaMelee>().UsePsionica(_psionica);
            }


            if (PsionicaActive >= 30)
            {
                PsionicaActive = 30;
            }
        }
    }
    public void UseActivePsionica(float value, GameObject target)
    {
        UseActivePsionicaEvent?.Invoke(value, target);

        PsionicaActive -= value;
        PsionicaActive = Mathf.Max(PsionicaActive, 0);
    }
}
