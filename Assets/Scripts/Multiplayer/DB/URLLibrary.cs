using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class URLLibrary
{
    //private const string _mainServer = "http://147.45.246.50/game/";
    private const string _mainServer = "89.169.1.90";
    private const string _localHost = "localhost";
    private const string _authorization = "authorization.php";
    private const string _registration = "registration.php";
    private const string _setBottle = "SetBottle.php";
    private const string _getBottle = "GetBottle.php";
    private const string _setHeroData = "SetHeroData.php";
    private const string _getHeroData = "GetHeroData.php";

    private readonly List<string> _heroName = new()
    {
        "testhero",
        "icedeath",
        "priest",
        "creeperpoison",
        "terrifyingelf",
        "kerrigan"
    };

    public static string Authorization { get { return _mainServer + _authorization; } }
    public static string Registration { get { return _mainServer + _registration; } }
    public static string SetBottle { get { return _mainServer + _setBottle; } }
    public static string GetBottle { get { return _mainServer + _getBottle; } }
    public static string SetHeroData { get { return _mainServer + _setHeroData; } }
    public static string GetHeroData { get { return _mainServer + _getHeroData; } }

    public static string MainServer => _mainServer;
    public static string LocalHost => _localHost;
}
