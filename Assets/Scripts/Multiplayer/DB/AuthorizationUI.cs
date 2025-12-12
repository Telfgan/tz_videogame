using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizationUI : MonoBehaviour
{
    [SerializeField] private Authorization _authorization;
    [SerializeField] private TMP_InputField _login;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private Button _signIn;
    [SerializeField] private Button _signUp;
    [SerializeField] private TMP_Text _error;

    private void Awake()
    {
        _login.onEndEdit.AddListener(_authorization.SetLogin);
        _password.onEndEdit.AddListener(_authorization.SetPassword);

        _authorization.SetLogin(_login.text);
        _authorization.SetPassword(_password.text);

        _signIn.onClick.AddListener(OnSignIn);

        _authorization.Error += OnError;
        OnPressButton.OnSpacePressed += OnSpaceKeyPressed;
    }

    private void OnDestroy()
    {
        _authorization.Error -= OnError;
        OnPressButton.OnSpacePressed -= OnSpaceKeyPressed;
    }

    private void OnSignIn()
    {
        _signIn.interactable = false;
        _signUp.interactable = false;
        _authorization.SignIn();
    }

    private void OnError(string data)
    {
        _signIn.interactable = true;
        _signUp.interactable = true;
        _error.text = data;
    }

    private void OnSpaceKeyPressed()
    {
        if (_signIn.interactable) OnSignIn();
    }
}
