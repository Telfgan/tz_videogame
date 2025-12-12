using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationUI : MonoBehaviour
{
    [SerializeField] private Registration _registration;
    [SerializeField] private TMP_InputField _login;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmPassword;
    [SerializeField] private Button _apply;
    [SerializeField] private Button _signIn;
    [SerializeField] private TMP_Text _error;

    private void Awake()
    {
        _login.onEndEdit.AddListener(_registration.SetLogin);
        _password.onEndEdit.AddListener(_registration.SetPassword);
        _confirmPassword.onEndEdit.AddListener(_registration.SetConfirmPassword);

        _apply.onClick.AddListener(OnSignIn);

        _registration.Error += OnError;
    }

    private void OnSignIn()
    {
        _apply.interactable = false;
        _signIn.interactable = false;
        _registration.SignUp();
    }

    private void OnError(string data)
    {
        _apply.interactable = true;
        _signIn.interactable = true;
        _error.text = data;
    }
}
