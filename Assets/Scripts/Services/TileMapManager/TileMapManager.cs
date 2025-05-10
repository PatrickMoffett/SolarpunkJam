using Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class TileMapManager : IService
{
    private Vector3 characterOffset = new Vector3(0.0f, -0.175f, 0f);
    private Tilemap _tilemap;
    // private AStarPathfinder _pathfinder;
    public TileMapManager()
    {
        // TODO: probably change this to when levels are loaded/unloaded
        ServiceLocator.Instance.Get<MonoBehaviorService>().StartEvent += Start;

    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindTileMap();
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        FindTileMap();
    }

    private void FindTileMap()
    {
        GameObject tilemapObject = GameObject.Find("Tilemap");
        if (tilemapObject)
        {
            _tilemap = tilemapObject.GetComponent<Tilemap>();
        }
        else
        {
            Debug.LogError("Tilemap object not found in the scene.");
        }
    }

    public Vector2Int GetTileCoordAtWorldPosition(Vector3 worldPosition)
    {
        if(_tilemap == null)
        {
            Debug.LogError("Tilemap is not initialized.");
            return Vector2Int.zero;
        }
        return (Vector2Int)_tilemap.WorldToCell(worldPosition);
    }

    public Vector3 GetWorldPositionAtTileCoord(Vector2Int tileCoord)
    {
        Vector3 position = _tilemap.CellToWorld((Vector3Int)tileCoord);
        position.z = 0f;
        return position;
    }
    public Vector3 GetCharacterOffset()
    {
        return characterOffset;
    }

    public bool IsWalkable(Vector2Int offset)
    {
        Tile tile =_tilemap.GetTile<Tile>(new Vector3Int(offset.x, offset.y, 0));
        if (tile == null)
        {
            // Tile is walkable
            return false;
        }
        ExtendedTileBase extendedTile = tile as ExtendedTileBase;
        if (extendedTile != null)
        {
            Debug.LogFormat("{0}:{1}", extendedTile.name, extendedTile.IsWalkable);
            // Check if the tile is walkable
            return extendedTile.IsWalkable;
        }
        Debug.LogError("Tile is not of type ExtendedTileBase");
        return false;
    }
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        //if (_pathfinder == null)
        {
            //_pathfinder = new AStarPathfinder();
        }
        //return _pathfinder.FindPathOffset(start, goal);
        return null;
    }
}