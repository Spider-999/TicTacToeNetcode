using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;
using NUnit.Framework;

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

    #region Events
    public event EventHandler<OnClickedOnGridTileEventArgs> OnClickedOnGridTile;
    public event EventHandler OnClientConnected;
    public event EventHandler OnCurrentPlayerChanged;

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
        if(_currentPlayer.Value != playerType)
        {
            Debug.Log("It's not your turn.");
            return;
        }

        OnClickedOnGridTile?.Invoke(this, new OnClickedOnGridTileEventArgs(position, playerType));
        SwitchCurrentPlayerType();
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

    private void SetPlayerTypes()
    {
        if (IsHost && _playerType == PlayerType.None)
        {
            // Set the host to a random player type.
            _playerType = GetRandomEnumValue<PlayerType>();
            _hostPlayerType.Value = _playerType;
            Debug.Log(_playerType);
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
    #endregion
}
