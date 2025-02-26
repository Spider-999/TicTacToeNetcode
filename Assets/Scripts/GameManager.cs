using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using NUnit.Framework;
using UnityEditor.U2D.Aseprite;
using UnityEngine.UIElements;

public enum PlayerType
{
    None,
    Cross,
    Circle
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    private PlayerType _playerType;
    private NetworkVariable<PlayerType> _currentPlayer = new NetworkVariable<PlayerType>(PlayerType.None);
    private NetworkVariable<PlayerType> _hostPlayerType = new NetworkVariable<PlayerType>(PlayerType.None);
    private NetworkVariable<PlayerType> _playerTypeWinner = new NetworkVariable<PlayerType>(PlayerType.None);
    [SerializeField] private GridManager _gridManager;
    private bool _gameOver = false;

    #region Events
    public event EventHandler<OnClickedOnGridTileEventArgs> OnClickedOnGridTile;
    public event EventHandler OnClientConnected;
    public event EventHandler OnCurrentPlayerChanged;
    public event EventHandler<OnPlayerTypeWonEventArgs> OnPlayerTypeWon;

    public class OnPlayerTypeWonEventArgs : EventArgs
    {
        public PlayerType PlayerTypeWinner { get; private set; }
        public OnPlayerTypeWonEventArgs(PlayerType playerTypeWinner)
        {
            PlayerTypeWinner = playerTypeWinner;
        }
    }

    public class OnClickedOnGridTileEventArgs : EventArgs
    {
        public Vector2 Position { get; private set; }
        public PlayerType PlayerType { get; private set; }
        public OnClickedOnGridTileEventArgs(Vector2 position, PlayerType playerType)
        {
            Position = position;
            PlayerType = playerType;
        }
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridTileRpc(Vector2 position, PlayerType playerType)
    {
        if (_gameOver)
            return;

        if (!ClickedOnGridChecks(position, playerType))
            return;

        ClickedOnGridCheckWin(playerType);
        //ShowGridManagerPlayerTypes();

        OnClickedOnGridTile?.Invoke(this, new OnClickedOnGridTileEventArgs(position, playerType));
        SwitchCurrentPlayerType();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void OnPlayerTypeWonRpc()
    {
        OnPlayerTypeWon?.Invoke(this, new OnPlayerTypeWonEventArgs(_playerTypeWinner.Value));
    }
    #endregion

    #region Inbuilt unity methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("GameManager already exists.");
        }
    }

    public override void OnNetworkSpawn()
    {
        SetPlayerTypes();
        SetCurrentPlayer();
    }
    #endregion

    #region Utility methods
    public PlayerType PlayerType
    {
        get => _playerType;
    }

    public PlayerType CurrentPlayer
    {
        get => _currentPlayer.Value;
    }

    /// <summary>
    /// Method just for debugging purposes.
    /// </summary>
    private void ShowGridManagerPlayerTypes()
    {
        foreach (var tile in _gridManager.Tiles)
        {
            Debug.Log($"Tile: {tile.Key} - PlayerType: {tile.Value.PlayerType} & {tile.Value}");
        }
    }

    private bool ClickedOnGridChecks(Vector2 position, PlayerType playerType)
    {
        if (_currentPlayer.Value != playerType)
        {
            Debug.Log("It's not your turn.");
            return false;
        }

        if (_gridManager.Tiles[position].PlayerType != PlayerType.None)
        {
            Debug.Log("Tile is already occupied.");
            return false;
        }

        if (_gridManager.Tiles.ContainsKey(position))
        {
            _gridManager.Tiles[position].PlayerType = playerType;
        }
        else
        {
            Debug.Log("Tile not found.");
            return false;
        }

        return true;
    }

    private bool CheckLines(PlayerType playerType)
    {
        for (int x = 0; x < 3; ++x)
        {
            Vector2 position1 = new Vector2(0, x * _gridManager.TileSize);
            Vector2 position2 = new Vector2(_gridManager.TileSize, x * _gridManager.TileSize);
            Vector2 position3 = new Vector2(_gridManager.TileSize * 2, x * _gridManager.TileSize);

            // Check horizontal lines
            if (_gridManager.Tiles[position1].PlayerType == playerType &&
                _gridManager.Tiles[position2].PlayerType == playerType &&
                _gridManager.Tiles[position3].PlayerType == playerType)
                return true;


            position1 = new Vector2(x * _gridManager.TileSize, 0);
            position2 = new Vector2(x * _gridManager.TileSize, _gridManager.TileSize);
            position3 = new Vector2(x * _gridManager.TileSize, _gridManager.TileSize * 2);

            // Check vertical lines
            if (_gridManager.Tiles[position1].PlayerType == playerType &&
                _gridManager.Tiles[position2].PlayerType == playerType &&
                _gridManager.Tiles[position3].PlayerType == playerType)
                return true;
        }
        return false;
    }

    private bool CheckDiagonals(PlayerType playerType)
    {
        Vector2 position = new Vector2(_gridManager.TileSize, _gridManager.TileSize);
        // Secondary diagonal
        if (_gridManager.Tiles[position * 0].PlayerType == playerType &&
            _gridManager.Tiles[position].PlayerType == playerType &&
            _gridManager.Tiles[position * 2].PlayerType == playerType)
            return true;

        Vector2 position1 = new Vector2(0, _gridManager.TileSize * 2);
        Vector2 position2 = new Vector2(_gridManager.TileSize, _gridManager.TileSize);
        Vector2 position3 = new Vector2(_gridManager.TileSize * 2, 0);
        if (_gridManager.Tiles[position1].PlayerType == playerType &&
           _gridManager.Tiles[position2].PlayerType == playerType &&
           _gridManager.Tiles[position3].PlayerType == playerType)
            return true;

        return false;
    }

    private bool CheckTie()
    {
        foreach (var tile in _gridManager.Tiles)
            if (tile.Value.PlayerType == PlayerType.None)
                return false;

        return true;
    }

    private void ClickedOnGridCheckWin(PlayerType playerType)
    {
        if(CheckLines(playerType) || CheckDiagonals(playerType))
        {
            Debug.Log(playerType + " won!");
            _playerTypeWinner.Value = playerType;
            _gameOver = true;
            OnPlayerTypeWonRpc();
        }

        if(CheckTie())
        {
            Debug.Log("It's a tie!");
            _gameOver = true;
        }
    }

    private void SetPlayerTypes()
    {
        if (IsHost && _playerType == PlayerType.None)
        {
            // Set the host to a random player type.
            _playerType = GetRandomEnumValue<PlayerType>();
            _hostPlayerType.Value = _playerType;
        }
        else if(IsClient && _playerType == PlayerType.None)
        { 
            // Set the other player to the opposite player type of the host.
            _playerType = (_hostPlayerType.Value == PlayerType.Cross) ? PlayerType.Circle : PlayerType.Cross;
        }
        Debug.Log($"Player {_playerType} has joined the game.");
    }

    private void SetCurrentPlayer()
    {
        // The cross player should start first always.
        // We set this only when the player is the host
        // because the host is the one who starts the game.
        // The networking is server authoritative.
        if (IsHost)
        {
            _currentPlayer.Value = PlayerType.Cross;
        }

        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnected;

        // Attach a listener to the current player network variable.
        // Modifies the variable both on the server and the client.
        _currentPlayer.OnValueChanged += (PlayerType previousPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayerChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnected(ulong obj)
    { 
        OnClientConnected?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchCurrentPlayerType()
    {
        switch (_currentPlayer.Value)
        {
            case PlayerType.Cross:
                _currentPlayer.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                _currentPlayer.Value = PlayerType.Cross;
                break;
            default:
                Debug.Log("Invalid player type.");
                break;
        }
    }

    private static T GetRandomEnumValue<T>()
    {
        Array values = Enum.GetValues(typeof(T));
        // Get a random value from the enum excluding the first value (None).
        return (T)values.GetValue(UnityEngine.Random.Range(1, values.Length));
    }

    internal void OnPlayAgainClicked()
    {
        throw new NotImplementedException();
    }
    #endregion
}
