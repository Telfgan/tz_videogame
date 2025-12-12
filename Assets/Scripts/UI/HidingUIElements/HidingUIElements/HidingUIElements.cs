using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HidingUIElements : NetworkBehaviour
{
    [SerializeField] private Character _player;
    [SerializeField] private GameObject _containerIcons;
    private List<Image> _images = new();
    private List<TMP_Text> _texts = new();

    private Dictionary<Image, Color> _originalImageColors = new();
    private Dictionary<TMP_Text, Color> _originalTextColors = new();

    private int _playerLayer;

    private void Awake()
    {
        _images.AddRange(GetComponentsInChildren<Image>());
        foreach (var image in _images)
        {
            _originalImageColors.Add(image, image.color);
        }

        _texts.AddRange(GetComponentsInChildren<TMP_Text>());
        foreach (var text in _texts)
        {
            _originalTextColors.Add(text, text.color);

        }
    }

    private void Start()
    {
        if (_player != null)
        {
            _player.OnDisappeared += OnHidingElements;
            _player.OnAppeared += OnRevealingElements;
        }
    }

    private void OnHidingElements()
    {
        PlayerLayer(_player.gameObject);

        if (_playerLayer == LayerMask.NameToLayer("Allies"))
        {
            foreach (var image in _images)
            {
                Color originalColorImage;

                var newImageTransparency = image.color;
                newImageTransparency.a = 1f;

                if (image != null && _originalImageColors.TryGetValue(image, out originalColorImage))
                {
                    image.color = new Color(originalColorImage.r, originalColorImage.g, originalColorImage.b, newImageTransparency.a);
                }
            }

            foreach (var text in _texts)
            {
                Color originalColorText;

                var newTextTransparency = text.color;
                newTextTransparency.a = 1f;

                if (text != null && _originalTextColors.TryGetValue(text, out originalColorText))
                {
                    text.color = new Color(originalColorText.r, originalColorText.g, originalColorText.b, newTextTransparency.a);
                }
            }
        }
        else if (_playerLayer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (var image in _images)
            {
                Color originalColorImage;

                var newImageTransparency = image.color;
                newImageTransparency.a = 0.0f;

                if (image != null && _originalImageColors.TryGetValue(image, out originalColorImage))
                {
                    image.color = new Color(originalColorImage.r, originalColorImage.g, originalColorImage.b, newImageTransparency.a);
                }

            }

            foreach (var text in _texts)
            {
                Color originalColorText;

                var newTextTransparency = text.color;
                newTextTransparency.a = 0.0f;

                if (text != null && _originalTextColors.TryGetValue(text, out originalColorText))
                {
                    text.color = new Color(originalColorText.r, originalColorText.g, originalColorText.b, newTextTransparency.a);
                }
            }
            _containerIcons.SetActive(false);
        }
    }

    private void OnRevealingElements()
    {
        if (_playerLayer == LayerMask.NameToLayer("Allies"))
        {
            foreach (var image in _images)
            {
                Color originalColorImage;

                if (image != null && _originalImageColors.TryGetValue(image, out originalColorImage))
                {
                    image.color = new Color(originalColorImage.r, originalColorImage.g, originalColorImage.b, originalColorImage.a);
                }

            }

            foreach (var text in _texts)
            {
                Color originalColorText;

                if (text != null && _originalTextColors.TryGetValue(text, out originalColorText))
                {
                    text.color = new Color(originalColorText.r, originalColorText.g, originalColorText.b, originalColorText.a);
                }

            }
        }
        else if (_playerLayer == LayerMask.NameToLayer("Enemy"))
        {
            foreach (var image in _images)
            {
                Color originalColorImage;

                if (image != null && _originalImageColors.TryGetValue(image, out originalColorImage))
                {
                    image.color = new Color(originalColorImage.r, originalColorImage.g, originalColorImage.b, originalColorImage.a);
                }

            }

            foreach (var text in _texts)
            {
                Color originalColorText;

                if (text != null && _originalTextColors.TryGetValue(text, out originalColorText))
                {
                    text.color = new Color(originalColorText.r, originalColorText.g, originalColorText.b, originalColorText.a);
                }

            }
            _containerIcons.SetActive(true);
        }
    }

    private void PlayerLayer(GameObject player)
    {
        _playerLayer = player.layer;
    }
}
