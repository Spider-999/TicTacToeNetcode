using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _offsetColor;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _tileHighlight;
    [SerializeField] private PlayerType _playerType;

    public PlayerType PlayerType
    {
        get => _playerType;
        set => _playerType = value;
    }

    public void SetColor(bool isOffset)
    {
        // Color the tile based on the offset passed value
        _spriteRenderer.color = isOffset ? _offsetColor : _baseColor;
    }

    private void OnMouseEnter()
    {
        _tileHighlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _tileHighlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        GameManager.Instance.ClickedOnGridTileRpc(transform.position, GameManager.Instance.PlayerType);
    }
}