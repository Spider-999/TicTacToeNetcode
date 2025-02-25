using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _hostButton;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnJoinButtonClicked);
        _hostButton.onClick.AddListener(OnHostButtonClicked);
    }

    private void OnHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
        HideNetworkUI();
    }

    private void OnJoinButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        HideNetworkUI();
    }

    private void HideNetworkUI()
    {
        gameObject.SetActive(false);
    }
}
