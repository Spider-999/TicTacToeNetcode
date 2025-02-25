using NUnit.Framework.Constraints;
using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject _crossGameObject;
    [SerializeField] private GameObject _circleGameObject;
    [SerializeField] private GameObject _arrowGameObject;
    [SerializeField] private GameObject _playerIndicatorGameObject;

    private void Start()
    {
        GameManager.Instance.OnCurrentPlayerChanged += GameManager_OnCurrentPlayerChanged;
        GameManager.Instance.OnClientConnected += GameManager_OnClientConnected;
    }

    private void GameManager_OnClientConnected(object sender, EventArgs e)
    {
        SetPlayerIndicator();
    }

    private void GameManager_OnCurrentPlayerChanged(object sender, EventArgs e)
    {
        SetArrowPosition();
    }

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
    }
}