using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    private int _width = 3;
    private int _height = 3;
    private int _tileSize = 3;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _backgroundTransform;
    private Dictionary<Vector2, Tile> _tiles;

    public Dictionary<Vector2, Tile> Tiles
    {
        get => _tiles;
        set => _tiles = value;
    }

    public int TileSize
    {
        get => _tileSize;
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void CenterCamera()
    {
        // Center the camera on the grid
        _cameraTransform.position = new Vector3(_width, _height, -20f);
        // Center the background on the grid
        _backgroundTransform.position = new Vector3(_width, _height, 0f);
    }

    private bool CheckOffset(int x, int y)
    {
        return (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
    }

    private void GenerateGrid()
    {
        // Instantiate the dictionary that holds all of the tiles
        _tiles = new Dictionary<Vector2, Tile>();

        // Create the grid
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // Create new tiles that are children to the gridmanager object.
                Tile tile = Instantiate(_tilePrefab, new Vector3(x * _tileSize, y * _tileSize, 0), Quaternion.identity, this.transform);
                // Scale the tile size
                tile.transform.localScale = new Vector3(_tileSize, _tileSize, 1);
                tile.name = $"Tile_{x}_{y}";

                // Check if the tile is offset and color it accordingly
                bool isOffset = CheckOffset(x, y);
                tile.SetColor(isOffset);

                // Add the tile to the dictionary
                _tiles.Add(new Vector2(x * _tileSize, y * _tileSize), tile);
            }
        }

        CenterCamera();
    }
}
