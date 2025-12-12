using Mirror;
using System.Collections;
using System.Collections.Generic;
using Telepathy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisconnectButtonUI : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void OnValidate()
    {
        _button = gameObject.GetComponent<Button>();
    }

    private void Awake()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        var gameRules = FindObjectOfType<GameRules>();

        if (gameRules != null && gameRules.IsStarted)
        {
            gameRules.CloseRoomOnClient();
        }
        else
        {
            HeroComponent[] hero = FindObjectsOfType<HeroComponent>();
            foreach (var item in hero)
            {
                if (item.isOwned == true)
                {
                    var roomName = item.NetworkSettings.RoomName;
                    item.DestroySelf();
                    ServerManager.Instance.EnableMenu();
                    SceneManager.UnloadSceneAsync(roomName);
                }
            }
        }
    }
}
