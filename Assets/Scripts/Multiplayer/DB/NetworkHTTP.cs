using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkHTTP : MonoBehaviour
{
    public static NetworkHTTP Instance { get; private set; }

    private Coroutine _authorizationCoroutine;

    public static HeroData ConvertInHeroData(string data)
    {
        return JsonUtility.FromJson<HeroData>(data);
    }

    public static HeroesData ConvertInHeroesData(string data)
    {
        return JsonUtility.FromJson<HeroesData>(data);
    }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Post(string uri, Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        _authorizationCoroutine = StartCoroutine(PostJob(uri, data, success, error = null));
    }

    /// <summary>
    /// Gets the number of bottles by id
    /// </summary>
    /// <param name="data">the key is "id" for the user id</param>
    public void PostGetBottle(Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        Post(URLLibrary.GetBottle, data, success, error);
    }

    /// <summary>
    /// sets the number of bottles by id
    /// </summary>
    /// <param name="data">The key is "id" for the user ID and "bottle" for the number of bottles. </param>
    public void PostSetBottle(Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        Post(URLLibrary.SetBottle, data, success, error);
    }

    /// <summary>
    /// Set values for level, experience, skill points
    /// </summary>
    /// <param name="data">"id" for user ID. "heroName" for the hero search. "heroLVL" set lvl. "heroExp" set exp. "heroSkillPoints" set skill points.</param>
    /// <param name="success">Nothing is returned</param>
    public void PostSetHeroData(Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        Post(URLLibrary.SetHeroData, data, success, error);
    }

    /// <summary>
    /// Gets information about the hero (level, experience, number of skill points). After receiving it, you need to convert the JSON array to a C# class (use the public static HeroData ConvertInHeroData)
    /// </summary>
    /// <param name="data">the key is "id" for the user id, "heroName" for the hero search</param>
    public void PostGetHeroData(Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        Post(URLLibrary.GetHeroData, data, success, error);
    }

    private IEnumerator PostJob(string uri, Dictionary<string, string> data, Action<string> success, Action<string> error = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(uri, data))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                error?.Invoke(www.error);
            else
                success?.Invoke(www.downloadHandler.text);
        }
    }
}

[System.Serializable]
public class HeroData
{
    public int id;
    public int lvl;
    public int exp;
    public int skillpoints;
    public int users_id;
    public int heroes_id;
}

[System.Serializable]
public class HeroesData
{
    public HeroData[] Property1;
}
