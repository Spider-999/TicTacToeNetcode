using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private GameObject _crossPrefab;
    [SerializeField] private GameObject _circlePrefab;

    private List<GameObject> _symbolsList;

    private void Awake()
    {
        _symbolsList = new List<GameObject>();
    }

    private void Start()
    {
        // Subscribe to the event that is triggered when a grid tile is clicked
        GameManager.Instance.OnClickedOnGridTile += GameManager_OnClickedOnGridTile;
        GameManager.Instance.OnPlayAgainClicked += GameManager_OnPlayAgainClicked;
    }

    private void GameManager_OnPlayAgainClicked(object sender, EventArgs e)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        ResetSymbols();
    }

    private void GameManager_OnClickedOnGridTile(object sender, GameManager.OnClickedOnGridTileEventArgs e)
    {
        CreateSignRpc(e.Position, e.PlayerType);
    }

    #region RPCs
    /// <summary>
    /// Create a sign on the grid based on the player type.
    /// </summary>
    [Rpc(SendTo.Server)]
    public void CreateSignRpc(Vector2 position, PlayerType playerType)
    {
        GameObject prefab = null;

        // Get the corresponding sign for the player type
        switch (playerType)
        {
            case PlayerType.Cross:
                prefab = _crossPrefab;
                break;
            case PlayerType.Circle:
                prefab = _circlePrefab;
                break;
            default:
                Debug.Log("Player type not set.");
                break;
        }

        if(prefab == null)
        {
            Debug.Log("[CreateSignRpc] - Couldn't get player sign prefab!");
            return;
        }

        // Instantiate the cross or circle prefab at the given position based on the player type
        GameObject spawnedObject = Instantiate(prefab, position, Quaternion.identity);
        // Spawn the cross prefab on the network and destroy it when the client disconnects
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
        spawnedObject.transform.SetParent(transform);
        _symbolsList.Add(spawnedObject);
    }

    private void ResetSymbols()
    {
        foreach (GameObject symbol in _symbolsList)
        {
            if (symbol != null)
            {
                Destroy(symbol);
            }
        }
        _symbolsList.Clear();
    }
    #endregion
}
