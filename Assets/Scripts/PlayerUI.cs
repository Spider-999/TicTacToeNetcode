using NUnit.Framework.Constraints;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _crossGameObject;
    [SerializeField] private GameObject _circleGameObject;
    [SerializeField] private GameObject _arrowGameObject;
    [SerializeField] private GameObject _playerIndicatorGameObject;
    [SerializeField] private GameObject _youWonGameObject;
    [SerializeField] private GameObject _youLoseGameObject;
    [SerializeField] private Button _playAgainGameObject;
    [SerializeField] private GameObject _gridManager;
    [SerializeField] private GameObject _gameVisualManager;

    private void Awake()
    {
        _playAgainGameObject.onClick.AddListener(() => { GameManager.Instance.OnPlayAgainClickedRpc(); });
    }

    private void Start()
    {
        GameManager.Instance.OnCurrentPlayerChanged += GameManager_OnCurrentPlayerChanged;
        GameManager.Instance.OnClientConnected += GameManager_OnClientConnected;
        GameManager.Instance.OnPlayerTypeWon += GameManager_OnPlayerTypeWon;
        GameManager.Instance.OnPlayAgainClicked += GameManager_OnPlayAgainClicked;
    }

    #region Event methods
    private void GameManager_OnPlayAgainClicked(object sender, EventArgs e)
    {
        HideUI();
    }

    private void GameManager_OnPlayerTypeWon(object sender, GameManager.OnPlayerTypeWonEventArgs e)
    {
        SetPlayerWonText(e);
    }

    private void GameManager_OnClientConnected(object sender, EventArgs e)
    {
        SetPlayerIndicator();
    }

    private void GameManager_OnCurrentPlayerChanged(object sender, EventArgs e)
    {
        SetArrowPosition();
    }
    #endregion


    #region Helper methods
    private void SetArrowPosition()
    {
        _playerIndicatorGameObject.SetActive(true);
        GameObject sign;
        if (GameManager.Instance.CurrentPlayer == PlayerType.Cross)
        {
            sign = _crossGameObject;
        }
        else
        {
            sign = _circleGameObject;
        }

        float x = _arrowGameObject.transform.position.x;
        float y = sign.transform.position.y;
        _arrowGameObject.transform.position = new Vector3(x, y, 0f);
    }

    private void SetPlayerIndicator()
    {
        GameObject sign;
        if (GameManager.Instance.PlayerType == PlayerType.Cross)
        {
            sign = _crossGameObject;
        }
        else
        {
            sign = _circleGameObject;
        }

        float x = _playerIndicatorGameObject.transform.position.x;
        float y = sign.transform.position.y;
        _playerIndicatorGameObject.transform.position = new Vector3(x, y, 0f);
        _playerIndicatorGameObject.SetActive(true);
    }

    private void SetPlayerWonText(GameManager.OnPlayerTypeWonEventArgs e)
    {
        if (GameManager.Instance.CurrentPlayer == PlayerType.Cross)
        {
            if (e.PlayerTypeWinner == PlayerType.Cross)
                _youWonGameObject.SetActive(true);
            else
                _youLoseGameObject.SetActive(true);
            _playAgainGameObject.gameObject.SetActive(true);
        }
        else if(GameManager.Instance.CurrentPlayer == PlayerType.Circle)
        {
            if (e.PlayerTypeWinner == PlayerType.Circle)
                _youWonGameObject.SetActive(true);
            else
                _youLoseGameObject.SetActive(true);
            _playAgainGameObject.gameObject.SetActive(true);
        }
    }

    private void HideUI()
    {
        _playAgainGameObject.gameObject.SetActive(false);
        _youWonGameObject.SetActive(false);
        _youLoseGameObject.SetActive(false);

    }
    #endregion
}