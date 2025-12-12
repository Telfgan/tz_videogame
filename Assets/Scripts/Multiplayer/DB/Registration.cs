using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Registration : MonoBehaviour
{
    private const string LOGIN = "login";
    private const string PASSWORD = "password";

    private int _id;
    private string _login;
    private string _password;
    private string _confirmPassword;

    public event Action<string> Error;
    public event Action<int> Successed;

    public void SetLogin(string login)
    {
        _login = login;
    }

    public void SetPassword(string password)
    {
        _password = password;
    }

    public void SetConfirmPassword(string password)
    {
        _confirmPassword = password;
    }

    public void SignUp()
    {
        if (string.IsNullOrEmpty(_login) || string.IsNullOrEmpty(_password) || string.IsNullOrEmpty(_confirmPassword))
        {
            Error?.Invoke("pass not validate");
            Debug.LogError("pass not validate");
            return;
        }
        if (_password != _confirmPassword)
        {
            Debug.LogError("pass not confirm");
            Error?.Invoke("pass not confirm");
            return;
        }

        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {LOGIN, _login},
            {PASSWORD, _password }
        };

        NetworkHTTP.Instance.Post(URLLibrary.Registration, data, Success);

        StartCoroutine(TimeOutJob());
    }

    private void Success(string data)
    {
        if (int.TryParse(data, out int id))
        {
            _id = id;
            Successed?.Invoke(id);
            Error?.Invoke("New user created");
        }
        else
        {
            Error?.Invoke(data);
        }
    }

    private IEnumerator TimeOutJob()
    {
        yield return new WaitForSecondsRealtime(15);

        if(_id == 0)
            Error?.Invoke("TimeOut");
    }
}
